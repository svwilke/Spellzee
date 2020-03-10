using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DB {

	public static string[] CharNames = new string[] { "Mathil", "Corex", "Wrathion", "Tor'Shul", "Dillo", "Hinnex", "Jaenna", "Logana", "Zalapha", "Kalakarr" };

	public static string[] EnemyNames = new string[] { "Rat", "Pixie", "Bat", "Wolf", "Cutpursh", "Golem" };
	public static int[] EnemyHPs = new int[] { 4, 4, 5, 7, 8, 10 };
	public static int[] EnemySpells = new int[] { 7, 9, 7, 6, 10, 8 };

	public static PawnTemplate[] Enemies = new PawnTemplate[] {
		//new EnemyTemplate("Text").SetMaxHp(8).SetTurnWeights(1F, 0F, 1F).AddSpells(Spells.Bite)
		//new EnemyTemplate("Sloth").SetMaxHp(3, 6).AddSpells(Spells.Bite).SetTurnWeights(1F, 0F, 6F),
		//new EnemyTemplate("Pixie").SetMaxHp(4).AddSpells(Spells.Cuteness),
		//new EnemyTemplate("Rat").SetMaxHp(5).AddSpells(Spells.Bite),
		//new EnemyTemplate("Cave Bat").SetMaxHp(5).AddSpells(Spells.Bite),
		//new EnemyTemplate("Wolf").SetMaxHp(6).AddSpells(Spells.Claw),
		//new EnemyTemplate("Cutpursh").SetMaxHp(8).AddSpells(Spells.Slice),
		//new EnemyTemplate("Golem").SetMaxHp(10).AddSpells(Spells.Stomp, Spells.Root)
		PawnTemplates.Sloth,
		PawnTemplates.Pixie,
		PawnTemplates.SewerRat,
		PawnTemplates.FieryBat,
		PawnTemplates.Wolf,
		PawnTemplates.Cutpurse,
		PawnTemplates.Golem
	};

	public static Equipment[] BuyableEquipments = new Equipment[] {
		Equipments.CharmOfPerfection,
		Equipments.SolarOrb,
		Equipments.LunarOrb,
		Equipments.ElementalFocus,
		Equipments.RingOfRestoration,
		Equipments.HealCharm,
		Equipments.IgneousRock,
		Equipments.TalismanOfDisorder,
		Equipments.ChaosGift
	};
}
