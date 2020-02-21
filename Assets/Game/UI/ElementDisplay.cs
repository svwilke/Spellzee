using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementDisplay {

	public Element element;
	public bool optional;
	public bool extension;
	public int extensionValue;

	public ElementDisplay(Element element, bool optional = false, bool extension = false) {
		this.element = element;
		this.optional = optional;
		this.extension = extension;
		extensionValue = 0;
	}

	public void Render(int x, int y, bool large = false) {
		ElementDisplay.Render(x, y, element, large, optional, extension ? extensionValue : -1);
	}

	public static void Render(int x, int y, Element element, bool large = false, bool optional = false, int extension = -1) {
		if(element == null) {
			return;
		}
		string name = element.GetName();
		if(!large) {
			name = name.Substring(0, 1);
		}
		Rect2i rect = new Rect2i(x, y, large ? 32 : 16, large ? 32 : 16);
		Rect2i innerRect = new Rect2i(x + (large ? 10 : 2), y + (large ? 4 : 2), 12, 12);
		if(optional || extension >= 0) {
			RB.AlphaSet(127);
		}
		RB.DrawRectFill(rect, Color.black);
		RB.DrawRectFill(rect.Expand(-1), Color.white);
		RB.AlphaSet(255);
		RB.DrawSprite(element.GetName(), innerRect);
		if(extension > 0) {
			Vector2i measure = RB.PrintMeasure(extension.ToString());
			Rect2i extRect = new Rect2i(rect.x + rect.width, rect.y + rect.height / 2 - measure.height / 2, measure).Offset(new Vector2i(1, 0));
			extRect.Expand(2);
			RB.DrawRectFill(extRect, Color.white);
			RB.DrawRect(extRect, Color.black);
			RB.Print(extRect, Color.black, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, extension.ToString());
		}
		if(large) {
			rect = rect.Offset(new Vector2i(0, 8));
			rect.Expand(0, -4);
			for(int dx = -1; dx <= 1; dx++) {
				for(int dy = -1; dy <= 1; dy++) {
					RB.Print(rect.Offset(new Vector2i(dx, dy)), Color.black, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, name);
				}
			}
			RB.Print(rect, element.GetColor(), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, name);
		}
	}
}
