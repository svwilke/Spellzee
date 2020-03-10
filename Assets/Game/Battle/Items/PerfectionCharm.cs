using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerfectionCharm : Equipment {

	public PerfectionCharm(string name) : base(name, "Additional total Affinities based on your needs.") {

	}

	public override void OnEquipped(Pawn pawn) {
		UpdateAffinityModifiers(pawn);
		pawn.OnSpellsChange.AddListener(UpdateAffinities);
		
	}

	public override void OnUnequipped(Pawn pawn) {
		for(int i = 0; i < Element.Count; i++) {
			pawn.Affinities[i].RemoveModifier(GetName());
		}
		pawn.OnSpellsChange.RemoveListener(UpdateAffinities);
	}

	public void UpdateAffinities(Battle battle, Pawn pawn) {
		UpdateAffinityModifiers(pawn);
	}

	public void UpdateAffinityModifiers(Pawn player) {
		int[] elementCount = new int[Element.Count];
		foreach(Spell spell in player.GetSpells()) {
			ElementDisplay[] displays = spell.GetElementDisplays(RollContext.Null);
			foreach(ElementDisplay display in displays) {
				elementCount[display.element.GetId()] += 1;
			}
		}
		for(int i = 0; i < elementCount.Length; i++) {
			player.Affinities[i].RemoveModifier(GetName());
			player.Affinities[i].AddModifier(new AttributeModifier(GetName(), AttributeModifier.Operation.AddTotal, elementCount[i]));
		}
	}
}
