using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTemplates {

	private static Registry<PlayerTemplate> Registry = new Registry<PlayerTemplate>();

	public static PlayerTemplate Wizard = Register("wizard", new PlayerTemplate("Wizard").SetMaxHp(16).AddSpells(Spells.Fireball, Spells.Whirlwind, Spells.Root, Spells.Revitalize));
	public static PlayerTemplate Sorcerer = Register("sorcerer", new PlayerTemplate("Sorcerer").SetMaxHp(16).AddSpells(Spells.Gust, Spells.Root, Spells.HolyLight, Spells.Flash));
	public static PlayerTemplate Mage = Register("mage", new WildMageTemplate("Mage").SetMaxHp(16));
	public static PlayerTemplate Druid = Register("druid", new PlayerTemplate("Druid").SetMaxHp(16).AddSpells(Spells.Whirlwind, Spells.ThrowStone, Spells.MendingHerbs, Spells.Revitalize));

	public static PlayerTemplate Register(string id, PlayerTemplate playerTemplate) {
		playerTemplate.SetId(id);
		Registry.Register(playerTemplate);
		return playerTemplate;
	}

	public static PlayerTemplate Get(string id) {
		return Registry.Get(id);
	}

	public static PlayerTemplate[] GetPlayableClasses() {
		return Registry.ToArray();
	}
}
