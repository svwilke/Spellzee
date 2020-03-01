using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class UIObj {

	public Screen screen;
    private KeyCode keyBind = KeyCode.None;

	public enum State {
		Enabled, Hovered, Pressed, Disabled
	}

	public State currentState = State.Enabled;

	public Vector2i pos;
	public Vector2i size;

    public int alignment;

	public bool isVisible = true;

	public bool IsInteractable { get { return isVisible && currentState != State.Disabled; } }

    protected UnityEvent onClick = new UnityEvent();
	protected bool hasOnClick = false;

	public virtual bool IsInBounds(Vector2i pos) {
		return pos.x >= this.pos.x && pos.y >= this.pos.y && pos.x <= this.pos.x + size.x && pos.y <= this.pos.y + size.y;
	}

	public abstract void Render();

    public void AddOnClick(System.Action action)
    {
		AddOnClick((UnityAction)(action.Invoke));
    }

	public void AddOnClick(UnityAction action)
	{
		onClick.AddListener(action);
		hasOnClick = true;
	}

	public void RemoveOnClick()
    {
		onClick.RemoveAllListeners();
		hasOnClick = false;
    }

	public void SetOnClick(System.Action onClick)
	{
		RemoveOnClick();
		AddOnClick(onClick);
	}

	public virtual void OnClick() {
        if(onClick != null && hasOnClick)
        {
            onClick.Invoke();
            Game.PlaySound(Game.AUDIO_BUTTON);
        }
    }

    public void SetKeybind(KeyCode keyCode) {
        keyBind = keyCode;
    }

    public bool HasKeybind() {
        return keyBind != KeyCode.None;
    }

    public KeyCode GetKeybind() {
        return keyBind;
    }

	public virtual void OnMouseEnter() { }

	public virtual void OnMouseExit() { }

	public virtual void OnGainFocus() { }

	public virtual void OnLoseFocus() { }

	public virtual void Update() { }

    public void SetPosition(Vector2i pos)
    {
        SetPosition(pos, alignment);
    }

    public void SetPosition(Vector2i pos, int align)
    {
        int x = pos.x;
        int y = pos.y;

        if ((align & RB.ALIGN_H_CENTER) == RB.ALIGN_H_CENTER)
        {
            x -= size.width / 2;
        }
        else
        if ((align & RB.ALIGN_H_RIGHT) == RB.ALIGN_H_RIGHT)
        {
            x -= size.width;
        }

        if ((align & RB.ALIGN_V_CENTER) == RB.ALIGN_V_CENTER)
        {
            y -= size.height / 2;
        }
        else
        if ((align & RB.ALIGN_V_BOTTOM) == RB.ALIGN_V_BOTTOM)
        {
            y -= size.height;
        }
        this.pos = new Vector2i(x, y);
    }
}
