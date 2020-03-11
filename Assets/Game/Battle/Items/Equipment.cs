using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : RegistryEntry<Equipment> {

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

	public virtual void OnEquipped(Pawn pawn) { }
	public virtual void OnUnequipped(Pawn pawn) { }
}
