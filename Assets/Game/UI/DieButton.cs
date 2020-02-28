using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;

public class DieButton : UIObj
{
	private int id;
	private Battle battle;

	private static KeyCode[] keyCodes = new KeyCode[] { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Z, KeyCode.U };

	public DieButton(Battle battle, int id, Vector2i pos) {
		this.id = id;
		this.battle = battle;
		size = new Vector2i(32, 32);
		SetPosition(pos);
		if(id >= 0 && id < keyCodes.Length) {
			SetKeybind(keyCodes[id]);
		}
	}

	public void SetPosition(Vector2i pos) {
		this.pos = pos;
	}

	public override void OnClick() {
		if(battle.rolls[id] != Element.None) {
			Game.client.Send(GameMsg.ToggleDieLock, new IntegerMessage() { value = id });
		}
	}

	public override void Render() {
		if(!isVisible) {
			return;
		}
		Color backgroundHighlight = Color.white;
		Color backgroundDefault = Color.gray;
		Color foregroundHighlight = Color.yellow;
		Color foregroundDefault = Color.white;
		bool highlight = currentState == State.Hovered || currentState == State.Pressed;
		//battle.rolls[id].Render(pos.x, pos.y);
		ElementDisplay.Render(pos.x, pos.y, battle.rolls[id], true);
		if(highlight) {
			RB.DrawRect(new Rect2i(pos.x, pos.y, size.x, size.y), Color.yellow);
		} else {
			//RB.DrawRect(new Rect2i(pos.x - 1, pos.y - 1, size.x + 2, size.y + 2), Color.black);
		}
	}
}
