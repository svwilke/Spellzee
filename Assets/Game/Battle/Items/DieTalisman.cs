using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieTalisman : Equipment {

	public DieTalisman(int id, string name) : base(id, name, "+1 Die\n-1 Roll") {

	}

	public override void OnEquipped(Pawn pawn) {
		PlayerPawn player = pawn as PlayerPawn;
		if(player != null) {
			player.DieCount.AddModifier(new AttributeModifier(GetName(), AttributeModifier.Operation.AddBase, 1));
			player.RollCount.AddModifier(new AttributeModifier(GetName(), AttributeModifier.Operation.SubtractBase, 1));
		}
		
	}

	public override void OnUnequipped(Pawn pawn) {
		PlayerPawn player = pawn as PlayerPawn;
		if(player != null) {
			player.DieCount.RemoveModifier(GetName());
			player.RollCount.RemoveModifier(GetName());
		}
	}
}
