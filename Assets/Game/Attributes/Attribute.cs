using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Attribute {

	private Dictionary<int, HashSet<AttributeModifier>> modifiers;

	private double cachedBase;
	private double cachedValue;
	private bool isDirty = false;

	private double defaultBase;
	private bool hasDefaultBase;

	public int Size { get { return modifierCount; } }

	private int modifierCount = 0;

	public Attribute(params AttributeModifier[] modifiers) {
		Clear();
		foreach(AttributeModifier modifier in modifiers) {
			AddModifier(modifier);
		}
	}

	public void Clear() {
		this.modifiers = new Dictionary<int, HashSet<AttributeModifier>>();
		for(int i = 0; i <= AttributeModifier.MaxPriority; i++) {
			this.modifiers.Add(i, new HashSet<AttributeModifier>());
		}
		modifierCount = 0;
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
			foreach(AttributeModifier mod in modifiers[prio]) {
				val = mod.Apply(val, baseValue);
			}
		}
		cachedValue = val;
		cachedBase = baseValue;
		return val;
	}

	public void AddModifier(AttributeModifier mod) {
		modifiers[mod.GetPriority()].Add(mod);
		isDirty = true;
		modifierCount += 1;
	}

	public void RemoveModifier(AttributeModifier mod) {
		if(modifiers[mod.GetPriority()].Contains(mod)) {
			modifiers[mod.GetPriority()].Remove(mod);
			modifierCount -= 1;
		}
	}

	public void RemoveModifier(string modName) {
		foreach(HashSet<AttributeModifier> mods in modifiers.Values) {
			HashSet<AttributeModifier> remove = new HashSet<AttributeModifier>();
			foreach(AttributeModifier mod in mods) {
				if(mod.GetName().Equals(modName)) {
					remove.Add(mod);
				}
			}
			mods.RemoveWhere(remove.Contains);
			modifierCount -= remove.Count;
		}
		isDirty = true;
	}

	public void Serialize(NetworkWriter writer) {
		writer.Write(Size);
		foreach(HashSet<AttributeModifier> mods in modifiers.Values) {
			foreach(AttributeModifier mod in mods) {
				mod.Serialize(writer);
			}
		}
	}

	public void Deserialize(NetworkReader reader) {
		Clear();
		int count = reader.ReadInt32();
		for(int i = 0; i < count; i++) {
			AttributeModifier mod = AttributeModifier.DeserializeNew(reader);
			AddModifier(mod);
		}
	}

	public static Attribute DeserializeNew(NetworkReader reader) {
		Attribute attr = new Attribute();
		attr.Deserialize(reader);
		return attr;
	}

	public static Attribute Clone(Attribute other) {
		Attribute attr = new Attribute();
		foreach(HashSet<AttributeModifier> mods in other.modifiers.Values) {
			foreach(AttributeModifier mod in mods) {
				attr.AddModifier(mod);
			}
		}
		return attr;
	}
}
