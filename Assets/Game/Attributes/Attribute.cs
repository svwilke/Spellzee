using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Attribute {

	private Dictionary<string, AttributeModifier> modifiers;

	private double cachedBase;
	private double cachedValue;
	private bool isDirty = false;

	private double defaultBase;
	private bool hasDefaultBase;

	public int Size { get { return modifiers.Count; } }

	public Attribute(params AttributeModifier[] modifiers) {
		this.modifiers = new Dictionary<string, AttributeModifier>();
		foreach(AttributeModifier modifier in modifiers) {
			AddModifier(modifier);
		}
	}

	public Attribute SetBaseValue(double value) {
		defaultBase = value;
		hasDefaultBase = true;
		return this;
	}

	public double GetValue() {
		if(!hasDefaultBase) {
			throw new System.Exception("GetValue() was invoked without a base value having been set on an Attribute.");
		}
		return GetValue(defaultBase);
	}

	public double GetValue(double baseValue) {
		if(cachedBase != baseValue) {
			isDirty = true;
		}
		if(!isDirty) {
			return cachedValue;
		}
		double val = baseValue;
		for(int prio = 0; prio <= AttributeModifier.MaxPriority; prio++) {
			foreach(AttributeModifier mod in modifiers.Values) {
				if(mod.GetPriority() == prio) {
					val = mod.Apply(val, baseValue);
				}
			}
		}
		cachedValue = val;
		cachedBase = baseValue;
		return val;
	}

	public void AddModifier(AttributeModifier mod) {
		modifiers[mod.GetName()] = mod;
		isDirty = true;
	}

	public void RemoveModifier(AttributeModifier mod) {
		RemoveModifier(mod.GetName());
	}

	public void RemoveModifier(string modName) {
		modifiers.Remove(modName);
		isDirty = true;
	}

	public void Serialize(NetworkWriter writer) {
		writer.Write(modifiers.Count);
		foreach(AttributeModifier mod in modifiers.Values) {
			mod.Serialize(writer);
		}
	}

	public void Deserialize(NetworkReader reader) {
		modifiers = new Dictionary<string, AttributeModifier>();
		int count = reader.ReadInt32();
		for(int i = 0; i < count; i++) {
			AddModifier(AttributeModifier.DeserializeNew(reader));
		}
	}

	public static Attribute DeserializeNew(NetworkReader reader) {
		Attribute attr = new Attribute();
		attr.Deserialize(reader);
		return attr;
	}

	public static Attribute Clone(Attribute other) {
		Attribute attr = new Attribute();
		foreach(AttributeModifier mod in other.modifiers.Values) {
			attr.AddModifier(mod);
		}
		return attr;
	}
}
