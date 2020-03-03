using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegenAilment : Ailment
{

	public RegenAilment(string fullName, string shortName, Color color) : base(fullName, shortName, color) {
		this.gainType = GainType.Once;
	}

	public override void OnGain(Pawn pawn, int intensity) {
		pawn.OnEndTurn.AddListener(Regen);
	}

	public override void OnLose(Pawn pawn, int intensity) {
		pawn.OnEndTurn.RemoveListener(Regen);
	}

	public void Regen(Battle battle, Pawn pawn) {
		int regen = pawn.GetAilment(this);
		EventBus.DamageHealEvent healEvent = new EventBus.DamageHealEvent(this, regen, regen);
		pawn.CmdHeal(healEvent);
		pawn.CmdSetAilment(this, regen - 1);
	}
}
