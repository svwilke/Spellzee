using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevitateStatus : AilmentStatus {

	public LevitateStatus() : base(StatusType.Negative, "Levitate", "levitate", Element.Air.GetColor(), 1) {

	}

	protected override void OnStatusAdded() {
		pawn.OnTakeDamage.AddListener(ProcLevitate);
	}

	protected override void OnStatusRemoved() {
		pawn.OnTakeDamage.RemoveListener(ProcLevitate);
	}

	protected override void OnTurnEnded() {
		SetValue(GetValue() + 1);
	}

	public void ProcLevitate(Pawn pawn, EventBus.DamageHealEvent dmgEvent) {
		dmgEvent.amount += GetValue();
		pawn.CmdRemoveStatus(this);
	}
}
