using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnStatus : AilmentStatus {

	public BurnStatus(int value) : base(StatusType.Negative, "Burn", "burn", Element.Fire.GetColor(), value) {
		description = "{0} {1}: At the end of its turn, this character takes {1} damage and {0} is reduced by 1.";
	}

	protected override void OnTurnEnded() {
		int burnAmount = GetValue();
		EventBus.DamageHealEvent damageEvent = new EventBus.DamageHealEvent(this, burnAmount);
		pawn.CmdDamage(damageEvent);
		SetValue(burnAmount - 1);
	}
}
