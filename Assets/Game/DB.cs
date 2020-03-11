using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DB {

	public static string[] CharNames = new string[] { "Mathil", "Corex", "Wrathion", "Tor'Shul", "Dillo", "Hinnex", "Jaenna", "Logana", "Zalapha", "Kalakarr" };

	public static PawnTemplate[] Enemies = new PawnTemplate[] {
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
		Equipments.ChaosGift,
		Equipments.HeadOfTheHydra
	};
}
