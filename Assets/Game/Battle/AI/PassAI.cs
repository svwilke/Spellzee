using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class PassAI : AIModule {

	public PassAI() : base(null) { }

	public override bool DoTurn(ServerBattle battle) {
		NetworkServer.SendToAll(GameMsg.Pass, new EmptyMessage());
		battle.NextTurn();
		return false;
	}
}
