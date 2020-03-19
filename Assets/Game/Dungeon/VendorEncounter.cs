using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class VendorEncounter : Encounter {

	public VendorEncounter() {

	}

	protected override void OnEncounterBegin() {
		foreach(Pawn p in players) {
			GameMsg.MsgPawn openVendorMsg = new GameMsg.MsgPawn() { pawn = p };
			NetworkServer.SendToClient(p.GetId(), GameMsg.OpenVendor, openVendorMsg);
		}
		game.OpenServerHandler(new VendorServerHandler(game, this, players.ToArray()));
	}

	protected override void OnEncounterEnd() {
		players.ForEach(p => p.OnSpellsChange.Invoke(null, p));
	}
}
