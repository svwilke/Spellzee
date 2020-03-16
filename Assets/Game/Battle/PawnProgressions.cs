using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PawnProgressions {

	private static Registry<PawnProgression> Registry = new Registry<PawnProgression>();

	public static PawnProgression GenericEnemy = Register("generic_enemy", new PawnProgression());

	public static PawnProgression Wizard = Register("wizard", new PawnProgression()
		.AddSpells(0, Spells.Fireball, Spells.Whirlwind, Spells.Root, Spells.Revitalize)
		.AddSpells(1, Spells.Incinerate, Spells.Ignite, Spells.Flamestorm, Spells.Gust)
		.AddSpells(2, Spells.Sandstorm, Spells.Kindle, Spells.Engulf, Spells.Earthquake)
		.AddSpells(3, Spells.Levitate, Spells.RainOfLife, Spells.ThrowStone, Spells.Tsunami, Spells.HealingHeat));

	public static PawnProgression Sorcerer = Register("sorcerer", new PawnProgression()
		.AddSpells(0, Spells.Flash, Spells.HolyLight, Spells.Gust, Spells.Root)
		.AddSpells(1, Spells.SolarRay, Spells.HealingRays, Spells.Kindle, Spells.Synthesis)
		.AddSpells(2, Spells.Sandstorm, Spells.Levitate, Spells.LionsRoar, Spells.Ignite, Spells.Sear)
		.AddSpells(3, Spells.ChaoticMind, Spells.Submerge, Spells.Revitalize, Spells.Reflection));

	public static PawnProgression Mage = Register("mage", new WildMageProgression());

	public static PawnProgression Druid = Register("druid", new PawnProgression()
		.AddSpells(0, Spells.Whirlwind, Spells.ThrowStone, Spells.MendingHerbs, Spells.Revitalize)
		.AddSpells(1, Spells.Earthquake, Spells.Gust, Spells.Flamestorm, Spells.AquaticBlast, Spells.Root)
		.AddSpells(2, Spells.Rockblast, Spells.Sandstorm, Spells.Levitate, Spells.RainOfLife, Spells.HealingRays)
		.AddSpells(3, Spells.Submerge, Spells.Tsunami, Spells.Synthesis, Spells.HealingHeat));

	public static PawnProgression Register(string id, PawnProgression progression) {
		progression.SetId(id);
		Registry.Register(progression);
		return progression;
	}

	public static PawnProgression Get(string id) {
		return Registry.Get(id);
	}
}
