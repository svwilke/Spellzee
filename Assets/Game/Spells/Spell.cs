using System;
using System.Collections.Generic;
using System.Linq;

public class Spell : RegistryEntry<Spell> {

	private string name;
	private string desc;

	private PatternMatcher pattern;
	private List<Func<PatternMatcher, SpellComponent>> componentFactories;

	public Spell(string name, string description, PatternMatcher pattern) {
		this.name = name;
		this.desc = description;
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

	public PatternMatcher GetPattern() {
		return pattern;
	}

	public virtual string GetLongDescription() {
		return desc;
	}

	public virtual string GetShortDescription(RollContext context) {
		string shortDesc = "";
		List<SpellComponent> componentList = BuildComponentList(context);
		bool requiresTarget = DoesRequireTarget(context);
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

	public virtual bool IsCastable(RollContext context) {
		if(context.GetCaster().GetSpellData(this).GetBool(DataKey.Disabled)) {
			return false;
		}
		List<SpellComponent> componentList = BuildComponentList(context);
		foreach(SpellComponent component in componentList) {
			if(!component.IsCastable(this, context)) {
				return false;
			}
		}
		return true;
	}

	public bool DoesRequireTarget(RollContext context) {
		return BuildComponentList(context).Any(sc => sc.GetTargetType() == SpellComponent.TargetType.Target);
	}

	public bool IsValidTarget(Pawn pawn, RollContext context) {
		return BuildComponentList(context).All(sc => sc.GetTargetType() != SpellComponent.TargetType.Target || sc.IsValidTarget(pawn, context));
	}

	public List<SpellComponent> BuildComponentList(RollContext context) {
		pattern.Match(context);
		List<SpellComponent> componentList = componentFactories.Select(func => func.Invoke(pattern)).ToList();
		context.GetCaster().OnBuildSpellComponents.Invoke(this, context, componentList);
		List<SpellComponent> zeroComponents = new List<SpellComponent>();
		foreach(SpellComponent sc in componentList) {
			IntSpellComponent isc = sc as IntSpellComponent;
			if(isc != null) {
				if(isc.GetValue() <= 0) {
					zeroComponents.Add(isc);
				}
			}
		}
		componentList.RemoveAll(zeroComponents.Contains);
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

	public bool IsElement(RollContext context, Element elem) {
		foreach(ElementDisplay ed in GetElementDisplays(context)) {
			if(ed.element == elem) {
				return true;
			}
		}
		return false;
	}
}
