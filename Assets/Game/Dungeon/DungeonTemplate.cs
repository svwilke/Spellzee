using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonTemplate : RegistryEntry<DungeonTemplate> {

	private string name;
	private string description;
	private List<Func<List<Pawn>, Encounter>> encounterFactories = new List<Func<List<Pawn>, Encounter>>();

	public DungeonTemplate(string name, string description) {
		this.name = name;
		this.description = description;
	}

	public string GetName() {
		return name;
	}

	public string GetDescription() {
		return description;
	}

	public DungeonTemplate AddEncounter(Func<List<Pawn>, Encounter> encounterFactory) {
		encounterFactories.Add(encounterFactory);
		return this;
	}

	public Dungeon Create(List<Pawn> players) {
		List<Encounter> encounters = new List<Encounter>(encounterFactories.Count);
		foreach(Func<List<Pawn>, Encounter> encounterFactory in encounterFactories) {
			encounters.Add(encounterFactory.Invoke(players));
		}
		Dungeon dungeon = new Dungeon(encounters);
		return dungeon;
	}
}
