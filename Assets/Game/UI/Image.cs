using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Image : UIObj
{

    protected Color tintColor;
    protected byte alpha;
	protected PackedSprite sprite;

	private Vector2i originalPos;

	public Image(Vector2i pos, PackedSprite sprite, int align = RB.ALIGN_H_LEFT | RB.ALIGN_V_CENTER) {
		this.sprite = sprite;
		size = sprite.Size;
		alpha = 255;
		tintColor = Color.white;
		SetPosition(originalPos = pos, alignment = align);
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
		if(!isVisible) {
			return;
		}
		RB.AlphaSet(alpha);
		RB.TintColorSet(tintColor);
		RB.DrawSprite(sprite, pos);
		RB.TintColorSet(Color.white);
		RB.AlphaSet(255);
	}
}
