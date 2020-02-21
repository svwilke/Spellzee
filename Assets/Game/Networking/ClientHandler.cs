using System.Collections.Generic;
using UnityEngine.Networking;

public class ClientHandler : NetworkHandler
{

	public override void Open() {
		foreach(KeyValuePair<short, NetworkMessageDelegate> handler in handlers) {
			Game.client.RegisterHandler(handler.Key, handler.Value);
		}
	}

	public override void Close() {
		foreach(short msgId in handlers.Keys) {
			Game.client.UnregisterHandler(msgId);
		}
	}
}
