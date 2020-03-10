﻿using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PawnTemplate : RegistryEntry<PawnTemplate> {

	protected string className;
	protected int maxHp = 16;
	protected List<string> knownSpells;
	protected double[] affinityModifiers;

	public PawnTemplate(string name) {
		className = name;
		knownSpells = new List<string>();
		affinityModifiers = new double[Element.Count];
	}

	public string GetName() {
		return className;
	}

	public PawnTemplate ZeroAffinites() {
		for(int i = 0; i < Element.Count; i++) {
			affinityModifiers[i] = 0 - Element.All[i].GetBaseAffinity();
		}
		return this;
	}

	public PawnTemplate SetAffinity(Element element, double value) {
		affinityModifiers[element.GetId()] = value - element.GetBaseAffinity();
		return this;
	}

	public PawnTemplate SetAffinities(double fire, double water, double earth, double air, double light, double dark, double chaos) {
		affinityModifiers[0] = fire - 10;
		affinityModifiers[1] = water - 10;
		affinityModifiers[2] = earth - 10;
		affinityModifiers[3] = air - 10;
		affinityModifiers[4] = light - 10;
		affinityModifiers[5] = dark - 10;
		affinityModifiers[6] = chaos;
		affinityModifiers[7] = 0;
		affinityModifiers[8] = 0;
		return this;
	}

	public PawnTemplate SetMaxHp(int maxHp) {
		this.maxHp = maxHp;
		return this;
	}

	public PawnTemplate AddSpells(params Spell[] spells) {
		knownSpells.AddRange(spells.Select(spell => spell.GetId()));
		return this;
	}

	public virtual Pawn Create(int playerCount, int level, Pawn.Team team) {
		Pawn pawn;
		pawn = new Pawn(className, (int)(maxHp * playerCount * (1.0 + (level * 0.15))), team);
		pawn.SetSprite(GetId());
		knownSpells.ForEach(pawn.AddSpell);
		for(int i = 0; i < pawn.Affinities.Length; i++) {
			pawn.Affinities[i].AddModifier(new AttributeModifier(GetName(), AttributeModifier.Operation.AddBase, affinityModifiers[i]));
		}
		pawn.SetAI(new SimpleAI(pawn));
		return pawn;
	}

	public virtual Pawn Create(LobbyClientHandler.LobbyPlayer lobbyPlayer) {
		Pawn player = new Pawn(lobbyPlayer.charName, maxHp, Pawn.Team.Friendly);
		player.SetId(lobbyPlayer.id);
		player.SetSprite(GetId());
		knownSpells.ForEach(player.AddSpell);
		for(int i = 0; i < player.Affinities.Length; i++) {
			player.Affinities[i].AddModifier(new AttributeModifier(GetName(), AttributeModifier.Operation.AddBase, affinityModifiers[i]));
		}
		return player;
	}
}