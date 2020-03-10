using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Battle {

	public Pawn[] pawns;

	public int currentTurn;

	public Element[] rolls;
	public bool[] locks;

	public int rollsHad = 3;
	public int rollsLeft = 3;

	public List<string> log;

	public int MaxTurn { get { return pawns.Length - 1; } }
	public const int MaxPawnCount = 16;

	private static int[] SlotsFriendly = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
	private static int[] SlotsHostile = new int[] { 12, 13, 14, 15, 8, 9, 10, 11 };

	public Battle() {
		rolls = new Element[5];
		locks = new bool[5];
		log = new List<string>();
		for(int i = 0; i < rolls.Length; i++) {
			rolls[i] = Element.None;
		}
		pawns = new Pawn[MaxPawnCount];
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
		if(id < pawns.Length && id >= 0) {
			return pawns[id];
		} else {
			throw new System.Exception("Could not get pawn with id " + id + ", out of range.");
		}
	}

	public void SetPawn(int id, Pawn pawn) {
		if(id < pawns.Length && id >= 0) {
			pawns[id] = pawn;
		} else {
			throw new System.Exception("Could not set pawn with id " + id + ", out of range.");
		}
	}

	public void CmdAddPawn(Pawn pawn) {
		AddPawn(pawn);
		NetworkServer.SendToAll(GameMsg.AddPawn, new GameMsg.MsgPawn() { pawn = pawn });
	}

	public void AddPawn(Pawn pawn) {
		int slot = GetSlotForTeam(pawn.team);
		if(slot >= 0) {
			SetPawn(slot, pawn);
			pawn.SetId(slot);
		}
	}

	public int GetSlotForTeam(Pawn.Team team) {
		int[] slots = team == Pawn.Team.Friendly ? SlotsFriendly : SlotsHostile;
		for(int i = 0; i < slots.Length; i++) {
			if(pawns[slots[i]] == null) {
				return slots[i];
			}
		}
		return -1;
	}

	public void CmdRemovePawn(Pawn pawn) {
		RemovePawn(pawn);
		NetworkServer.SendToAll(GameMsg.RemovePawn, new GameMsg.MsgPawn() { pawn = pawn });
	}

	public void RemovePawn(Pawn pawn) {
		pawns[pawn.GetId()] = null;
	}

	public Pawn GetClientPawn() {
		return pawns[Game.peerId];
	}

	public Pawn GetCurrentPawn() {
		return pawns[currentTurn];
	}

	public int GetPawnCount() {
		int count = 0;
		for(int i = 0; i < pawns.Length; i++) {
			if(pawns[i] != null) {
				count++;
			}
		}
		return count;
	}

	public List<int> GetPawnIds() {
		List<int> ids = new List<int>();
		for(int i = 0; i < pawns.Length; i++) {
			if(pawns[i] != null) {
				ids.Add(i);
			}
		}
		return ids;
	}

	public List<Pawn> GetPawns(Pawn.Team team) {
		List<Pawn> list = new List<Pawn>();
		for(int i = 0; i < pawns.Length; i++) {
			if(pawns[i] != null) {
				if(pawns[i].team == team) {
					list.Add(pawns[i]);
				}
			}
		}
		return list;
	}

	public List<Pawn> GetCurrentTargets() {
		List<Pawn> list = new List<Pawn>();
		Pawn.Team currentTeam = GetCurrentPawn().team;
		for(int i = 0; i < pawns.Length; i++) {
			if(pawns[i] != null) {
				if(pawns[i].team != currentTeam) {
					list.Add(pawns[i]);
				}
			}
		}
		return list;
	}

	public List<Pawn> GetCurrentAllies() {
		List<Pawn> list = new List<Pawn>();
		Pawn.Team currentTeam = GetCurrentPawn().team;
		for(int i = 0; i < pawns.Length; i++) {
			if(pawns[i] != null) {
				if(pawns[i].team == currentTeam) {
					list.Add(pawns[i]);
				}
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
		writer.Write(GetPawnCount());
		for(int i = 0; i < pawns.Length; i++) {
			if(pawns[i] != null) {
				writer.Write(i);
				pawns[i].Serialize(writer);
			}
		}
		writer.Write(currentTurn);
		writer.Write(rolls.Length);
		for(int i = 0; i < rolls.Length; i++) {
			writer.Write((byte)rolls[i].GetId());
			writer.Write(locks[i]);
		}
	}

	public void Deserialize(NetworkReader reader) {
		int pawnCount = reader.ReadInt32();
		pawns = new Pawn[MaxPawnCount];
		for(int i = 0; i < pawnCount; i++) {
			pawns[reader.ReadInt32()] = Pawn.DeserializeNew(reader);
		}
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
		clone.pawns = new Pawn[MaxPawnCount];
		for(int i = 0; i < MaxPawnCount; i++) {
			if(other.pawns[i] != null) {
				clone.pawns[i] = Pawn.Clone(other.pawns[i]);
			}
		}
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
