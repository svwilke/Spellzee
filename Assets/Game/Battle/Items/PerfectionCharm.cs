using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerfectionCharm : Equipment {

	public PerfectionCharm(int id, string name) : base(id, name, "Additional total Affinities based on your needs.") {

	}

	public override void OnEquipped(Pawn pawn) {
		PlayerPawn player = pawn as PlayerPawn;
		if(player != null) {
			UpdateAffinityModifiers(player);
			player.OnSpellsChange.AddListener(UpdateAffinities);
		}
		
	}

	public override void OnUnequipped(Pawn pawn) {
		PlayerPawn player = pawn as PlayerPawn;
		if(player != null) {
			for(int i = 0; i < Element.Count; i++) {
				player.Affinities[i].RemoveModifier(GetName());
			}
			player.OnSpellsChange.RemoveListener(UpdateAffinities);
		}
	}

	public void UpdateAffinities(Battle battle, Pawn pawn) {
		if(pawn is PlayerPawn) {
			UpdateAffinityModifiers(pawn as PlayerPawn);
		}
	}

	public void UpdateAffinityModifiers(PlayerPawn player) {
		int[] elementCount = new int[Element.Count];
		foreach(Spell spell in player.GetSpells()) {
			ElementDisplay[] displays = spell.GetElementDisplays(null);
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
