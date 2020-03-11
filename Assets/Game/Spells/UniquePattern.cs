using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniquePattern : PatternMatcher {

	protected int uniqueCountRequired;
	protected ElementDisplay[] displays;

	public UniquePattern(int count) {
		uniqueCountRequired = count;
		displays = new ElementDisplay[count];
		for(int i = 0; i < count; i++) {
			displays[i] = new ElementDisplay(Element.None, false);
		}
	}

	public override void Match(RollContext context) {
		rollContext = context;
		HashSet<Element> uniqueElements = new HashSet<Element>(context.GetElementsRolled());
		doesMatch = uniqueElements.Count >= uniqueCountRequired;
	}

	public override ElementDisplay[] GetElementDisplays() {
		return displays;
	}
}
