using System.Collections.Generic;
using UnityEngine.Events;

public class IgniteRock : Equipment {

	public IgniteRock(string name, string desc) : base(name, desc) {

	}

	public override void OnEquipped(Pawn pawn) {
		pawn.OnBuildSpellComponents.AddListener(AddBurnAilment);
		pawn.OnSpellComponentCaster.AddListener(SubtractFireDamage);
	}

	public override void OnUnequipped(Pawn pawn) {
		pawn.OnBuildSpellComponents.RemoveListener(AddBurnAilment);
		pawn.OnSpellComponentCaster.RemoveListener(SubtractFireDamage);
	}

	public void AddBurnAilment(Spell spell, RollContext context, List<SpellComponent> components) {
		Dictionary<SpellComponent.TargetType, int> damageComponentCounts = new Dictionary<SpellComponent.TargetType, int>();
		if(spell.IsElement(context, Element.Fire)) {
			foreach(SpellComponent sc in components) {
				if(sc is DamageComponent) {
					SpellComponent.TargetType tt = sc.GetTargetType();
					if(damageComponentCounts.ContainsKey(tt)) {
						damageComponentCounts[tt] += 1;
					} else {
						damageComponentCounts[tt] = 1;
					}
				}
			}
		}
		foreach(KeyValuePair<SpellComponent.TargetType, int> dcc in damageComponentCounts) {
			components.Add(new AilmentComponent(dcc.Key, intensity => new BurnStatus(intensity), dcc.Value));
		}
	}

	public void SubtractFireDamage(Spell spell, RollContext context, SpellComponent component) {
		if(component is DamageComponent) {
			if(spell.IsElement(context, Element.Fire)) {
				(component as DamageComponent).AddModifier(AttributeModifier.Operation.SubtractBase, 1);
			}
		}
	}
}
