using System.Collections;
using System.Collections.Generic;

public class Registry<T> : IEnumerable<T> where T : RegistryEntry<T>
{

	private Dictionary<string, T> entries = new Dictionary<string, T>();
	private string localizationPrefix = "";

	public Registry(string localizationPrefix = "") {
		this.localizationPrefix = localizationPrefix;
	}

	public T Get(string id) {
		if(!entries.ContainsKey(id)) {
			throw new System.Exception("Registry object not found with id " + id + ".");
		}
		return entries[id];
	}

	public void Register(RegistryEntry<T> entry) {
		string id = entry.GetId();
		if(entries.ContainsKey(id)) {
			throw new System.Exception("Duplicate registry entry with id " + id + ".");
		}
		entry.SetUnlocalizedString(localizationPrefix + "." + id);
		entries.Add(id, entry.GetRegistryObject());
	}

	public IEnumerator<T> GetEnumerator() {
		return entries.Values.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return entries.Values.GetEnumerator();
	}
}
