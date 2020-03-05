using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Locale {

	private string locale;
	private Dictionary<string, string> translationMap = new Dictionary<string, string>();

	public Locale(string localeId) {
		locale = localeId;
		Load();
	}

	public string Translate(string translationKey) {
		Debug.Log("Translating: " + translationKey);
		if(!translationMap.ContainsKey(translationKey)) {
			return translationKey;
		}
		return translationMap[translationKey];
	}

	private void Load() {
		TextAsset langFile = Resources.Load<TextAsset>(locale);
		if(langFile == null) {
			Debug.LogError("Could not load language file: " + locale + ".json");
		} else {
			translationMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(langFile.text);
		}
	}
}
