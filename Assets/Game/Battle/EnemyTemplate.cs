using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTemplate {

	private string name;
	private int minMaxHp;
	private int maxMaxHp;
	private List<int> knownSpells;
	private float spellWeight = 8F;
	private float useItemWeight = 0F;
	private float passWeight = 1F;

	public EnemyTemplate(string name) {
		this.name = name;
		knownSpells = new List<int>();
	}

	public EnemyTemplate SetMaxHp(int maxHp) {
		return SetMaxHp(maxHp, maxHp);
	}

	public EnemyTemplate SetMaxHp(int min, int max) {
		minMaxHp = min;
		maxMaxHp = max;
		return this;
	}

	public EnemyTemplate AddSpells(params int[] spellIds) {
		knownSpells.AddRange(spellIds);
		return this;
	}

	public EnemyTemplate SetTurnWeights(float castSpell, float useItem, float pass) {
		spellWeight = castSpell;
		useItemWeight = useItem;
		passWeight = pass;
		return this;
	}

	public EnemyPawn Create(int playerCount, int level) {
		EnemyPawn enemy = new EnemyPawn(this.name, (int)(Random.Range(minMaxHp, maxMaxHp + 1) * playerCount * (1 + (level * 0.15))));
		foreach(int spellId in knownSpells) {
			enemy.AddSpell(spellId);
		}
		enemy.spellWeight = spellWeight;
		enemy.useItemWeight = useItemWeight;
		enemy.passWeight = passWeight;
		return enemy;
	}
}
