using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameBinding : MonoBehaviour {

	public NetworkManager networkManager;

	void Awake() {
		Game game = new Game(this);
		RB.Initialize(game);
	}
}
