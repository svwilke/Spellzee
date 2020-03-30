using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ChoiceEncounter : Encounter
{
	private List<Choice> choices = new List<Choice>();
	private int choiceIndex;
	private int playerIndex;

	public ChoiceEncounter(params Choice[] choices) : this((IEnumerable<Choice>)choices) {
		
	}

	public ChoiceEncounter(IEnumerable<Choice> choices) {
		this.choices = choices.ToList();
		choiceIndex = 0;
	}

	protected override void OnEncounterBegin() {
		playerIndex = Random.Range(0, players.Count);
		game.OpenServerHandler(new ChoiceServerHandler(game, this));
		ShowOptions();
	}

	protected override void OnEncounterEnd() {
		
	}

	private void ShowOptions() {
		NetworkServer.SendToAll(GameMsg.OpenChoice, new GameMsg.MsgOptionList() { currentIndex = playerIndex, description = GetCurrentChoice().GetDescription(), options = GetCurrentChoice().GetOptions(players, playerIndex) });
	}

	public void Next() {
		choiceIndex++;
		if(choiceIndex >= choices.Count) {
			End();
		} else {
			playerIndex = Random.Range(0, players.Count);
			ShowOptions();
		}
	}

	public void Execute(int option) {
		GetCurrentChoice().Execute(players, playerIndex, option);
		Next();
	}

	public Choice GetCurrentChoice() {
		return choices[choiceIndex];
	}
}
