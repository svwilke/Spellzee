using System;
using System.Collections.Generic;
using UnityEngine;

public class OptionTemplate {

	private string text;
	private string textLocked;

	private Func<List<Pawn>, int, bool> condition;

	public OptionTemplate(string text) : this(text, "", (pl, i) => true) {

	}

	public OptionTemplate(string text, Func<List<Pawn>, int, bool> condition) : this(text, null, condition){

	}

	public OptionTemplate(string text, string textLocked, Func<List<Pawn>, int, bool> condition) {
		this.text = text;
		this.textLocked = textLocked;
		this.condition = condition;
	}

	public Option Create(List<Pawn> players, int currentIndex) {
		Option option = new Option();
		if(condition.Invoke(players, currentIndex)) {
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
