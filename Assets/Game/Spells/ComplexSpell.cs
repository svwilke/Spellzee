using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComplexSpell : Spell
{
	private ElementDisplay[] displays;
	private ElemMatch[] elemMatches;
	private bool activateOnAll = false;
	private System.Action<RollContext, int> cast;
	private int optionalCount;

	private string shortDescriptionRequired = "";
	private string shortDescriptionOptional = "";

	public ComplexSpell(string name, string desc, bool requiresTarget, System.Action<RollContext, int> cast, bool activateOnAll = false, params ElemMatch[] elemMatches) : base(name, desc, requiresTarget) {
		this.elemMatches = elemMatches;
		this.cast = cast;
		this.activateOnAll = activateOnAll;
		displays = new ElementDisplay[elemMatches.Length];
		optionalCount = 0;
		for(int i = 0; i < displays.Length; i++) {
			displays[i] = new ElementDisplay(elemMatches[i].elem, elemMatches[i].optional);
			if(elemMatches[i].optional) {
				optionalCount += 1;
			}
		}
	}

	public ComplexSpell SetShortDescriptionRequired(string desc) {
		this.shortDescriptionRequired = desc;
		return this;
	}

	public ComplexSpell SetShortDescriptionOptional(string desc) {
		shortDescriptionOptional = desc;
		return this;
	}

	public override string GetShortDescription(RollContext context) {
		if(shortDescriptionRequired.Length > 0 && shortDescriptionOptional.Length > 0) {
			if(GetOptionalMatchCount(context) < optionalCount) {
				return shortDescriptionRequired;
			}
			return shortDescriptionOptional;
		}
		return base.GetShortDescription(context);
	}

	public override void Cast(RollContext context) {
		cast.Invoke(context, GetOptionalMatchCount(context));
	}

	public override bool Matches(RollContext context) {
		return GetOptionalMatchCount(context) >= 0;
	}

	public int GetOptionalMatchCount(RollContext context) {
		Element[] rolls = context.GetElementsRolled();
		bool[] usedRolls = new bool[rolls.Length];
		bool[] matchedElems = new bool[elemMatches.Length];
		bool allRequiredMatched = true;
		int optionalMatchCount = 0;
		for(int i = 0; i < elemMatches.Length && allRequiredMatched; i++) {
			Element lookingFor = elemMatches[i].elem;
			for(int j = 0; j < rolls.Length; j++) {
				if(!usedRolls[j]) {
					if(rolls[j] == lookingFor) {
						usedRolls[j] = true;
						matchedElems[i] = true;
						if(elemMatches[i].optional) {
							optionalMatchCount += 1;
							if(!activateOnAll) {
								displays[i].optional = false;
							}
						}
						break;
					}
				}
			}
			if(!matchedElems[i]) {
				if(elemMatches[i].optional) {
					displays[i].optional = true;
				} else {
					allRequiredMatched = false;
				}
			}
		}
		if(activateOnAll) {
			for(int i = 0; i < elemMatches.Length; i++) {
				if(elemMatches[i].optional) {
					displays[i].optional = optionalMatchCount < optionalCount;
				}
			}
		}
		return allRequiredMatched ? optionalMatchCount : -1;
	}

	public override ElementDisplay[] GetElementDisplays(RollContext context) {
		return displays;
	}

	public struct ElemMatch {
		public Element elem;
		public bool optional;
	}
}
