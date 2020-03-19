using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomDamageComponent : DamageComponent {

	protected Attribute maxDamage;

	public RandomDamageComponent(double minValue, double maxValue) : base(minValue) {
		maxDamage = new Attribute().SetBaseValue(maxValue);
	}

	public override void AddModifier(AttributeModifier.Operation operation, double modifier) {
		AddModifierMin(operation, modifier);
		AddModifierMax(operation, modifier);
	}

	public void AddModifierMin(AttributeModifier.Operation operation, double modifier) {
		base.AddModifier(operation, modifier);
	}

	public void AddModifierMax(AttributeModifier.Operation operation, double modifier) {
		maxDamage.AddModifier(new AttributeModifier(maxDamage.Size.ToString(), operation, modifier));
	}
	
	public int GetMinValue() {
		return Mathf.Max(0, GetValue());
	}

	public int GetMaxValue() {
		return (int)maxDamage.GetValue();
	}

	public override bool IsValid(Spell spell, RollContext context) {
		return !((GetMinValue() <= 0 && GetMaxValue() <= 0) || GetMaxValue() < GetMinValue());
	}

	public override void Execute(Spell spell, RollContext context) {
		context.GetCaster().OnSpellComponentCaster.Invoke(spell, context, this);
		int dmgMin = GetMinValue();
		int dmgMax = GetMaxValue();
		GetTargets().ForEach(pawn => {
			int dmg = UnityEngine.Random.Range(dmgMin, dmgMax + 1);
			DamageComponent singleTarget = new DamageComponent(dmg);
			singleTarget.SetTarget(GetTarget());
			pawn.OnSpellComponentTarget.Invoke(spell, context, singleTarget);
			EventBus.DamageHealEvent damageEvent = new EventBus.DamageHealEvent(spell, singleTarget, singleTarget.GetValue());
			pawn.CmdDamage(damageEvent);
		});
	}

	public override string GetDescription(Spell spell, RollContext context) {
		UpdateComponentForDescription(spell, context);
		string desc = string.Format("Deal {0}-{1} damage", GetMinValue(), GetMaxValue());
		desc += DescriptionHelper.GetDescriptionSuffix(GetTargetType(), GetTargetGroup());
		desc += ".";
		return desc;
	}
}
