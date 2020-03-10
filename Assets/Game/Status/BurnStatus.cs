using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnStatus : AilmentStatus {

	public BurnStatus(int value) : base(StatusType.Negative, "Burn", "burn", Element.Fire.GetColor(), value) {

	}

	protected override void OnTurnEnded() {
		int burnAmount = GetValue();
		EventBus.DamageHealEvent damageEvent = new EventBus.DamageHealEvent(this, burnAmount);
		pawn.CmdDamage(damageEvent);
		SetValue(burnAmount - 1);
	}
}
