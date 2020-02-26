using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Screen {

	protected Vector2i size;
	protected Game game;
	protected List<UIObj> uiObjs;
	private UIObj mousePressedOn = null;
	private UIObj focusedObj = null;
	private HashSet<UIObj> objUnderMouse = new HashSet<UIObj>();
	private string tooltip = "";

	private MessageBox currentMsgBox = null;

	public Screen(Game game, Vector2i size) {
		this.game = game;
		this.size = size;
		uiObjs = new List<UIObj>();
		OnConstruct();
	}

	public void AddUIObj(UIObj obj) {
		obj.screen = this;
		uiObjs.Add(obj);
	}

	public void RemoveUIObj(UIObj obj) {
		obj.screen = null;
		uiObjs.Remove(obj);
	}

	public void SetTooltip(string tooltip) {
		this.tooltip = tooltip;
	}

	public virtual void Render() {
		foreach(UIObj uiObj in uiObjs) {
			if(uiObj.isVisible) {
				uiObj.Render();
			}
		}
		RenderForeground();
		if(currentMsgBox != null) {
			RB.AlphaSet(120);
			RB.DrawRectFill(new Rect2i(0, 0, size.width, size.height), Color.black);
			RB.AlphaSet(255);
			currentMsgBox.Render();
		}
		if(tooltip.Length > 0) {
			Vector2i tooltipSize = RB.PrintMeasure(tooltip) + new Vector2i(6, 6);
			Vector2i topRightPos = RB.PointerPos() - new Vector2i(0, tooltipSize.y);
			if(topRightPos.y >= 0) {
				if(topRightPos.x + tooltipSize.width < RB.DisplaySize.width) {
					RenderTooltip(new Rect2i(topRightPos, tooltipSize), tooltip);
				} else {
					RenderTooltip(new Rect2i(RB.PointerPos() - tooltipSize, tooltipSize), tooltip);
				}
			} else {
				if(topRightPos.x + tooltipSize.width < RB.DisplaySize.width) {
					RenderTooltip(new Rect2i(RB.PointerPos(), tooltipSize), tooltip);
				} else {
					RenderTooltip(new Rect2i(RB.PointerPos() - new Vector2i(tooltipSize.x, 0), tooltipSize), tooltip);
				}
			}
		}
	}

	public virtual void RenderForeground() {
		
	}

	private void RenderTooltip(Rect2i rect, string text) {
		RB.DrawRectFill(rect, Color.white);
		RB.DrawRect(rect, Color.black);
		RB.Print(rect.Expand(-2), Color.black, RB.ALIGN_H_LEFT | RB.ALIGN_V_CENTER, text);
	}

	public virtual void Update(bool hasFocus = true) {
		Vector2i mousePos = RB.PointerPos() + new Vector2i(0, 1); // + 0, 1 is workaround for weird hovering behaviour?!
		UIObj on = null;
		List<UIObj> objsNoLongerUnderMouse = new List<UIObj>();
		foreach(UIObj obj in objUnderMouse) {
			if(!obj.IsInBounds(mousePos) || !obj.isVisible) {
				if(objUnderMouse.Contains(obj)) {
					objsNoLongerUnderMouse.Add(obj);
					obj.OnMouseExit();
					EventBus.UIMouseExit.Invoke(obj);
				}
			}
		}
		objUnderMouse.RemoveWhere((obj) => objsNoLongerUnderMouse.Contains(obj));
		List<UIObj> uiObjsToUpdate = uiObjs;
		if(currentMsgBox != null) {
			uiObjsToUpdate = currentMsgBox.GetUIObjs();
		}
		foreach(UIObj obj in uiObjsToUpdate) {
			if(obj.IsInBounds(mousePos) && obj.isVisible) {
				if(!objUnderMouse.Contains(obj)) {
					objUnderMouse.Add(obj);
					obj.OnMouseEnter();
					EventBus.UIMouseEnter.Invoke(obj);
				}
			}
			if(obj.IsInBounds(mousePos) && obj.IsInteractable) {// && hasFocus) {
				if(obj.currentState != UIObj.State.Hovered) {
					EventBus.UIHoverStart.Invoke(obj);
				}
				obj.currentState = UIObj.State.Hovered;
				on = obj;
			} else 
			if(obj.currentState != UIObj.State.Disabled) {
				if(obj.currentState == UIObj.State.Hovered) {
					EventBus.UIHoverEnd.Invoke(obj);
				}
				obj.currentState = UIObj.State.Enabled;
			}
			
			obj.Update();
		}
		if(RB.ButtonPressed(RB.BTN_POINTER_A) && hasFocus) {
			if(on != null) {
				on.currentState = UIObj.State.Pressed;
				mousePressedOn = on;
				SetFocus(on);
			} else {
				SetFocus(null);
			}
		} else
		if(RB.ButtonReleased(RB.BTN_POINTER_A)) {
			if(on == mousePressedOn && on != null) {
				if(on.currentState == UIObj.State.Hovered) {
					EventBus.UIClick.Invoke(on);
				}
				mousePressedOn.OnClick();
			}
			mousePressedOn = null;
		} else
		if(RB.ButtonDown(RB.BTN_POINTER_A) && hasFocus) {
			if(mousePressedOn != null && on == mousePressedOn) {
				mousePressedOn.currentState = UIObj.State.Pressed;
			} else
			if(on != null) {
				on.currentState = UIObj.State.Enabled;
			}
		}

	}

	private void SetFocus(UIObj obj) {
		if(focusedObj != null) {
			focusedObj.OnLoseFocus();
		}
		focusedObj = obj;
		if(focusedObj != null) {
			focusedObj.OnGainFocus();
		}
	}

	public void ShowMessageBox(MessageBox msgBox) {
		currentMsgBox = msgBox;
	}

	public void CloseMessageBox() {
		currentMsgBox = null;
	}

	public virtual void OnConstruct() {
		
	}

	public virtual void OnOpen() {

	}

	public virtual void OnClose() {

	}

	public void Clear() {
		uiObjs.Clear();
	}
}
