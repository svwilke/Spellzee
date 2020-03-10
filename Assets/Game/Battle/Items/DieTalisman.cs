using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieTalisman : Equipment {

	public DieTalisman(string name) : base(name, "+1 Die\n-1 Roll") {

	}

	public override void OnEquipped(Pawn pawn) {
		pawn.DieCount.AddModifier(new AttributeModifier(GetName(), AttributeModifier.Operation.AddBase, 1));
		pawn.RollCount.AddModifier(new AttributeModifier(GetName(), AttributeModifier.Operation.SubtractBase, 1));
	}

	public override void OnUnequipped(Pawn pawn) {
		pawn.DieCount.RemoveModifier(GetName());
		pawn.RollCount.RemoveModifier(GetName());
	}
}
