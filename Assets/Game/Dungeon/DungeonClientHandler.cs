using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DungeonClientHandler : ClientHandler {

	protected Game game;

	public DungeonClientHandler(Game game) {
		this.game = game;
		AddHandler(GameMsg.StartBattle, OnBattleStart);
		AddHandler(GameMsg.OpenChoice, OnOpenChoice);
		AddHandler(GameMsg.OpenVendor, OnOpenVendor);
		AddHandler(GameMsg.OpenWorld, OnOpenWorld);
	}

	public void OnBattleStart(NetworkMessage msg) {
		Battle battle = msg.ReadMessage<GameMsg.MsgStartBattle>().battle;
		game.OpenScreen(new BattleScreen(game, RB.DisplaySize, battle));
		game.OpenClientHandler(new BattleClientHandler(game, battle));
	}

	public void OnOpenChoice(NetworkMessage msg) {
		Pawn pawn = msg.ReadMessage<GameMsg.MsgPawn>().pawn;
		ChoiceScreen screen = new ChoiceScreen(game, RB.DisplaySize, pawn);
		game.OpenScreen(screen);
		game.OpenClientHandler(new ChoiceClientHandler(game, pawn, screen));
	}

	public void OnOpenVendor(NetworkMessage msg) {
		Pawn pawn = msg.ReadMessage<GameMsg.MsgPawn>().pawn;
		VendorScreen screen = new VendorScreen(game, RB.DisplaySize, pawn);
		game.OpenScreen(screen);
		game.OpenClientHandler(new VendorClientHandler(game, pawn, screen));
	}

	public void OnOpenWorld(NetworkMessage msg) {
		WorldScreen screen = new WorldScreen(game, RB.DisplaySize);
		game.OpenScreen(screen);
		game.OpenClientHandler(new WorldClientHandler(game, screen));
	}
}
