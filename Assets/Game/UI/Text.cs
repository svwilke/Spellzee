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
		if(renderTooltip) {
			string ttt = tooltip;
			Vector2i tooltipSize = RB.PrintMeasure(ttt) + new Vector2i(6, 6);
			Vector2i topRightPos = RB.PointerPos() - new Vector2i(0, tooltipSize.y);
			if(topRightPos.y >= 0) {
				if(topRightPos.x + tooltipSize.width < RB.DisplaySize.width) {
					RenderTooltip(new Rect2i(topRightPos, tooltipSize), ttt);
				} else {
					RenderTooltip(new Rect2i(RB.PointerPos() - tooltipSize, tooltipSize), ttt);
				}
			} else {
				if(topRightPos.x + tooltipSize.width < RB.DisplaySize.width) {
					RenderTooltip(new Rect2i(RB.PointerPos(), tooltipSize), ttt);
				} else {
					RenderTooltip(new Rect2i(RB.PointerPos() - new Vector2i(tooltipSize.x, 0), tooltipSize), ttt);
				}
			}
		}
	}

	private void RenderTooltip(Rect2i rect, string text) {
		RB.DrawRectFill(rect, Color.white);
		RB.DrawRect(rect, Color.black);
		RB.Print(rect.Expand(-2), Color.black, RB.ALIGN_H_LEFT | RB.ALIGN_V_CENTER, text);
	}

	public override void Update() {
		base.Update();
		if(tooltip.Length > 0) {
			if(new Rect2i(pos, size).Contains(RB.PointerPos())) {
				renderTooltip = true;
			} else {
				renderTooltip = false;
			}
		}
	}
}
