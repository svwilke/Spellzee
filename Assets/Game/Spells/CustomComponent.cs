using System;

public class CustomComponent : SpellComponent {

	private Action<Spell, RollContext> execution;
	private Func<Spell, RollContext, string> description;

	public CustomComponent(TargetType targetType, Action<Spell, RollContext> execution, Func<Spell, RollContext, string> description) : base(targetType) {
		this.execution = execution;
		this.description = description;
	}

	public override void Execute(Spell spell, RollContext context) {
		execution.Invoke(spell, context);
	}

	public override string GetDescription(Spell spell, RollContext context) {
		return description.Invoke(spell, context);
	}
}
