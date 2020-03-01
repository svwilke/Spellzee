public class RegistryEntry<T> where T : RegistryEntry<T> {

	private string id;

	public T SetId(string id) {
		this.id = id;
		return (T)this;
	}

	public T GetRegistryObject() {
		return (T)this;
	}

	public string GetId() {
		return id;
	}
}
