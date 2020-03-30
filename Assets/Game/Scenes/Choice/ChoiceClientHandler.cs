using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class ChoiceClientHandler : EncounterClientHandler {

	private ChoiceBox choiceBox;

	public ChoiceClientHandler(Game game, ChoiceBox choiceBox) : base(game) {
		this.game = game;
		this.choiceBox = choiceBox;
		AddHandler(GameMsg.OpenChoice, OnUpdateChoice);
	}

	public void OnUpdateChoice(NetworkMessage msg) {
		GameMsg.MsgOptionList msgOptionList = msg.ReadMessage<GameMsg.MsgOptionList>();
		choiceBox.UpdateOptions(msgOptionList.options, msgOptionList.currentIndex == Game.peerId);
	}
}
