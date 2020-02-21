using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipArea : UIObj {

	private string text;

	public TooltipArea(Vector2i pos, Vector2i size, string tooltip = "") {
		this.pos = pos;
		this.size = size;
		text = tooltip;
	}

	public TooltipArea(Rect2i rect, string tooltip = "") : this(rect.min, rect.max - rect.min, tooltip) {
		
	}

	public void SetTooltip(string text) {
		this.text = text;
	}

	public override void Render() {
		
	}

	public override void OnMouseEnter() {
		screen.SetTooltip(this.text);
	}

	public override void OnMouseExit() {
		screen.SetTooltip("");
	}
}
