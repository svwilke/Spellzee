using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class BattleServerHandler : EncounterServerHandler {

	private ServerBattle battle;

	public BattleServerHandler(Game game, Encounter encounter, ServerBattle battle) : base(game, encounter) {
		this.battle = battle;
		AddHandler(GameMsg.ToggleDieLock, OnDieLockToggle);
		AddHandler(GameMsg.Roll, OnRoll);
		AddHandler(GameMsg.CastSpell, OnCastSpell);
		AddHandler(GameMsg.Pass, OnPass);
		EventBus.PawnDied.AddListener(OnPawnDied);
		EventBus.CastSpellPost.AddListener(AfterCastSpell);

		battle.SetupTurn();
	}

	public void OnRoll(NetworkMessage msg) {
		if(msg.conn.connectionId == battle.currentTurn && battle.rollsLeft > 0) {
			Pawn pawn = battle.GetCurrentPawn();
			if(pawn.HasAI()) {
				return;
			}
			int[] rolls = new int[battle.rolls.Length];
			double[] weights = new double[pawn.Affinities.Length];
			for(int i = 0; i < weights.Length; i++) {
				weights[i] = pawn.GetAffinity(i);
			}
			for(int i = 0; i < rolls.Length; i++) {
				if(battle.locks[i]) {
					rolls[i] = battle.rolls[i].GetId();
				} else {
					rolls[i] = Element.All[REX.Weighted(weights)].GetId();
				}
				battle.rolls[i] = Element.All[rolls[i]];
			}
			battle.rollsLeft -= 1;
			NetworkServer.SendToAll(GameMsg.Roll, new GameMsg.MsgIntegerArray() { array = rolls });
		}
	}

	public void OnDieLockToggle(NetworkMessage msg) {
		if(msg.conn.connectionId == battle.currentTurn) {
			if(battle.GetCurrentPawn().HasAI()) {
				return;
			}
			IntegerMessage actualMsg = msg.ReadMessage<IntegerMessage>();
			int die = actualMsg.value;
			bool newLockState = !battle.locks[die];
			if(newLockState) {
				int currentlyLocked = 0;
				for(int i = 0; i < battle.locks.Length; i++) {
					if(battle.locks[die]) {
						currentlyLocked++;
					}
				}
				int possibleLocks = (int)battle.GetCurrentPawn().LockCount.GetValue(battle.locks.Length);
				if(currentlyLocked >= possibleLocks) {
					string dieText = "more than @FFFFFF" + possibleLocks + ((possibleLocks == 1) ? " die" : " dice");
					if(possibleLocks == 0) {
						dieText = "any dice";
					}
					NetworkServer.SendToClient(msg.conn.connectionId, GameMsg.ShowMessage, new StringMessage("You can't lock " + dieText + " at the moment."));
					return;
				}
			}
			battle.locks[die] = !battle.locks[die];
			NetworkServer.SendToAll(GameMsg.ToggleDieLock, actualMsg);
		}
	}

	public void OnCastSpell(NetworkMessage msg) {
		if(msg.conn.connectionId == battle.currentTurn) {
			if(battle.GetCurrentPawn().HasAI()) {
				return;
			}
			GameMsg.MsgCastSpell actualMsg = msg.ReadMessage<GameMsg.MsgCastSpell>();
			battle.CastSpell(actualMsg.spellId, actualMsg.targetId);
		}
	}

	private void OnPawnDied(Battle b, Pawn pawn) {
		Pawn actualPawn = battle.GetPawn(pawn.GetId());
		if(actualPawn.HasEquipped(Equipments.HeadOfTheHydra)) {
			actualPawn.CmdHeal(new EventBus.DamageHealEvent((int)pawn.MaxHp.GetValue() / 2 - pawn.CurrentHp));
			actualPawn.CmdRevive();
			actualPawn.Unequip(Equipments.HeadOfTheHydra);
			return;
		}

		if(!actualPawn.IsMinion() && actualPawn.team == Pawn.Team.Hostile && !(actualPawn.GetAI() is PassAI)) {
			foreach(Pawn p in battle.GetPawns(Pawn.Team.Friendly)) {
				if(!p.IsMinion()) {
					p.AddXP(actualPawn.GetXPGain() + actualPawn.Level);
					p.Synchronize();
				}
			}
		}

		bool allAlliesDead = battle.AreAllDead(Pawn.Team.Friendly);
		if(pawn.IsMinion()) {
			battle.CmdRemovePawn(pawn);
		}
		if(allAlliesDead) {
			NetworkServer.SendToAll(GameMsg.EndGame, new StringMessage("You all died."));
			return;
		}
		if(battle.AreAllDead(Pawn.Team.Hostile)) {

			List<Pawn> friendlies = battle.GetPawns(Pawn.Team.Friendly);
			List<Pawn> hostiles = battle.GetPawns(Pawn.Team.Hostile);

			foreach(Pawn p in friendlies) {
				p.Restore();
			}

			NetworkServer.SendToAll(GameMsg.EndBattle, new StringMessage("You emerge victorious."));
			/*
			if(Game.enemy % DB.Enemies.Length == DB.Enemies.Length - 1) {
				int level = Mathf.FloorToInt((Game.enemy + 1) / DB.Enemies.Length);
				if(level % 2 == 1) {
					OpenChoice(level);
				} else {
					OpenVendor();
				}
				return;
			}

			foreach(Pawn p in hostiles) {
				battle.CmdRemovePawn(p);
			}

			Pawn enemy = game.CreateNextEnemy();
			battle.CmdAddPawn(enemy);
			*/
			return;
		}
	}

	public void AfterCastSpell(Battle battle, Pawn pawn, Pawn target, string spellId) {
		this.battle.NextTurn();
	}

	public void OnPass(NetworkMessage msg) {
		if(msg.conn.connectionId == battle.currentTurn) {
			if(battle.GetCurrentPawn().HasAI()) {
				return;
			}
			NetworkServer.SendToAll(GameMsg.Pass, new EmptyMessage());
			battle.NextTurn();
		}
	}

	public override void Close() {
		base.Close();
		EventBus.PawnDied.RemoveListener(OnPawnDied);
		EventBus.CastSpellPost.RemoveListener(AfterCastSpell);
	}
}
