using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PatternMatcher {

	protected bool doesMatch = false;
	protected RollContext rollContext = null;

	public abstract void Match(RollContext context);

	public virtual bool Matches() {
		return doesMatch;
	}

	public RollContext GetContext() {
		return rollContext;
	}

	public abstract ElementDisplay[] GetElementDisplays();
}
