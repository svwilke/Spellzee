using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ScrollArea : UIObjGroup
{
	private int minY, maxY;
	private int currentY;
	private int scrollerSize;
	private Dictionary<UIObj, int> originalYPositions = new Dictionary<UIObj, int>();

	public ScrollArea(Vector2i pos, Vector2i size) {
		this.pos = pos;
		this.size = size;
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
	private int scrollStartPointer = -1;
	private int scrollStartScroller = -1;

	public override void Update() {
		Vector2i pos = RB.PointerPos();
		pos.y -= 1;
		if(IsInBounds(pos)) {
			if(RB.PointerScrollDelta() > 0) {
				SetScrollPosition(GetScrollPosition() - 0.05F);
				Debug.Log(RB.PointerScrollDelta());
			} else
			if(RB.PointerScrollDelta() < 0) {
				SetScrollPosition(GetScrollPosition() + 0.05F);
			}
		}
		UpdateSize();
		
		if(pos.y >= this.pos.y && pos.y < this.pos.y + size.height && pos.x >= this.pos.x + size.width - 5) {
			if((RB.ButtonDown(RB.BTN_POINTER_A) && isScrolling) || RB.ButtonPressed(RB.BTN_POINTER_A)) {
				if(!isScrolling && pos.y >= currentY && pos.y < currentY + scrollerSize) {
					isScrolling = true;
					scrollStartPointer = pos.y;
					scrollStartScroller = GetScrollerPos(currentY);
				} else {
					if(!isScrolling) {
						SetScrollerPos(pos.y);
						isScrolling = true;
						scrollStartPointer = pos.y;
						scrollStartScroller = GetScrollerPos(currentY);
					} else {
						SetScrollerPos(scrollStartScroller + (pos.y - scrollStartPointer));
					}
				}
			}
		} else
		if(RB.ButtonDown(RB.BTN_POINTER_A) && isScrolling) {
			SetScrollerPos(scrollStartScroller + (pos.y - scrollStartPointer));
		}
		if(RB.ButtonReleased(RB.BTN_POINTER_A)) {
			isScrolling = false;
		}
	}

	public float GetScrollPosition() {
		return (currentY - minY) / (float)(maxY - minY);
	}

	public void SetScrollPosition(float percentageScrolled) {
		UpdateSize();
		SetCurrentY(minY + (int)((maxY - minY) * percentageScrolled));
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
		if(containedUIObjs.Count > 0) {
			minY = containedUIObjs.Select(uiObj => originalYPositions[uiObj]).Min();
			maxY = containedUIObjs.Select(uiObj => originalYPositions[uiObj] + uiObj.size.height).Max() - size.height;
			if(maxY < minY) {
				maxY = minY;
			}
			SetCurrentY(currentY);
			scrollerSize = (int)(size.height * ((float)size.height / (maxY + size.height - minY)));
		} else {
			minY = 0;
			maxY = 0;
			scrollerSize = size.height;
		}
	}

	private float GetScrollbarPercentage(int y) {
		return Mathf.Clamp01((float)(y - pos.y) / size.height);
	}

	private int GetScrollerPos(int currentY) {
		return pos.y + (int)(Mathf.Clamp01((currentY - minY) / (float)(maxY - minY)) * (size.height - scrollerSize));
	}

	private void SetScrollerPos(int newY) {
		SetCurrentY(minY + (int)(Mathf.Clamp01((newY - pos.y) / (float)(size.height - scrollerSize)) * (maxY - minY)));
	}

	public override void OnClick() {
		
	}
}
