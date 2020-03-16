using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class ChoiceClientHandler : EncounterClientHandler {

	private ChoiceScreen screen;
	private Pawn pawn;

	public ChoiceClientHandler(Game game, Pawn pawn, ChoiceScreen screen) : base(game) {
		this.game = game;
		this.pawn = pawn;
		this.screen = screen;
		AddHandler(GameMsg.ShopList, OnShopList);
	}

	public void OnShopList(NetworkMessage msg) {
		string eqId = msg.ReadMessage<StringMessage>().value;
		Equipment eq = Equipments.Get(eqId);
		screen.newItem.SetText("To buy: " + eq.GetName());
		screen.newItem.SetTooltip(eq.GetDescription());
		screen.newItem.FitSizeToText();
		screen.newItem.SetPosition(screen.newItem.pos, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER);
		screen.eqToBuy = eqId;
		screen.EnableItemBuy();
	}
}
