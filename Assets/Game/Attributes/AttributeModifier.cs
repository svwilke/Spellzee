using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AttributeModifier {

	private string name;
	private Operation op;
	private double modifier;

	public AttributeModifier(string name, Operation op, double modifier) {
		this.name = name;
		this.op = op;
		this.modifier = modifier;
	}

	public string GetName() {
		return name;
	}

	public double Apply(double currentValue, double baseValue) {
		switch(op) {
			case Operation.AddBase:
			case Operation.AddTotal:
				return currentValue + modifier;
			case Operation.SubtractBase:
			case Operation.SubtractTotal:
				return currentValue - modifier;
			case Operation.MultiplyBaseAndAdd:
				return currentValue + (baseValue * modifier);
			case Operation.MultiplyTotal:
				return currentValue * modifier;
			case Operation.Set:
				return modifier;
		}
		return currentValue;
	}

	public int GetPriority() {
		switch(op) {
			case Operation.AddBase:
			case Operation.SubtractBase:
				return 0;
			case Operation.MultiplyBaseAndAdd:
				return 1;
			case Operation.MultiplyTotal:
				return 2;
			case Operation.AddTotal:
			case Operation.SubtractTotal:
				return 3;
			case Operation.Set:
				return 4;
		}
		return -1;
	}

	public void Serialize(NetworkWriter writer) {
		writer.Write(name);
		writer.Write((byte)op);
		writer.Write(modifier);
	}

	public void Deserialize(NetworkReader reader) {
		name = reader.ReadString();
		op = (Operation)reader.ReadByte();
		modifier = reader.ReadDouble();
	}

	public static AttributeModifier DeserializeNew(NetworkReader reader) {
		AttributeModifier mod = new AttributeModifier("", Operation.AddBase, 0);
		mod.Deserialize(reader);
		return mod;
	}

	public const int MaxPriority = 4;

	public enum Operation {
		AddBase, SubtractBase, MultiplyBaseAndAdd, MultiplyTotal, AddTotal, SubtractTotal, Set
	}
}
