using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PawnTemplates {

	private static Registry<PawnTemplate> Registry = new Registry<PawnTemplate>();

	public static PawnTemplate Wizard = Register("wizard", new PawnTemplate("Wizard", 16, PawnProgressions.Wizard).AddSpells(Spells.Fireball, Spells.Whirlwind, Spells.Root, Spells.Revitalize).SetAffinities(15, 15, 15, 15, 0, 0, 0));
	public static PawnTemplate Sorcerer = Register("sorcerer", new PawnTemplate("Sorcerer", 16, PawnProgressions.Sorcerer).AddSpells(Spells.Gust, Spells.Root, Spells.HolyLight, Spells.Flash));
	public static PawnTemplate Mage = Register("mage", new WildMageTemplate("Mage", 16, PawnProgressions.Mage));
	public static PawnTemplate Druid = Register("druid", new PawnTemplate("Druid", 18, PawnProgressions.Druid).AddSpells(Spells.Whirlwind, Spells.ThrowStone, Spells.MendingHerbs, Spells.Revitalize));
	
	public static PawnTemplate Dummy = Register("dummy", new PawnTemplate("Dummy", 10, PawnProgressions.GenericEnemy).ZeroAffinites().SetAffinity(Element.None, 10).AddSpells(Spells.Fireball));
	
	public static PawnTemplate Sloth = Register("sloth", new PawnTemplate("Sloth", 4, PawnProgressions.GenericEnemy).ZeroAffinites().SetAffinity(Element.Physical, 10).SetAffinity(Element.None, 90).AddSpells(Spells.Bite));
	public static PawnTemplate Pixie = Register("pixie", new PawnTemplate("Pixie", 4, PawnProgressions.GenericEnemy).ZeroAffinites().SetAffinity(Element.Physical, 10).SetAffinity(Element.None, 10).AddSpells(Spells.Cuteness));
	public static PawnTemplate SewerRat = Register("sewer_rat", new PawnTemplate("Sewer Rat", 5, PawnProgressions.GenericEnemy).ZeroAffinites().SetAffinity(Element.Physical, 10).SetAffinity(Element.None, 10).AddSpells(Spells.Bite));
	public static PawnTemplate FieryBat = Register("fiery_bat", new PawnTemplate("Fiery Bat", 6, PawnProgressions.GenericEnemy).ZeroAffinites().SetAffinity(Element.Fire, 10).SetAffinity(Element.Physical, 10).SetAffinity(Element.None, 10).AddSpells(Spells.Ignite, Spells.Bite));
	public static PawnTemplate Wolf = Register("wolf", new PawnTemplate("Wolf", 6, PawnProgressions.GenericEnemy).ZeroAffinites().SetAffinity(Element.Physical, 10).SetAffinity(Element.None, 10).AddSpells(Spells.Claw));
	public static PawnTemplate Cutpurse = Register("cutpurse", new PawnTemplate("Cutpursh", 8, PawnProgressions.GenericEnemy).ZeroAffinites().SetAffinity(Element.Physical, 10).SetAffinity(Element.None, 10).AddSpells(Spells.Slice).SetXPGain(2));
	public static PawnTemplate Golem = Register("golem", new PawnTemplate("Golem", 10, PawnProgressions.GenericEnemy).ZeroAffinites().SetAffinity(Element.Physical, 15).SetAffinity(Element.None, 5).AddSpells(Spells.Stomp).SetXPGain(3));
	public static PawnTemplate RatKing = Register("rat_king", new PawnTemplate("Rat King", 14, PawnProgressions.GenericEnemy).ZeroAffinites().SetAffinity(Element.Earth, 20).SetAffinity(Element.Water, 20).SetAffinity(Element.Physical, 20).AddSpells(Spells.Root, Spells.AquaticBlast, Spells.LeadThePack).SetXPGain(3));

	public static PawnTemplate Rasputin = Register("rasputin", new PawnTemplate("Rasputin", 9, PawnProgressions.GenericEnemy).ZeroAffinites().SetAffinity(Element.Physical, 10).SetAffinity(Element.None, 15).SetAffinity(Element.Dark, 5).AddSpells(Spells.Cuteness, Spells.ConsumingDarkness, Spells.Claw));
	
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
