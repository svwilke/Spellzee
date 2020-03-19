using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IgniteRock : Equipment {

	public IgniteRock(string name, string desc) : base(name, desc) {

	}

	public override void OnEquipped(Pawn pawn) {
		pawn.OnBuildSpellComponents.AddListener(AddBurnAilment);
	}

	public override void OnUnequipped(Pawn pawn) {
		pawn.OnBuildSpellComponents.RemoveListener(AddBurnAilment);
	}

	public void AddBurnAilment(Spell spell, RollContext context, List<SpellComponent> components) {
		List<SpellComponent> toRemove = new List<SpellComponent>();
		List<SpellComponent> toAdd = new List<SpellComponent>();
		foreach(SpellComponent sc in components) {
			DamageComponent dc = sc as DamageComponent;
			if(dc != null) {
				int value = Mathf.Min(dc.GetValue(), 2);
				dc.AddModifier(AttributeModifier.Operation.SubtractTotal, value);
				AilmentComponent ac = new AilmentComponent(intensity => new BurnStatus(intensity), value);
				ac.SetTarget(dc.GetTarget());
				toAdd.Add(ac);
			}
		}
		components.RemoveAll(toRemove.Contains);
		components.AddRange(toAdd);
	}
}
