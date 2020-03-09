using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollContext {

	public static RollContext Null = new RollContext(null, new Element[0], 0, 0, null, null, new List<Pawn>(), new List<Pawn>());

	private Battle battle;

	private Element[] elementsRolled;

	private int rollsDone;
	private int rollsLeft;

	private Pawn caster;
	private Pawn target;
	private List<Pawn> enemies;
	private List<Pawn> allies;

	public RollContext(Battle battle, Element[] elements, int rollsDone, int rollsLeft, Pawn caster, Pawn target, List<Pawn> enemies, List<Pawn> allies) {
		this.battle = battle;
		this.elementsRolled = elements;
		this.rollsDone = rollsDone;
		this.rollsLeft = rollsLeft;
		this.caster = caster;
		this.target = target;
		this.enemies = enemies;
		this.allies = allies;
	}

	public Pawn[] GetPawns() {
		List<Pawn> list = new List<Pawn>(enemies);
		list.AddRange(allies);
		return list.ToArray();
	}

	public Pawn GetCaster() {
		return caster;
	}

	public int GetRollsDone() {
		return rollsDone;
	}

	public int GetRollsLeft() {
		return rollsLeft;
	}

	public Element[] GetElementsRolled() {
		return elementsRolled;
	}

	public int GetElementCount(Element elem, bool countChaos) {
		int count = 0;
		for(int i = 0; i < elementsRolled.Length; i++) {
			if(elementsRolled[i] == elem || (countChaos && elementsRolled[i] == Element.Chaos)) {
				count++;
			}
		}
		return count;
	}

	public Pawn GetTarget() {
		return target;
	}

	public Pawn[] GetAllies() {
		return allies.ToArray();
	}

	public Pawn[] GetEnemies() {
		return enemies.ToArray();
	}

	public bool IsAlly(Pawn pawn) {
		return allies.Contains(pawn);
	}

	public bool IsEnemy(Pawn pawn) {
		return enemies.Contains(pawn);
	}

	public void ForEachEnemy(System.Action<Pawn> func) {
		foreach(Pawn p in enemies) {
			func.Invoke(p);
		}
	}

	public void ForEachAlly(System.Action<Pawn> func) {
		foreach(Pawn p in allies) {
			func.Invoke(p);
		}
	}
}
