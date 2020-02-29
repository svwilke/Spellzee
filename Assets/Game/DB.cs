using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DB {

	public static string[] CharNames = new string[] { "Mathil", "Corex", "Wrathion", "Tor'Shul", "Dillo", "Hinnex", "Jaenna", "Logana", "Zalapha", "Kalakarr" };

	public static string[] EnemyNames = new string[] { "Rat", "Pixie", "Bat", "Wolf", "Cutpursh", "Golem" };
	public static int[] EnemyHPs = new int[] { 4, 4, 5, 7, 8, 10 };
	public static int[] EnemySpells = new int[] { 7, 9, 7, 6, 10, 8 };

	public static EnemyTemplate[] Enemies = new EnemyTemplate[] {
		//new EnemyTemplate("Text").SetMaxHp(8).AddSpells(7).SetTurnWeights(0F, 0F, 6F)
		new EnemyTemplate("Sloth").SetMaxHp(3, 6).AddSpells(7).SetTurnWeights(1F, 0F, 6F),
		new EnemyTemplate("Pixie").SetMaxHp(4).AddSpells(9),
		new EnemyTemplate("Rat").SetMaxHp(4).AddSpells(7),
		new EnemyTemplate("Bat").SetMaxHp(5).AddSpells(7),
		new EnemyTemplate("Wolf").SetMaxHp(7).AddSpells(6),
		new EnemyTemplate("Cutpursh").SetMaxHp(8).AddSpells(10),
		new EnemyTemplate("Golem").SetMaxHp(10).AddSpells(8)
	};

	public static PlayerTemplate[] Classes = new PlayerTemplate[] {
		new PlayerTemplate("Wizard").SetMaxHp(16).AddSpells(0, 1, 2, 3, 20).SetAffinities(15, 15, 15, 15, 0, 0, 0),
		new PlayerTemplate("Sorcerer").SetMaxHp(16).AddSpells(1, 2, 12, 17),
		new PlayerTemplate("Druid").SetMaxHp(18).AddSpells(1, 22, 23, 3),
		new PlayerTemplate("Warlock").SetMaxHp(16).AddSpells(25, 15, 21, 2),
		new WildMageTemplate("Wild Mage").SetMaxHp(16)
	};

	public static int[] BuyableSpells = new int[] { 0, 1, 2, 3, 4, 5, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28 };

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
		new SimpleSpell(18, "Consuming Darkness", "Apply 3 Blind.", Element.Dark, 4, true, (ctx) => ctx.GetTarget().CmdApplyAilment(1, 3)),
		new SimpleSpell(19, "Earthquake", "Deal 8 damage randomly split among all allies and enemies.", Element.Earth, 4, false, (ctx) => {
			Pawn[] pawns = ctx.GetPawns();
			int[] dmgSplits = new int[pawns.Length];
			for(int i = 0; i < 8; i++) {
				int pawnIndex = Random.Range(0, pawns.Length);
				dmgSplits[pawnIndex] += 1;
			}
			for(int i = 0; i < dmgSplits.Length; i++) {
				if(dmgSplits[i] > 0) {
					pawns[i].CmdDamage(dmgSplits[i]);
				}
			}
		}),
		new SimpleActivatableSpell(20, "Gust", "Deal 2 damage, or 5 damage if you only rolled once.", "Deal 2 damage.", "Deal 5 damage", Element.Air, 3, true,
			(ctx) => ctx.GetRollsDone() == 1, (ctx, actv) => ctx.GetTarget().CmdDamage(actv ? 5 : 2)),
		new SimpleSpell(21, "Rockblast", "Deal 6 damage", Element.Earth, 4, true, (ctx) => ctx.GetTarget().CmdDamage(6)),
		new SimpleSpell(22, "Throw a stone", "Deal 2 damage.", Element.Earth, 2, true, (ctx) => ctx.GetTarget().CmdDamage(2)),
		new SimpleSpell(23, "Mending Herbs", "Restore 1 life and apply 2 Regenerate.", Element.Earth, 3, true, (ctx) => {ctx.GetTarget().CmdHeal(1); ctx.GetTarget().CmdApplyAilment(2, 2); }),
		new ComplexSpell(24, "Sandstorm", "Deal 2 damage. Apply 1 Blind with an additional Earth and Air.", true, (ctx, opt) => {
			ctx.GetTarget().CmdDamage(2);
			if(opt == 2) {
				ctx.GetTarget().CmdApplyAilment(1, 1);
			}
		}, true, new ComplexSpell.ElemMatch() { elem = Element.Earth, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Air, optional = false },
		new ComplexSpell.ElemMatch() { elem = Element.Earth, optional = true }, new ComplexSpell.ElemMatch() { elem = Element.Air, optional = true }),
		new SimpleSpell(25, "Drain Life", "Deal 2 damage to a target and restore 1 life to yourself.", Element.Dark, 3, true, (ctx) => {
			ctx.GetTarget().CmdDamage(2);
			ctx.GetCaster().CmdHeal(1);
		}),
		new ComplexSpell(26, "Eclipse", "Restore 2 life to all allies and deal 2 damage to all enemies.", false, (ctx, opt) => {ctx.ForEachAlly(p => p.CmdHeal(2)); ctx.ForEachEnemy(p => p.CmdDamage(2)); }, true,
			new ComplexSpell.ElemMatch() { elem = Element.Dark, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Dark, optional = false },
			new ComplexSpell.ElemMatch() { elem = Element.Light, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Light, optional = false }),
		new ComplexSpell(27, "Synthesis", "Restore a target to full life and apply 2 Blind.", true, (ctx, opt) => {ctx.GetTarget().CmdHeal(ctx.GetTarget().MaxHp - ctx.GetTarget().CurrentHp); ctx.GetTarget().CmdApplyAilment(1, 2); }, true,
			new ComplexSpell.ElemMatch() { elem = Element.Earth, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Earth, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Earth, optional = false },
			new ComplexSpell.ElemMatch() { elem = Element.Light, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Light, optional = false }),
		new SimpleSpell(28, "Hollow Shell", "Revive a dead target if it has more than 0 life.", Element.Dark, 3, true, (ctx) => {
			Pawn target = ctx.GetTarget();
			if(!target.IsAlive() && target.CurrentHp > 0) {
				target.CmdRevive();
			}
		})
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
