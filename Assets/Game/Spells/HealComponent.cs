public class HealComponent : IntSpellComponent {

	public HealComponent(double baseValue) : base(baseValue) {

	}

	public override void Execute(Spell spell, RollContext context) {
		context.GetCaster().OnSpellComponentCaster.Invoke(spell, context, this);
		int heal = GetValue();
		GetTargets().ForEach(pawn => {
			HealComponent singleTarget = new HealComponent(heal);
			singleTarget.SetTarget(GetTarget());
			pawn.OnSpellComponentTarget.Invoke(spell, context, singleTarget);
			EventBus.DamageHealEvent healEvent = new EventBus.DamageHealEvent(spell, singleTarget, singleTarget.GetValue());
			pawn.CmdHeal(healEvent);
		});
	}

	public override string GetDescription(Spell spell, RollContext context) {
		UpdateComponentForDescription(spell, context);
		string desc = string.Format("Restore {0} life", GetValue());
		desc += DescriptionHelper.GetDescriptionSuffix(GetTargetType(), GetTargetGroup());
		desc += ".";
		return desc;
	}
}
