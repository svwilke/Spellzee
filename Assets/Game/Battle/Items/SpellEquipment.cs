using UnityEngine.Events;

public class SpellEquipment : Equipment {

	private bool isCaster = false;
	private UnityAction<Spell, RollContext, SpellComponent> componentListener;

	public SpellEquipment(string name, string desc, bool caster, UnityAction<Spell, RollContext, SpellComponent> componentListener) : base(name, desc) {
		this.isCaster = caster;
		this.componentListener = componentListener;
	}

	public override void OnEquipped(Pawn pawn) {
		if(isCaster) {
			pawn.OnSpellComponentCaster.AddListener(componentListener);
		} else {
			pawn.OnSpellComponentTarget.AddListener(componentListener);
		}
	}

	public override void OnUnequipped(Pawn pawn) {
		if(isCaster) {
			pawn.OnSpellComponentCaster.RemoveListener(componentListener);
		} else {
			pawn.OnSpellComponentTarget.RemoveListener(componentListener);
		}
	}
}
