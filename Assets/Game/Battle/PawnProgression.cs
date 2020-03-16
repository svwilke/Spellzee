using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnProgression : RegistryEntry<PawnProgression> {

	private Dictionary<int, List<Spell>> availableSpells = new Dictionary<int, List<Spell>>();

	public PawnProgression AddSpells(int level, params Spell[] spells) {
		List<Spell> spellList;
		if(!availableSpells.TryGetValue(level, out spellList)) {
			spellList = new List<Spell>();
			availableSpells.Add(level, spellList);
		}
		spellList.AddRange(spells);
		return this;
	}

	public virtual int GetRequiredXP(int level) {
		if(level < 4) {
			return level * 7;
		} else
		if(level < 9) {
			return level * 7 * 3 + (level - 3) * 12;
		} else {
			return level * 7 * 3 + (level - 3) * 12 * 5 + (level - 8) * 18;
		}
	}

	public virtual List<Spell> GetSpells(int level) {
		if(availableSpells.ContainsKey(level)) {
			return new List<Spell>(availableSpells[level]);
		}
		return new List<Spell>();
	}

	public virtual List<Spell> GetAccumulatedSpells(int level) {
		List<Spell> spells = new List<Spell>();
		for(int i = 0; i <= level; i++) {
			spells.AddRange(GetSpells(i));
		}
		return spells;
	}
}
