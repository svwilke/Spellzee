using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ChoiceEncounter : Encounter
{
	private List<Choice> choices = new List<Choice>();
	private int choiceIndex;
	private int playerIndex;
	private bool[] playersDone;

	public ChoiceEncounter(params Choice[] choices) : this((IEnumerable<Choice>)choices) {
		
	}

	public ChoiceEncounter(IEnumerable<Choice> choices) {
		this.choices = choices.ToList();
		choiceIndex = 0;
	}

	protected override void OnEncounterBegin() {
		playerIndex = Random.Range(0, players.Count);
		playersDone = new bool[players.Count];
		game.OpenServerHandler(new ChoiceServerHandler(game, this));
		ShowOptions();
	}

	protected override void OnEncounterEnd() {
		
	}

	private void ShowOptions() {
		NetworkServer.SendToAll(GameMsg.OpenChoice, new GameMsg.MsgOptionList() { currentIndex = playerIndex, description = GetCurrentChoice().GetDescription(), options = GetCurrentChoice().GetOptions(this) });
	}

	public List<Pawn> GetPlayers() {
		return players;
	}

	public Pawn GetCurrentPlayer() {
		return players[playerIndex];
	}

	public int GetCurrentPlayerIndex() {
		return playerIndex;
	}

	public int GetPlayerCountNotDone() {
		return playersDone.Where(b => !b).Count();
	}

	public void Next() {
		choiceIndex++;
		if(choiceIndex >= choices.Count) {
			End();
		} else {
			playerIndex = Random.Range(0, players.Count);
			playersDone = new bool[players.Count];
			ShowOptions();
		}
	}

	public void Pass() {
		int index = (playerIndex + 1) % players.Count;
		while(index != playerIndex && playersDone[index]) {
			index = (index + 1) % players.Count;
		}
		playerIndex = index;
		ShowOptions();
	}

	public void Done() {
		playersDone[playerIndex] = true;
		if(GetCurrentChoice().AreAllPlayersRequired()) {
			if(GetPlayerCountNotDone() == 0) {
				Next();
			} else {
				Pass();
			}
		} else {
			Next();
		}
	}

	public void Execute(int option) {
		Choice choice = GetCurrentChoice();
		choice.Execute(this, option);
	}

	public Choice GetCurrentChoice() {
		return choices[choiceIndex];
	}
}
