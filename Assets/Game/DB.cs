﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DB {

	public static string[] CharNames = new string[] { "Mathil", "Corex", "Wrathion", "Tor'Shul", "Dillo", "Hinnex", "Jaenna", "Logana", "Zalapha", "Kalakarr" };

	public static string[] EnemyNames = new string[] { "Rat", "Pixie", "Bat", "Wolf", "Cutpursh", "Golem" };
	public static int[] EnemyHPs = new int[] { 4, 4, 5, 7, 8, 10 };
	public static int[] EnemySpells = new int[] { 7, 9, 7, 6, 10, 8 };

	public static EnemyTemplate[] Enemies = new EnemyTemplate[] {
		//new EnemyTemplate("Text").SetMaxHp(8).AddSpells(7).SetTurnWeights(0F, 0F, 6F)
		new EnemyTemplate("Sloth").SetMaxHp(3, 6).AddSpells(Spells.Bite).SetTurnWeights(1F, 0F, 6F),
		new EnemyTemplate("Pixie").SetMaxHp(4).AddSpells(Spells.Cuteness),
		new EnemyTemplate("Rat").SetMaxHp(4).AddSpells(Spells.Bite),
		new EnemyTemplate("Cave Bat").SetMaxHp(5).AddSpells(Spells.Bite),
		new EnemyTemplate("Wolf").SetMaxHp(7).AddSpells(Spells.Claw),
		new EnemyTemplate("Cutpursh").SetMaxHp(8).AddSpells(Spells.Slice),
		new EnemyTemplate("Golem").SetMaxHp(10).AddSpells(Spells.Stomp)
	};

	public static PlayerTemplate[] Classes = new PlayerTemplate[] {
		new PlayerTemplate("Wizard").SetMaxHp(16).AddSpells(Spells.Fireball, Spells.Whirlwind, Spells.Root, Spells.Revitalize).SetAffinities(15, 15, 15, 15, 0, 0, 0),
		new PlayerTemplate("Sorcerer").SetMaxHp(16).AddSpells(Spells.Whirlwind, Spells.Root, Spells.HolyLight, Spells.Flash),
		new PlayerTemplate("Druid").SetMaxHp(18).AddSpells(Spells.Whirlwind, Spells.ThrowStone, Spells.MendingHerbs, Spells.Revitalize),
		new PlayerTemplate("Warlock").SetMaxHp(16).AddSpells(Spells.DrainLife, Spells.Ignite, Spells.Rockblast, Spells.Root),
		new WildMageTemplate("Wild Mage").SetMaxHp(16)
	};

	public static Spell[] BuyableSpells = new Spell[] {
		Spells.Fireball, Spells.Whirlwind, Spells.Root, Spells.Revitalize, Spells.Incinerate, Spells.CureOfTheWoods, Spells.Flamestorm, Spells.HolyLight, Spells.SolarRay, Spells.HealingRays,
		Spells.Ignite, Spells.Tsunami, Spells.Flash, Spells.ConsumingDarkness, Spells.Earthquake, Spells.Gust, Spells.Rockblast, Spells.ThrowStone, Spells.MendingHerbs, Spells.Sandstorm,
		Spells.DrainLife, Spells.Eclipse, Spells.Synthesis, Spells.HollowShell
	};

	public static Ailment[] Ailments = new Ailment[] {
		new BurnAilment(0, "Burn", "Brn", Color.red),
		new BlindAilment(1, "Blind", "Bld", Element.Light.GetColor()),
		new RegenAilment(2, "Regen", "Rgn", Element.Earth.GetColor())
	};

	public static Equipment[] Equipments = new Equipment[] {
		new PerfectionCharm(0, "Charm of Perfection"),
		new AffinityEquipment(1, "Solar Orb", AttributeModifier.Operation.AddBase, 0, 0, 0, 0, 5, 0, 0),
		new AffinityEquipment(2, "Lunar Orb", AttributeModifier.Operation.AddBase, 0, 0, 0, 0, 0, 5, 0),
		new AffinityEquipment(3, "Elemental Focus", AttributeModifier.Operation.AddBase, 2, 2, 2, 2, 0, 0, 0),
		new DieTalisman(4, "Talisman of Disorder"),
		new RestoreRing(5, "Ring of Restoration")
	};
}
