using System;
using System.Collections.Generic;
using System.Linq;

public class Spell : RegistryEntry<Spell> {

	private string name;
	private string desc;
	private bool requiresTarget;

	private PatternMatcher pattern;
	private List<Func<PatternMatcher, SpellComponent>> componentFactories;

	public Spell(string name, string description, bool requiresTarget, PatternMatcher pattern) {
		this.name = name;
		this.desc = description;
		this.requiresTarget = requiresTarget;
		this.pattern = pattern;
		componentFactories = new List<Func<PatternMatcher, SpellComponent>>();
	}

	public Spell AddComponent(Func<PatternMatcher, SpellComponent> componentFactory) {
		componentFactories.Add(componentFactory);
		return this;
	}

	public string GetName() {
		return name;
	}

	public virtual string GetLongDescription() {
		return desc;
	}

	public virtual string GetShortDescription(RollContext context) {
		string shortDesc = "";
		List<SpellComponent> componentList = BuildComponentList(context);
		foreach(SpellComponent component in componentList) {
			string compDesc = component.GetDescription(this, context);
			if(compDesc != null && compDesc.Length > 0) {
				if(requiresTarget && component.GetTargetType() == SpellComponent.TargetType.Target) {
					shortDesc += compDesc;
					shortDesc += " ";
				} else
				if(!requiresTarget) {
					shortDesc += compDesc;
					shortDesc += " ";
				}
			}
		}
		return shortDesc.Trim();
	}

	public bool DoesRequireTarget() {
		return requiresTarget;
	}

	public List<SpellComponent> BuildComponentList(RollContext context) {
		pattern.Match(context);
		List<SpellComponent> componentList = componentFactories.Select(func => func.Invoke(pattern)).ToList();
		context.GetCaster().OnBuildSpellComponents.Invoke(this, context, componentList);
		return componentList;
	}

	public bool Matches(RollContext context) {
		pattern.Match(context);
		return pattern.Matches();
	}

	public void Cast(RollContext context) {
		List<SpellComponent> components = BuildComponentList(context);
		foreach(SpellComponent sc in components) {
			sc.Execute(this, context);
		}
	}

	public ElementDisplay[] GetElementDisplays(RollContext context) {
		pattern.Match(context);
		return pattern.GetElementDisplays();
	}
}
