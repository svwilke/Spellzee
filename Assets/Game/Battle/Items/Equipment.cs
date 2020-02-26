using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Equipment {

	private int id;
	private string name;
	protected string description;

	public Equipment(int id, string name, string description) {
		this.id = id;
		this.name = name;
		this.description = description;
	}

	public int GetId() {
		return id;
	}

	public string GetName() {
		return name;
	}

	public string GetDescription() {
		return description;
	}

	public abstract void OnEquipped(Pawn pawn);
	public abstract void OnUnequipped(Pawn pawn);
}
