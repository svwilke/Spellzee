using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Battle {

	public Pawn[] allies;
	public Pawn enemy;

	public int currentTurn;

	public Element[] rolls;
	public bool[] locks;

	public int rollsHad = 3;
	public int rollsLeft = 3;

	public List<string> log;

	public int MaxTurn { get { return allies.Length; } }

	public Battle() {
		rolls = new Element[5];
		locks = new bool[5];
		log = new List<string>();
		for(int i = 0; i < rolls.Length; i++) {
			rolls[i] = Element.None;
		}
	}

	public void SetRollCount(int count) {
		rollsHad = rollsLeft = count;
	}

	public void SetDieCount(int count) {
		Element[] newRolls = new Element[count];
		bool[] newLocks = new bool[count];
		for(int i = 0; i < rolls.Length && i < count; i++) {
			newRolls[i] = rolls[i];
			newLocks[i] = locks[i];
		}
		for(int i = rolls.Length; i < count; i++) {
			newRolls[i] = Element.None;
		}
		rolls = newRolls;
		locks = newLocks;
	}

	public void ResetDice() {
		for(int i = 0; i < rolls.Length; i++) {
			rolls[i] = Element.None;
		}
	}

	public void ResetLocks() {
		for(int i = 0; i < locks.Length; i++) {
			locks[i] = false;
		}
	}

	public Pawn GetPawn(int id) {
		if(id < allies.Length) {
			return allies[id];
		}
		return enemy;
	}

	public void SetPawn(int id, Pawn pawn) {
		if(id < allies.Length) {
			allies[id] = pawn;
		} else {
			enemy = pawn;
		}
	}

	public Pawn GetClientPawn() {
		return allies[Game.peerId];
	}

	public Pawn GetCurrentPawn() {
		if(currentTurn >= allies.Length) {
			return enemy;
		} else {
			return allies[currentTurn];
		}
	}

	public List<Pawn> GetCurrentTargets() {
		List<Pawn> list = new List<Pawn>();
		if(currentTurn >= allies.Length) {
			for(int i = 0; i < allies.Length; i++) {
				list.Add(allies[i]);
			}
		} else {
			list.Add(enemy);
		}
		return list;
	}

	public List<Pawn> GetCurrentAllies() {
		List<Pawn> list = new List<Pawn>();
		if(currentTurn >= allies.Length) {
			list.Add(enemy);
		} else {
			for(int i = 0; i < allies.Length; i++) {
				list.Add(allies[i]);
			}
		}
		return list;
	}

	public RollContext BuildContext(int targetPawnId = -1) {
		Pawn targetPawn = null;
		if(targetPawnId >= 0) {
			targetPawn = GetPawn(targetPawnId);
		}
		return new RollContext(this, rolls, rollsHad - rollsLeft, rollsLeft, GetCurrentPawn(), targetPawn, GetCurrentTargets(), GetCurrentAllies());
	}

	public void Serialize(NetworkWriter writer) {
		writer.Write(allies.Length);
		for(int i = 0; i < allies.Length; i++) {
			allies[i].Serialize(writer);
		}
		enemy.Serialize(writer);
		writer.Write(currentTurn);
		writer.Write(rolls.Length);
		for(int i = 0; i < rolls.Length; i++) {
			writer.Write((byte)rolls[i].GetId());
			writer.Write(locks[i]);
		}
	}

	public void Deserialize(NetworkReader reader) {
		int allyCount = reader.ReadInt32();
		allies = new Pawn[allyCount];
		for(int i = 0; i < allyCount; i++) {
			allies[i] = Pawn.DeserializeNew(reader);
		}
		enemy = Pawn.DeserializeNew(reader);
		currentTurn = reader.ReadInt32();
		int rollCount = reader.ReadInt32();
		rolls = new Element[rollCount];
		locks = new bool[rollCount];
		for(int i = 0; i < rollCount; i++) {
			rolls[i] = Element.All[reader.ReadByte()];
			locks[i] = reader.ReadBoolean();
		}
	}

	public static Battle DeserializeNew(NetworkReader reader) {
		Battle b = new Battle();
		b.Deserialize(reader);
		return b;
	}

	public static Battle Clone(Battle other) {
		Battle clone = new Battle();
		clone.allies = new Pawn[other.allies.Length];
		for(int i = 0; i < clone.allies.Length; i++) {
			clone.allies[i] = Pawn.Clone(other.allies[i]);
		}
		clone.enemy = Pawn.Clone(other.enemy);
		clone.currentTurn = other.currentTurn;
		clone.rolls = new Element[other.rolls.Length];
		clone.locks = new bool[other.rolls.Length];
		for(int i = 0; i < clone.rolls.Length; i++) {
			clone.rolls[i] = other.rolls[i];
			clone.locks[i] = other.locks[i];
		}
		return clone;
	}
}
