using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class ChoiceServerHandler : EncounterServerHandler {

	private Pawn[] pawns;
	private int level;

	public ChoiceServerHandler(Game game, Encounter encounter, Pawn[] pawns, int level) : base(game, encounter) {
		this.pawns = pawns;
		this.level = level;
		AddHandler(GameMsg.BuySpell, OnBuyItem);
		AddHandler(GameMsg.DropSpell, OnAddSlot);
	}

	public void OnBuyItem(NetworkMessage msg) {
		int pawnId = msg.conn.connectionId;
		StringMessage message = msg.ReadMessage<StringMessage>();
		string equipId = message.value;
		pawns[pawnId].Equip(equipId);
		ReadyPlayer(pawnId);
	}

	public void OnAddSlot(NetworkMessage msg) {
		int pawnId = msg.conn.connectionId;
		pawns[pawnId].SpellSlotCount.AddModifier(new AttributeModifier("Level " + level, AttributeModifier.Operation.AddBase, 1));
		ReadyPlayer(pawnId);
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
