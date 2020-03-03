using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectAilment : Ailment
{

	public ProtectAilment(string fullName, string shortName, Color color) : base(fullName, shortName, color) {
		this.gainType = GainType.Once;
	}

	public override void ApplyToPawn(Pawn pawn, int intensity) {
		pawn.SetAilment(this, pawn.GetAilment(this) + intensity);
	}

	public override void OnGain(Pawn pawn, int intensity) {
		pawn.OnTakeDamage.AddListener(ReduceDamage);
	}

	public override void OnLose(Pawn pawn, int intensity) {
		pawn.OnTakeDamage.RemoveListener(ReduceDamage);
	}

	public void ReduceDamage(Pawn pawn, EventBus.DamageHealEvent dmgEvent) {
		int intensity = pawn.GetAilment(this);
		dmgEvent.amount -= intensity;
		pawn.CmdSetAilment(this, 0);
	}
}
