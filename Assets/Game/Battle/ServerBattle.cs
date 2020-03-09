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
		while(!GetCurrentPawn().IsAlive() && currentTurn != start) {
			currentTurn = (currentTurn + 1) % (MaxTurn + 1);
		}

		SetupTurn();
		
		NetworkServer.SendToAll(GameMsg.NextTurn, new IntegerMessage(currentTurn));
		if(GetCurrentPawn() is EnemyPawn && !AreAllAlliesDead()) {
			game.StartCoroutine(DoAITurn(GetCurrentPawn() as EnemyPawn));
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
		GameMsg.MsgCastSpell msg = new GameMsg.MsgCastSpell() { spellId = spellId, targetId = targetPawnId };

		Pawn pawn = GetCurrentPawn();
		if(Random.value > pawn.HitChance.GetValue()) {
			NetworkServer.SendToAll(GameMsg.Miss, msg);
			NextTurn();
			return;
		}
		
		NetworkServer.SendToAll(GameMsg.CastSpell, msg);
		Spells.Get(spellId).Cast(BuildContext(targetPawnId));
		NetworkServer.SendToAll(GameMsg.CastSpellEnd, msg);
	}

	public IEnumerator DoAITurn(EnemyPawn enemyPawn) {
		do {
			yield return new WaitForSeconds(1F);
		} while(enemyPawn.DoTurn(this));
	}

	public bool AreAllAlliesDead() {
		bool allAlliesDead = true;
		for(int i = 0; i < allies.Length; i++) {
			if(allies[i].IsAlive()) {
				allAlliesDead = false;
				break;
			}
		}
		return allAlliesDead;
	}
}
