using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;

public class ChoiceScreen : Screen {

	private TextButton itemButton;
	private TextButton spellButton;
	private PlayerPawn pawn;

	public Text newItem;

	private Text header;

	private MessageBox waitForPlayersMsg;

	private string eq;
	public string eqToBuy = null;

	public ChoiceScreen(Game game, Vector2i size, PlayerPawn pawn) : base(game, size) {
		this.pawn = pawn;
		waitForPlayersMsg = new MessageBox("Waiting for other players...");
	}

	public void EnableItemBuy() {
		itemButton.currentState = UIObj.State.Enabled;
	}

	public override void OnConstruct() {
		AddUIObj(itemButton = new TextButton(new Vector2i(size.width / 2 - 100, size.height / 2 - 20), "Get Item", RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER));
		itemButton.SetOnClick(() => {
			Game.client.Send(GameMsg.BuySpell, new StringMessage(eqToBuy));
			itemButton.currentState = UIObj.State.Disabled;
			spellButton.currentState = UIObj.State.Disabled;
			ShowMessageBox(waitForPlayersMsg);
			
		});
		itemButton.currentState = UIObj.State.Disabled;

		AddUIObj(spellButton = new TextButton(new Vector2i(size.width / 2 + 100, size.height / 2 - 20), "+1 Extra Spell Slot", RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER));
		spellButton.SetOnClick(() => {
			Game.client.Send(GameMsg.DropSpell, new EmptyMessage());
			spellButton.currentState = UIObj.State.Disabled;
			itemButton.currentState = UIObj.State.Disabled;
			ShowMessageBox(waitForPlayersMsg);
		});

		newItem = new Text(new Vector2i(size.width / 2 - 100, size.height / 2), new Vector2i(), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER);
		newItem.SetColor(Color.black);
		AddUIObj(newItem);
		
		header = new Text(new Vector2i(size.width / 2, 80), new Vector2i(100, 20), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, "@w214Choose");
		header.FitSizeToText();
		header.SetPosition(header.pos, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER);
		header.SetEffect(Text.Outline);
		header.SetColor(Color.white);
		AddUIObj(header);
	}
}
