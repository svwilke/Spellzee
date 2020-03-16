using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon {

	private Game game;
	private List<Encounter> encounters = new List<Encounter>();
	private int progress = -1;
	private List<Pawn> players;

	public Dungeon(IEnumerable<Encounter> encounters) {
		this.encounters = encounters.ToList();
	}

	public List<Encounter> GetEncounters() {
		return new List<Encounter>(encounters);
	}

	public void EnterDungeon(Game game, List<Pawn> players) {
		this.players = players;
		this.game = game;
		progress = -1;
		NextEncounter();
	}

	public void ExitDungeon() {
		game.OpenServerHandler(new WorldServerHandler(game, players));
	}

	public void NextEncounter() {
		if(!HasNextEncounter()) {
			ExitDungeon();
		} else {
			progress += 1;
			GetCurrentEncounter().Begin(game, players);
		}
	}

	public Encounter GetCurrentEncounter() {
		if(progress < 0 || progress >= GetEncounterCount()) {
			return null;
		} else {
			return encounters[progress];
		}
	}

	public bool HasNextEncounter() {
		return progress + 1 < GetEncounterCount();
	}

	public int GetEncounterCount() {
		return encounters.Count;
	}
}
