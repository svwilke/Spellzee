using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextButton : UIObj {

	private string text;
	private System.Action onClickAction;
	private Vector2i originalPos;

	public TextButton(Vector2i pos, string text, int align = RB.ALIGN_H_LEFT | RB.ALIGN_V_CENTER) {
		size = RB.PrintMeasure(text);
		size.width += 8;
		size.height += 8;
		SetPosition(originalPos = pos, alignment = align);
		this.text = text;
	}

	public void SetText(string text) {
		this.text = text;
		size = RB.PrintMeasure(text);
		size.width += 8;
		size.height += 8;
		SetPosition(originalPos, alignment);
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
