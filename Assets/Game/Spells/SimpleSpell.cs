using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpell : Spell
{
	private ElementDisplay[] displays;
	private Element element;
	private int count;
	private System.Action<RollContext> cast;

	public SimpleSpell(string name, string desc, Element elem, int count, bool requiresTarget, System.Action<RollContext> cast) : base(name, desc, requiresTarget) {
		element = elem;
		this.count = count;
		displays = new ElementDisplay[count];
		for(int i = 0; i < count; i++) {
			displays[i] = new ElementDisplay(element, false);
		}
		this.cast = cast;
	}

	public override void Cast(RollContext context) {
		cast.Invoke(context);
	}

	public override bool Matches(RollContext context) {
		return context.GetElementCount(element, true) >= count;
	}

	public override ElementDisplay[] GetElementDisplays(RollContext context) {
		return displays;
	}
}
