using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleActivatableSpell : ActivatableSpell
{
	private ElementDisplay[] displays;
	private Element element;
	private int count;
	private System.Predicate<RollContext> activation;

	public SimpleActivatableSpell(int id, string name, string longDescription, string shortDescriptionInactive, string shortDescriptionActive, Element elem, int count,
			bool requiresTarget, System.Predicate<RollContext> activation, System.Action<RollContext, bool> cast) : base(id, name, longDescription, shortDescriptionInactive, shortDescriptionActive, requiresTarget, cast) {
		element = elem;
		this.count = count;
		displays = new ElementDisplay[count];
		for(int i = 0; i < count; i++) {
			displays[i] = new ElementDisplay(element, false);
		}
		this.activation = activation;
	}

	public override bool Matches(RollContext context) {
		return context.GetElementCount(element, true) >= count;
	}

	public override ElementDisplay[] GetElementDisplays(RollContext context) {
		return displays;
	}

	public override bool IsActive(RollContext context) {
		return activation.Invoke(context);
	}
}
