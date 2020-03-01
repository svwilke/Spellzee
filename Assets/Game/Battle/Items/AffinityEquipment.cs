using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffinityEquipment : Equipment {

	private AttributeModifier.Operation operation;
	private double[] affinityModifiers;

	public AffinityEquipment(string name, AttributeModifier.Operation operation, double fire, double water, double earth, double air, double light, double dark, double chaos) : base(name, "") {
		this.operation = operation;
		affinityModifiers = new double[] {
			fire, water, earth, air, light, dark, chaos
		};
		string desc = "";
		string format = "{0} modifier, {1} element";
		switch(operation) {
			case AttributeModifier.Operation.AddBase:
				format = "+{0} base {1} Affinity";
				break;
			case AttributeModifier.Operation.AddTotal:
				format = "+{0} total {1} Affinity";
				break;
			case AttributeModifier.Operation.MultiplyBaseAndAdd:
				format = "+{0}% {1} Affinity";
				break;
			case AttributeModifier.Operation.MultiplyTotal:
				format = "+{0}% total {1} Affinity";
				break;
			case AttributeModifier.Operation.SubtractBase:
				format = "-{0} base {1} Affinity";
				break;
			case AttributeModifier.Operation.SubtractTotal:
				format = "-{0} total {1} Affinity";
				break;
			case AttributeModifier.Operation.Set:
				format = "{1} Affinity is {0}";
				break;
		}
		for(int i = 0; i < affinityModifiers.Length; i++) {
			if(affinityModifiers[i] != 0) {
				if(desc.Length > 0) {
					desc += "\n";
				}
				desc += string.Format(format, affinityModifiers[i], Element.All[i].GetColoredName());
			}
		}
		description = desc;
	}

	public override void OnEquipped(Pawn pawn) {
		PlayerPawn player = pawn as PlayerPawn;
		if(player != null) {
			for(int i = 0; i < Element.Count; i++) {
				player.Affinities[i].AddModifier(new AttributeModifier(GetName(), operation, affinityModifiers[i]));
			}
		}
		
	}

	public override void OnUnequipped(Pawn pawn) {
		PlayerPawn player = pawn as PlayerPawn;
		if(player != null) {
			for(int i = 0; i < Element.Count; i++) {
				player.Affinities[i].RemoveModifier(GetName());
			}
		}
	}
}
