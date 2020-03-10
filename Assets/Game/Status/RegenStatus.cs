using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegenStatus : AilmentStatus {

	public RegenStatus(int value) : base(StatusType.Positive, "Regenerate", "regen", Element.Earth.GetColor(), value) {
		description = "{0} {1}: At the end of its turn, this character restores {1} life and {0} is reduced by 1.";
	}

	protected override void OnTurnEnded() {
		int healAmount = GetValue();
		EventBus.DamageHealEvent healEvent = new EventBus.DamageHealEvent(this, healAmount);
		pawn.CmdHeal(healEvent);
		SetValue(healAmount - 1);
	}
}
