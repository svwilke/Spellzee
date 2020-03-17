using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionalPattern : SimplePattern {

	private Dictionary<Element, int> elementCountsOptional;
	private int optionalDisplayStart = int.MaxValue;
	private bool optionalFulfilled = false;

	public OptionalPattern(params Element[] requiredElements) : base(requiredElements) {

	}

	public OptionalPattern SetOptional(params Element[] optionalElements) {
		elementCountsOptional = new Dictionary<Element, int>(elementCounts);
		ElementDisplay[] newDisplays = new ElementDisplay[displays.Length + optionalElements.Length];
		optionalDisplayStart = displays.Length;
		for(int i = 0; i < newDisplays.Length; i++) {
			if(i < optionalDisplayStart) {
				newDisplays[i] = displays[i];
			} else {
				Element e = optionalElements[i - optionalDisplayStart];
				newDisplays[i] = new ElementDisplay(e, true);
				if(elementCountsOptional.ContainsKey(e)) {
					elementCountsOptional[e] = elementCountsOptional[e] + 1;
				} else {
					elementCountsOptional[e] = 1;
				}
			}
			
		}
		displays = newDisplays;
		return this;
	}

	public override void Match(RollContext context) {
		base.Match(context);
		optionalFulfilled = OptionalFulfilled(context);
		for(int i = optionalDisplayStart; i < displays.Length; i++) {
			displays[i].optional = !optionalFulfilled;
		}
	}

	private bool OptionalFulfilled(RollContext context) {
		foreach(KeyValuePair<Element, int> elemCount in elementCountsOptional) {
			if(context.GetElementCount(elemCount.Key) < elemCount.Value) {
				return false;
			}
		}
		return true;
	}

	public bool OptionalFulfilled() {
		return optionalFulfilled;
	}
}
