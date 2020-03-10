using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlindStatus : AilmentStatus {

	public BlindStatus(int value) : base(StatusType.Negative, "Blind", "blind", Element.Light.GetColor(), value) {
		description = "{0} {1}: Hit Chance -50% for {1} turns.";
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
