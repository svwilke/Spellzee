using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Text : UIObj
{
	
	public const int Shadow = 1;
	public const int Outline = 2;

	private string text;
	private int flags;
	private int effect;
	private Color color;

	private string tooltip;
	private bool renderTooltip;

	public Text(Vector2i pos, Vector2i size, int flags, string text = "") {
		this.pos = pos;
		this.size = size;
		this.flags = flags;
		this.text = text;
		color = Color.black;
		tooltip = "";
		effect = 0;
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

	public void FitSizeToText(int expand = 0) {
		size = RB.PrintMeasure(text);
		if(expand != 0) {
			size += new Vector2i(expand * 2, expand * 2);
			pos -= new Vector2i(expand, expand);
		}
	}

	public string GetText() {
		return text;
	}

	public void SetTooltip(string text) {
		tooltip = text;
	}

	public void SetFlags(int flags) {
		this.flags = flags;
	}

	public override void Render() {
		Rect2i rect = new Rect2i(pos, size);
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
	}

	public override void OnMouseEnter() {
		screen.SetTooltip(tooltip);
	}

	public override void OnMouseExit() {
		screen.SetTooltip("");
	}
}
