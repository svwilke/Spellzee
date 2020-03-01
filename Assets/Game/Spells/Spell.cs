﻿public abstract class Spell : RegistryEntry<Spell> {

	private string name;
	private string desc;
	private bool requiresTarget;

	public Spell(string name, string description, bool requiresTarget) {
		this.name = name;
		this.desc = description;
		this.requiresTarget = requiresTarget;
	}

	public string GetName() {
		return name;
	}

	public virtual string GetLongDescription() {
		return desc;
	}

	public virtual string GetShortDescription(RollContext context) {
		return desc;
	}

	public bool DoesRequireTarget() {
		return requiresTarget;
	}

	public abstract bool Matches(RollContext context);
	public abstract void Cast(RollContext context);
	public abstract ElementDisplay[] GetElementDisplays(RollContext context);
}
