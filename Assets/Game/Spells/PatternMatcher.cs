using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PatternMatcher {

	protected bool doesMatch = false;
	protected RollContext rollContext = RollContext.Null;
	protected int distance = int.MaxValue;
	protected HashSet<int> matchingDice = new HashSet<int>();

	public abstract void Match(RollContext context);

	public virtual bool Matches() {
		return doesMatch;
	}

	public RollContext GetContext() {
		return rollContext;
	}

	public abstract ElementDisplay[] GetElementDisplays();

	public virtual int GetDistance() {
		return distance;
	}

	public virtual HashSet<int> GetMatchingDiceIndices() {
		return matchingDice;
	}

	protected void Reset() {
		doesMatch = false;
		rollContext = RollContext.Null;
		distance = int.MaxValue;
		matchingDice.Clear();
	}
}
