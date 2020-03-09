using System.Collections.Generic;
using System.Linq;
public class DamageComponent : IntSpellComponent {

	public DamageComponent(TargetType targetType, double baseValue) : base(targetType, baseValue) {

	}

	public override void Execute(Spell spell, RollContext context) {
		context.GetCaster().OnSpellComponentCaster.Invoke(spell, context, this);
		int dmg = GetValue();
		GetTargets(context).ForEach(pawn => {
			DamageComponent singleTarget = new DamageComponent(targetType, dmg);
			pawn.OnSpellComponentTarget.Invoke(spell, context, singleTarget);
			EventBus.DamageHealEvent damageEvent = new EventBus.DamageHealEvent(spell, singleTarget, singleTarget.GetValue());
			pawn.CmdDamage(damageEvent);
		});
	}

	public override string GetDescription(Spell spell, RollContext context) {
		UpdateComponentForDescription(spell, context);
		string desc = string.Format("Deal {0} damage", GetValue());
		desc += DescriptionHelper.GetDescriptionSuffix(targetType, targetGroup);
		desc += ".";
		return desc;
	}
}
