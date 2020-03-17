using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendingPattern : SimplePattern {

	private Element extensionElement;
	private int optionalDisplayStart = int.MaxValue;
	private int extensionAmount = 0;

	public ExtendingPattern(params Element[] requiredElements) : base(requiredElements) {

	}

	public ExtendingPattern SetExtension(Element element) {
		extensionElement = element;
		ElementDisplay[] newDisplays = new ElementDisplay[displays.Length + 1];
		for(int i = 0; i < displays.Length; i++) {
			newDisplays[i] = displays[i];
		}
		newDisplays[displays.Length] = new ElementDisplay(element, false, true);
		displays = newDisplays;
		return this;
	}

	public override void Match(RollContext context) {
		base.Match(context);
		extensionAmount = 0;
		if(elementCounts.ContainsKey(extensionElement)) {
			extensionAmount -= elementCounts[extensionElement];
		}
		extensionAmount += context.GetElementCount(extensionElement);
		if(extensionAmount < 0) {
			extensionAmount = 0;
		}
		displays[displays.Length - 1].extensionValue = extensionAmount;
	}

	public int GetExtensionAmount() {
		return extensionAmount;
	}
}
