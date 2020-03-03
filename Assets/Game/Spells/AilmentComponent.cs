using System.Linq;
public class AilmentComponent : IntSpellComponent {

	protected Ailment ailment;

	public AilmentComponent(TargetType targetType, Ailment ailment, double baseValue) : base(targetType, baseValue) {
		this.ailment = ailment;
	}

	public override void Execute(Spell spell, RollContext context) {
		context.GetCaster().OnSpellComponentCaster.Invoke(spell, context, this);
		int val = GetValue();
		GetTargets(context).ForEach(pawn => {
			AilmentComponent singleTarget = new AilmentComponent(targetType, ailment, val);
			pawn.OnSpellComponentTarget.Invoke(spell, context, singleTarget);
			pawn.CmdApplyAilment(ailment, singleTarget.GetValue());
		});
	}

	public override string GetDescription(Spell spell, RollContext context) {
		UpdateComponentForDescription(spell, context);
		string desc = string.Format("Apply {0} {1}", GetValue(), ailment.GetFullName());
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
