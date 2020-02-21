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
	public static EvtPawn NewEnemy = new EvtPawn();
	public static EvtPawn PawnDied = new EvtPawn();

	public static EvtPawnPawnInteger CastSpellPre = new EvtPawnPawnInteger();
	public static EvtPawnPawnInteger CastSpellPost = new EvtPawnPawnInteger();

	[System.Serializable]
	public class EvtUIObj : UnityEvent<UIObj> { }

	[System.Serializable]
	public class EvtPawn : UnityEvent<Battle, Pawn> { }

	[System.Serializable]
	public class EvtPawnInteger : UnityEvent<Battle, Pawn, int> { }

	[System.Serializable]
	public class EvtPawnPawnInteger : UnityEvent<Battle, Pawn, Pawn, int> { }
}
