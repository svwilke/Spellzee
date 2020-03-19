using System;

public class CustomComponent : SpellComponent {

	private Action<Spell, RollContext, Target> execution;
	private Func<Spell, RollContext, Target, string> description;

	public CustomComponent(Action<Spell, RollContext, Target> execution, Func<Spell, RollContext, Target, string> description) : base() {
		this.execution = execution;
		this.description = description;
	}

	public override void Execute(Spell spell, RollContext context) {
		execution.Invoke(spell, context, GetTarget());
	}

	public override string GetDescription(Spell spell, RollContext context) {
		return description.Invoke(spell, context, GetTarget());
	}
}
