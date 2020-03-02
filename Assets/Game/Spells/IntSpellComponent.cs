public abstract class IntSpellComponent : SpellComponent {

	protected Attribute attribute;
	private int modifierCount = 0;

	public IntSpellComponent(TargetType targetType, double baseValue) : base(targetType) {
		attribute = new Attribute().SetBaseValue(baseValue);
	}

	public void AddModifier(AttributeModifier.Operation operation, double modifier) {
		attribute.AddModifier(new AttributeModifier(modifierCount.ToString(), operation, modifier));
		modifierCount++;
	}

	public int GetValue() {
		return (int)attribute.GetValue();
	}
}
