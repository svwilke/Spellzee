using System;
using System.Linq;
using System.Collections.Generic;

public abstract class SpellComponent {

	private Target target;

	public SpellComponent() {

	}

	public Target GetTarget() {
		return target;
	}

	public void SetTarget(Target target) {
		this.target = target;
	}

	public List<Pawn> GetTargets() {
		return target.GetTargets();
	}

	public TargetType GetTargetType() {
		return target.GetTargetType();
	}

	public SpellComponent SetTargetType(TargetType targetType) {
		target.SetTargetType(targetType);
		return this;
	}

	public TargetGroup GetTargetGroup() {
		return target.GetTargetGroup();
	}

	public SpellComponent SetTargetGroup(TargetGroup targetGroup) {
		target.SetTargetGroup(targetGroup);
		return this;
	}

	public SpellComponent SetCustomTargetGroup(Func<Pawn, RollContext, bool> targetCondition) {
		target.SetCustomTargetGroup(targetCondition);
		return this;
	}

	public List<Pawn> GetPossibleTargets(RollContext context) {
		return context.GetPawns().Where(pawn => IsValidTarget(pawn, context)).ToList();
	}

	public virtual bool IsCastable(Spell spell, RollContext context) {
		if(GetTargetType() != TargetType.None) {
			return GetPossibleTargets(context).Count > 0;
		} else {
			return true;
		}
	}

	public bool IsValidTarget(Pawn pawn, RollContext context) {
		return target.IsValidTarget(pawn, context);
	}

	public virtual bool IsValid(Spell spell, RollContext context) {
		return true;
	}

	public abstract void Execute(Spell spell, RollContext context);
	public abstract string GetDescription(Spell spell, RollContext context);

	protected void UpdateComponentForDescription(Spell spell, RollContext context) {
		context.GetCaster().OnSpellComponentCaster.Invoke(spell, context, this);
		List<Pawn> targets = GetPossibleTargets(context);
		if(targets.Count == 1) {
			targets[0].OnSpellComponentTarget.Invoke(spell, context, this);
		}
	}
}
