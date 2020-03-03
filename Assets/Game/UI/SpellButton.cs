using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;

public class SpellButton : UIObj
{
	private new BattleScreen screen;
	public Spell spell;
	private Battle battle;

	public bool castable = false;
	private bool blocked = false;

	public SpellButton(BattleScreen screen, Battle battle, Spell spell, Vector2i pos, bool blocked) {
		SetSpell(spell);
		this.screen = screen;
		this.battle = battle;
		size = new Vector2i(92, 28);
		SetPosition(pos);
		this.blocked = blocked;
	}

	public void SetPosition(Vector2i pos) {
		this.pos = pos;
	}

	public override void OnClick() {
		if(!blocked) {
			if(spell.DoesRequireTarget()) {
				screen.BeginTargeting(spell, this);
			} else {
				Game.client.Send(GameMsg.CastSpell, new GameMsg.MsgCastSpell() { spellId = spell.GetId() });
			}
		}
	}

	public void SetSpell(Spell spell) {
		this.spell = spell;
	}

	private bool renderTooltip = false;

	public override void Render() {
		if(!isVisible) {
			return;
		}
		bool highlight = currentState == State.Hovered || currentState == State.Pressed;

		RB.DrawRectFill(new Rect2i(pos, size), currentState == State.Pressed ? Color.yellow : castable ? Color.white : Color.cyan);

		RB.Print(pos + new Vector2i(2, 2), Color.black, spell.GetName());

		ElementDisplay[] displays = spell.GetElementDisplays(battle.BuildContext(screen.GetCurrentTargetPawnId()));
		int px = pos.x + 2;
		int py = pos.y + size.height - 18;
		for(int i = 0; i < displays.Length; i++) {
			displays[i].Render(px + i * 18, py);
		}

		if(highlight) {
			RB.DrawRect(new Rect2i(pos.x - 1, pos.y - 1, size.x + 2, size.y + 2), Color.yellow);
		} else {
			RB.DrawRect(new Rect2i(pos.x - 1, pos.y - 1, size.x + 2, size.y + 2), Color.black);
		}
	}

	public override void OnMouseEnter() {
		screen.SetTooltip(spell.GetShortDescription(battle.BuildContext(screen.GetCurrentTargetPawnId())));
	}

	public override void OnMouseExit() {
		screen.SetTooltip("");
	}
}
