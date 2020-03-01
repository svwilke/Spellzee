using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;

public class VendorButton : UIObj
{
	public Spell spell;

	public bool castable = false;
	public bool blocked = false;
	private bool toBuy = false;

	public VendorButton(Spell spell, Vector2i pos, bool toBuy = false, bool blocked = false) {
		SetSpell(spell);
		size = new Vector2i(92, 28);
		SetPosition(pos, RB.ALIGN_H_CENTER | RB.ALIGN_V_TOP);
		this.toBuy = toBuy;
		this.blocked = blocked;
	}

	public void SetPosition(Vector2i pos) {
		this.pos = pos;
	}

	public override void OnClick() {
		MessageBox box;
		if(!blocked) {
			if(toBuy) {
				box = new MessageBox("Do you really want to learn " + spell.GetName() + "?");
				box.AddButton("Yes", () => {
					Game.client.Send(GameMsg.BuySpell, new StringMessage(spell.GetId()));
					screen.ShowMessageBox(null);
					currentState = State.Disabled;
				});
				box.AddButton("No", () => screen.ShowMessageBox(null));
			} else {
				box = new MessageBox("Do you really want to forget " + spell.GetName() + "?");
				box.AddButton("Yes", () => {
					Game.client.Send(GameMsg.DropSpell, new StringMessage(spell.GetId()));
					screen.ShowMessageBox(null);
				});
				box.AddButton("No", () => screen.ShowMessageBox(null));
			}
		} else {
			box = new MessageBox("You currently can't learn this spell,\nyou need to forget one first.");
			box.AddButton("Got it", () => {
				screen.ShowMessageBox(null);
			});
		}
		screen.ShowMessageBox(box);
	}

	public void SetSpell(Spell spell) {
		this.spell = spell;
	}

	public override void Render() {
		if(!isVisible) {
			return;
		}
		bool highlight = currentState == State.Hovered || currentState == State.Pressed;
		bool disabled = currentState == State.Disabled || blocked;

		RB.DrawRectFill(new Rect2i(pos, size), currentState == State.Pressed ? Color.yellow : disabled ? Color.gray : Color.white);

		RB.Print(pos + new Vector2i(2, 2), Color.black, spell.GetName());

		ElementDisplay[] displays = spell.GetElementDisplays(RollContext.Null);
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
}
