using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealCharm : Equipment {

	public HealCharm(string name) : base(name, "Whenever you are healed by a spell, restore 1 additional life.") {

	}

	public override void OnEquipped(Pawn pawn) {
		pawn.OnSpellComponentTarget.AddListener(OnSpellComponentTarget);
	}

	public override void OnUnequipped(Pawn pawn) {
		pawn.OnSpellComponentTarget.RemoveListener(OnSpellComponentTarget);
	}

	public void OnSpellComponentTarget(Spell spell, RollContext ctx, SpellComponent sc) {
		if(sc is HealComponent) {
			(sc as HealComponent).AddModifier(AttributeModifier.Operation.AddTotal, 1);
		}
	}
}
