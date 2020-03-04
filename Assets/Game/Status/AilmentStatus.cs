using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;

public class AilmentStatus : Status {

	[OdinSerialize] private int value;
	[OdinSerialize] private string shortName;
	[OdinSerialize] private string fullName;

	public AilmentStatus(StatusType type, string fullName, string shortName, Color color, int value) : base(type) {
		this.value = value;
		this.shortName = shortName;
		this.fullName = fullName;
		SetVisible(true);
		SetColor(color);
	}

	public string GetFullName() {
		return fullName;
	}

	public int GetValue() {
		return value;
	}

	public void SetValue(int newValue) {
		if(newValue <= 0) {
			pawn.CmdRemoveStatus(this);
		} else {
			int oldValue = value;
			value = newValue;
			OnValueUpdated(oldValue, newValue);
			pawn.CmdUpdateStatuses();
		}
	}

	protected virtual void OnValueUpdated(int oldValue, int newValue) {
		
	}

	public override bool Merge(Status status) {
		value += (status as AilmentStatus).value;
		return true;
	}

	public override string GetDisplayText() {
		return shortName + " " + value;
	}
}
