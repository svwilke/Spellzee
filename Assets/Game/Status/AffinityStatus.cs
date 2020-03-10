using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffinityStatus : DurationStatus {

	private Element element;
	private AttributeModifier modifier;

	public AffinityStatus(StatusType type, Element element, int duration, AttributeModifier.Operation operation, double value) : this(type, element, duration, new AttributeModifier(operation, value)) {
		
	}

	public AffinityStatus(StatusType type, Element element, int duration, AttributeModifier modifier) : base(type, duration) {
		this.element = element;
		this.modifier = modifier;
	}

	protected override void OnStatusAdded() {
		pawn.Affinities[element.GetId()].AddModifier(modifier);
		pawn.Synchronize();
	}

	protected override void OnStatusRemoved() {
		pawn.Affinities[element.GetId()].RemoveModifier(modifier);
		pawn.Synchronize();
	}
}
