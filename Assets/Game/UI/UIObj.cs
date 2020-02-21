using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIObj {

	public Screen screen;

	public const int ALIGN_LEFT = 1;
	public const int ALIGN_CENTER = 2;
	public const int ALIGN_RIGHT = 4;

	public enum State {
		Enabled, Hovered, Pressed, Disabled
	}

	public State currentState = State.Enabled;

	public Vector2i pos;
	public Vector2i size;

	public bool isVisible = true;

	public bool IsInteractable { get { return isVisible && currentState != State.Disabled; } }

	public virtual bool IsInBounds(Vector2i pos) {
		return pos.x >= this.pos.x && pos.y >= this.pos.y && pos.x <= this.pos.x + size.x && pos.y <= this.pos.y + size.y;
	}

	public abstract void Render();

	public virtual void OnClick() { }

	public virtual void OnMouseEnter() { }

	public virtual void OnMouseExit() { }

	public virtual void OnGainFocus() { }

	public virtual void OnLoseFocus() { }

	public virtual void Update() {
		
	}
}
