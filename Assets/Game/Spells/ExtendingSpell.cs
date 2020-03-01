using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendingSpell : Spell
{
	private ElementDisplay[] displays;
	private System.Action<RollContext, int> cast;

	private Element element;
	private int requiredCount;
	private int baseValue;
	private double valuePerExtension;

	private string shortDescription = "";

	public ExtendingSpell(string name, string longDescription, string shortDescription, bool requiresTarget,
			Element elem, int requiredCount, int baseValue, double valueIncrease, System.Action<RollContext, int> cast) : base(name, longDescription, requiresTarget) {
		element = elem;
		this.requiredCount = requiredCount;
		this.baseValue = baseValue;
		valuePerExtension = valueIncrease;
		this.cast = cast;

		this.shortDescription = shortDescription;

		displays = new ElementDisplay[requiredCount + 1];
		for(int i = 0; i < displays.Length; i++) {
			displays[i] = new ElementDisplay(elem, false);
			if(i == displays.Length - 1) {
				displays[i].extension = true;
				displays[i].extensionValue = 0;
			}
		}
	}


	public override string GetShortDescription(RollContext context) {
		return string.Format(shortDescription, GetValue(context));
	}

	public int GetValue(RollContext context) {
		int extensionAmount = GetExtensionAmount(context);
		if(extensionAmount >= 0) {
			return baseValue + (int)(extensionAmount * valuePerExtension);
		}
		return baseValue;
	}

	public override void Cast(RollContext context) {
		cast.Invoke(context, GetValue(context));
	}

	public override bool Matches(RollContext context) {
		return GetExtensionAmount(context) >= 0;
	}

	public int GetExtensionAmount(RollContext context) {
		if(context == null) {
			return -requiredCount;
		}
		return context.GetElementCount(element, true) - requiredCount;
	}

	public override ElementDisplay[] GetElementDisplays(RollContext context) {
		displays[displays.Length - 1].extensionValue = Mathf.Max(0, GetExtensionAmount(context));
		return displays;
	}

	public struct ElemMatch {
		public Element elem;
		public bool optional;
	}
}
