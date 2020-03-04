using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegenStatus : AilmentStatus {

	public RegenStatus(int value) : base(StatusType.Positive, "Regenerate", "Rgn", Element.Earth.GetColor(), value) {

	}

	protected override void OnTurnEnded() {
		int healAmount = GetValue();
		EventBus.DamageHealEvent healEvent = new EventBus.DamageHealEvent(this, healAmount);
		pawn.CmdHeal(healEvent);
		SetValue(healAmount - 1);
	}
}
