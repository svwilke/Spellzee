using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockStatus : AilmentStatus {

	public ShockStatus(int value) : base(StatusType.Negative, "Shock", "shock", Element.Water.GetColor(), value) {
		
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
