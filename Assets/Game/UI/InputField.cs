using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputField : UIObj {

	private FastString text;
	private bool hasFocus = false;
	private Vector2i originalPos;

	private System.Action<FastString> onCompleteEditAction;

	private int maxWidth;

	public InputField(Vector2i pos, int align = RB.ALIGN_H_LEFT | RB.ALIGN_V_CENTER, int maxStringWidth = -1) {
		size = RB.PrintMeasure(text);
		alignment = align;
		this.text = new FastString(128);
		originalPos = pos;
		SetPosition(pos, align);
		maxWidth = maxStringWidth;
	}

	public void SetOnCompleteEdit(System.Action<FastString> action) {
		onCompleteEditAction = action;
	}

	public void SetText(string text) {
		this.text.Set(text);
		Recalc();
	}

	public void AddText(string text) {
		this.text.Append(text);
		Recalc();
	}

	public void RemoveText(int spaces) {
		text.Remove(text.Length - spaces);
		Recalc();
	}

	private void Recalc() {
		size = text.Length == 0 ? RB.PrintMeasure(" ") : RB.PrintMeasure(text);
		size = new Vector2i(size.x + 6, size.y + 2);
        SetPosition(originalPos, alignment);
	}

	public override void Render() {
		bool append = (RB.Ticks / 30) % 2 == 0;
		if(append && hasFocus) {
			text.Append('|');
		}
		if(currentState == State.Hovered || currentState == State.Pressed || hasFocus) {
			RB.DrawRectFill(new Rect2i(pos.x - 4, pos.y - 2, size.x + 2, size.y + 2), Color.black);
			RB.DrawRectFill(new Rect2i(pos.x - 3, pos.y - 1, size), Color.yellow);
		}
		//RB.Print(new Vector2i(pos.x + 1, pos.y + 1), Color.black, RB.NO_INLINE_COLOR, text);
		RB.Print(new Vector2i(pos.x, pos.y), Color.black, text);
		if(append && hasFocus) {
			text.Remove(text.Length - 1);
		}
	}

	private ulong TickBackspaceDown = 0;
	public override void Update() {
		base.Update();
		if(hasFocus) {
			if(RB.KeyPressed(KeyCode.Return)) {
				OnLoseFocus();
				return;
			}
			if(RB.KeyPressed(KeyCode.Backspace)) {
				TickBackspaceDown = RB.Ticks;
			}
			if(RB.KeyDown(KeyCode.Backspace)) {
				if((RB.Ticks - TickBackspaceDown) % 8 == 0) {
					//if(text.EndsWith("|")) {
					//	RemoveText(2);
					//	text.Append('|');
					//} else {
						RemoveText(1);
					//}
				}
			} else {
				if(maxWidth < 0 || RB.PrintMeasure(text.ToString() + RB.InputString()).width <= maxWidth) {
					AddText(RB.InputString().Trim());
				}
			}
		}
	}

	public override void OnGainFocus() {
		if(IsInteractable) {
			hasFocus = true;
		}
	}

	public override void OnLoseFocus() {
		if(hasFocus) {
			if(onCompleteEditAction != null) {
				onCompleteEditAction.Invoke(text);
			}
		}
		hasFocus = false;
	}
}
