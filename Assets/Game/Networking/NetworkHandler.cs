using System.Collections.Generic;
using UnityEngine.Networking;

public abstract class NetworkHandler {

	protected Dictionary<short, NetworkMessageDelegate> handlers = new Dictionary<short, NetworkMessageDelegate>();

	public void AddHandler(short msgId, NetworkMessageDelegate handler) {
		handlers.Add(msgId, handler);
	}

	public abstract void Open();
	public abstract void Close();
}
