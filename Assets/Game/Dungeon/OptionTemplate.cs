using System;
using System.Collections.Generic;
using UnityEngine;

public class OptionTemplate {

	private string text;
	private string textLocked;

	private Func<ChoiceEncounter, bool> condition;

	public OptionTemplate(string text) : this(text, "", (e) => true) {

	}

	public OptionTemplate(string text, Func<ChoiceEncounter, bool> condition) : this(text, null, condition){

	}

	public OptionTemplate(string text, string textLocked, Func<ChoiceEncounter, bool> condition) {
		this.text = text;
		this.textLocked = textLocked;
		this.condition = condition;
	}

	public Option Create(ChoiceEncounter encounter) {
		Option option = new Option();
		if(condition.Invoke(encounter)) {
			option.isLocked = false;
			option.isHidden = false;
			option.text = text;
		} else {
			option.isLocked = textLocked != null;
			option.isHidden = textLocked == null;
			option.text = textLocked ?? "";
		}
		return option;
	}
}
