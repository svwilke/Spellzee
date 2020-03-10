using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class ChoiceServerHandler : ServerHandler {

	private Game game;
	private Pawn[] pawns;
	private bool[] ready;
	private int level;

	public ChoiceServerHandler(Game game, Pawn[] pawns, int level) {
		this.game = game;
		this.pawns = pawns;
		this.level = level;
		this.ready = new bool[pawns.Length];
		AddHandler(GameMsg.BuySpell, OnBuyItem);
		AddHandler(GameMsg.DropSpell, OnAddSlot);
	}

	public void OnReady(int pawnId) {
		ready[pawnId] = true;
		for(int i = 0; i < pawns.Length; i++) {
			if(!ready[i]) {
				return;
			}
		}
		OpenVendor();
	}

	public void OnBuyItem(NetworkMessage msg) {
		int pawnId = msg.conn.connectionId;
		StringMessage message = msg.ReadMessage<StringMessage>();
		string equipId = message.value;
		pawns[pawnId].Equip(equipId);
		OnReady(pawnId);
	}

	public void OnAddSlot(NetworkMessage msg) {
		int pawnId = msg.conn.connectionId;
		pawns[pawnId].SpellSlotCount.AddModifier(new AttributeModifier("Level " + level, AttributeModifier.Operation.AddBase, 1));
		OnReady(pawnId);
	}

	public void OpenVendor() {
		for(int i = 0; i < pawns.Length; i++) {
			GameMsg.MsgPawn openVendorMsg = new GameMsg.MsgPawn() { pawn = pawns[i] };
			NetworkServer.SendToClient(i, GameMsg.OpenVendor, openVendorMsg);
		}
		game.OpenServerHandler(new VendorServerHandler(game, pawns));
	}

	public override void Open() {
		base.Open();
		for(int i = 0; i < pawns.Length; i++) {
			List<Equipment> buyable = new List<Equipment>(DB.BuyableEquipments);
			buyable.RemoveAll(pawns[i].HasEquipped);
			if(buyable.Count > 0) {
				int shop = Random.Range(0, buyable.Count);
				NetworkServer.SendToClient(i, GameMsg.ShopList, new StringMessage(buyable[shop].GetId()));
			}
		}
	}
}
