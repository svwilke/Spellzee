using System;

public class TargetComponent : SpellComponent {

	private string desc;

	public TargetComponent(TargetType targetType, string desc = "") : base(targetType) {
		this.desc = desc;
	}

	public override void Execute(Spell spell, RollContext context) {
		// Intentionally empty
	}

	public override string GetDescription(Spell spell, RollContext context) {
		return desc;
	}
}
