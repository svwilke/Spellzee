using System.Linq;
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
		string[] array = msg.ReadMessage<GameMsg.MsgStringArray>().array;
		DungeonTemplate template = DungeonTemplates.Get(array[0]);
		string[] path = array.Skip(1).ToArray();
		game.OpenClientHandler(new DungeonClientHandler(game, template, path, 0));
	}

	public void OnShowDungeon(NetworkMessage msg) {
		screen.ShowDungeon(msg.ReadMessage<IntegerMessage>().value);
	}
}
