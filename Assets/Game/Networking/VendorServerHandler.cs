using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
		battle.allies = pawns;
		battle.enemy = game.CreateNextEnemy();
		battle.enemy.SetId(battle.allies.Length);
		GameMsg.MsgStartBattle startBattleMsg = new GameMsg.MsgStartBattle() { battle = battle };
		NetworkServer.SendToAll(GameMsg.StartBattle, startBattleMsg);
		game.OpenServerHandler(new BattleServerHandler(game, battle));
	}

	public void OnBuySpell(NetworkMessage msg) {
		int pawnId = msg.conn.connectionId;
		IntegerMessage message = msg.ReadMessage<IntegerMessage>();
		int spellId = message.value;
		pawns[pawnId].AddSpell(spellId);
		msg.conn.Send(GameMsg.BuySpell, message);
	}

	public void OnDropSpell(NetworkMessage msg) {
		int pawnId = msg.conn.connectionId;
		IntegerMessage message = msg.ReadMessage<IntegerMessage>();
		int spellId = message.value;
		pawns[pawnId].RemoveSpell(spellId);
		msg.conn.Send(GameMsg.DropSpell, message);
	}

	public override void Open() {
		base.Open();
		int shopAmount = 1;
		for(int i = 0; i < pawns.Length; i++) {
			int[] shop = new int[shopAmount];
			for(int j = 0; j < shop.Length; j++) {
				shop[j] = REX.Choice(DB.BuyableSpells);
			}
			NetworkServer.SendToClient(i, GameMsg.ShopList, new GameMsg.MsgIntegerArray(shop));
		}
	}
}
