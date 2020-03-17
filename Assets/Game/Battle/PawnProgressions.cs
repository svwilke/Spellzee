using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PawnProgressions {

	private static Registry<PawnProgression> Registry = new Registry<PawnProgression>();

	public static PawnProgression GenericEnemy = Register("generic_enemy", new PawnProgression());

	public static PawnProgression Wizard = Register("wizard", new PawnProgression()
		.AddSpells(0, Spells.Fireball, Spells.Whirlwind, Spells.Root, Spells.Revitalize, Spells.Ignite, Spells.HealingHeat, Spells.Incinerate)
		.AddSpells(1, Spells.Submerge, Spells.ThrowStone, Spells.Flamestorm, Spells.Earthquake)
		.AddSpells(2, Spells.Kindle, Spells.Sear, Spells.Engulf, Spells.RagingWinds)
		.AddSpells(3, Spells.Levitate, Spells.RainOfLife, Spells.Sandstorm, Spells.Tsunami, Spells.MendingHerbs));

	public static PawnProgression Sorcerer = Register("sorcerer", new PawnProgression()
		.AddSpells(0, Spells.Flash, Spells.HolyLight, Spells.Gust, Spells.Root, Spells.Eclipse, Spells.Kindle, Spells.Fireball)
		.AddSpells(1, Spells.SolarRay, Spells.HealingRays, Spells.LionsRoar, Spells.Synthesis, Spells.Restore)
		.AddSpells(2, Spells.Sandstorm, Spells.Levitate, Spells.Ignite, Spells.Sear, Spells.Enlighten)
		.AddSpells(3, Spells.ChaoticMind, Spells.Submerge, Spells.Reflection, Spells.Incinerate, Spells.Tsunami));

	public static PawnProgression Mage = Register("mage", new WildMageProgression());

	public static PawnProgression Druid = Register("druid", new PawnProgression()
		.AddSpells(0, Spells.Whirlwind, Spells.ThrowStone, Spells.MendingHerbs, Spells.Revitalize, Spells.Flamestorm, Spells.AquaticBlast, Spells.Earthquake)
		.AddSpells(1, Spells.RainOfLife, Spells.Gust, Spells.Root, Spells.Rockblast, Spells.HealingHeat, Spells.RagingWinds)
		.AddSpells(2, Spells.Submerge, Spells.Levitate, Spells.ChaoticMind, Spells.Restore, Spells.Thunderstorm)
		.AddSpells(3, Spells.Sandstorm, Spells.TrustyCompanion, Spells.Synthesis, Spells.Flash, Spells.Reflection));

	public static PawnProgression Warlock = Register("warlock", new PawnProgression()
		.AddSpells(0, Spells.Puncture, Spells.AquaticBlast, Spells.Restore, Spells.StoneWall, Spells.DrainLife, Spells.ChaoticMind, Spells.Ravnica)
		.AddSpells(1, Spells.Eclipse, Spells.TakeSight, Spells.Thunderstorm, Spells.Gust, Spells.HollowShell, Spells.Ignite)
		.AddSpells(2, Spells.Engulf, Spells.VoidBarrier, Spells.Stare, Spells.ConsumingDarkness, Spells.RainOfLife, Spells.Reflection)
		.AddSpells(3, Spells.Flamestorm, Spells.Kindle, Spells.Submerge, Spells.Rockblast, Spells.Tsunami));

	public static PawnProgression Register(string id, PawnProgression progression) {
		progression.SetId(id);
		Registry.Register(progression);
		return progression;
	}

	public static PawnProgression Get(string id) {
		return Registry.Get(id);
	}
}
