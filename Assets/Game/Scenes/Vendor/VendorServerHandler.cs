using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class VendorServerHandler : ServerHandler {

	private Game game;
	private Pawn[] pawns;
	private bool[] ready;

	public VendorServerHandler(Game game, Pawn[] pawns) {
		this.game = game;
		this.pawns = pawns;
		this.ready = new bool[pawns.Length];
		AddHandler(GameMsg.Ready, OnReady);
		AddHandler(GameMsg.BuySpell, OnBuySpell);
		AddHandler(GameMsg.DropSpell, OnDropSpell);
		AddHandler(GameMsg.SwapSpells, OnSwapSpells);
	}

	public void OnReady(NetworkMessage msg) {
		int pawnId = msg.conn.connectionId;
		ready[pawnId] = true;
		for(int i = 0; i < pawns.Length; i++) {
			if(!ready[i]) {
				return;
			}
		}
		ServerBattle battle = new ServerBattle(game);
		foreach(Pawn pawn in pawns) {
			battle.AddPawn(pawn);
		}
		for(int i = 0; i < pawns.Length; i++) {
			pawns[i].OnSpellsChange.Invoke(null, pawns[i]);
			pawns[i].Heal(pawns[i].MaxHp);
		}
		Pawn enemy = game.CreateNextEnemy();
		battle.AddPawn(enemy);
		GameMsg.MsgStartBattle startBattleMsg = new GameMsg.MsgStartBattle() { battle = battle };
		NetworkServer.SendToAll(GameMsg.StartBattle, startBattleMsg);
		game.OpenServerHandler(new BattleServerHandler(game, battle));
	}

	public void OnBuySpell(NetworkMessage msg) {
		int pawnId = msg.conn.connectionId;
		StringMessage message = msg.ReadMessage<StringMessage>();
		string spellId = message.value;
		pawns[pawnId].AddSpell(spellId);
		msg.conn.Send(GameMsg.BuySpell, message);
	}

	public void OnDropSpell(NetworkMessage msg) {
		int pawnId = msg.conn.connectionId;
		StringMessage message = msg.ReadMessage<StringMessage>();
		string spellId = message.value;
		pawns[pawnId].RemoveSpell(spellId);
		msg.conn.Send(GameMsg.DropSpell, message);
	}

	public void OnSwapSpells(NetworkMessage msg) {
		GameMsg.MsgIntegerArray msgArray = msg.ReadMessage<GameMsg.MsgIntegerArray>();
		int id0 = msgArray.array[0];
		int id1 = msgArray.array[1];
		int pawnId = msg.conn.connectionId;
		Pawn pawn = pawns[pawnId];
		List<string> spells = pawn.GetKnownSpellIdsMutable();
		string temp = spells[id0];
		spells[id0] = spells[id1];
		spells[id1] = temp;
	}

	public override void Open() {
		base.Open();
		int shopAmount = 1;
		for(int i = 0; i < pawns.Length; i++) {
			List<string> buyableSpells = Spells.GetCastableSpells().Select(spell => spell.GetId()).ToList();
			buyableSpells.RemoveAll(pawns[i].DoesKnowSpell);
			string[] shop = REX.Choice(buyableSpells, shopAmount);
			NetworkServer.SendToClient(i, GameMsg.ShopList, new GameMsg.MsgStringArray(shop));
		}
	}
}
