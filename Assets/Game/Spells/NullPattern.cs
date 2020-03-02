using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NullPattern : PatternMatcher {

	protected ElementDisplay[] displays;

	public NullPattern() {
		displays = new ElementDisplay[0];
	}

	public override void Match(RollContext context) {
		doesMatch = false;
		rollContext = context;
	}

	public override ElementDisplay[] GetElementDisplays() {
		return displays;
	}
}
