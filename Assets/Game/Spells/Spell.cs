using System;
using System.Collections.Generic;
using System.Linq;

public class Spell : RegistryEntry<Spell> {

	private string name;
	private string desc;

	private PatternMatcher pattern;
	private List<Func<PatternMatcher, SpellComponent>> componentFactories;

	private List<Func<PatternMatcher, Target>> targetFactories;
	private int activeTargetIndex = -1;
	private Dictionary<int, int> componentTargetMapping;

	public Spell(string name, string description, PatternMatcher pattern) {
		this.name = name;
		this.desc = description;
		this.pattern = pattern;
		componentFactories = new List<Func<PatternMatcher, SpellComponent>>();
		targetFactories = new List<Func<PatternMatcher, Target>>();
		componentTargetMapping = new Dictionary<int, int>();
	}

	public Spell AddTarget(Func<PatternMatcher, Target> targetFactory) {
		targetFactories.Add(targetFactory);
		activeTargetIndex++;
		return this;
	}

	public Target[] BuildTargets() {
		return targetFactories.Select(tf => tf.Invoke(pattern)).ToArray();
	}

	public Spell AddComponent(Func<PatternMatcher, SpellComponent> componentFactory) {
		componentTargetMapping.Add(componentFactories.Count, activeTargetIndex);
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
				//if(requiresTarget && component.GetTargetType() == SpellComponent.TargetType.Target) {
					shortDesc += compDesc;
					shortDesc += "\n";
				/*} else
				if(!requiresTarget) {
					shortDesc += compDesc;
					shortDesc += "\n";
				}*/
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
		Target[] targets;
		BuildComponentList(context, out targets);
		return targets.Any(target => target.GetTargetType() == TargetType.Target);
		//return BuildComponentList(context).Any(sc => sc.GetTargetType() == TargetType.Target);
	}

	public bool IsValidTarget(Pawn pawn, RollContext context) {
		Target[] targets;
		BuildComponentList(context, out targets);
		return targets.All(target => target.IsValidTarget(pawn, context));
		//return BuildComponentList(context).All(sc => sc.GetTargetType() != TargetType.Target || sc.IsValidTarget(pawn, context));
	}

	public List<SpellComponent> BuildComponentList(RollContext context) {
		Target[] targets;
		return BuildComponentList(context, out targets);
	}

	public List<SpellComponent> BuildComponentList(RollContext context, out Target[] targets) {
		pattern.Match(context);
		targets = BuildTargets();
		List<SpellComponent> componentList = componentFactories.Select(func => func.Invoke(pattern)).ToList();
		for(int i = 0; i < componentList.Count; i++) {
			int targetIndex = -1;
			Target target = new Target(TargetType.None);
			componentTargetMapping.TryGetValue(i, out targetIndex);
			if(targetIndex >= 0 && targetIndex < targets.Length) {
				target = targets[targetIndex];
			}
			componentList[i].SetTarget(target);
		}
		context.GetCaster().OnBuildSpellComponents.Invoke(this, context, componentList);
		componentList.RemoveAll(component => !component.IsValid(this, context));
		return componentList;
	}

	public bool Matches(RollContext context) {
		pattern.Match(context);
		return pattern.Matches();
	}

	public void Cast(RollContext context) {
		Target[] targets;
		List<SpellComponent> components = BuildComponentList(context, out targets);
		for(int i = 0; i < targets.Length; i++) {
			targets[i].Resolve(context);
		}
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
