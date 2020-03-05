public class RegistryEntry<T> where T : RegistryEntry<T> {

	private string id;
	private string unlocalizedString;

	public T SetId(string id) {
		this.id = id;
		return (T)this;
	}

	public void SetUnlocalizedString(string fullUnlocalizedString) {
		unlocalizedString = fullUnlocalizedString;
	}

	public string GetUnlocalizedString() {
		return unlocalizedString;
	}

	public string GetLocalizedString(string suffix = "") {
		string key = GetUnlocalizedString();
		if(suffix != null && suffix.Length > 0) {
			key += "." + suffix;
		}
		return Game.locale.Translate(key);
	}

	public T GetRegistryObject() {
		return (T)this;
	}

	public string GetId() {
		return id;
	}
}
