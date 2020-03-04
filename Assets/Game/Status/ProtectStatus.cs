using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectStatus : AilmentStatus {

	public ProtectStatus(int value) : base(StatusType.Positive, "Protect", "Prt", Element.Water.GetColor(), value) {
		
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
