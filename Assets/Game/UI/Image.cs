using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Image : UIObj
{

	private Color tintColor;
	private byte alpha;
	private PackedSprite sprite;
	private int align;
	private System.Action onClickAction;
	private Vector2i originalPos;

	public Image(Vector2i pos, PackedSprite sprite, int align = ALIGN_LEFT) {
		this.sprite = sprite;
		size = sprite.Size;
		alpha = 255;
		tintColor = Color.white;
		SetPosition(this.originalPos = pos, this.align = align);
	}

	public void SetPosition(Vector2i pos, int align) {
		if((align & ALIGN_CENTER) == ALIGN_CENTER) {
			this.pos = pos - size / 2;
		} else
		if((align & ALIGN_LEFT) == ALIGN_LEFT) {
			this.pos = new Vector2i(pos.x, pos.y - size.y / 2);
		} else
		if((align & ALIGN_RIGHT) == ALIGN_RIGHT) {
			this.pos = new Vector2i(pos.x - size.x, pos.y - size.y / 2);
		} else {
			this.pos = pos;
		}
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
		SetPosition(originalPos, align);
	}

	public void SetOnClick(System.Action action) {
		onClickAction = action;
	}

	public override void OnClick() {
		if(onClickAction != null) {
			onClickAction.Invoke();
		}
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
