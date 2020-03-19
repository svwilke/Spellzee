using System;

public class NullComponent : SpellComponent {

	public NullComponent() : base() {

	}

	public override void Execute(Spell spell, RollContext context) {
		
	}

	public override string GetDescription(Spell spell, RollContext context) {
		return "";
	}
}
