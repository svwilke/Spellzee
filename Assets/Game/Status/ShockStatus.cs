using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockStatus : AilmentStatus {

	public ShockStatus(int value) : base(StatusType.Negative, "Shock", "Shk", Element.Water.GetColor(), value) {
		
	}

	protected override void OnStatusAdded() {
		PlayerPawn player = pawn as PlayerPawn;
		if(player != null) {
			player.LockCount.AddModifier(new AttributeModifier(GetFullName(), AttributeModifier.Operation.Set, 0));
		}
	}

	protected override void OnStatusRemoved() {
		PlayerPawn player = pawn as PlayerPawn;
		if(player != null) {
			player.LockCount.RemoveModifier(GetFullName());
		}
	}

	protected override void OnTurnEnded() {
		SetValue(GetValue() - 1);
	}
}
