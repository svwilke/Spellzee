using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PawnTemplates {

	private static Registry<PawnTemplate> Registry = new Registry<PawnTemplate>();

	public static PawnTemplate Wizard = Register("wizard", new PawnTemplate("Wizard").SetMaxHp(16).AddSpells(Spells.Fireball, Spells.Whirlwind, Spells.Root, Spells.Revitalize).SetAffinities(15, 15, 15, 15, 0, 0, 0));
	public static PawnTemplate Sorcerer = Register("sorcerer", new PawnTemplate("Sorcerer").SetMaxHp(16).AddSpells(Spells.Gust, Spells.Root, Spells.HolyLight, Spells.Flash));
	public static PawnTemplate Mage = Register("mage", new WildMageTemplate("Mage").SetMaxHp(16));
	public static PawnTemplate Druid = Register("druid", new PawnTemplate("Druid").SetMaxHp(16).AddSpells(Spells.Whirlwind, Spells.ThrowStone, Spells.MendingHerbs, Spells.Revitalize));

	public static PawnTemplate Sloth = Register("sloth", new PawnTemplate("Sloth").SetMaxHp(4).ZeroAffinites().SetAffinity(Element.Physical, 10).SetAffinity(Element.None, 90).AddSpells(Spells.Bite));
	public static PawnTemplate Pixie = Register("pixie", new PawnTemplate("Pixie").SetMaxHp(4).ZeroAffinites().SetAffinity(Element.Physical, 10).SetAffinity(Element.None, 10).AddSpells(Spells.Cuteness));
	public static PawnTemplate SewerRat = Register("sewer_rat", new PawnTemplate("Sewer Rat").SetMaxHp(5).ZeroAffinites().SetAffinity(Element.Physical, 10).SetAffinity(Element.None, 10).AddSpells(Spells.Bite));
	public static PawnTemplate FieryBat = Register("fiery_bat", new PawnTemplate("Fiery Bat").SetMaxHp(6).ZeroAffinites().SetAffinity(Element.Fire, 10).SetAffinity(Element.Physical, 10).SetAffinity(Element.None, 10).AddSpells(Spells.Ignite, Spells.Bite));
	public static PawnTemplate Wolf = Register("wolf", new PawnTemplate("Wolf").SetMaxHp(6).ZeroAffinites().SetAffinity(Element.Physical, 10).SetAffinity(Element.None, 10).AddSpells(Spells.Claw));
	public static PawnTemplate Cutpurse = Register("cutpurse", new PawnTemplate("Cutpursh").SetMaxHp(8).ZeroAffinites().SetAffinity(Element.Physical, 10).SetAffinity(Element.None, 10).AddSpells(Spells.Slice));
	public static PawnTemplate Golem = Register("golem", new PawnTemplate("Golem").SetMaxHp(10).ZeroAffinites().SetAffinity(Element.Physical, 15).SetAffinity(Element.None, 5).AddSpells(Spells.Stomp));

	public static PawnTemplate Rasputin = Register("rasputin", new PawnTemplate("Rasputin").SetMaxHp(9).ZeroAffinites().SetAffinity(Element.Physical, 10).SetAffinity(Element.None, 15).SetAffinity(Element.Dark, 5).AddSpells(Spells.Cuteness, Spells.ConsumingDarkness, Spells.Claw));
	
	public static PawnTemplate Register(string id, PawnTemplate playerTemplate) {
		playerTemplate.SetId(id);
		Registry.Register(playerTemplate);
		return playerTemplate;
	}

	public static PawnTemplate Get(string id) {
		return Registry.Get(id);
	}

	public static PawnTemplate[] GetPlayableClasses() {
		return new PawnTemplate[] { Wizard, Sorcerer, Mage, Druid };
	}
}
