using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Sirenix.Serialization;

public class SpellData {

	[OdinSerialize] private Dictionary<string, string> strings = new Dictionary<string, string>();
	[OdinSerialize] private Dictionary<string, int> ints = new Dictionary<string, int>();
	[OdinSerialize] private Dictionary<string, bool> bools = new Dictionary<string, bool>();

	public void SetString(string key, string value) {
		strings[key] = value;
	}

	public string GetString(string key) {
		if(!HasString(key)) {
			return "";
		}
		return strings[key];
	}

	public bool HasString(string key) {
		return strings.ContainsKey(key);
	}

	public void SetInt(string key, int value) {
		ints[key] = value;
	}

	public int GetInt(string key) {
		if(!HasInt(key)) {
			return 0;
		}
		return ints[key];
	}

	public bool HasInt(string key) {
		return ints.ContainsKey(key);
	}

	public void SetBool(string key, bool value) {
		bools[key] = value;
	}

	public bool GetBool(string key) {
		if(!HasBool(key)) {
			return false;
		}
		return bools[key];
	}

	public bool HasBool(string key) {
		return bools.ContainsKey(key);
	}

	public void Serialize(NetworkWriter writer) {
		byte[] bytes = SerializationUtility.SerializeValue(this, DataFormat.Binary);
		writer.WriteBytesFull(bytes);
	}

	public static SpellData DeserializeNew(NetworkReader reader) {
		return SerializationUtility.DeserializeValue<SpellData>(reader.ReadBytesAndSize(), DataFormat.Binary);
	}
}
