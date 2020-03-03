using System.Collections.Generic;
using UnityEngine.Events;

public class EventBus {

	public static EvtUIObj UIMouseEnter = new EvtUIObj();
	public static EvtUIObj UIMouseExit = new EvtUIObj();
	public static EvtUIObj UIHoverStart = new EvtUIObj();
	public static EvtUIObj UIHoverEnd = new EvtUIObj();
	public static EvtUIObj UIClick = new EvtUIObj();

	public static EvtPawnInteger PawnDamage = new EvtPawnInteger();
	public static EvtPawnInteger PawnHeal = new EvtPawnInteger();
	public static EvtPawn PawnUpdate = new EvtPawn();
	public static EvtPawn PawnDied = new EvtPawn();

	public static EvtPawnPawnString CastSpellPre = new EvtPawnPawnString();
	public static EvtPawnPawnString CastSpellPost = new EvtPawnPawnString();

	[System.Serializable]
	public class EvtUIObj : UnityEvent<UIObj> { }

	[System.Serializable]
	public class EvtPawn : UnityEvent<Battle, Pawn> { }

	[System.Serializable]
	public class EvtPawnInteger : UnityEvent<Battle, Pawn, int> { }

	[System.Serializable]
	public class EvtPawnString : UnityEvent<Battle, Pawn, string> { }

	[System.Serializable]
	public class EvtPawnPawnString : UnityEvent<Battle, Pawn, Pawn, string> { }

	[System.Serializable]
	public class EvtSpellComponent : UnityEvent<Spell, RollContext, SpellComponent> { }

	[System.Serializable]
	public class EvtSpellComponentList : UnityEvent<Spell, RollContext, List<SpellComponent>> { }

	[System.Serializable]
	public class EvtDamageHeal : UnityEvent<Pawn, DamageHealEvent> { }

	public class DamageHealEvent {
		private Spell spell;
		private SpellComponent component;
		private Ailment ailment;
		private int ailmentIntensity;
		public int amount;
		
		public DamageHealEvent(int amount) {
			this.amount = amount;
		}
		
		public DamageHealEvent(Spell spell, SpellComponent component, int amount) : this(amount) {
			this.spell = spell;
			this.component = component;
		}

		public DamageHealEvent(Ailment ailment, int ailmentIntensity, int amount) : this(amount) {
			this.ailment = ailment;
			this.ailmentIntensity = ailmentIntensity;
		}

		public Spell GetSpell() {
			return spell;
		}

		public SpellComponent GetSpellComponent() {
			return component;
		}

		public Ailment GetAilment() {
			return ailment;
		}

		public int GetAilmentIntensity() {
			return ailmentIntensity;
		}

		public bool IsSourceSpell() {
			return spell != null;
		}

		public bool IsSourceAilment() {
			return ailment != null;
		}
	}
}
