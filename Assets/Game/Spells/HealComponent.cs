public class HealComponent : IntSpellComponent {

	public HealComponent(TargetType targetType, double baseValue) : base(targetType, baseValue) {

	}

	public override void Execute(Spell spell, RollContext context) {
		context.GetCaster().OnSpellComponentCaster.Invoke(spell, context, this);
		int heal = GetValue();
		GetTargets(context).ForEach(pawn => {
			HealComponent singleTarget = new HealComponent(targetType, heal);
			pawn.OnSpellComponentTarget.Invoke(spell, context, singleTarget);
			pawn.CmdHeal(singleTarget.GetValue());
		});
	}

	public override string GetDescription(Spell spell, RollContext context) {
		// invoke necessary events for description matching behaviour (with modifiers!)
		string desc = string.Format("Restore {0} life", GetValue());
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
