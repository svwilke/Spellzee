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
			pawn.CmdDamage(singleTarget.GetValue());
		});
	}

	public override string GetDescription(Spell spell, RollContext context) {
		UpdateComponentForDescription(spell, context);
		string desc = string.Format("Deal {0} damage", GetValue());
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
