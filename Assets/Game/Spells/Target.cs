using System;
using System.Linq;
using System.Collections.Generic;

public class Target {

	protected TargetType targetType;
	protected TargetGroup targetGroup;
	protected Func<Pawn, RollContext, bool> targetCondition;

	private List<Pawn> resolvedTargets;

	public Target(TargetType targetType, TargetGroup targetGroup = TargetGroup.Any) {
		this.targetType = targetType;
		this.targetGroup = targetGroup;
		resolvedTargets = new List<Pawn>();
	}

	public bool HasTarget() {
		return resolvedTargets != null && resolvedTargets.Count > 0;
	}

	public List<Pawn> GetTargets() {
		return resolvedTargets;
	}

	public void Resolve(RollContext context) {
		resolvedTargets.Clear();
		switch(targetType) {
			case TargetType.Caster:
				resolvedTargets.Add(context.GetCaster());
				break;
			case TargetType.Target:
				Pawn target = context.GetTarget();
				if(target != null) {
					resolvedTargets.Add(target);
				}
				break;
			case TargetType.Allies:
				resolvedTargets.AddRange(context.GetAllies());
				break;
			case TargetType.Enemies:
				resolvedTargets.AddRange(context.GetEnemies());
				break;
			case TargetType.Random:
				resolvedTargets.Add(REX.Choice(GetPossibleTargets(context)));
				break;
			case TargetType.All:
				resolvedTargets.AddRange(context.GetPawns());
				break;
		}
	}

	public TargetType GetTargetType() {
		return targetType;
	}

	public Target SetTargetType(TargetType targetType) {
		this.targetType = targetType;
		return this;
	}

	public TargetGroup GetTargetGroup() {
		return targetGroup;
	}

	public Target SetTargetGroup(TargetGroup targetGroup) {
		this.targetGroup = targetGroup;
		return this;
	}

	public Target SetCustomTargetGroup(Func<Pawn, RollContext, bool> targetCondition) {
		SetTargetGroup(TargetGroup.Custom);
		this.targetCondition = targetCondition;
		return this;
	}

	public List<Pawn> GetPossibleTargets(RollContext context) {
		return context.GetPawns().Where(pawn => IsValidTarget(pawn, context)).ToList();
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
}

public enum TargetType
{
	None, Caster, Target, Allies, Enemies, Random, All
}

public enum TargetGroup
{
	Any, AnyOther, Ally, AllyOther, Enemy, Custom
}