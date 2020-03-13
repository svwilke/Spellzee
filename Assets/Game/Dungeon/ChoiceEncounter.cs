using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class ChoiceEncounter : Encounter {

	public ChoiceEncounter() {

	}

	protected override void OnEncounterBegin() {
		foreach(Pawn p in players) {
			GameMsg.MsgPawn openChoiceMsg = new GameMsg.MsgPawn() { pawn = p };
			NetworkServer.SendToClient(p.GetId(), GameMsg.OpenChoice, openChoiceMsg);
		}
		game.OpenServerHandler(new ChoiceServerHandler(game, this, players.ToArray(), 0));
	}

	protected override void OnEncounterEnd() {
		
	}
}
