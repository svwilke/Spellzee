using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spells {

	public static Registry<Spell> Registry = new Registry<Spell>();

	public static Spell Fireball = Register("fireball", new SimpleSpell("Fireball", "Deal 5 damage.", Element.Fire, 4, true, (ctx) => ctx.GetTarget().CmdDamage(5)));
	public static Spell Whirlwind = Register("whirlwind", new SimpleSpell("Whirlwind", "Deal 3 damage.", Element.Air, 3, true, (ctx) => ctx.GetTarget().CmdDamage(3)));
	public static Spell Root = Register("root", new SimpleSpell("Root", "Restore 2 life to yourself.", Element.Earth, 2, false, (ctx) => ctx.GetCaster().CmdHeal(2)));
	public static Spell Revitalize = Register("revitalize", new SimpleSpell("Revitalize", "Restore 5 life.", Element.Water, 4, true, (ctx) => ctx.GetTarget().CmdHeal(5)));
	public static Spell Incinerate = Register("incinerate", new SimpleSpell("Incinerate", "Deal 9 damage.", Element.Fire, 5, true, (ctx) => ctx.GetTarget().CmdDamage(9)));
	public static Spell CureOfTheWoods = Register("cure_of_the_woods", new SimpleSpell("Cure of the Woods", "Restore 5 life to all allies.", Element.Earth, 5, false, (ctx) => ctx.ForEachAlly((p) => p.CmdHeal(5))));
	public static Spell Flamestorm = Register("flamestorm", new ComplexSpell("Flamestorm", "Deal 2 damage to all enemies. Deals 4 damage with an additional Fire and Air.", false, (ctx, opt) => ctx.ForEachEnemy((p) => p.CmdDamage(opt == 2 ? 4 : 2)), true,
		new ComplexSpell.ElemMatch() { elem = Element.Fire, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Air, optional = false },
		new ComplexSpell.ElemMatch() { elem = Element.Fire, optional = true }, new ComplexSpell.ElemMatch() { elem = Element.Air, optional = true })
		.SetShortDescriptionOptional("Deal 4 damage to all enemies.")
		.SetShortDescriptionRequired("Deal 2 damage to all enemies."));
	public static Spell HolyLight = Register("holy_light", new SimpleSpell("Holy Light", "Deal 4 damage.", Element.Light, 3, true, (ctx) => ctx.GetTarget().CmdDamage(4)));
	public static Spell SolarRay = Register("solar_ray", new ComplexSpell("Solar Ray", "Deal 6 damage.", true, (ctx, opt) => ctx.GetTarget().CmdDamage(6), true,
		new ComplexSpell.ElemMatch() { elem = Element.Fire, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Fire, optional = false },
		new ComplexSpell.ElemMatch() { elem = Element.Light, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Light, optional = false }));
	public static Spell HealingRays = Register("healing_rays", new SimpleSpell("Healing Rays", "Restore 2 life to all allies.", Element.Light, 3, false, (ctx) => ctx.ForEachAlly((p) => p.CmdHeal(2))));
	public static Spell Ignite = Register("ignite", new SimpleSpell("Ignite", "Apply 2 Burn.", Element.Fire, 3, true, (ctx) => ctx.GetTarget().CmdApplyAilment(Ailments.Burn, 2)));
	public static Spell Tsunami = Register("tsunami", new ExtendingSpell("Tsunami", "Deal 1 base damage. Deal 2 additional damage for each additional Water.", "Deal {0} damage.", true, Element.Water, 2, 1, 2, (ctx, val) => ctx.GetTarget().CmdDamage(val)));
	public static Spell Flash = Register("flash", new SimpleSpell("Flash", "Apply 1 Blind.", Element.Light, 2, true, (ctx) => ctx.GetTarget().CmdApplyAilment(Ailments.Blind, 1)));
	public static Spell ConsumingDarkness = Register("consuming_darkness", new SimpleSpell("Consuming Darkness", "Apply 3 Blind.", Element.Dark, 4, true, (ctx) => ctx.GetTarget().CmdApplyAilment(Ailments.Blind, 3)));
	public static Spell Earthquake = Register("earthquake", new SimpleSpell("Earthquake", "Deal 8 damage randomly split among all allies and enemies.", Element.Earth, 4, false, (ctx) => {
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
		}));
	public static Spell Gust = Register("gust", new SimpleActivatableSpell("Gust", "Deal 2 damage, or 5 damage if you only rolled once.", "Deal 2 damage.", "Deal 5 damage", Element.Air, 3, true,
		(ctx) => ctx.GetRollsDone() == 1, (ctx, actv) => ctx.GetTarget().CmdDamage(actv ? 5 : 2)));
	public static Spell Rockblast = Register("rockblast", new SimpleSpell("Rockblast", "Deal 6 damage", Element.Earth, 4, true, (ctx) => ctx.GetTarget().CmdDamage(6)));
	public static Spell ThrowStone = Register("throw_stone", new SimpleSpell("Throw a stone", "Deal 2 damage.", Element.Earth, 2, true, (ctx) => ctx.GetTarget().CmdDamage(2)));
	public static Spell MendingHerbs = Register("mending_herbs", new SimpleSpell("Mending Herbs", "Restore 1 life and apply 2 Regenerate.", Element.Earth, 3, true, (ctx) => { ctx.GetTarget().CmdHeal(1); ctx.GetTarget().CmdApplyAilment(Ailments.Regen, 2); }));
	public static Spell Sandstorm = Register("sandstorm", new ComplexSpell("Sandstorm", "Deal 2 damage. Apply 1 Blind with an additional Earth and Air.", true, (ctx, opt) => {
			ctx.GetTarget().CmdDamage(2);
			if(opt == 2) {
				ctx.GetTarget().CmdApplyAilment(Ailments.Blind, 1);
			}
		}, true, new ComplexSpell.ElemMatch() { elem = Element.Earth, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Air, optional = false },
		new ComplexSpell.ElemMatch() { elem = Element.Earth, optional = true }, new ComplexSpell.ElemMatch() { elem = Element.Air, optional = true }));
	public static Spell DrainLife = Register("drain_life", new SimpleSpell("Drain Life", "Deal 2 damage to a target and restore 1 life to yourself.", Element.Dark, 3, true, (ctx) => {
			ctx.GetTarget().CmdDamage(2);
			ctx.GetCaster().CmdHeal(1);
		}));
	public static Spell Eclipse = Register("eclipse", new ComplexSpell("Eclipse", "Restore 2 life to all allies and deal 2 damage to all enemies.", false, (ctx, opt) => { ctx.ForEachAlly(p => p.CmdHeal(2)); ctx.ForEachEnemy(p => p.CmdDamage(2)); }, true,
		new ComplexSpell.ElemMatch() { elem = Element.Dark, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Dark, optional = false },
		new ComplexSpell.ElemMatch() { elem = Element.Light, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Light, optional = false }));
	public static Spell Synthesis = Register("synthesis", new ComplexSpell("Synthesis", "Restore a target to full life and apply 2 Blind.", true, (ctx, opt) => { ctx.GetTarget().CmdHeal(ctx.GetTarget().MaxHp - ctx.GetTarget().CurrentHp); ctx.GetTarget().CmdApplyAilment(Ailments.Blind, 2); }, true,
		new ComplexSpell.ElemMatch() { elem = Element.Earth, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Earth, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Earth, optional = false },
		new ComplexSpell.ElemMatch() { elem = Element.Light, optional = false }, new ComplexSpell.ElemMatch() { elem = Element.Light, optional = false }));
	public static Spell HollowShell = Register("hollow_shell", new SimpleSpell("Hollow Shell", "Revive a dead target if it has more than 0 life.", Element.Dark, 3, true, (ctx) => {
		Pawn target = ctx.GetTarget();
		if(!target.IsAlive() && target.CurrentHp > 0) {
			target.CmdRevive();
		}
	}));

	// Enemy Spells

	public static Spell Claw = Register("claw", new SimpleSpell("Claw", "Deal 2 damage.", Element.None, 4, false, (ctx) => ctx.ForEachEnemy((p) => p.CmdDamage(2))));
	public static Spell Bite = Register("bite", new SimpleSpell("Bite", "Deal 1 damage.", Element.None, 4, false, (ctx) => ctx.ForEachEnemy((p) => p.CmdDamage(1))));
	public static Spell Stomp = Register("stomp", new SimpleSpell("Stomp", "Deal 3 damage.", Element.None, 4, false, (ctx) => ctx.ForEachEnemy((p) => p.CmdDamage(3))));
	public static Spell Cuteness = Register("cuteness", new SimpleSpell("Cuteness", "Deal 1 damage.", Element.None, 4, true, (ctx) => ctx.GetTarget().CmdDamage(1)));
	public static Spell Slice = Register("slice", new SimpleSpell("Slice", "Deal 2 damage.", Element.None, 4, false, (ctx) => ctx.ForEachEnemy((p) => p.CmdDamage(2))));


	public static Spell Register(string id, Spell spell) {
		spell.SetId(id);
		Registry.Register(spell);
		return spell;
	}
}
