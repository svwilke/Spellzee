using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DB {

	public static string[] CharNames = new string[] { "Mathil", "Corex", "Wrathion", "Tor'Shul", "Dillo", "Hinnex", "Jaenna", "Logana", "Zalapha", "Kalakarr" };

	public static string[] EnemyNames = new string[] { "Rat", "Pixie", "Bat", "Wolf", "Cutpursh", "Golem" };
	public static int[] EnemyHPs = new int[] { 4, 4, 5, 7, 8, 10 };
	public static int[] EnemySpells = new int[] { 7, 9, 7, 6, 10, 8 };

	public static EnemyTemplate[] Enemies = new EnemyTemplate[] {
		new EnemyTemplate("Sloth").SetMaxHp(4, 6).AddSpells(7).SetTurnWeights(1F, 0F, 6F),
		new EnemyTemplate("Pixie").SetMaxHp(4).AddSpells(9),
		new EnemyTemplate("Rat").SetMaxHp(4).AddSpells(7),
		new EnemyTemplate("Bat").SetMaxHp(5).AddSpells(7),
		new EnemyTemplate("Wolf").SetMaxHp(7).AddSpells(6),
		new EnemyTemplate("Cutpursh").SetMaxHp(8).AddSpells(10),
		new EnemyTemplate("Golem").SetMaxHp(10).AddSpells(8)
	};

	public static PlayerTemplate[] Classes = new PlayerTemplate[] {
		new PlayerTemplate("Wizard").SetMaxHp(16).AddSpells(0, 1, 2, 3).SetAffinities(15, 15, 15, 15, 0, 0, 0),
		new PlayerTemplate("Sorcerer").SetMaxHp(16).AddSpells(1, 2, 12, 17)
	};

	public static int[] BuyableSpells = new int[] { 0, 1, 2, 3, 4, 5, 11, 12, 13, 14, 15, 16, 17 };

	public static class Spells
	{
		public static Spell Fireball;
		public static Spell Whirlwind;
		public static Spell Root;
	}

	public static Spell[] SpellList = new Spell[] {
		Spells.Fireball = new SimpleSpell(0, "Fireball", "Deal 5 damage.", Element.Fire, 4, true, (ctx) => ctx.GetTarget().CmdDamage(5)),
		Spells.Whirlwind = new SimpleSpell(1, "Whirlwind", "Deal 3 damage.", Element.Air, 3, true, (ctx) => ctx.GetTarget().CmdDamage(3)),
		Spells.Root = new SimpleSpell(2, "Root", "Restore 2 life to yourself.", Element.Earth, 2, false, (ctx) => ctx.GetCaster().CmdHeal(2)),
		new SimpleSpell(3, "Revitalize", "Restore 5 life.", Element.Water, 4, true, (ctx) => ctx.GetTarget().CmdHeal(5)),
		new SimpleSpell(4, "Incinerate", "Deal 9 damage.", Element.Fire, 5, true, (ctx) => ctx.GetTarget().CmdDamage(9)),
		new SimpleSpell(5, "Cure of the Woods", "Restore 5 life to all allies.", Element.Earth, 5, false, (ctx) => ctx.ForEachAlly((p) => p.CmdHeal(5))),
		// Enemy Spells
		new SimpleSpell(6, "Claw", "Deal 2 damage.", Element.None, 4, false, (ctx) => ctx.ForEachEnemy((p) => p.CmdDamage(2))),
		new SimpleSpell(7, "Bite", "Deal 1 damage.", Element.None, 4, false, (ctx) => ctx.ForEachEnemy((p) => p.CmdDamage(1))),
		new SimpleSpell(8, "Stomp", "Deal 3 damage.", Element.None, 4, false, (ctx) => ctx.ForEachEnemy((p) => p.CmdDamage(3))),
		new SimpleSpell(9, "Cuteness", "Deal 1 damage.", Element.None, 4, true, (ctx) => ctx.GetTarget().CmdDamage(1)),
		new SimpleSpell(10, "Slice", "Deal 2 damage.", Element.None, 4, false, (ctx) => ctx.ForEachEnemy((p) => p.CmdDamage(2))),

		new ComplexSpell(11, "Flamestorm", "Deal 2 damage to all enemies. Deals 4 damage with an additional Fire and Air.", false, (ctx, opt) => ctx.ForEachEnemy((p) => p.CmdDamage(opt == 2 ? 4 : 2)), true,
			new ComplexSpell.ElemMatch() { elem = Element.Fire, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Air, optional = false },
			new ComplexSpell.ElemMatch() { elem = Element.Fire, optional = true }, new ComplexSpell.ElemMatch() { elem = Element.Air, optional = true })
			.SetShortDescriptionOptional("Deal 4 damage to all enemies.")
			.SetShortDescriptionRequired("Deal 2 damage to all enemies."),
		new SimpleSpell(12, "Holy Light", "Deal 4 damage.", Element.Light, 3, true, (ctx) => ctx.GetTarget().CmdDamage(4)),
		new ComplexSpell(13, "Solar Ray", "Deal 6 damage.", true, (ctx, opt) => ctx.GetTarget().CmdDamage(6), true,
			new ComplexSpell.ElemMatch() { elem = Element.Fire, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Fire, optional = false },
			new ComplexSpell.ElemMatch() { elem = Element.Light, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Light, optional = false }),
		new SimpleSpell(14, "Healing Rays", "Restore 2 life to all allies.", Element.Light, 3, false, (ctx) => ctx.ForEachAlly((p) => p.CmdHeal(2))),
		new SimpleSpell(15, "Ignite", "Apply 2 Burn.", Element.Fire, 3, true, (ctx) => ctx.GetTarget().CmdApplyAilment(0, 2)),
		new ExtendingSpell(16, "Tsunami", "Deal 1 base damage. Deal 2 additional damage for each additional Water.", "Deal {0} damage.", true, Element.Water, 2, 1, 2, (ctx, val) => ctx.GetTarget().CmdDamage(val)),
		new SimpleSpell(17, "Flash", "Apply 1 Blind.", Element.Light, 2, true, (ctx) => ctx.GetTarget().CmdApplyAilment(1, 1)),
		new SimpleSpell(18, "Consuming Darkness", "Apply 3 Blind.", Element.Dark, 4, true, (ctx) => ctx.GetTarget().CmdApplyAilment(1, 3))
	};

	public static Ailment[] Ailments = new Ailment[] {
		new BurnAilment(0, "Burn", "Brn", Color.red),
		new BlindAilment(1, "Blind", "Bld", Element.Light.GetColor())
	};

	public static Equipment[] Equipments = new Equipment[] {
		new PerfectionCharm(0, "Charm of Perfection"),
		new AffinityEquipment(1, "Solar Orb", AttributeModifier.Operation.AddBase, 0, 0, 0, 0, 5, 0, 0),
		new AffinityEquipment(2, "Lunar Orb", AttributeModifier.Operation.AddBase, 0, 0, 0, 0, 0, 5, 0),
		new AffinityEquipment(3, "Elemental Focus", AttributeModifier.Operation.AddBase, 2, 2, 2, 2, 0, 0, 0),
		new DieTalisman(4, "Flaying Talisman")
	};
}
