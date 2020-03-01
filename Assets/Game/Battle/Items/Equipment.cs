using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Equipment : RegistryEntry<Equipment> {

	private string name;
	protected string description;

	public Equipment(string name, string description) {
		this.name = name;
		this.description = description;
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
