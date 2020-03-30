using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class ItemSlotEncounter : Encounter {

	public ItemSlotEncounter() {

	}

	protected override void OnEncounterBegin() {
		foreach(Pawn p in players) {
			GameMsg.MsgPawn openChoiceMsg = new GameMsg.MsgPawn() { pawn = p };
			NetworkServer.SendToClient(p.GetId(), GameMsg.OpenItemSlot, openChoiceMsg);
		}
		game.OpenServerHandler(new ItemSlotServerHandler(game, this, players.ToArray(), 0));
	}

	protected override void OnEncounterEnd() {
		
	}
}
