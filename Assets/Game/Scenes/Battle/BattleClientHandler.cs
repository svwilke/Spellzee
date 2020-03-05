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
		AddHandler(GameMsg.OpenChoice, OnOpenChoice);
		AddHandler(GameMsg.EndGame, OnEndGame);
		AddHandler(GameMsg.Miss, OnMiss);
		AddHandler(GameMsg.ShowMessage, OnShowMessage);
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

	public void OnShowMessage(NetworkMessage msg) {
		string text = msg.ReadMessage<StringMessage>().value;
		MessageBox msgBox = new MessageBox(text);
		msgBox.AddButton("Got it", () => game.GetOpenScreen().CloseMessageBox());
		game.GetOpenScreen().ShowMessageBox(msgBox);
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
		Game.PlaySound(Game.AUDIO_BUTTON);
	}

	public void OnTakeDamage(NetworkMessage msg) {
		GameMsg.MsgIntegerArray message = msg.ReadMessage<GameMsg.MsgIntegerArray>();
		Pawn pawn = battle.GetPawn(message.array[0]);
		bool aliveBefore = pawn.IsAlive();
		pawn.Damage(message.array[1]);
		Game.PlaySound(Game.AUDIO_HURT);
		EventBus.PawnDamage.Invoke(battle, pawn, message.array[1]);
		if(aliveBefore && !pawn.IsAlive()) {
			EventBus.PawnDied.Invoke(battle, pawn);
		}
	}

	public void OnHeal(NetworkMessage msg) {
		GameMsg.MsgIntegerArray message = msg.ReadMessage<GameMsg.MsgIntegerArray>();
		battle.GetPawn(message.array[0]).Heal(message.array[1]);
		Game.PlaySound(Game.AUDIO_HEAL);
		EventBus.PawnHeal.Invoke(battle, battle.GetPawn(message.array[0]), message.array[1]);
	}

	public void OnCastSpell(NetworkMessage msg) {
		BattleScreen screen = (game.GetOpenScreen() as BattleScreen);
		Battle b = screen.battle;
		Pawn caster = b.GetCurrentPawn();
		GameMsg.MsgCastSpell actualMsg = msg.ReadMessage<GameMsg.MsgCastSpell>();
		Pawn target = null;
		if(actualMsg.targetId >= 0) {
			target = b.GetPawn(actualMsg.targetId);
		}
		EventBus.CastSpellPre.Invoke(b, caster, target, actualMsg.spellId);
	}

	public void OnCastSpellEnd(NetworkMessage msg) {
		GameMsg.MsgCastSpell actualMsg = msg.ReadMessage<GameMsg.MsgCastSpell>();
		Pawn target = null;
		if(actualMsg.targetId >= 0) {
			target = battle.GetPawn(actualMsg.targetId);
		}
		EventBus.CastSpellPost.Invoke(battle, battle.GetCurrentPawn(), target, actualMsg.spellId);
	}

	public void OnPass(NetworkMessage msg) {
		BattleScreen screen = (game.GetOpenScreen() as BattleScreen);
		Battle b = screen.battle;
		b.log.Add(b.GetCurrentPawn().GetName() + " passes...");
	}
	
	public void OnPawnUpdate(NetworkMessage msg) {
		GameMsg.MsgPawn pawnMsg = msg.ReadMessage<GameMsg.MsgPawn>();
		Pawn newPawn = pawnMsg.pawn;
		Pawn oldPawn = battle.GetPawn(newPawn.GetId());
		EventBus.PawnUpdate.Invoke(battle, newPawn);
		battle.SetPawn(newPawn.GetId(), newPawn);
		BattleScreen screen = game.GetOpenScreen() as BattleScreen;
		if(screen != null) {
			if(screen.spellPane.GetOpenTabIndex() == newPawn.GetId()) {
				screen.ViewSpellTab(newPawn.GetId());
			}
		}
	}

	public void OnAilmentUpdate(NetworkMessage msg) {
		GameMsg.MsgStatusList msgStatusList = msg.ReadMessage<GameMsg.MsgStatusList>();
		Pawn pawn = battle.GetPawn(msgStatusList.pawnId);
		Debug.Log(pawn.GetName() + " has " + msgStatusList.statuses.Count + " statuses");
		pawn.SetStatuses(msgStatusList.statuses);
	}

	public void OnNextTurn(NetworkMessage msg) {
		int turn = msg.ReadMessage<IntegerMessage>().value;
		battle.currentTurn = turn;
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
		StringMessage actualMsg = msg.ReadMessage<StringMessage>();
		Spell spell = Spells.Get(actualMsg.value);
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

	public void OnOpenChoice(NetworkMessage msg) {
		PlayerPawn pawn = msg.ReadMessage<GameMsg.MsgPawn>().pawn as PlayerPawn;
		ChoiceScreen screen = new ChoiceScreen(game, RB.DisplaySize, pawn);
		MessageBox msgBox = new MessageBox("Congratulations! This area is cleared");
		game.OpenClientHandler(new ChoiceClientHandler(game, pawn, screen));
		msgBox.AddButton("Continue", () => {
			game.OpenScreen(screen);
		});
		game.GetOpenScreen().ShowMessageBox(msgBox);
	}

	public void OnOpenVendor(NetworkMessage msg) {
		PlayerPawn pawn = msg.ReadMessage<GameMsg.MsgPawn>().pawn as PlayerPawn;
		VendorScreen screen = new VendorScreen(game, RB.DisplaySize, pawn);
		MessageBox msgBox = new MessageBox("Congratulations! This area is cleared");
		game.OpenClientHandler(new VendorClientHandler(game, pawn, screen));
		msgBox.AddButton("Continue", () => {
			game.OpenScreen(screen);
		});
		game.GetOpenScreen().ShowMessageBox(msgBox);
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
