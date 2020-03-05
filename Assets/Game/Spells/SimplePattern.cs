using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePattern : PatternMatcher {

	protected Dictionary<Element, int> elementCounts;
	protected ElementDisplay[] displays;

	public SimplePattern(params Element[] elements) {
		displays = new ElementDisplay[elements.Length];
		elementCounts = new Dictionary<Element, int>();
		for(int i = 0; i < elements.Length; i++) {
			displays[i] = new ElementDisplay(elements[i], false);
			if(elementCounts.ContainsKey(elements[i])) {
				elementCounts[elements[i]] = elementCounts[elements[i]] + 1;
			} else {
				elementCounts[elements[i]] = 1;
			}
		}
	}

	public SimplePattern(Element element, int count) {
		displays = new ElementDisplay[count];
		elementCounts = new Dictionary<Element, int>();
		for(int i = 0; i < count; i++) {
			displays[i] = new ElementDisplay(element, false);
		}
		elementCounts.Add(element, count);
	}

	public override void Match(RollContext context) {
		rollContext = context;
		foreach(KeyValuePair<Element, int> elemCount in elementCounts) {
			if(context.GetElementCount(elemCount.Key, false) < elemCount.Value) {
				doesMatch = false;
				return;
			}
		}
		doesMatch = true;
	}

	public override ElementDisplay[] GetElementDisplays() {
		return displays;
	}
}
