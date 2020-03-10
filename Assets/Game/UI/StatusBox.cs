using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusBox : UIObj
{
	private List<Status> statuses;

	public StatusBox(Vector2i pos, int width, List<Status> statuses) {
		this.pos = pos;
		int statusDescH = 10;
		int statusDescW = width;
		foreach(Status status in statuses) {
			string desc = status.GetDescription();
			if(desc != null && desc.Length > 0) {
				int h = RB.PrintMeasure(new Rect2i(pos.x + 5, pos.y, statusDescW - 10, 10000), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP | RB.TEXT_OVERFLOW_WRAP, desc).height;
				if(statusDescH > 10) {
					statusDescH += 3;
				}
				statusDescH += h;
			}
		}
		size = new Vector2i(statusDescW, statusDescH);
		this.statuses = statuses;
	}

	public override void Render() {
		if(statuses.Count > 0) {
			Rect2i statusDescRect = new Rect2i(pos, size);
			RB.DrawRect(new Rect2i(pos.x + 1, pos.y + 1, size.width, size.height), Color.black);
			RB.DrawRectFill(statusDescRect, Color.gray);
			RB.DrawRect(statusDescRect.Expand(-1), Color.white);

			int currentY = pos.y + 5;
			bool first = true;
			foreach(Status status in statuses) {
				string desc = status.GetDescription();
				if(desc != null && desc.Length > 0) {
					int h = RB.PrintMeasure(new Rect2i(pos.x + 5, currentY, size.width - 10, 10000), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP | RB.TEXT_OVERFLOW_WRAP, desc).height;
					if(!first) {
						currentY += 3;
					}
					RB.Print(new Rect2i(pos.x + 5, currentY, size.width - 10, 10000), Color.black, RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP | RB.TEXT_OVERFLOW_WRAP, desc);
					currentY += h;
					first = false;
				}
			}
		}
	}
}
