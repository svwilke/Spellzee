using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EncounterClientHandler : ClientHandler {

	protected Game game;
	protected DungeonTemplate template;
	protected string[] path;
	protected int currentEncounter;

	public EncounterClientHandler(Game game) {
		this.game = game;
		AddHandler(GameMsg.EndEncounter, OnEncounterEnd);
	}

	public EncounterClientHandler SetDungeon(DungeonTemplate template, string[] path, int currentEncounter) {
		this.template = template;
		this.path = path;
		this.currentEncounter = currentEncounter;
		return this;
	}

	public DungeonTemplate GetDungeonTemplate() {
		return template;
	}

	public string[] GetDungeonPath() {
		return path;
	}

	public int GetCurrentEncounter() {
		return currentEncounter;
	}

	public void OnEncounterEnd(NetworkMessage msg) {
		game.OpenClientHandler(new DungeonClientHandler(game, template, path, currentEncounter + 1));
	}
}
