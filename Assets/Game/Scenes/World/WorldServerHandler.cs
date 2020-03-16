using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class WorldServerHandler : ServerHandler {

	protected Game game;
	private List<Pawn> players;

	public WorldServerHandler(Game game, List<Pawn> players) {
		this.game = game;
		this.players = players;
	}

	public void EnterDungeon(DungeonTemplate dungeonTemplate) {
		NetworkServer.SendToAll(GameMsg.EnterDungeon, new EmptyMessage());
		game.EnterDungeon(dungeonTemplate.Create(players), players);
	}

	public override void Open() {
		base.Open();
		foreach(Pawn pawn in players) {
			pawn.FullRestore();
		}
		NetworkServer.SendToAll(GameMsg.OpenWorld, new EmptyMessage());
	}
}
