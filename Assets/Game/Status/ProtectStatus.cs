using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectStatus : AilmentStatus {

	public ProtectStatus(int value) : base(StatusType.Positive, "Protect", "protect", Element.Water.GetColor(), value) {
		description = "{0} {1}: When this character takes damage, the damage is reduced by {1} and {0} is removed.";
	}

	protected override void OnStatusAdded() {
		pawn.OnTakeDamage.AddListener(Protect);
	}

	protected override void OnStatusRemoved() {
		pawn.OnTakeDamage.RemoveListener(Protect);
	}

	public void Protect(Pawn pawn, EventBus.DamageHealEvent dmgEvent) {
		int prt = Mathf.Min(GetValue(), dmgEvent.amount);
		dmgEvent.amount -= prt;
		pawn.CmdRemoveStatus(this);
	}
}
