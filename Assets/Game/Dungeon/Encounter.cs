using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public abstract class Encounter {

	protected Game game;
	protected List<Pawn> players;
	protected List<Pawn> allies;

	public void Begin(Game game, List<Pawn> players, List<Pawn> allies) {
		this.game = game;
		this.players = players;
		this.allies = allies;
		OnEncounterBegin();
	}

	public void End() {
		NetworkServer.SendToAll(GameMsg.EndEncounter, new EmptyMessage());
		OnEncounterEnd();
		game.GetCurrentDungeon().NextEncounter();
	}

	protected abstract void OnEncounterBegin();

	protected abstract void OnEncounterEnd();
}
