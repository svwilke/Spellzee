using System.Collections.Generic;

public abstract class SpellComponent {

	protected TargetType targetType;

	public SpellComponent(TargetType targetType) {
		this.targetType = targetType;
	}

	public List<Pawn> GetTargets(RollContext context) {
		List<Pawn> targetList = new List<Pawn>();
		switch(targetType) {
			case TargetType.Caster:
				targetList.Add(context.GetCaster());
				break;
			case TargetType.Target:
				targetList.Add(context.GetTarget());
				break;
			case TargetType.Allies:
				targetList.AddRange(context.GetAllies());
				break;
			case TargetType.Enemies:
				targetList.AddRange(context.GetEnemies());
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

	public abstract void Execute(Spell spell, RollContext context);
	public abstract string GetDescription(Spell spell, RollContext context);

	public enum TargetType {
		None, Caster, Target, Allies, Enemies, All
	}
}
