using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spells {

	private static Registry<Spell> Registry = new Registry<Spell>();

	public static Spell Fireball = Register("fireball", new Spell("Fireball", "Deal 5 damage.", true, new SimplePattern(Element.Fire, Element.Fire, Element.Fire, Element.Fire))
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Target, 5)));
	public static Spell Whirlwind = Register("whirlwind", new Spell("Whirlwind", "Deal 3 damage.", true, new SimplePattern(Element.Air, Element.Air, Element.Air))
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Target, 3)));
	public static Spell Root = Register("root", new Spell("Root", "Restore 2 life to yourself.", false, new SimplePattern(Element.Earth, Element.Earth))
		.AddComponent(pm => new HealComponent(SpellComponent.TargetType.Caster, 2)));
	public static Spell Revitalize = Register("revitalize", new Spell("Revitalize", "Restore 5 life.", true, new SimplePattern(Element.Water, Element.Water, Element.Water, Element.Water))
		.AddComponent(pm => new HealComponent(SpellComponent.TargetType.Target, 5)));
	public static Spell Incinerate = Register("incinerate", new Spell("Incinerate", "Deal 9 damage.", true, new SimplePattern(Element.Fire, Element.Fire, Element.Fire, Element.Fire, Element.Fire))
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Target, 9)));
	public static Spell CureOfTheWoods = Register("cure_of_the_woods", new Spell("Cure of the Woods", "Restore 5 life to all allies.", false, new SimplePattern(Element.Earth, Element.Earth, Element.Earth, Element.Earth, Element.Earth))
		.AddComponent(pm => new HealComponent(SpellComponent.TargetType.Allies, 5)));
	public static Spell Flamestorm = Register("flamestorm", new Spell("Flamestorm", "Deal 2 damage, or 4 damage with an additional Fire and Air.", true, new OptionalPattern(Element.Fire, Element.Air).SetOptional(Element.Fire, Element.Air))
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Target, (pm as OptionalPattern).OptionalFulfilled() ? 4 : 2)));
	public static Spell HolyLight = Register("holy_light", new Spell("Holy Light", "Deal 4 damage.", true, new SimplePattern(Element.Light, Element.Light, Element.Light))
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Target, 4)));
	public static Spell SolarRay = Register("solar_ray", new Spell("Solar Ray", "Deal 6 damage.", true, new SimplePattern(Element.Fire, Element.Fire, Element.Light, Element.Light))
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Target, 6)));
	public static Spell HealingRays = Register("healing_rays", new Spell("Healing Rays", "Restore 2 life to all allies.", false, new SimplePattern(Element.Light, Element.Light, Element.Light))
		.AddComponent(pm => new HealComponent(SpellComponent.TargetType.Allies, 2)));
	public static Spell Ignite = Register("ignite", new Spell("Ignite", "Apply 2 Burn.", true, new SimplePattern(Element.Fire, Element.Fire, Element.Fire))
		.AddComponent(pm => new AilmentComponent(SpellComponent.TargetType.Target, Ailments.Burn, 2)));
	public static Spell Tsunami = Register("tsunami", new Spell("Tsunami", "Deal 1 damage, +2 additional damage for each additional Water.", true, new ExtendingPattern(Element.Water, Element.Water).SetExtension(Element.Water))
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Target, 1 + (pm as ExtendingPattern).GetExtensionAmount() * 2)));
	public static Spell Flash = Register("flash", new Spell("Flash", "Apply 1 Blind.", true, new SimplePattern(Element.Light, Element.Light))
		.AddComponent(pm => new AilmentComponent(SpellComponent.TargetType.Target, Ailments.Blind, 1)));
	public static Spell ConsumingDarkness = Register("consuming_darkness", new Spell("Consuming Darkness", "Apply 3 Blind.", true, new SimplePattern(Element.Dark, Element.Dark, Element.Dark, Element.Dark))
		.AddComponent(pm => new AilmentComponent(SpellComponent.TargetType.Target, Ailments.Blind, 3)));
	public static Spell Earthquake = Register("earthquake", new Spell("Earthquake", "Deal 4 damage randomly split among all allies and 8 damage randomly split among all enemies.", false, new SimplePattern(Element.Earth, Element.Earth, Element.Earth, Element.Earth))
		.AddComponent(pm => new CustomComponent(SpellComponent.TargetType.All, (spell, context) => {
			DamageComponent allyDamage = new DamageComponent(SpellComponent.TargetType.Allies, 4);
			context.GetCaster().OnSpellComponentCaster.Invoke(spell, context, allyDamage);
			int dmgAllies = allyDamage.GetValue();
			Pawn[] allies = context.GetAllies();
			int[] damagePerAlly = new int[allies.Length];
			for(int i = 0; i < dmgAllies; i++) {
				damagePerAlly[Random.Range(0, allies.Length)] += 1;
			}
			for(int i = 0; i < allies.Length; i++) {
				if(damagePerAlly[i] > 0) {
					DamageComponent singleTargetDamage = new DamageComponent(SpellComponent.TargetType.Allies, damagePerAlly[i]);
					allies[i].OnSpellComponentTarget.Invoke(spell, context, singleTargetDamage);
					allies[i].CmdDamage(singleTargetDamage.GetValue());
				}
			}
			DamageComponent enemyDamage = new DamageComponent(SpellComponent.TargetType.Enemies, 8);
			context.GetCaster().OnSpellComponentCaster.Invoke(spell, context, enemyDamage);
			int dmgEnemies = enemyDamage.GetValue();
			Pawn[] enemies = context.GetEnemies();
			int[] damagePerEnemy = new int[allies.Length];
			for(int i = 0; i < dmgEnemies; i++) {
				damagePerEnemy[Random.Range(0, enemies.Length)] += 1;
			}
			for(int i = 0; i < enemies.Length; i++) {
				if(damagePerEnemy[i] > 0) {
					DamageComponent singleTargetDamage = new DamageComponent(SpellComponent.TargetType.Enemies, damagePerEnemy[i]);
					enemies[i].OnSpellComponentTarget.Invoke(spell, context, singleTargetDamage);
					enemies[i].CmdDamage(singleTargetDamage.GetValue());
				}
			}
		}, (spell, context) => "Deal 4 damage randomly split among all allies\nand 8 damage randomly split among all enemies.")));
	public static Spell Gust = Register("gust", new Spell("Gust", "Deal 2 damage, or 5 damage if you only rolled once.", true, new SimplePattern(Element.Air, Element.Air, Element.Air))
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Target, pm.GetContext().GetRollsDone() <= 1 ? 5 : 2)));
	public static Spell Rockblast = Register("rockblast", new Spell("Rockblast", "Deal 6 damage.", true, new SimplePattern(Element.Earth, Element.Earth, Element.Earth, Element.Earth))
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Target, 6)));
	public static Spell ThrowStone = Register("throw_stone", new Spell("Throw a stone", "Deal 2 damage.", true, new SimplePattern(Element.Earth, Element.Earth))
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Target, 2)));
	public static Spell MendingHerbs = Register("mending_herbs", new Spell("Mending Herbs", "Restore 1 life and apply 2 Regenerate.", true, new SimplePattern(Element.Earth, Element.Earth, Element.Earth))
		.AddComponent(pm => new HealComponent(SpellComponent.TargetType.Target, 1))
		.AddComponent(pm => new AilmentComponent(SpellComponent.TargetType.Target, Ailments.Regen, 2)));
	public static Spell Sandstorm = Register("sandstorm", new Spell("Sandstorm", "Deal 2 damage. Apply 1 Blind with an additional Earth and Air.", true, new OptionalPattern(Element.Earth, Element.Air).SetOptional(Element.Earth, Element.Air))
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Target, 2))
		.AddComponent(pm => {
			if((pm as OptionalPattern).OptionalFulfilled()) {
				return new AilmentComponent(SpellComponent.TargetType.Target, Ailments.Blind, 1);
			} else {
				return new NullComponent();
			}
		}));
	public static Spell DrainLife = Register("drain_life", new Spell("Drain Life", "Deal 2 damage to a target and restore 1 life to yourself.", true, new SimplePattern(Element.Dark, Element.Dark, Element.Dark))
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Target, 2))
		.AddComponent(pm => new HealComponent(SpellComponent.TargetType.Caster, 1)));
	public static Spell Eclipse = Register("eclipse", new Spell("Eclipse", "Deal 2 damage to all enemies and restore 2 life to all allies.", false, new SimplePattern(Element.Dark, Element.Dark, Element.Light, Element.Light))
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Enemies, 2))
		.AddComponent(pm => new HealComponent(SpellComponent.TargetType.Allies, 2)));
	public static Spell Synthesis = Register("synthesis", new Spell("Synthesis", "Restore a target to full life and apply 2 Blind.", true, new SimplePattern(Element.Earth, Element.Earth, Element.Earth, Element.Light, Element.Light))
		.AddComponent(pm => new AilmentComponent(SpellComponent.TargetType.Target, Ailments.Blind, 2))
		.AddComponent(pm => new CustomComponent(SpellComponent.TargetType.Target, (spell, context) => {
			Pawn target = context.GetTarget();
			HealComponent heal = new HealComponent(SpellComponent.TargetType.Target, target.MaxHp - target.CurrentHp);
			context.GetCaster().OnSpellComponentCaster.Invoke(spell, context, heal);
			target.OnSpellComponentTarget.Invoke(spell, context, heal);
			target.CmdHeal(heal.GetValue());
		}, (spell, context) => "Restore to full life.")));
	public static Spell HollowShell = Register("hollow_shell", new Spell("Hollow Shell", "Revive a dead target if it has more than 0 life.", true, new SimplePattern(Element.Dark, Element.Dark, Element.Dark))
		.AddComponent(pm => new CustomComponent(SpellComponent.TargetType.Target, (spell, context) => context.GetTarget().CmdRevive(), (spell, context) => context.GetTarget() == null ? "Revive a target." : (!context.GetTarget().IsAlive() && context.GetTarget().CurrentHp > 0) ? "Revive." : "Do nothing.")));

	// Enemy Spells

	public static Spell Cuteness = Register("cuteness", new Spell("Cuteness", "Deal 1 damage.", true, new NullPattern())
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Target, 1)));
	public static Spell Bite = Register("bite", new Spell("Bite", "Deal 1 damage to all enemies.", false, new NullPattern())
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Enemies, 1)));
	public static Spell Claw = Register("claw", new Spell("Claw", "Deal 2 damage to all enemies.", false, new NullPattern())
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Enemies, 2)));
	public static Spell Slice = Register("slice", new Spell("Slice", "Deal 2 damage to all enemies.", false, new NullPattern())
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Enemies, 2)));
	public static Spell Stomp = Register("stomp", new Spell("Stomp", "Deal 2 damage to all enemies.", false, new NullPattern())
		.AddComponent(pm => new DamageComponent(SpellComponent.TargetType.Enemies, 2)));

	public static Spell Register(string id, Spell spell) {
		spell.SetId(id);
		Registry.Register(spell);
		return spell;
	}

	public static Spell Get(string id) {
		return Registry.Get(id);
	}
}
