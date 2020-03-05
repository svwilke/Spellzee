using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DurationStatus : Status {

	private int duration;

	public DurationStatus(StatusType type, int duration) : base(type) {
		this.duration = duration;
	}

	protected override void OnTurnEnded() {
		if(age >= duration) {
			pawn.CmdRemoveStatus(this);
		}
	}
}
