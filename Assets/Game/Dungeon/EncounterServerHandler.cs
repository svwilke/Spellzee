using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EncounterServerHandler : ServerHandler {

	protected Game game;
	protected Encounter encounter;
	private bool[] ready;

	public EncounterServerHandler(Game game, Encounter encounter) {
		this.game = game;
		this.encounter = encounter;
		AddHandler(GameMsg.Ready, OnReady);
		ready = new bool[game.GetPlayerCount()];
	}

	public void OnReady(NetworkMessage msg) {
		ReadyPlayer(msg.conn.connectionId);
	}

	protected void ReadyPlayer(int playerId) {
		ready[playerId] = true;
		for(int i = 0; i < ready.Length; i++) {
			if(!ready[i]) {
				return;
			}
		}
		encounter.End();
	}
}
