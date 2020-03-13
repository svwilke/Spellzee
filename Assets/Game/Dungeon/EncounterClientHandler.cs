using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EncounterClientHandler : ClientHandler {

	protected Game game;

	public EncounterClientHandler(Game game) {
		this.game = game;
		AddHandler(GameMsg.EndEncounter, OnEncounterEnd);
	}

	public void OnEncounterEnd(NetworkMessage msg) {
		game.OpenClientHandler(new DungeonClientHandler(game));
	}
}
