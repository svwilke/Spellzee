using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class BattleClientHandler : EncounterClientHandler {

	private Battle battle;
	private BattleScreen screen;

	public BattleClientHandler(Game game, Battle battle, BattleScreen screen) : base(game) {
		this.game = game;
		this.battle = battle;
		this.screen = screen;
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
		AddHandler(GameMsg.EndGame, OnEndGame);
		AddHandler(GameMsg.Miss, OnMiss);
		AddHandler(GameMsg.ShowMessage, OnShowMessage);
		AddHandler(GameMsg.AddPawn, OnPawnAdd);
		AddHandler(GameMsg.RemovePawn, OnPawnRemove);
	}

	private BattleScreen GetScreen() {
		BattleScreen scr = screen.GetActualScreen();
		if(scr != screen) {
			screen = scr;
		}
		return screen;
	}

	public void OnBattleStart(NetworkMessage msg) {
		Battle battle = msg.ReadMessage<GameMsg.MsgStartBattle>().battle;
		// way to start battle on server needed TODO
		BattleScreen screen = new BattleScreen(game, RB.DisplaySize, battle);
		game.OpenScreen(screen);
		game.OpenClientHandler(new BattleClientHandler(game, battle, screen));
	}

	public void OnSetupTurn(NetworkMessage msg) {
		GameMsg.MsgIntegerArray setup = msg.ReadMessage<GameMsg.MsgIntegerArray>();
		battle.SetDieCount(setup.array[0]);
		battle.SetRollCount(setup.array[1]);
		BattleScreen screen = GetScreen();
		if(screen != null) {
			screen.UpdateDieButtons();
		}
	}

	public void OnShowMessage(NetworkMessage msg) {
		string text = msg.ReadMessage<StringMessage>().value;
		MessageBox msgBox = new MessageBox(text);
		msgBox.AddButton("Got it", () => game.GetOpenScreen().CloseMessageBox());
		game.GetOpenScreen().ShowMessageBox(msgBox);
	}

	public void OnRoll(NetworkMessage msg) {
		int[] rollIds = msg.ReadMessage<GameMsg.MsgIntegerArray>().array;
		Element[] rolls = new Element[rollIds.Length];
		for(int i = 0; i < rollIds.Length; i++) {
			rolls[i] = Element.All[rollIds[i]];
		}
		battle.rollsLeft -= 1;
		battle.rolls = rolls;
		for(int i = 0; i < battle.rolls.Length; i++) {
			int rollSoundIndex = Game.AUDIO_ROLL_MIN + Random.Range(0, Game.AUDIO_ROLL_COUNT);
			Game.PlaySound(rollSoundIndex);
		}
		BattleScreen screen = GetScreen();
		if(screen != null) {
			screen.UpdateContext();
		}
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
		Pawn caster = battle.GetCurrentPawn();
		GameMsg.MsgCastSpell actualMsg = msg.ReadMessage<GameMsg.MsgCastSpell>();
		Pawn target = null;
		if(actualMsg.targetId >= 0) {
			target = battle.GetPawn(actualMsg.targetId);
		}
		EventBus.CastSpellPre.Invoke(battle, caster, target, actualMsg.spellId);
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
		GetScreen().AddLog(battle.GetCurrentPawn().GetName() + " passes...");
	}
	
	public void OnPawnUpdate(NetworkMessage msg) {
		GameMsg.MsgPawn pawnMsg = msg.ReadMessage<GameMsg.MsgPawn>();
		Pawn newPawn = pawnMsg.pawn;
		Pawn oldPawn = battle.GetPawn(newPawn.GetId());
		EventBus.PawnUpdate.Invoke(battle, newPawn);
		battle.SetPawn(newPawn.GetId(), newPawn);
		BattleScreen screen = GetScreen();
		if(screen != null) {
			if(screen.spellPane.GetOpenTabIndex() == newPawn.GetId()) {
				screen.ViewSpellTab(newPawn.GetId());
			}
			screen.Rebuild();
		}
	}

	public void OnPawnAdd(NetworkMessage msg) {
		GameMsg.MsgPawn pawnMsg = msg.ReadMessage<GameMsg.MsgPawn>();
		battle.AddPawn(pawnMsg.pawn);
		BattleScreen screen = GetScreen();
		if(screen != null) {
			screen.Rebuild();
		}
	}

	public void OnPawnRemove(NetworkMessage msg) {
		GameMsg.MsgPawn pawnMsg = msg.ReadMessage<GameMsg.MsgPawn>();
		battle.RemovePawn(pawnMsg.pawn);
		BattleScreen screen = GetScreen();
		if(screen != null) {
			screen.Rebuild();
		}
	}

	public void OnAilmentUpdate(NetworkMessage msg) {
		GameMsg.MsgStatusList msgStatusList = msg.ReadMessage<GameMsg.MsgStatusList>();
		Pawn pawn = battle.GetPawn(msgStatusList.pawnId);
		pawn.SetStatuses(msgStatusList.statuses);
	}

	public void OnNextTurn(NetworkMessage msg) {
		int turn = msg.ReadMessage<IntegerMessage>().value;
		battle.currentTurn = turn;
		for(int i = 0; i < battle.rolls.Length; i++) {
			battle.rolls[i] = Element.None;
			battle.locks[i] = false;
		}
		BattleScreen screen = GetScreen();
		if(screen != null) {
			screen.ViewSpellTab(battle.currentTurn);
			screen.UpdateContext();
		}
	}

	public void OnMiss(NetworkMessage msg) {
		StringMessage actualMsg = msg.ReadMessage<StringMessage>();
		Spell spell = Spells.Get(actualMsg.value);
		GetScreen().AddLog(battle.GetCurrentPawn().GetName() + " tries to " + (spell.IsElement(battle.BuildContext(), Element.Physical) ? "use " : "cast ") + spell.GetName() + " but misses...");
	}

	public void OnEndBattle(NetworkMessage msg) {
		MessageBox msgBox = new MessageBox(msg.ReadMessage<StringMessage>().value);
		msgBox.AddButton("Continue", () => {
			Game.client.Send(GameMsg.Ready, new EmptyMessage());
			MessageBox waitMsgBox = new MessageBox("Waiting for other players...");
			game.GetOpenScreen().ShowMessageBox(waitMsgBox);
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
