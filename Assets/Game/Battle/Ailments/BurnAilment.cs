using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnAilment : Ailment
{

	public BurnAilment(string fullName, string shortName, Color color) : base(fullName, shortName, color) {
		this.gainType = GainType.Once;
	}

	public override void ApplyToPawn(Pawn pawn, int intensity) {
		pawn.SetAilment(this, pawn.GetAilment(this) + intensity);
	}

	public override void OnGain(Pawn pawn, int intensity) {
		pawn.OnEndTurn.AddListener(Burn);
	}

	public override void OnLose(Pawn pawn, int intensity) {
		pawn.OnEndTurn.RemoveListener(Burn);
	}

	public void Burn(Battle battle, Pawn pawn) {
		int intensity = pawn.GetAilment(this);
		pawn.CmdDamage(intensity);
		pawn.CmdSetAilment(this, intensity - 1);
	}
}
