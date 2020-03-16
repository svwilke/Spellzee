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
	protected bool hasBackground = false;
	protected Color backgroundColor;

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

	public void SetBackground(Color color) {
		hasBackground = true;
		backgroundColor = color;
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
		if(hasBackground) {
			RB.DrawRectFill(new Rect2i(pos, size), backgroundColor);
		}
		if(hasOutline) {
			RB.DrawRect(new Rect2i(pos, size).Expand(1), outlineColor);
		}
		RB.AlphaSet(alpha);
		Color oldTint = RB.TintColorGet();
		RB.TintColorSet(tintColor);
		int oldSheet = RB.SpriteSheetGet();
		RB.SpriteSheetSet(sheet);
		RB.DrawSprite(sprite, pos);
		RB.SpriteSheetSet(oldSheet);
		RB.TintColorSet(oldTint);
		RB.AlphaSet(255);
	}
}
