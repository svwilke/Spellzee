using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActivatableSpell : Spell
{
	private System.Action<RollContext, bool> cast;

	private string shortDescriptionInactive = "";
	private string shortDescriptionActive = "";

	public ActivatableSpell(string name, string longDescription, string shortDescriptionInactive, string shortDescriptionActive,
			bool requiresTarget, System.Action<RollContext, bool> cast) : base(name, longDescription, requiresTarget) {
		this.cast = cast;
		this.shortDescriptionInactive = shortDescriptionInactive;
		this.shortDescriptionActive = shortDescriptionActive;
	}

	public override string GetShortDescription(RollContext context) {
		if(shortDescriptionInactive.Length > 0 && shortDescriptionActive.Length > 0) {
			if(IsActive(context)) {
				return shortDescriptionActive;
			}
			return shortDescriptionInactive;
		}
		return base.GetShortDescription(context);
	}

	public override void Cast(RollContext context) {
		cast.Invoke(context, IsActive(context));
	}

	public abstract bool IsActive(RollContext context);
}
