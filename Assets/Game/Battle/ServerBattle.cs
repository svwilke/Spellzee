using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class ServerBattle : Battle
{

	private Game game;

	public ServerBattle(Game game) : base() {
		this.game = game;
	}

	public void CmdSetDieCount(int count) {
		SetDieCount(count);
		//NetworkServer.SendToAll()
	}

	public void ToggleLock(int dieIndex, int clientIndex = -1) {
		bool newLockState = !locks[dieIndex];
		if(newLockState) {
			int currentlyLocked = 0;
			for(int i = 0; i < locks.Length; i++) {
				if(locks[dieIndex]) {
					currentlyLocked++;
				}
			}
			int possibleLocks = (int)GetCurrentPawn().LockCount.GetValue(locks.Length);
			if(currentlyLocked >= possibleLocks) {
				if(clientIndex > -1) {
					string dieText = "more than @FFFFFF" + possibleLocks + ((possibleLocks == 1) ? " die" : " dice");
					if(possibleLocks == 0) {
						dieText = "any dice";
					}
					NetworkServer.SendToClient(clientIndex, GameMsg.ShowMessage, new StringMessage("You can't lock " + dieText + " at the moment."));
				}
				return;
			}
		}
		locks[dieIndex] = !locks[dieIndex];
		NetworkServer.SendToAll(GameMsg.ToggleDieLock, new IntegerMessage(dieIndex));
	}

	public void Roll() {
		Pawn pawn = GetCurrentPawn();
		int[] rolls = new int[this.rolls.Length];
		double[] weights = new double[pawn.Affinities.Length];
		for(int i = 0; i < weights.Length; i++) {
			weights[i] = pawn.GetAffinity(i);
		}
		for(int i = 0; i < rolls.Length; i++) {
			if(locks[i]) {
				rolls[i] = this.rolls[i].GetId();
			} else {
				rolls[i] = Element.All[REX.Weighted(weights)].GetId();
			}
			this.rolls[i] = Element.All[rolls[i]];
		}
		rollsLeft -= 1;
		NetworkServer.SendToAll(GameMsg.Roll, new GameMsg.MsgIntegerArray() { array = rolls });
	}

	public void NextTurn() {
		if(AreAllDead(Pawn.Team.Friendly) || AreAllDead(Pawn.Team.Hostile)) {
			return;
		}

		GetCurrentPawn().OnEndTurn.Invoke(this, GetCurrentPawn());
		
		int start = currentTurn;
		currentTurn = (currentTurn + 1) % (MaxTurn + 1);
		while((GetCurrentPawn() == null || !GetCurrentPawn().IsAlive()) && currentTurn != start) {
			currentTurn = (currentTurn + 1) % (MaxTurn + 1);
		}

		SetupTurn();
		
		NetworkServer.SendToAll(GameMsg.NextTurn, new IntegerMessage(currentTurn));
		if(GetCurrentPawn().HasAI() && !AreAllDead(Pawn.Team.Friendly)) {
			game.StartCoroutine(DoAITurn(GetCurrentPawn()));
		}
	}

	public void SetupTurn() {
		Pawn pawn = GetCurrentPawn();
		pawn.OnSetupTurn.Invoke(this, pawn);
		SetDieCount(Mathf.Max(1, (int)pawn.DieCount.GetValue()));
		SetRollCount(Mathf.Max(0, (int)pawn.RollCount.GetValue()));
		ResetDice();
		ResetLocks();
		
		NetworkServer.SendToAll(GameMsg.SetupTurn, new GameMsg.MsgIntegerArray(rolls.Length, rollsHad));
		pawn.OnBeginTurn.Invoke(this, pawn);
	}

	public void CastSpell(string spellId, int targetPawnId = -1) {
		
		Pawn pawn = GetCurrentPawn();
		if(Random.value > pawn.HitChance.GetValue()) {
			GameMsg.MsgCastSpell msg = new GameMsg.MsgCastSpell() { spellId = spellId, targetId = targetPawnId };
			NetworkServer.SendToAll(GameMsg.Miss, msg);
			NextTurn();
			return;
		}

		DoCastSpell(spellId, targetPawnId);
	}

	public void DoCastSpell(string spellId, int targetPawnId = -1, bool sendBeginMsg = true, bool sendEndMsg = true) {
		GameMsg.MsgCastSpell msg = new GameMsg.MsgCastSpell() { spellId = spellId, targetId = targetPawnId };

		Pawn pawn = GetCurrentPawn();
		Spell spell = Spells.Get(spellId);
		RollContext context = BuildContext(targetPawnId);

		if(sendBeginMsg) {
			NetworkServer.SendToAll(GameMsg.CastSpell, msg);
		}
		pawn.OnBeforeSpellCast.Invoke(spell, context);

		spell.Cast(BuildContext(targetPawnId));

		if(sendEndMsg) {
			NetworkServer.SendToAll(GameMsg.CastSpellEnd, msg);
		}
		pawn.OnAfterSpellCast.Invoke(spell, context);
	}

	public IEnumerator DoAITurn(Pawn aiPawn) {
		AIModule ai = aiPawn.GetAI();
		do {
			yield return new WaitForSeconds(1F);
		} while(ai.DoTurn(this));
	}

	public bool AreAllDead(Pawn.Team team) {
		for(int i = 0; i < pawns.Length; i++) {
			if(pawns[i] != null && pawns[i].IsAlive() && pawns[i].team == team) {
				return false;
			}
		}
		return true;
	}
}
