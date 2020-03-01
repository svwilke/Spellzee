using System.Collections.Generic;
using UnityEngine;

public class TabbedPane : UIObj
{

	private int currentlyOpenTab = 0;
	private string[] titles;
	private List<UIObj>[] tabs;
	private int[] tabTitleX;

	private int tabY;

	private bool hideTabs = false;

	public TabbedPane(Vector2i pos, Vector2i size, bool hideTabs = false) {
		this.pos = pos;
		this.size = size;
		this.hideTabs = hideTabs;
	}

	public void SetTabs(string[] tabTitles) {
		titles = tabTitles;
		tabs = new List<UIObj>[tabTitles.Length];
		tabY = pos.y + RB.PrintMeasure(tabTitles[0]).height + 4;
		tabTitleX = new int[tabTitles.Length + 1];
		for(int i = 0; i < tabs.Length; i++) {
			tabs[i] = new List<UIObj>();
		}
	}

	public void SetTabsHidden(bool hideTabs) {
		this.hideTabs = hideTabs;
	}

	public bool GetTabsHidden() {
		return hideTabs;
	}

	public void AddToTab(int index, UIObj obj) {
		tabs[index].Add(obj);
		obj.isVisible = index == currentlyOpenTab;
	}

	public void OpenTab(int index) {
		foreach(UIObj obj in tabs[currentlyOpenTab]) {
			obj.isVisible = false;
		}
		foreach(UIObj obj in tabs[index]) {
			obj.isVisible = true;
		}
		currentlyOpenTab = index;
	}

	public int GetOpenTabIndex() {
		return currentlyOpenTab;
	}

	public override void Render() {
		if(titles != null && !hideTabs) {
			int x = pos.x + 1;
			for(int i = 0; i < titles.Length; i++) {
				tabTitleX[i] = x;
				Vector2i textSize = RB.PrintMeasure(titles[i]);
				if(currentlyOpenTab == i) {
					RB.DrawRectFill(new Rect2i(x, pos.y, textSize.width + 5, textSize.height + 5), Color.gray);
					RB.DrawRect(new Rect2i(x, pos.y, textSize.width + 5, textSize.height + 5), Color.white);
					RB.Print(new Vector2i(x + 3, pos.y + 3), Color.black, titles[i]);
					RB.Print(new Vector2i(x + 2, pos.y + 2), Color.white, titles[i]);
					x += textSize.width + 5;
				} else {
					RB.DrawRectFill(new Rect2i(x, pos.y + 1, textSize.width + 2, textSize.height + 4), Color.gray);
					RB.Print(new Vector2i(x + 1, pos.y + 2), Color.black, titles[i]);
					//RB.Print(new Vector2i(x + 2, tabY + 2), Color.white, titles[i]);
					x += textSize.width + 2;
				}

			}
			tabTitleX[titles.Length] = x;
		}

		int y = tabY;
		int h = size.y - (tabY - pos.y);
		if(hideTabs) {
			y = pos.y;
			h = size.y;
		}
		Rect2i tabRect = new Rect2i(pos.x, y, size.x, h);
		RB.DrawRectFill(tabRect, Color.gray);
		RB.DrawRect(tabRect.Expand(-1), Color.white);
	}

	public override void OnClick() {
		if(hideTabs) {
			return;
		}
		Vector2i pos = RB.PointerPos();
		if(pos.y < tabY) {
			for(int i = 0; i < tabs.Length; i++) {
				if(pos.x >= tabTitleX[i] && pos.x < tabTitleX[i + 1]) {
					OpenTab(i);
					Game.PlaySound(Game.AUDIO_BUTTON);
					return;
				}
			}
		}
	}
}
