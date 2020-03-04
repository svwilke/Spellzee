using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlindStatus : AilmentStatus {

	public BlindStatus(int value) : base(StatusType.Negative, "Blind", "Bld", Element.Light.GetColor(), value) {
		
	}

	protected override void OnStatusAdded() {
		pawn.HitChance.AddModifier(new AttributeModifier(GetFullName(), AttributeModifier.Operation.MultiplyTotal, 0.5));
	}

	protected override void OnStatusRemoved() {
		pawn.HitChance.RemoveModifier(GetFullName());
	}

	protected override void OnTurnEnded() {
		SetValue(GetValue() - 1);
	}
}
