using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Choice {

	private string description;
	private List<OptionTemplate> optionTemplates = new List<OptionTemplate>();
	private List<Action<List<Pawn>, int>> actions = new List<Action<List<Pawn>, int>>();

	public Choice(string desc) {
		description = desc;
	}

	public string GetDescription() {
		return description;
	}

	public Choice AddOption(OptionTemplate template, Action<List<Pawn>, int> action) {
		optionTemplates.Add(template);
		actions.Add(action);
		return this;
	}

	public void Execute(List<Pawn> players, int currentIndex, int actionIndex) {
		actions[actionIndex].Invoke(players, currentIndex);
	}

	public List<Option> GetOptions(List<Pawn> players, int currentIndex) {
		return optionTemplates.Select(ot => ot.Create(players, currentIndex)).ToList();
	}
}

public class Option {
	public bool isLocked = false;
	public bool isHidden = false;
	public string text = "";

	public void Serialize(NetworkWriter writer) {
		writer.Write(isLocked);
		writer.Write(isHidden);
		writer.Write(text);
	}

	public void Deserialize(NetworkReader reader) {
		isLocked = reader.ReadBoolean();
		isHidden = reader.ReadBoolean();
		text = reader.ReadString();
	}

	public static Option DeserializeNew(NetworkReader reader) {
		Option o = new Option();
		o.Deserialize(reader);
		return o;
	}
}