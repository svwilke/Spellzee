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

	public void NextTurn() {

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
