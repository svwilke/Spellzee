using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageButton : UIObj
{

	private PackedSprite sprite;
	private int align;
	private System.Action onClickAction;
	private Vector2i originalPos;

	public ImageButton(Vector2i pos, PackedSprite sprite, int align = ALIGN_LEFT) {
		this.sprite = sprite;
		size = sprite.Size;
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
			RB.SoundPlay(Game.AUDIO_BUTTON, Game.volume);
		}
	}

	public override void Render() {
		if(!isVisible) {
			return;
		}
		int off = 0;
		if(currentState != State.Pressed) {
			RB.TintColorSet(Color.black);
			RB.DrawSprite(sprite, pos + new Vector2i(1, 1));
		} else {
			off = 1;
		}
		bool highlight = currentState == State.Hovered || currentState == State.Pressed;
		if(highlight) {
			RB.TintColorSet(Color.yellow);
		} else
		if(currentState == State.Disabled) {
			RB.TintColorSet(Color.gray);
		} else {
			RB.TintColorSet(Color.white);
		}
		RB.DrawSprite(sprite, pos + new Vector2i(off, off));
		RB.TintColorSet(Color.white);
	}
}
