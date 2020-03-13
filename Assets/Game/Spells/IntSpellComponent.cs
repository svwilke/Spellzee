public abstract class IntSpellComponent : SpellComponent {

	protected Attribute attribute;

	public IntSpellComponent(TargetType targetType, double baseValue) : base(targetType) {
		attribute = new Attribute().SetBaseValue(baseValue);
	}

	public virtual void AddModifier(AttributeModifier.Operation operation, double modifier) {
		attribute.AddModifier(new AttributeModifier(attribute.Size.ToString(), operation, modifier));
	}

	public override bool IsValid(Spell spell, RollContext context) {
		return GetValue() > 0;
	}

	public int GetValue() {
		return (int)attribute.GetValue();
	}
}
