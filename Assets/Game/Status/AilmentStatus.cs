using System;
using UnityEngine;
using Sirenix.Serialization;

public class AilmentStatus : Status {

	[OdinSerialize] private int value;
	[OdinSerialize] private string spriteName;
	[OdinSerialize] private string fullName;

	[OdinSerialize] protected string description;

	public AilmentStatus(StatusType type, string fullName, string spriteName, Color color, int value) : base(type) {
		this.value = value;
		this.spriteName = spriteName;
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

	public override int Render(int x, int y) {
		RB.DrawSprite(spriteName, new Vector2i(x, y));
		int w = RB.PrintMeasure(GetValue().ToString()).width;
		RB.Print(new Vector2i(x + 9, y), Color.black, GetValue().ToString());
		return 8 + w + 3;
	}

	public override string GetDescription() {
		if(description != null) {
			return string.Format(description, fullName, GetValue());
		} else {
			return base.GetDescription();
		}
	}
}
