using System.Collections.Generic;
using UnityEngine.Events;

public class IgniteRock : Equipment {

	public IgniteRock(string name, string desc) : base(name, desc) {

	}

	public override void OnEquipped(Pawn pawn) {
		pawn.OnBuildSpellComponents.AddListener(AddBurnAilment);
		//pawn.OnSpellComponentCaster.AddListener(SubtractFireDamage);
	}

	public override void OnUnequipped(Pawn pawn) {
		pawn.OnBuildSpellComponents.RemoveListener(AddBurnAilment);
		//pawn.OnSpellComponentCaster.RemoveListener(SubtractFireDamage);
	}

	public void AddBurnAilment(Spell spell, RollContext context, List<SpellComponent> components) {
		Dictionary<SpellComponent.TargetType, int> damageComponentCounts = new Dictionary<SpellComponent.TargetType, int>();
		List<SpellComponent> toRemove = new List<SpellComponent>();
		if(spell.IsElement(context, Element.Fire)) {
			foreach(SpellComponent sc in components) {
				DamageComponent dc = sc as DamageComponent;
				if(dc != null && dc.GetValue() >= 1) {
					SpellComponent.TargetType tt = sc.GetTargetType();
					if(damageComponentCounts.ContainsKey(tt)) {
						damageComponentCounts[tt] += 1;
					} else {
						damageComponentCounts[tt] = 1;
					}
					dc.AddModifier(AttributeModifier.Operation.SubtractBase, 1);
				}
			}
		}
		components.RemoveAll(toRemove.Contains);
		foreach(KeyValuePair<SpellComponent.TargetType, int> dcc in damageComponentCounts) {
			AilmentComponent burnFound = null;
			foreach(SpellComponent sc in components) {
				if(sc.GetTargetType() == dcc.Key) {
					AilmentComponent ac = sc as AilmentComponent;
					if(ac != null && ac.GetAilment() is BurnStatus) {
						burnFound = ac;
						break;
					}
				}
			}
			if(burnFound != null) {
				burnFound.AddModifier(AttributeModifier.Operation.AddBase, dcc.Value);
			} else {
				components.Add(new AilmentComponent(dcc.Key, intensity => new BurnStatus(intensity), dcc.Value));
			}
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
