using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestoreRing : Equipment
{

	public RestoreRing(int id, string name) : base(id, name, "You restore twice as much health when an enemy is killed.") {
		
	}

	public override void OnEquipped(Pawn pawn) {
		PlayerPawn player = pawn as PlayerPawn;
		if(player != null) {
			player.EndOfBattleRestoration.AddModifier(new AttributeModifier(GetName(), AttributeModifier.Operation.MultiplyTotal, 2));
		}
	}

	public override void OnUnequipped(Pawn pawn) {
		PlayerPawn player = pawn as PlayerPawn;
		if(player != null) {
			player.EndOfBattleRestoration.RemoveModifier(GetName());
		}
	}
}
