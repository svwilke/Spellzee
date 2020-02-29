using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class BattleServerHandler : ServerHandler {

	private Game game;
	private ServerBattle battle;

	public BattleServerHandler(Game game, ServerBattle battle) {
		this.game = game;
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
			PlayerPawn pawn = battle.GetCurrentPawn() as PlayerPawn;
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
				int possibleLocks = (int)(battle.GetCurrentPawn() as PlayerPawn).LockCount.GetValue(battle.locks.Length);
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
			GameMsg.MsgIntegerArray actualMsg = msg.ReadMessage<GameMsg.MsgIntegerArray>();
			battle.CastSpell(actualMsg.array[0], actualMsg.array.Length > 1 ? actualMsg.array[1] : -1);
		}
	}


	private void OnPawnDied(Battle b, Pawn pawn) {
		bool allAlliesDead = battle.AreAllAlliesDead();
		if(allAlliesDead) {
			NetworkServer.SendToAll(GameMsg.EndGame, new StringMessage("You all died."));
			return;
		}
		if(!battle.enemy.IsAlive()) {

			for(int i = 0; i < battle.allies.Length; i++) {
				battle.allies[i].Restore();
			}
			
			if(Game.enemy % DB.Enemies.Length == DB.Enemies.Length - 1) {
				int level = Mathf.FloorToInt((Game.enemy + 1) / DB.Enemies.Length);
				if(level % 2 == 1) {
					OpenChoice(level);
				} else {
					OpenVendor();
				}
				return;
			}
			Pawn enemy = game.CreateNextEnemy();
			enemy.SetId(battle.allies.Length);
			//this.battle.enemy.Update(enemy);
			battle.enemy = enemy;
			GameMsg.MsgPawn enemyUpdate = new GameMsg.MsgPawn() { pawn = enemy };
			NetworkServer.SendToAll(GameMsg.UpdatePawn, enemyUpdate);
			/*
			battle.enemy = game.CreateNextEnemy();
			battle.enemy.SetId(battle.allies.Length);
			GameMsg.MsgStartBattle startBattleMsg = new GameMsg.MsgStartBattle() { battle = battle };
			NetworkServer.SendToAll(GameMsg.StartBattle, startBattleMsg);
			*/
			return;
		}
	}

	public void AfterCastSpell(Battle battle, Pawn pawn, Pawn target, int spellId) {
		this.battle.NextTurn();
	}
	/*
	public void NextTurn() {
		int start = battle.currentTurn;
		battle.currentTurn = (battle.currentTurn + 1) % (battle.MaxTurn + 1);
		while(!battle.GetCurrentPawn().IsAlive() && battle.currentTurn != start) {
			battle.currentTurn = (battle.currentTurn + 1) % (battle.MaxTurn + 1);
		}

		if(battle.GetCurrentPawn() is PlayerPawn) {
			PlayerPawn currentPlayer = battle.GetCurrentPawn() as PlayerPawn;
			battle.SetDieCount(Mathf.Max(1, (int)currentPlayer.DieCount.GetValue(5.0)));
			
		}
		battle.ResetLocks();
		NetworkServer.SendToAll(GameMsg.NextTurn, new IntegerMessage(battle.currentTurn));
		if(battle.GetCurrentPawn() is EnemyPawn && !AreAllAlliesDead()) {
			/*
			int spell = REX.Choice(battle.enemy.GetSpells()).GetId();
			CastSpell(spell);
			*//*
			game.StartCoroutine(DoAITurn(battle.GetCurrentPawn() as EnemyPawn));
		}
	}*/

	public void OnPass(NetworkMessage msg) {
		if(msg.conn.connectionId == battle.currentTurn) {
			NetworkServer.SendToAll(GameMsg.Pass, new EmptyMessage());
			battle.NextTurn();
		}
	}

	public void OpenChoice(int level) {
		for(int i = 0; i < battle.allies.Length; i++) {
			GameMsg.MsgPawn openChoiceMsg = new GameMsg.MsgPawn() { pawn = battle.allies[i] };
			NetworkServer.SendToClient(i, GameMsg.OpenChoice, openChoiceMsg);
		}
		game.OpenServerHandler(new ChoiceServerHandler(game, battle.allies, level));
	}

	public void OpenVendor() {
		for(int i = 0; i < battle.allies.Length; i++) {
			GameMsg.MsgPawn openVendorMsg = new GameMsg.MsgPawn() { pawn = battle.allies[i] };
			NetworkServer.SendToClient(i, GameMsg.OpenVendor, openVendorMsg);
		}
		game.OpenServerHandler(new VendorServerHandler(game, battle.allies));
	}

	public override void Close() {
		base.Close();
		EventBus.PawnDied.RemoveListener(OnPawnDied);
		EventBus.CastSpellPost.RemoveListener(AfterCastSpell);
	}
}
