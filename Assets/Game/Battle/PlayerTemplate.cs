using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTemplate {

	private string className;
	private int maxHp = 16;
	private List<int> knownSpells;
	private double[] affinityModifiers;

	public PlayerTemplate(string name) {
		className = name;
		knownSpells = new List<int>();
		affinityModifiers = new double[7];
	}

	public string GetName() {
		return className;
	}

	public PlayerTemplate SetAffinities(double fire, double water, double earth, double air, double light, double dark, double chaos) {
		affinityModifiers[0] = fire - 10;
		affinityModifiers[1] = water - 10;
		affinityModifiers[2] = earth - 10;
		affinityModifiers[3] = air - 10;
		affinityModifiers[4] = light - 10;
		affinityModifiers[5] = dark - 10;
		affinityModifiers[6] = chaos;
		return this;
	}

	public PlayerTemplate SetMaxHp(int maxHp) {
		this.maxHp = maxHp;
		return this;
	}

	public PlayerTemplate AddSpells(params int[] spellIds) {
		knownSpells.AddRange(spellIds);
		return this;
	}

	public PlayerPawn Create(LobbyClientHandler.LobbyPlayer lobbyPlayer) {
		PlayerPawn player = new PlayerPawn(lobbyPlayer.charName, maxHp);
		player.SetId(lobbyPlayer.id);
		foreach(int spellId in knownSpells) {
			player.AddSpell(spellId);
		}
		for(int i = 0; i < player.Affinities.Length; i++) {
			player.Affinities[i].AddModifier(new AttributeModifier(GetName(), AttributeModifier.Operation.AddBase, affinityModifiers[i]));
		}
		return player;
	}
}
