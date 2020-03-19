using System;
public class AilmentComponent : IntSpellComponent {

	protected Func<int, AilmentStatus> ailmentFactory;

	public AilmentComponent(Func<int, AilmentStatus> ailmentFactory, double baseValue) : base(baseValue) {
		this.ailmentFactory = ailmentFactory;
	}

	public AilmentStatus GetAilment() {
		return ailmentFactory.Invoke(GetValue());
	}

	public override void Execute(Spell spell, RollContext context) {
		context.GetCaster().OnSpellComponentCaster.Invoke(spell, context, this);
		int val = GetValue();
		GetTargets().ForEach(pawn => {
			AilmentComponent singleTarget = new AilmentComponent(ailmentFactory, val);
			singleTarget.SetTarget(GetTarget());
			pawn.OnSpellComponentTarget.Invoke(spell, context, singleTarget);
			AilmentStatus status = singleTarget.ailmentFactory.Invoke(singleTarget.GetValue());
			pawn.CmdAddStatus(status);
		});
	}

	public override string GetDescription(Spell spell, RollContext context) {
		UpdateComponentForDescription(spell, context);
		string desc = string.Format("Apply {0} {1}", GetValue(), ailmentFactory.Invoke(0).GetFullName());
		desc += DescriptionHelper.GetDescriptionSuffix(GetTargetType(), GetTargetGroup());
		desc += ".";
		return desc;
	}
}
