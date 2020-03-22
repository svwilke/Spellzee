using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ScrollArea : UIObjGroup
{

	private int currentlyOpenTab = 0;
	private int[] tabTitleX;

	private int minY, maxY;
	private int currentY;
	private int scrollerSize;
	private Dictionary<UIObj, int> originalYPositions = new Dictionary<UIObj, int>();

	private bool hideTabs = false;

	public ScrollArea(Vector2i pos, Vector2i size, bool hideTabs = false) {
		this.pos = pos;
		this.size = size;
		this.hideTabs = hideTabs;
	}

	public override void AddUIObj(UIObj obj) {
		base.AddUIObj(obj);
		originalYPositions[obj] = obj.pos.y;
		obj.pos.y += currentY;
		obj.manualRender = true;
	}

	public override void Render() {
		Rect2i visibleRect = new Rect2i(pos.x, pos.y, size.width - 5, size.height);
		Rect2i scrollbarRect = new Rect2i(pos.x + size.width - 4, pos.y, 3, size.height);
		RB.DrawRectFill(scrollbarRect, Color.black);
		Rect2i scroller = new Rect2i(pos.x + size.width - 4, GetScrollerPos(currentY), 3, scrollerSize);
		RB.DrawRectFill(scroller, Color.white);
		containedUIObjs.ForEach(uiObj => uiObj.isVisible = visibleRect.Intersects(uiObj.GetRect()));
		RB.ClipSet(visibleRect);
		containedUIObjs.ForEach(uiObj => {
			uiObj.Render();
		});
		RB.ClipReset();
	}

	private bool isScrolling = false;

	public override void Update() {
		base.Update();
		Vector2i pos = RB.PointerPos();
		if(IsInBounds(pos)) {
			if(RB.PointerScrollDelta() > 0) {
				SetCurrentY(currentY - 1);
			} else
			if(RB.PointerScrollDelta() < 0) {
				SetCurrentY(currentY + 1);
			}
		}
		UpdateSize();
		
		if(pos.y >= this.pos.y && pos.y < this.pos.y + size.height && pos.x >= this.pos.x + size.width - 5) {
			if((RB.ButtonDown(RB.BTN_POINTER_A) && isScrolling) || RB.ButtonPressed(RB.BTN_POINTER_A)) {
				float p = GetScrollbarPercentage(pos.y);
				SetCurrentY(minY + (int)((maxY - minY) * p));
				isScrolling = true;
			}
		} else
		if(RB.ButtonDown(RB.BTN_POINTER_A) && isScrolling) {
			float p = GetScrollbarPercentage(pos.y);
			SetCurrentY(minY + (int)((maxY - minY) * p));
		}
		if(RB.ButtonReleased(RB.BTN_POINTER_A)) {
			isScrolling = false;
		}
	}

	private void SetCurrentY(int curY) {
		currentY = curY;
		if(currentY > maxY) {
			currentY = maxY;
		}
		if(currentY < minY) {
			currentY = minY;
		}
		containedUIObjs.ForEach(uiObj => uiObj.pos.y = originalYPositions[uiObj] - (currentY - minY));
	}

	private void UpdateSize() {
		minY = containedUIObjs.Select(uiObj => originalYPositions[uiObj]).Min();
		maxY = containedUIObjs.Select(uiObj => originalYPositions[uiObj] + uiObj.size.height).Max() - size.height;
		if(maxY < minY) {
			maxY = minY;
		}
		SetCurrentY(currentY);
		int red = maxY + size.height - minY;
		int blue = size.height;
		int black = size.height;
		//scrollerSize = (int)(Mathf.Clamp01((float)black / red) * blue);
		scrollerSize = (int)(size.height * ((float)size.height / (maxY + size.height - minY)));
	}

	private float GetScrollbarPercentage(int y) {
		return Mathf.Clamp01((float)(y - pos.y) / size.height);
	}

	private int GetScrollerPos(int currentY) {
		return pos.y + (int)((currentY - minY) / (float)(maxY - minY) * (size.height - scrollerSize));
	}

	public override void OnClick() {
		
	}
}
