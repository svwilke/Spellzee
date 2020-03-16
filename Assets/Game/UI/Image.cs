using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Image : UIObj
{

    protected Color tintColor;
    protected byte alpha;
	protected PackedSprite sprite;
	protected int sheet;

	protected bool hasOutline = false;
	protected Color outlineColor;

	private Vector2i originalPos;

	public Image(Vector2i pos, int sheet, PackedSprite sprite, int align = RB.ALIGN_H_LEFT | RB.ALIGN_V_CENTER) {
		this.sprite = sprite;
		this.sheet = sheet;
		size = sprite.Size;
		alpha = 255;
		tintColor = Color.white;
		SetPosition(originalPos = pos, alignment = align);
	}

	public void SetOutline(Color color) {
		hasOutline = true;
		outlineColor = color;
	}

	public void SetAlpha(byte alpha) {
		this.alpha = alpha;
	}

	public void SetTint(Color color) {
		this.tintColor = color;
	}

	public void SetSprite(PackedSprite sprite) {
		this.sprite = sprite;
		size = sprite.Size;
		SetPosition(originalPos, alignment);
	}

	public override void Render() {
		if(!isVisible || sprite == null) {
			return;
		}
		if(hasOutline) {
			RB.DrawRect(new Rect2i(pos, size).Expand(1), outlineColor);
		}
		RB.AlphaSet(alpha);
		RB.TintColorSet(tintColor);
		int prev = RB.SpriteSheetGet();
		RB.SpriteSheetSet(sheet);
		RB.DrawSprite(sprite, pos);
		RB.SpriteSheetSet(prev);
		RB.TintColorSet(Color.white);
		RB.AlphaSet(255);
	}
}
