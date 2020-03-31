using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Choice {

	private string description;
	private List<OptionTemplate> optionTemplates = new List<OptionTemplate>();
	private List<Action<ChoiceEncounter>> actions = new List<Action<ChoiceEncounter>>();
	private bool allPlayersRequired;

	public Choice(string desc, bool isAllPlayerChoice = false) {
		description = desc;
		allPlayersRequired = isAllPlayerChoice;
	}

	public bool AreAllPlayersRequired() {
		return allPlayersRequired;
	}

	public string GetDescription() {
		return description;
	}

	public Choice AddOption(OptionTemplate template, Action<ChoiceEncounter> action) {
		optionTemplates.Add(template);
		actions.Add(action);
		return this;
	}

	public Choice AddPass() {
		return AddOption(new OptionTemplate("Pass", e => e.GetPlayerCountNotDone() > 1), e => e.Pass());
	}

	public void Execute(ChoiceEncounter encounter, int actionIndex) {
		actions[actionIndex].Invoke(encounter);
	}

	public List<Option> GetOptions(ChoiceEncounter encounter) {
		List<Option> options = new List<Option>();
		for(int i = 0; i < optionTemplates.Count; i++) {
			OptionTemplate template = optionTemplates[i];
			Option o = template.Create(encounter);
			o.actionId = i;
			if(!o.isHidden) {
				options.Add(o);
			}
		}
		return options;
	}
}

public class Option {
	public bool isLocked = false;
	public bool isHidden = false;
	public string text = "";
	public int actionId = -1;

	public void Serialize(NetworkWriter writer) {
		writer.Write(isLocked);
		writer.Write(isHidden);
		writer.Write(text);
		writer.Write(actionId);
	}

	public void Deserialize(NetworkReader reader) {
		isLocked = reader.ReadBoolean();
		isHidden = reader.ReadBoolean();
		text = reader.ReadString();
		actionId = reader.ReadInt32();
	}

	public static Option DeserializeNew(NetworkReader reader) {
		Option o = new Option();
		o.Deserialize(reader);
		return o;
	}
}