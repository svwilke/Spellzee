using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class VendorClientHandler : EncounterClientHandler {

	private VendorScreen screen;
	private Pawn pawn;

	public VendorClientHandler(Game game, Pawn pawn, VendorScreen screen) : base(game) {
		this.game = game;
		this.pawn = pawn;
		this.screen = screen;
		AddHandler(GameMsg.StartBattle, OnBattleStart);
		AddHandler(GameMsg.ShopList, OnShopList);
		AddHandler(GameMsg.BuySpell, OnBuySpell);
		AddHandler(GameMsg.DropSpell, OnDropSpell);
	}

	public void OnBattleStart(NetworkMessage msg) {
		Battle battle = msg.ReadMessage<GameMsg.MsgStartBattle>().battle;
		game.OpenScreen(new BattleScreen(game, RB.DisplaySize, battle));
		game.OpenClientHandler(new BattleClientHandler(game, battle));
	}

	public void OnShopList(NetworkMessage msg) {
		List<string> buyList = new List<string>(msg.ReadMessage<GameMsg.MsgStringArray>().array);
		screen.UpdateBuy(buyList);
	}

	public void OnBuySpell(NetworkMessage msg) {
		pawn.AddSpell(msg.ReadMessage<StringMessage>().value);
		screen.BlockBuying();
		screen.UpdateSell(new List<string>(pawn.GetKnownSpellIds()));
	}

	public void OnDropSpell(NetworkMessage msg) {
		pawn.RemoveSpell(msg.ReadMessage<StringMessage>().value);
		screen.UpdateSell(new List<string>(pawn.GetKnownSpellIds()));
	}
}
