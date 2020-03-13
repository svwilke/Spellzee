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
	}

	public void OnBattleStart(NetworkMessage msg) {
		Battle battle = msg.ReadMessage<GameMsg.MsgStartBattle>().battle;
		game.OpenScreen(new BattleScreen(game, RB.DisplaySize, battle));
		game.OpenClientHandler(new BattleClientHandler(game, battle));
	}

	public void OnOpenChoice(NetworkMessage msg) {
		Pawn pawn = msg.ReadMessage<GameMsg.MsgPawn>().pawn;
		ChoiceScreen screen = new ChoiceScreen(game, RB.DisplaySize, pawn);
		MessageBox msgBox = new MessageBox("Congratulations! This area is cleared");
		game.OpenClientHandler(new ChoiceClientHandler(game, pawn, screen));
		msgBox.AddButton("Continue", () => {
			game.OpenScreen(screen);
		});
		game.GetOpenScreen().ShowMessageBox(msgBox);
	}

	public void OnOpenVendor(NetworkMessage msg) {
		Pawn pawn = msg.ReadMessage<GameMsg.MsgPawn>().pawn;
		VendorScreen screen = new VendorScreen(game, RB.DisplaySize, pawn);
		MessageBox msgBox = new MessageBox("Congratulations! This area is cleared");
		game.OpenClientHandler(new VendorClientHandler(game, pawn, screen));
		msgBox.AddButton("Continue", () => {
			game.OpenScreen(screen);
		});
		game.GetOpenScreen().ShowMessageBox(msgBox);
	}
}
