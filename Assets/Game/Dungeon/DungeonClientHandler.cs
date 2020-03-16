using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DungeonClientHandler : ClientHandler {

	protected Game game;
	protected DungeonTemplate template;
	protected string[] path;
	protected int currentEncounter;

	public DungeonClientHandler(Game game, DungeonTemplate template, string[] path, int currentEncounter) {
		this.game = game;
		this.template = template;
		this.path = path;
		this.currentEncounter = currentEncounter;
		AddHandler(GameMsg.StartBattle, OnBattleStart);
		AddHandler(GameMsg.OpenChoice, OnOpenChoice);
		AddHandler(GameMsg.OpenVendor, OnOpenVendor);
		AddHandler(GameMsg.OpenWorld, OnOpenWorld);
	}

	public void OnBattleStart(NetworkMessage msg) {
		Battle battle = msg.ReadMessage<GameMsg.MsgStartBattle>().battle;
		BattleScreen screen = new BattleScreen(game, RB.DisplaySize, battle);
		game.OpenScreen(screen);
		game.OpenClientHandler(new BattleClientHandler(game, battle, screen).SetDungeon(template, path, currentEncounter));
	}

	public void OnOpenChoice(NetworkMessage msg) {
		Pawn pawn = msg.ReadMessage<GameMsg.MsgPawn>().pawn;
		ChoiceScreen screen = new ChoiceScreen(game, RB.DisplaySize, pawn);
		game.OpenScreen(screen);
		game.OpenClientHandler(new ChoiceClientHandler(game, pawn, screen).SetDungeon(template, path, currentEncounter));
	}

	public void OnOpenVendor(NetworkMessage msg) {
		Pawn pawn = msg.ReadMessage<GameMsg.MsgPawn>().pawn;
		VendorScreen screen = new VendorScreen(game, RB.DisplaySize, pawn);
		game.OpenScreen(screen);
		game.OpenClientHandler(new VendorClientHandler(game, pawn, screen).SetDungeon(template, path, currentEncounter));
	}

	public void OnOpenWorld(NetworkMessage msg) {
		GameMsg.MsgDungeonList dungeonListMsg = msg.ReadMessage<GameMsg.MsgDungeonList>();
		WorldScreen screen = new WorldScreen(game, RB.DisplaySize, dungeonListMsg.dungeonPaths);
		game.OpenScreen(screen);
		game.OpenClientHandler(new WorldClientHandler(game, screen));
	}
}
