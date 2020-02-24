using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageBox : UIObj {

	public const int Shadow = 1;
	public const int Outline = 2;

	private string text;
	private int flags;
	private int effect;
	private Color color;

	private Color boxBackgroundColor;
	private Color boxOutlineColor;

	private int buttonY = 0;

	private int totalButtonWidth = 0;
	private List<UIObj> buttons = new List<UIObj>();

	public MessageBox(string text, int flags = RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP, int maxSize = -1) {
		if(maxSize > -1) {
			flags |= RB.TEXT_OVERFLOW_WRAP;
			size = RB.PrintMeasure(new Rect2i(0, 0, maxSize, 100000), flags, text);
		} else {
			size = RB.PrintMeasure(text);
		}
		size += new Vector2i(8, 8);
		size += new Vector2i(0, 20);
		pos = RB.DisplaySize / 2 - size / 2;
		buttonY = pos.y + size.height - 12;
		this.flags = flags;
		this.text = text;
		color = Color.black;
		boxBackgroundColor = Color.white;
		boxOutlineColor = Color.black;
		effect = 0;
	}

	public void AddButton(string text, System.Action onClick) {
		int measure = RB.PrintMeasure(text).width + 8;
		if(totalButtonWidth > 0) {
			totalButtonWidth += 4;
		}
		totalButtonWidth += measure;
		TextButton button = new TextButton(new Vector2i(0, 0), text);
		buttons.Add(button);
		button.SetOnClick(onClick);
		int x = pos.x + size.width / 2 - totalButtonWidth / 2;
		for(int i = 0; i < buttons.Count; i++) {
			TextButton b = buttons[i] as TextButton;
			b.SetPosition(new Vector2i(x, buttonY));
			x += b.size.width + 4;
		}
		if(totalButtonWidth + 8 > size.width) {
			size.width = totalButtonWidth + 8;
			pos = RB.DisplaySize / 2 - size / 2;
		}
	}

	public void SetEffect(int effect) {
		this.effect = effect;
	}

	public void SetColor(Color color) {
		this.color = color;
	}

	public void SetText(string text) {
		this.text = text;
	}

	public void SetFlags(int flags) {
		this.flags = flags;
	}

	public override void Render() {
		Rect2i rect = new Rect2i(pos, size);
		RB.DrawRectFill(rect, boxBackgroundColor);
		RB.DrawRect(rect, boxOutlineColor);
		rect.Expand(-4);
		if((effect & Shadow) == Shadow) {
			RB.Print(rect.Offset(new Vector2i(1, 1)), Color.black, RB.NO_INLINE_COLOR | flags, text);
		} else
		if((effect & Outline) == Outline) {
			for(int dx = -1; dx <= 1; dx++) {
				for(int dy = -1; dy <= 1; dy++) {
					RB.Print(rect.Offset(new Vector2i(dx, dy)), Color.black, RB.NO_INLINE_COLOR | flags, text);
				}
			}
		}
		RB.Print(rect, color, flags, text);
		foreach(TextButton button in buttons) {
			button.Render();
		}
	}

	public List<UIObj> GetUIObjs() {
		return buttons;
	}
}
