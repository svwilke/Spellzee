using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class BattleClientHandler : ClientHandler {

	private Game game;
	private Battle battle;

	public BattleClientHandler(Game game, Battle battle) {
		this.game = game;
		this.battle = battle;
		AddHandler(GameMsg.StartBattle, OnBattleStart);
		AddHandler(GameMsg.Roll, OnRoll);
		AddHandler(GameMsg.ToggleDieLock, OnDieLockToggle);
		AddHandler(GameMsg.CastSpell, OnCastSpell);
		AddHandler(GameMsg.CastSpellEnd, OnCastSpellEnd);
		AddHandler(GameMsg.Pass, OnPass);
		AddHandler(GameMsg.EndBattle, OnEndBattle);
		AddHandler(GameMsg.TakeDamage, OnTakeDamage);
		AddHandler(GameMsg.Heal, OnHeal);
		AddHandler(GameMsg.NextTurn, OnNextTurn);
		AddHandler(GameMsg.UpdatePawn, OnPawnUpdate);
		AddHandler(GameMsg.UpdateAilment, OnAilmentUpdate);
		AddHandler(GameMsg.SetupTurn, OnSetupTurn);
		AddHandler(GameMsg.OpenVendor, OnOpenVendor);
		AddHandler(GameMsg.EndGame, OnEndGame);
		AddHandler(GameMsg.Miss, OnMiss);
	}

	public void OnBattleStart(NetworkMessage msg) {
		Battle battle = msg.ReadMessage<GameMsg.MsgStartBattle>().battle;
		// way to start battle on server needed TODO
		game.OpenScreen(new BattleScreen(game, RB.DisplaySize, battle));
		game.OpenClientHandler(new BattleClientHandler(game, battle));
	}

	public void OnSetupTurn(NetworkMessage msg) {
		GameMsg.MsgIntegerArray setup = msg.ReadMessage<GameMsg.MsgIntegerArray>();
		battle.SetDieCount(setup.array[0]);
		battle.SetRollCount(setup.array[1]);
		(game.GetOpenScreen() as BattleScreen).UpdateDieButtons();
	}

	public void OnRoll(NetworkMessage msg) {
		BattleScreen screen = (game.GetOpenScreen() as BattleScreen);
		Battle b = screen.battle;
		int[] rollIds = msg.ReadMessage<GameMsg.MsgIntegerArray>().array;
		Element[] rolls = new Element[rollIds.Length];
		for(int i = 0; i < rollIds.Length; i++) {
			rolls[i] = Element.All[rollIds[i]];
		}
		b.rollsLeft -= 1;
		b.rolls = rolls;
		//RB.SoundPlay(Game.AUDIO_ROLL, Game.volume);
		screen.UpdateContext();
	}

	public void OnDieLockToggle(NetworkMessage msg) {
		Battle b = (game.GetOpenScreen() as BattleScreen).battle;
		int die = msg.ReadMessage<IntegerMessage>().value;
		b.locks[die] = !b.locks[die];
		RB.SoundPlay(Game.AUDIO_BUTTON, Game.volume);
	}

	public void OnTakeDamage(NetworkMessage msg) {
		GameMsg.MsgIntegerArray message = msg.ReadMessage<GameMsg.MsgIntegerArray>();
		Pawn pawn = battle.GetPawn(message.array[0]);
		bool aliveBefore = pawn.IsAlive();
		pawn.Damage(message.array[1]);
		RB.SoundPlay(Game.AUDIO_HURT, Game.volume);
		EventBus.PawnDamage.Invoke(battle, pawn, message.array[1]);
		if(aliveBefore && !pawn.IsAlive()) {
			EventBus.PawnDied.Invoke(battle, pawn);
		}
	}

	public void OnHeal(NetworkMessage msg) {
		GameMsg.MsgIntegerArray message = msg.ReadMessage<GameMsg.MsgIntegerArray>();
		battle.GetPawn(message.array[0]).Heal(message.array[1]);
		RB.SoundPlay(Game.AUDIO_HEAL, Game.volume);
		EventBus.PawnHeal.Invoke(battle, battle.GetPawn(message.array[0]), message.array[1]);
	}

	public void OnCastSpell(NetworkMessage msg) {
		BattleScreen screen = (game.GetOpenScreen() as BattleScreen);
		Battle b = screen.battle;
		Pawn caster = b.GetCurrentPawn();
		GameMsg.MsgIntegerArray actualMsg = msg.ReadMessage<GameMsg.MsgIntegerArray>();
		Pawn target = null;
		if(actualMsg.array.Length > 1 && actualMsg.array[1] >= 0) {
			target = b.GetPawn(actualMsg.array[1]);
		}
		EventBus.CastSpellPre.Invoke(b, caster, target, actualMsg.array[0]);
		/*spell.Cast(b.BuildContext());
		bool allAlliesDead = true;
		for(int i = 0; i < b.allies.Length; i++) {
			if(b.allies[i].IsAlive()) {
				allAlliesDead = false;
				break;
			}
		}
		if(allAlliesDead) {
			game.ShowMessage("You all died.", () => {
				game.OpenScreen(new MainScreen(game, RB.DisplaySize));
			});
			return;
		}
		if(!b.enemy.IsAlive()) {
			if(game.IsHost()) {
				Battle battle = new Battle();
				battle.allies = b.allies;
				for(int i = 0; i < battle.allies.Length; i++) {
					battle.allies[i].Restore();
				}
				if(Game.enemy == DB.EnemyNames.Length - 1) {
					NetworkServer.SendToAll(GameMsg.EndGame, new EmptyMessage());
					return;
				}
				battle.enemy = game.CreateNextEnemy();
				battle.enemy.SetId(battle.allies.Length);
				GameMsg.MsgStartBattle startBattleMsg = new GameMsg.MsgStartBattle() { battle = battle };
				NetworkServer.SendToAll(GameMsg.StartBattle, startBattleMsg);
			}
			return;
		}*/
		//NextTurn(b, screen);
	}

	public void OnCastSpellEnd(NetworkMessage msg) {
		GameMsg.MsgIntegerArray actualMsg = msg.ReadMessage<GameMsg.MsgIntegerArray>();
		Pawn target = null;
		if(actualMsg.array.Length > 1 && actualMsg.array[1] >= 0) {
			target = battle.GetPawn(actualMsg.array[1]);
		}
		EventBus.CastSpellPost.Invoke(battle, battle.GetCurrentPawn(), target, actualMsg.array[0]);
	}

	public void OnPass(NetworkMessage msg) {
		BattleScreen screen = (game.GetOpenScreen() as BattleScreen);
		Battle b = screen.battle;
		b.log.Add(b.GetCurrentPawn().GetName() + " passes...");
		//NextTurn(b, screen);
	}
	
	public void OnPawnUpdate(NetworkMessage msg) {
		GameMsg.MsgPawn pawnMsg = msg.ReadMessage<GameMsg.MsgPawn>();
		Pawn newPawn = pawnMsg.pawn;
		Pawn oldPawn = battle.GetPawn(newPawn.GetId());
		if(battle.allies.Length == newPawn.GetId()) {
			EventBus.NewEnemy.Invoke(battle, oldPawn);
		}
		oldPawn.Update(newPawn);
		/*
			GameMsg.MsgPawn pawnMsg = msg.ReadMessage<GameMsg.MsgPawn>();
			Pawn newPawn = pawnMsg.pawn;
			Pawn oldPawn = battle.GetPawn(newPawn.GetId());
			Debug.Log("updating pawn...");
			int hpDiff = newPawn.CurrentHp - oldPawn.CurrentHp;
			if(hpDiff < 0) {
				Debug.Log("invoking dmg event");
				EventBus.PawnDamage.Invoke(battle, oldPawn, -hpDiff);
			} else
			if(hpDiff > 0) {
				EventBus.PawnHeal.Invoke(battle, oldPawn, hpDiff);
			}
			if(oldPawn.IsAlive() && !newPawn.IsAlive()) {
				EventBus.PawnDied.Invoke(battle, oldPawn);
			}
			oldPawn.Update(newPawn);
			*/
	}

	public void OnAilmentUpdate(NetworkMessage msg) {
		GameMsg.MsgIntegerArray msgIntegerArray = msg.ReadMessage<GameMsg.MsgIntegerArray>();
		Pawn pawn = battle.GetPawn(msgIntegerArray.array[1]);
		if(msgIntegerArray.array[0] == 0) {
			pawn.SetAilment(msgIntegerArray.array[2], msgIntegerArray.array[3]);
		} else {
			pawn.ApplyAilment(msgIntegerArray.array[2], msgIntegerArray.array[3]);
		}
	}

	public void OnNextTurn(NetworkMessage msg) {
		int turn = msg.ReadMessage<IntegerMessage>().value;
		battle.currentTurn = turn;
		battle.rollsLeft = 3;
		for(int i = 0; i < battle.rolls.Length; i++) {
			battle.rolls[i] = Element.None;
			battle.locks[i] = false;
		}
		BattleScreen screen = game.GetOpenScreen() as BattleScreen;
		if(battle.currentTurn < battle.allies.Length) {
			screen.ViewSpellTab(battle.currentTurn);
		}
		screen.UpdateContext();
	}

	public void OnMiss(NetworkMessage msg) {
		GameMsg.MsgIntegerArray msgInts = msg.ReadMessage<GameMsg.MsgIntegerArray>();
		Spell spell = DB.SpellList[msgInts.array[0]];
		battle.log.Add(battle.GetCurrentPawn().GetName() + " tries to cast " + spell.GetName() + " but misses...");
	}

	public void OnEndBattle(NetworkMessage msg) {
		MessageBox msgBox = new MessageBox(msg.ReadMessage<StringMessage>().value);
		if(game.IsHost()) {
			msgBox.AddButton("Continue", () => {
				(game.GetServerHandler() as BattleServerHandler).OpenVendor();
				game.GetOpenScreen().CloseMessageBox();
			});
		} else {
			msgBox.AddButton("Continue", () => {
				if(game.GetOpenScreen() is VendorScreen) {
					game.GetOpenScreen().CloseMessageBox();
				} else {
					msgBox.SetText("Waiting on the host to continue...");
				}
			});
		}
		game.GetOpenScreen().ShowMessageBox(msgBox);
	}

	public void OnOpenVendor(NetworkMessage msg) {
		PlayerPawn pawn = msg.ReadMessage<GameMsg.MsgPawn>().pawn as PlayerPawn;
		VendorScreen screen = new VendorScreen(game, RB.DisplaySize, pawn);
		game.OpenClientHandler(new VendorClientHandler(game, pawn, screen));
		game.OpenScreen(screen);
	}

	public void OnEndGame(NetworkMessage msg) {
		MessageBox msgBox = new MessageBox(msg.ReadMessage<StringMessage>().value);
		msgBox.AddButton("Back to menu", () => {
			game.CancelConnection();
			game.OpenScreen(new MainScreen(game, RB.DisplaySize));
		});
		game.GetOpenScreen().ShowMessageBox(msgBox);
	}
}
