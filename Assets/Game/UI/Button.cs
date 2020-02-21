using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : UIObj {

	private string text;
	private int align;
	private System.Action onClickAction;
	private Vector2i originalPos;

	public Button(Vector2i pos, string text, int align = ALIGN_LEFT) {
		size = RB.PrintMeasure(text);
		size.width += 8;
		size.height += 8;
		SetPosition(this.originalPos = pos, this.align = align);
		this.text = text;
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

	public void SetText(string text) {
		this.text = text;
		size = RB.PrintMeasure(text);
		size.width += 8;
		size.height += 8;
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
			RB.DrawRectFill(new Rect2i(pos.x + 1, pos.y + 1, size.x - 1, size.y - 1), Color.black);
		} else {
			off = 1;
		}
		Color backgroundHighlight = Color.white;
		Color backgroundDefault = Color.gray;
		Color foregroundHighlight = Color.yellow;
		Color foregroundDefault = Color.white;
		bool highlight = currentState == State.Hovered || currentState == State.Pressed;
		RB.DrawRectFill(new Rect2i(pos.x + off, pos.y + off, size.x - 1, size.y - 1), highlight ? backgroundHighlight : backgroundDefault);
		RB.DrawRect(new Rect2i(pos.x + 1 + off, pos.y + 1 + off, size.width - 3, size.height - 3), highlight ? foregroundHighlight : foregroundDefault);
		RB.Print(new Vector2i(pos.x + 4 + off, pos.y + 4 + off), Color.black, RB.NO_INLINE_COLOR, text);
		if(currentState != State.Disabled) RB.Print(new Vector2i(pos.x + 3 + off, pos.y + 3 + off), highlight ? foregroundHighlight : foregroundDefault, text);
	}
}
