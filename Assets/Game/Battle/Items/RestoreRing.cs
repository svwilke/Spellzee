﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestoreRing : Equipment
{

	public RestoreRing(string name) : base(name, "You restore twice as much health when an enemy is killed.") {
		
	}

	public override void OnEquipped(Pawn pawn) {
		pawn.EndOfBattleRestoration.AddModifier(new AttributeModifier(GetName(), AttributeModifier.Operation.MultiplyTotal, 2));
	}

	public override void OnUnequipped(Pawn pawn) {
		pawn.EndOfBattleRestoration.RemoveModifier(GetName());
	}
}
