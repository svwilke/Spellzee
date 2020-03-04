using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffinityStatus : Status {

	private Element element;
	private AttributeModifier modifier;
	private int duration;

	public AffinityStatus(StatusType type, Element element, int duration, AttributeModifier.Operation operation, double value) : this(type, element, duration, new AttributeModifier(operation, value)) {
		
	}

	public AffinityStatus(StatusType type, Element element, int duration, AttributeModifier modifier) : base(type) {
		this.element = element;
		this.modifier = modifier;
		this.duration = duration;
	}

	protected override void OnStatusAdded() {
		PlayerPawn player = pawn as PlayerPawn;
		if(player != null) {
			player.Affinities[element.GetId()].AddModifier(modifier);
			player.Synchronize();
		}
	}

	protected override void OnStatusRemoved() {
		PlayerPawn player = pawn as PlayerPawn;
		if(player != null) {
			player.Affinities[element.GetId()].RemoveModifier(modifier);
			player.Synchronize();
		}
	}

	protected override void OnTurnEnded() {
		if(age >= duration) {
			pawn.RemoveStatus(this);
		}
	}
}
