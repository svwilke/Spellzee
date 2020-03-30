using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class ChoiceServerHandler : EncounterServerHandler {

	public ChoiceServerHandler(Game game, ChoiceEncounter encounter) : base(game, encounter) {
		AddHandler(GameMsg.SelectChoice, OnChoiceSelected);
	}

	public void OnChoiceSelected(NetworkMessage msg) {
		int option = msg.ReadMessage<IntegerMessage>().value;
		(encounter as ChoiceEncounter).Execute(option);
	}
	

}
