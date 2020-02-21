using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : UIObj {

	private List<UIObj>[] subElements;
	private int lineSize;
	private int lineCount;

	public Panel(Vector2i pos, Vector2i size, int lineSize = 12, int align = ALIGN_LEFT) {
		this.size = new Vector2i(size.x + 6, size.y + 6);
		this.lineSize = lineSize;
		lineCount = (size.y - 6) / lineSize;
		subElements = new List<UIObj>[lineCount];
		for(int i = 0; i < subElements.Length; i++) {
			subElements[i] = new List<UIObj>();
		}
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

	public void AddObj(UIObj element, int line, int align = ALIGN_LEFT) {
		subElements[line].Add(element);
		element.pos = new Vector2i(pos.x + 3, pos.y + 3 + line * lineSize + lineSize / 2);
		if((align & ALIGN_CENTER) == ALIGN_CENTER) {
			element.pos = pos - new Vector2i(size.x, lineSize) / 2;
		} else
		if((align & ALIGN_LEFT) == ALIGN_LEFT) {
			element.pos = new Vector2i(element.pos.x, element.pos.y - lineSize / 2);
		} else
		if((align & ALIGN_RIGHT) == ALIGN_RIGHT) {
			element.pos = new Vector2i(element.pos.x - size.x, element.pos.y - lineSize / 2);
		}
	}

	public override void OnClick() {

	}

	public override void Render() {
		RB.DrawRectFill(new Rect2i(pos.x, pos.y, size.x, size.y), Color.gray);
		RB.DrawRect(new Rect2i(pos.x + 1, pos.y + 1, size.width - 3, size.height - 3), Color.white);
		int x = pos.x + 3;
		int y = pos.y + 3;
	}
}
