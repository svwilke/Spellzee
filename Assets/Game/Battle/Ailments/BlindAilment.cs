using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlindAilment : Ailment
{

	public BlindAilment(string fullName, string shortName, Color color) : base(fullName, shortName, color) {
		this.gainType = GainType.Once;
	}

	public override void OnGain(Pawn pawn, int intensity) {
		pawn.MissChance.AddModifier(new AttributeModifier("Blinded", AttributeModifier.Operation.AddBase, 0.5));
		pawn.OnEndTurn.AddListener(RemoveBlind);
	}

	public override void OnLose(Pawn pawn, int intensity) {
		pawn.MissChance.RemoveModifier("Blinded");
		pawn.OnEndTurn.RemoveListener(RemoveBlind);
	}

	public void RemoveBlind(Battle battle, Pawn pawn) {
		pawn.CmdSetAilment(this, pawn.GetAilment(this) - 1);
	}
}
