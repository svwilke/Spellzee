using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
}
