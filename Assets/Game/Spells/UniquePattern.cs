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
		Reset();
		rollContext = context;
		HashSet<Element> uniqueElements = new HashSet<Element>();
		Element[] rolled = context.GetElementsRolled();
		for(int i = 0; i < rolled.Length; i++) {
			if(!uniqueElements.Contains(rolled[i])) {
				uniqueElements.Add(rolled[i]);
				matchingDice.Add(i);
			}
		}
		distance = Mathf.Max(0, uniqueCountRequired - uniqueElements.Count);
		doesMatch = distance == 0;
	}

	public override ElementDisplay[] GetElementDisplays() {
		return displays;
	}
}
