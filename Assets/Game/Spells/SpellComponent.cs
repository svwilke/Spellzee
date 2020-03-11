using System;
using System.Linq;
using System.Collections.Generic;

public abstract class SpellComponent {

	protected TargetType targetType;
	protected TargetGroup targetGroup;
	protected Func<Pawn, RollContext, bool> targetCondition;

	public SpellComponent(TargetType targetType, TargetGroup targetGroup = TargetGroup.Any) {
		this.targetType = targetType;
		this.targetGroup = targetGroup;
	}

	public List<Pawn> GetTargets(RollContext context) {
		List<Pawn> targetList = new List<Pawn>();
		switch(targetType) {
			case TargetType.Caster:
				targetList.Add(context.GetCaster());
				break;
			case TargetType.Target:
				Pawn target = context.GetTarget();
				if(target != null) {
					targetList.Add(target);
				}
				break;
			case TargetType.Allies:
				targetList.AddRange(context.GetAllies());
				break;
			case TargetType.Enemies:
				targetList.AddRange(context.GetEnemies());
				break;
			case TargetType.Random:
				targetList.Add(REX.Choice(GetPossibleTargets(context)));
				break;
			case TargetType.All:
				targetList.AddRange(context.GetPawns());
				break;
		}
		return targetList;
	}

	public TargetType GetTargetType() {
		return targetType;
	}

	public SpellComponent SetTargetType(TargetType targetType) {
		this.targetType = targetType;
		return this;
	}

	public TargetGroup GetTargetGroup() {
		return targetGroup;
	}

	public SpellComponent SetTargetGroup(TargetGroup targetGroup) {
		this.targetGroup = targetGroup;
		return this;
	}

	public SpellComponent SetCustomTargetGroup(Func<Pawn, RollContext, bool> targetCondition) {
		SetTargetGroup(TargetGroup.Custom);
		this.targetCondition = targetCondition;
		return this;
	}

	public List<Pawn> GetPossibleTargets(RollContext context) {
		return context.GetPawns().Where(pawn => IsValidTarget(pawn, context)).ToList();
	}

	public virtual bool IsCastable(Spell spell, RollContext context) {
		if(targetType != TargetType.None) {
			return GetPossibleTargets(context).Count > 0;
		} else {
			return true;
		}
	}

	public bool IsValidTarget(Pawn pawn, RollContext context) {
		switch(targetGroup) {
			case TargetGroup.Any:
				return true;
			case TargetGroup.AnyOther:
				return pawn != context.GetCaster();
			case TargetGroup.Ally:
				return context.IsAlly(pawn);
			case TargetGroup.AllyOther:
				return context.IsAlly(pawn) && pawn != context.GetCaster();
			case TargetGroup.Enemy:
				return context.IsEnemy(pawn);
			case TargetGroup.Custom:
				return targetCondition != null && targetCondition.Invoke(pawn, context);
		}
		return false;
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

	public enum TargetType {
		None, Caster, Target, Allies, Enemies, Random, All
	}

	public enum TargetGroup {
		Any, AnyOther, Ally, AllyOther, Enemy, Custom
	}
}
