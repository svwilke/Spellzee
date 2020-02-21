using System.Collections.Generic;
using UnityEngine.Networking;

public class ServerHandler : NetworkHandler
{

	public override void Open() {
		foreach(KeyValuePair<short, NetworkMessageDelegate> handler in handlers) {
			NetworkServer.RegisterHandler(handler.Key, handler.Value);
		}
	}

	public override void Close() {
		foreach(short msgId in handlers.Keys) {
			NetworkServer.UnregisterHandler(msgId);
		}
	}
}
