using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class WorldClientHandler : ClientHandler {

	protected Game game;
	protected WorldScreen screen;

	public WorldClientHandler(Game game, WorldScreen screen) {
		this.game = game;
		this.screen = screen;
		AddHandler(GameMsg.EnterDungeon, OnEnterDungeon);
		AddHandler(GameMsg.ShowDungeon, OnShowDungeon);
	}

	public void OnEnterDungeon(NetworkMessage msg) {
		game.OpenClientHandler(new DungeonClientHandler(game));
	}

	public void OnShowDungeon(NetworkMessage msg) {
		screen.ShowDungeon(msg.ReadMessage<IntegerMessage>().value);
	}
}
