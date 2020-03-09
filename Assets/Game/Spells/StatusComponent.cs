using System;

public class StatusComponent : SpellComponent {

	protected Func<Status> statusFactory;
	protected string shortDescription;

	public StatusComponent(TargetType targetType, string shortDescription, Func<Status> statusFactory) : base(targetType) {
		this.statusFactory = statusFactory;
		this.shortDescription = shortDescription;
	}

	public override void Execute(Spell spell, RollContext context) {
		context.GetCaster().OnSpellComponentCaster.Invoke(spell, context, this);
		GetTargets(context).ForEach(pawn => {
			StatusComponent singleTarget = new StatusComponent(targetType, shortDescription, statusFactory);
			pawn.OnSpellComponentTarget.Invoke(spell, context, singleTarget);
			pawn.CmdAddStatus(singleTarget.statusFactory.Invoke());
		});
	}

	public override string GetDescription(Spell spell, RollContext context) {
		UpdateComponentForDescription(spell, context);
		string desc = "{0}";
		desc = DescriptionHelper.GetDescriptionPrefix(targetType, targetGroup) + desc;
		return string.Format(desc, shortDescription);
	}
}
