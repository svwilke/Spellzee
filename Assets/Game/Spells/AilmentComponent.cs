using System;
public class AilmentComponent : IntSpellComponent {

	protected Func<int, AilmentStatus> ailmentFactory;

	public AilmentComponent(TargetType targetType, Func<int, AilmentStatus> ailmentFactory, double baseValue) : base(targetType, baseValue) {
		this.ailmentFactory = ailmentFactory;
	}

	public override void Execute(Spell spell, RollContext context) {
		context.GetCaster().OnSpellComponentCaster.Invoke(spell, context, this);
		int val = GetValue();
		GetTargets(context).ForEach(pawn => {
			AilmentComponent singleTarget = new AilmentComponent(targetType, ailmentFactory, val);
			pawn.OnSpellComponentTarget.Invoke(spell, context, singleTarget);
			AilmentStatus status = singleTarget.ailmentFactory.Invoke(singleTarget.GetValue());
			pawn.CmdAddStatus(status);
		});
	}

	public override string GetDescription(Spell spell, RollContext context) {
		UpdateComponentForDescription(spell, context);
		string desc = string.Format("Apply {0} {1}", GetValue(), ailmentFactory.Invoke(0).GetFullName());
		switch(targetType) {
			case TargetType.Caster:
				desc += " to yourself";
				break;
			case TargetType.Allies:
				desc += " to all allies";
				break;
			case TargetType.Enemies:
				desc += " to all enemies";
				break;
			case TargetType.All:
				desc += " to everyone";
				break;
		}
		desc += ".";
		return desc;
	}
}
