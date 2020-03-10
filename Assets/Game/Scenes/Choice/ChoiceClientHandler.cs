using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class ChoiceClientHandler : ClientHandler {

	private ChoiceScreen screen;
	private Game game;
	private Pawn pawn;

	public ChoiceClientHandler(Game game, Pawn pawn, ChoiceScreen screen) {
		this.game = game;
		this.pawn = pawn;
		this.screen = screen;
		AddHandler(GameMsg.StartBattle, OnBattleStart);
		AddHandler(GameMsg.ShopList, OnShopList);
		AddHandler(GameMsg.OpenVendor, OnOpenVendor);
	}

	public void OnBattleStart(NetworkMessage msg) {
		Battle battle = msg.ReadMessage<GameMsg.MsgStartBattle>().battle;
		game.OpenScreen(new BattleScreen(game, RB.DisplaySize, battle));
		game.OpenClientHandler(new BattleClientHandler(game, battle));
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

	public void OnOpenVendor(NetworkMessage msg) {
		Pawn pawn = msg.ReadMessage<GameMsg.MsgPawn>().pawn;
		VendorScreen screen = new VendorScreen(game, RB.DisplaySize, pawn);
		game.OpenClientHandler(new VendorClientHandler(game, pawn, screen));
		game.OpenScreen(screen);
	}
}
