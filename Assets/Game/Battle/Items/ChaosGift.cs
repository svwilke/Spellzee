using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaosGift : Equipment {

	public ChaosGift(string name) : base(name, "+1 Die\nYour targettable spells choose targets randomly.") {

	}

	public override void OnEquipped(Pawn pawn) {
		pawn.DieCount.AddModifier(new AttributeModifier(GetName(), AttributeModifier.Operation.AddBase, 1));
		pawn.OnBuildSpellComponents.AddListener(MakeTargetsRandom);
	}

	public override void OnUnequipped(Pawn pawn) {
		pawn.DieCount.RemoveModifier(GetName());
		pawn.OnBuildSpellComponents.RemoveListener(MakeTargetsRandom);
	}

	public void MakeTargetsRandom(Spell spell, RollContext context, List<SpellComponent> spellComponents) {
		foreach(SpellComponent sc in spellComponents) {
			if(sc.GetTargetType() == SpellComponent.TargetType.Target) {
				sc.SetTargetType(SpellComponent.TargetType.Random);
			}
		}
	}	
}
