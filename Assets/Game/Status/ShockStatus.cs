using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockStatus : AilmentStatus {

	public ShockStatus(int value) : base(StatusType.Negative, "Shock", "shock", Element.Water.GetColor(), value) {
		description = "{0} {1}: This character can not lock for {1} turns.";
	}

	protected override void OnStatusAdded() {
		pawn.LockCount.AddModifier(new AttributeModifier(GetFullName(), AttributeModifier.Operation.Set, 0));
	}

	protected override void OnStatusRemoved() {
		pawn.LockCount.RemoveModifier(GetFullName());
	}

	protected override void OnTurnEnded() {
		SetValue(GetValue() - 1);
	}
}
