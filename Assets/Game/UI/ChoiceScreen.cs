using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;

public class ChoiceScreen : Screen {

	private TextButton itemButton;
	private TextButton spellButton;
	private PlayerPawn pawn;

	public Text newItem;
	private Text currentItem;

	private Text header;

	private MessageBox waitForPlayersMsg;

	private int eq = 0;
	public int eqToBuy = -1;

	public ChoiceScreen(Game game, Vector2i size, PlayerPawn pawn) : base(game, size) {
		this.pawn = pawn;
		waitForPlayersMsg = new MessageBox("Waiting for other players...");
		int[] equip = this.pawn.GetEquipment();
		if(this.pawn.GetEquipment().Length > 0) {
			eq = equip[0];
			currentItem.SetText("Current item: " + DB.Equipments[eq].GetName());
			currentItem.SetTooltip(DB.Equipments[eq].GetDescription());
		} else {
			currentItem.SetTooltip("Current item: none");
			eq = -1;
		}
		currentItem.FitSizeToText();
	}

	public override void OnConstruct() {
		AddUIObj(itemButton = new TextButton(new Vector2i(size.width / 2 - 100, size.height / 2 - 20), "Get Item", RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER));
		itemButton.SetOnClick(() => {
			if(eq >= 0) {
				MessageBox reallyItem = new MessageBox("Buying this will replace your current item.");
				reallyItem.AddButton("Buy anyway", () => {
					Game.client.Send(GameMsg.BuySpell, new IntegerMessage(eqToBuy));
					itemButton.currentState = UIObj.State.Disabled;
					spellButton.currentState = UIObj.State.Disabled;
					ShowMessageBox(waitForPlayersMsg);
				});
				reallyItem.AddButton("Please no", () => CloseMessageBox());
				ShowMessageBox(reallyItem);
			} else {
				Game.client.Send(GameMsg.BuySpell, new IntegerMessage(eqToBuy));
				itemButton.currentState = UIObj.State.Disabled;
				spellButton.currentState = UIObj.State.Disabled;
				ShowMessageBox(waitForPlayersMsg);
			}
			
		});

		AddUIObj(spellButton = new TextButton(new Vector2i(size.width / 2 + 100, size.height / 2 - 20), "+1 Extra Spell Slot", RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER));
		spellButton.SetOnClick(() => {
			Game.client.Send(GameMsg.DropSpell, new EmptyMessage());
			spellButton.currentState = UIObj.State.Disabled;
			itemButton.currentState = UIObj.State.Disabled;
			ShowMessageBox(waitForPlayersMsg);
		});

		newItem = new Text(new Vector2i(size.width / 2 - 160, size.height / 2), new Vector2i(), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER);
		currentItem = new Text(new Vector2i(size.width / 2 - 160, size.height / 2 + 20), new Vector2i(), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER);
		newItem.SetColor(Color.black);
		currentItem.SetColor(Color.black);
		AddUIObj(newItem);
		AddUIObj(currentItem);

		header = new Text(new Vector2i(size.width / 2, 80), new Vector2i(100, 20), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, "@w214Choose");
		header.FitSizeToText();
		header.SetEffect(Text.Outline);
		header.SetColor(Color.white);
		AddUIObj(header);
	}
}
