using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spells {

	private static Registry<Spell> Registry = new Registry<Spell>();

	public static Spell Fireball = Register("fireball", new Spell("Fireball", "Deal 5 damage.", new SimplePattern(Element.Fire, 4))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new DamageComponent(5)));
	public static Spell Whirlwind = Register("whirlwind", new Spell("Whirlwind", "Deal 3 damage.", new SimplePattern(Element.Air, 3))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new DamageComponent(3)));
	public static Spell Root = Register("root", new Spell("Root", "Restore 2 life to yourself.", new SimplePattern(Element.Earth, 2))
		.AddTarget(pm => new Target(TargetType.Caster))
		.AddComponent(pm => new HealComponent(2)));
	public static Spell Revitalize = Register("revitalize", new Spell("Revitalize", "Restore 5 life.", new SimplePattern(Element.Water, 4))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new HealComponent(5)));
	public static Spell Incinerate = Register("incinerate", new Spell("Incinerate", "Deal 9 damage.", new SimplePattern(Element.Fire, 5))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new DamageComponent(9)));
	public static Spell RainOfLife = Register("rain_of_life", new Spell("Rain of Life", "Restore 5 life to all allies.", new SimplePattern(Element.Water, 5))
		.AddTarget(pm => new Target(TargetType.Allies))
		.AddComponent(pm => new HealComponent(5)));
	public static Spell Flamestorm = Register("flamestorm", new Spell("Flamestorm", "Deal 2 damage, or 4 damage with an additional Fire and Air.", new OptionalPattern(Element.Fire, Element.Air).SetOptional(Element.Fire, Element.Air))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new DamageComponent((pm as OptionalPattern).OptionalFulfilled() ? 4 : 2)));
	public static Spell HolyLight = Register("holy_light", new Spell("Holy Light", "Deal 4 damage.", new SimplePattern(Element.Light, 3))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new DamageComponent(4)));
	public static Spell SolarRay = Register("solar_ray", new Spell("Solar Ray", "Deal 6 damage.", new SimplePattern(Element.Fire, Element.Fire, Element.Light, Element.Light))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new DamageComponent(6)));
	public static Spell HealingRays = Register("healing_rays", new Spell("Healing Rays", "Restore 2 life to all allies.", new SimplePattern(Element.Light, 3))
		.AddTarget(pm => new Target(TargetType.Allies))
		.AddComponent(pm => new HealComponent(2)));
	public static Spell Ignite = Register("ignite", new Spell("Ignite", "Apply 2 Burn.", new SimplePattern(Element.Fire, Element.Fire, Element.Fire))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new AilmentComponent(intensity => new BurnStatus(intensity), 2)));
	public static Spell Tsunami = Register("tsunami", new Spell("Tsunami", "Deal 1 damage, +2 additional damage for each additional Water.", new ExtendingPattern(Element.Water, Element.Water).SetExtension(Element.Water))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new DamageComponent(1 + (pm as ExtendingPattern).GetExtensionAmount() * 2)));
	public static Spell Flash = Register("flash", new Spell("Flash", "Apply 1 Blind.", new SimplePattern(Element.Light, Element.Light))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new AilmentComponent(intensity => new BlindStatus(intensity), 1)));
	public static Spell TakeSight = Register("take_sight", new Spell("Take Sight", "Apply 3 Blind.", new SimplePattern(Element.Dark, 4))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new AilmentComponent(intensity => new BlindStatus(intensity), 3)));
	public static Spell Earthquake = Register("earthquake", new Spell("Earthquake", "Deal 4 damage randomly split among all allies and 8 damage randomly split among all enemies.", new SimplePattern(Element.Earth, 4))
		.AddComponent(pm => new CustomComponent((spell, context, target) => {
			DamageComponent allyDamage = new DamageComponent(4);
			context.GetCaster().OnSpellComponentCaster.Invoke(spell, context, allyDamage);
			int dmgAllies = allyDamage.GetValue();
			Pawn[] allies = context.GetAllies();
			int[] damagePerAlly = new int[allies.Length];
			for(int i = 0; i < dmgAllies; i++) {
				damagePerAlly[Random.Range(0, allies.Length)] += 1;
			}
			for(int i = 0; i < allies.Length; i++) {
				if(damagePerAlly[i] > 0) {
					DamageComponent singleTargetDamage = new DamageComponent(damagePerAlly[i]);
					allies[i].OnSpellComponentTarget.Invoke(spell, context, singleTargetDamage);
					EventBus.DamageHealEvent damageEvent = new EventBus.DamageHealEvent(Spells.Earthquake, singleTargetDamage, singleTargetDamage.GetValue());
					allies[i].CmdDamage(damageEvent);
				}
			}
			DamageComponent enemyDamage = new DamageComponent(8);
			context.GetCaster().OnSpellComponentCaster.Invoke(spell, context, enemyDamage);
			int dmgEnemies = enemyDamage.GetValue();
			Pawn[] enemies = context.GetEnemies();
			int[] damagePerEnemy = new int[allies.Length];
			for(int i = 0; i < dmgEnemies; i++) {
				damagePerEnemy[Random.Range(0, enemies.Length)] += 1;
			}
			for(int i = 0; i < enemies.Length; i++) {
				if(damagePerEnemy[i] > 0) {
					DamageComponent singleTargetDamage = new DamageComponent(damagePerEnemy[i]);
					enemies[i].OnSpellComponentTarget.Invoke(spell, context, singleTargetDamage);
					EventBus.DamageHealEvent damageEvent = new EventBus.DamageHealEvent(Spells.Earthquake, singleTargetDamage, singleTargetDamage.GetValue());
					enemies[i].CmdDamage(damageEvent);
				}
			}
		}, (spell, context, target) => "Deal 4 damage randomly split among all allies\nand 8 damage randomly split among all enemies.")));
	public static Spell Gust = Register("gust", new Spell("Gust", "Deal 2 damage, or 5 damage if you only rolled once.", new SimplePattern(Element.Air, 3))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new DamageComponent(pm.GetContext().GetRollsDone() <= 1 ? 5 : 2)));
	public static Spell Rockblast = Register("rockblast", new Spell("Rockblast", "Deal 6 damage.", new SimplePattern(Element.Earth, 4))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new DamageComponent(6)));
	public static Spell ThrowStone = Register("throw_stone", new Spell("Throw a stone", "Deal 2 damage.", new SimplePattern(Element.Earth, 2))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new DamageComponent(2)));
	public static Spell MendingHerbs = Register("mending_herbs", new Spell("Mending Herbs", "Restore 1 life and apply 2 Regenerate.", new SimplePattern(Element.Earth, 3))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new HealComponent(1))
		.AddComponent(pm => new AilmentComponent(intensity => new RegenStatus(intensity), 2)));
	public static Spell Sandstorm = Register("sandstorm", new Spell("Sandstorm", "Deal 2 damage. Apply 1 Blind with an additional Earth and Air.", new OptionalPattern(Element.Earth, Element.Air).SetOptional(Element.Earth, Element.Air))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new DamageComponent(2))
		.AddComponent(pm => {
			if((pm as OptionalPattern).OptionalFulfilled()) {
				return new AilmentComponent(intensity => new BlindStatus(intensity), 1);
			} else {
				return new NullComponent();
			}
		}));
	public static Spell DrainLife = Register("drain_life", new Spell("Drain Life", "Deal 1 damage to a target and restore 1 life to yourself.", new SimplePattern(Element.Dark, 2))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new DamageComponent(1))
		.AddTarget(pm => new Target(TargetType.Caster))
		.AddComponent(pm => new HealComponent(1)));
	public static Spell Eclipse = Register("eclipse", new Spell("Eclipse", "Deal 2 damage to all enemies and restore 2 life to all allies.", new SimplePattern(Element.Dark, Element.Dark, Element.Light, Element.Light))
		.AddTarget(pm => new Target(TargetType.Enemies))
		.AddComponent(pm => new DamageComponent(2))
		.AddTarget(pm => new Target(TargetType.Allies))
		.AddComponent(pm => new HealComponent(2)));
	public static Spell Synthesis = Register("synthesis", new Spell("Synthesis", "Restore a target to full life and apply 2 Blind.", new SimplePattern(Element.Earth, Element.Earth, Element.Earth, Element.Light, Element.Light))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new AilmentComponent(intensity => new BlindStatus(intensity), 2))
		.AddComponent(pm => new CustomComponent((spell, context, target) => {
			target.GetTargets().ForEach(pawn => {
				HealComponent heal = new HealComponent(pawn.GetMaxHp() - pawn.CurrentHp);
				context.GetCaster().OnSpellComponentCaster.Invoke(spell, context, heal);
				pawn.OnSpellComponentTarget.Invoke(spell, context, heal);
				EventBus.DamageHealEvent evtHeal = new EventBus.DamageHealEvent(Spells.Synthesis, heal, heal.GetValue());
				pawn.CmdHeal(evtHeal);
			});
		}, (spell, context, target) => "Restore" + DescriptionHelper.GetDescriptionInfix(target) + " to full life.")));
	public static Spell HollowShell = Register("hollow_shell", new Spell("Hollow Shell", "Revive a dead target if it has more than 0 life.", new SimplePattern(Element.Dark, 3))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new CustomComponent((spell, context, target) => target.GetTargets().ForEach(pawn => pawn.CmdRevive()), (spell, context, target) => "Revive" + DescriptionHelper.GetDescriptionInfix(target) + ".")));
	public static Spell VoidBarrier = Register("void_barrier", new Spell("Void Barrier", "Apply 4 Protect.", new SimplePattern(Element.Dark, 3))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new AilmentComponent(intensity => new ProtectStatus(intensity), 4)));
	public static Spell AquaticBlast = Register("aquatic_blast", new Spell("Aquatic Blast", "Deal 2-3 damage.", new SimplePattern(Element.Water, 2))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new RandomDamageComponent(2, 3)));
	public static Spell Submerge = Register("submerge", new Spell("Submerge", "You gain +20 Water Affinity for 1 turn.", new SimplePattern(Element.Water))
		.AddTarget(pm => new Target(TargetType.Caster))
		.AddComponent(pm => new StatusComponent("+20 Water Affinity for 1 turn.", () => new AffinityStatus(Status.StatusType.Positive, Element.Water, 1, AttributeModifier.Operation.AddBase, 20))));
	public static Spell Levitate = Register("levitate", new Spell("Levitate", "Apply Levitate.", new SimplePattern(Element.Air, 4))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new StatusComponent("Apply Levitate.", () => new LevitateStatus())));
	public static Spell ConsumingDarkness = Register("consuming_darkness", new Spell("Consuming Darkness", "For 1 turn, everyone has -10 Light Affinity and +10 Dark Affinity.", new SimplePattern(Element.Dark, 2))
		.AddTarget(pm => new Target(TargetType.All))
		.AddComponent(pm => new StatusComponent("-10 Light Affinity for 1 turn.", () => new AffinityStatus(Status.StatusType.Negative, Element.Light, 1, AttributeModifier.Operation.SubtractBase, 10)))
		.AddComponent(pm => new StatusComponent("+10 Dark Affinity for 1 turn.", () => new AffinityStatus(Status.StatusType.Positive, Element.Dark, 1, AttributeModifier.Operation.AddBase, 10))));

	public static Spell Ravnica = Register("ravnica", new Spell("Ravnica", "Summon Rasputin.", new SimplePattern(Element.Dark, 3))
		.AddComponent(pm => new SummonComponent(PawnTemplates.Rasputin, 1, 0, 1)));
	public static Spell LionsRoar = Register("lions_roar", new Spell("Lion's Roar", "You gain +2 Fire Affinity until the end of battle.", new SimplePattern(Element.Fire, 2))
		.AddTarget(pm => new Target(TargetType.Caster))
		.AddComponent(pm => new StatusComponent("+2 Fire Affinity until the end of battle.",
		() => new CustomStatus(Status.StatusType.Positive,
		pawn => pawn.Affinities[Element.Fire.GetId()].AddModifier(new AttributeModifier("lions_roar", AttributeModifier.Operation.AddBase, 2)),
		pawn => pawn.Affinities[Element.Fire.GetId()].RemoveModifier("lions_roar")))));
	public static Spell ChaoticMind = Register("chaotic_mind", new Spell("Chaotic Mind", "Cast a random damage spell on the target. Requires 5 unique elements to be cast.", new UniquePattern(5))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new CustomComponent((spell, context, target) => {
			List<Spell> validSpells = GetCastableSpells().Where(sp => sp.BuildComponentList(context).Any(sc => sc is DamageComponent && sc.GetTargetType() == target.GetTargetType() && target.GetTargets().Any(t => sc.IsValidTarget(t, context)))).ToList();
			Spell toCast = REX.Choice(validSpells);
			(context.GetBattle() as ServerBattle).DoCastSpell(toCast.GetId(), context.GetTarget().GetId(), true, false);
		}, (spell, context, target) => "Cast a random damage spell.")));
	public static Spell Reflection = Register("reflection", new Spell("Reflection", "Your next spell is cast twice.", new SimplePattern(Element.Water, 3))
		.AddTarget(pm => new Target(TargetType.Caster))
		.AddComponent(pm => new StatusComponent("The next spell is cast twice.", () => new DoubleSpellStatus())));

	public static Spell HealingHeat = Register("healing_heat", new Spell("Healing Heat", "Restore life to you equal to Burn on the target.", new SimplePattern(Element.Fire, Element.Fire, Element.Earth))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new CustomComponent((spell, ctx, target) => {
			int burn = target.GetTargets().Select(t => t.GetAilmentValue(typeof(BurnStatus))).Sum();
			HealComponent heal = new HealComponent(burn);
			Target casterTarget = new Target(TargetType.Caster, TargetGroup.Any);
			heal.SetTarget(casterTarget);
			casterTarget.Resolve(ctx);
			heal.Execute(spell, ctx);
		}, (spell, ctx, target) => "Restore life to yourself equal to Burn" + DescriptionHelper.GetDescriptionOnfix(target) + ".")));
	public static Spell Engulf = Register("engulf", new Spell("Engulf", "Double the amount of Burn on the target.", new SimplePattern(Element.Fire, 3))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new CustomComponent((spell, ctx, target) => {
			target.GetTargets().ForEach(pawn => {
				int burn = pawn.GetAilmentValue(typeof(BurnStatus));
				if(burn > 0) {
					pawn.CmdAddStatus(new BurnStatus(burn));
				}
			});
		}, (spell, ctx, target) => "Double Burn" + DescriptionHelper.GetDescriptionOnfix(target) + ".")));
	public static Spell Sear = Register("sear", new Spell("Sear", "Inflict 4 Burn and restore 4 life to the target.", new SimplePattern(Element.Fire, 4))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new AilmentComponent(intensity => new BurnStatus(intensity), 4))
		.AddComponent(pm => new HealComponent(4)));
	public static Spell Kindle = Register("kindle", new Spell("Kindle", "Deal 1-3 damage.", new SimplePattern(Element.Fire, 2))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new RandomDamageComponent(1, 3)));
	public static Spell Puncture = Register("puncture", new Spell("Puncture", "Deal 2 damage to a random enemy 2 times.", new SimplePattern(Element.Dark, 3))
		.AddTarget(pm => new Target(TargetType.Random).SetTargetGroup(TargetGroup.Enemy))
		.AddComponent(pm => new DamageComponent(2))
		.AddTarget(pm => new Target(TargetType.Random).SetTargetGroup(TargetGroup.Enemy))
		.AddComponent(pm => new DamageComponent(2)));
	public static Spell Restore = Register("restore", new Spell("Restore", "Restore 4 life.", new SimplePattern(Element.Water, 3))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new HealComponent(4)));
	public static Spell TrustyCompanion = Register("trusty_companion", new Spell("Trusty Companion", "Summon a Wolf with level equal to yours.", new SimplePattern(Element.Earth, 4))
		.AddComponent(pm => new SummonComponent(PawnTemplates.Wolf, 1, pm.GetContext().GetCaster().Level, 1)));
	public static Spell StoneWall = Register("stone_wall", new Spell("Stone Wall", "Apply 3 Protect to all allies.", new SimplePattern(Element.Earth, 4))
		.AddTarget(pm => new Target(TargetType.Allies))
		.AddComponent(pm => new AilmentComponent(intensity => new ProtectStatus(intensity), 3)));
	public static Spell RagingWinds = Register("raging_winds", new Spell("Raging Winds", "Deal 1 damage. Increase this spell's damage by 1 until the end of the dungeon.", new SimplePattern(Element.Air, 3))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new DamageComponent(1 + pm.GetContext().GetCaster().GetSpellData(Spells.RagingWinds).GetInt(DataKey.CastCount)))
		.AddTarget(pm => new Target(TargetType.None))
		.AddComponent(pm => new CustomComponent((spell, ctx, target) => {
			SpellData sd = ctx.GetCaster().GetSpellData(spell);
			sd.SetInt(DataKey.CastCount, sd.GetInt(DataKey.CastCount) + 1);
			ctx.GetCaster().Synchronize();
		}, (spell, ctx, target) => "")));
	public static Spell Thunderstorm = Register("thunderstorm", new Spell("Thunderstorm", "Deal 2 damage, or 2 damage to all enemies with an additional Water and Air.", new OptionalPattern(Element.Water, Element.Air).SetOptional(Element.Water, Element.Air))
		.AddTarget(pm => new Target((pm as OptionalPattern).OptionalFulfilled() ? TargetType.Enemies : TargetType.Target))
		.AddComponent(pm => new DamageComponent(2)));
	public static Spell Enlighten = Register("enlighten", new Spell("Enlighten", "Give a target +1 Die for their next turn.", new SimplePattern(Element.Light, 3))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new StatusComponent("+1 Die for one turn.",
		() => new CustomDurationStatus(Status.StatusType.Positive, 1,
		pawn => pawn.DieCount.AddModifier(new AttributeModifier("enlighten", AttributeModifier.Operation.AddBase, 1)),
		pawn => pawn.DieCount.RemoveModifier("enlighten")))));

	// Enemy Spells

	public static Spell Cuteness = Register("cuteness", new Spell("Cuteness", "Deal 1 damage.", new SimplePattern(Element.Physical, 1))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new DamageComponent(1)));
	public static Spell Bite = Register("bite", new Spell("Bite", "Deal 2 damage.", new SimplePattern(Element.Physical, 2))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new DamageComponent(2)));
	public static Spell Claw = Register("claw", new Spell("Claw", "Deal 2 damage to all enemies.", new SimplePattern(Element.Physical, 3))
		.AddTarget(pm => new Target(TargetType.Enemies))
		.AddComponent(pm => new DamageComponent(2)));
	public static Spell Slice = Register("slice", new Spell("Slice", "Deal 2 damage to all enemies.", new SimplePattern(Element.Physical, 3))
		.AddTarget(pm => new Target(TargetType.Enemies))
		.AddComponent(pm => new DamageComponent(2)));
	public static Spell Stomp = Register("stomp", new Spell("Stomp", "Deal 3 damage to all enemies.", new SimplePattern(Element.Physical, 4))
		.AddTarget(pm => new Target(TargetType.Enemies))
		.AddComponent(pm => new DamageComponent(3)));
	public static Spell Bash = Register("bash", new Spell("Bash", "Deal 1 damage and apply 1 Shock to all enemies.", new SimplePattern(Element.Physical, 3))
		.AddTarget(pm => new Target(TargetType.Enemies))
		.AddComponent(pm => new DamageComponent(1))
		.AddComponent(pm => new AilmentComponent(intensity => new ShockStatus(intensity), 1)));
	public static Spell LeadThePack = Register("lead_the_pack", new Spell("Lead the Pack", "Summon a Level 1 Sewer Rat.", new SimplePattern(Element.Physical, 2))
		.AddComponent(pm => new SummonComponent(PawnTemplates.SewerRat, 1, 0, 1)));
	public static Spell Stare = Register("stare", new Spell("Stare", "Apply 1 Shock.", new SimplePattern(Element.Dark, 3))
		.AddTarget(pm => new Target(TargetType.Target))
		.AddComponent(pm => new AilmentComponent(intensity => new ShockStatus(intensity), 1)));
	public static Spell Harden = Register("harden", new Spell("Harden", "Apply 2 Protect to yourself.", new SimplePattern(Element.Earth, Element.Physical))
		.AddTarget(pm => new Target(TargetType.Caster))
		.AddComponent(pm => new AilmentComponent(intensity => new ProtectStatus(intensity), 2)));

	public static Spell Register(string id, Spell spell) {
		spell.SetId(id);
		Registry.Register(spell);
		return spell;
	}

	public static Spell Get(string id) {
		return Registry.Get(id);
	}

	public static Spell[] GetCastableSpells() {
		return Registry.Where(spell => !(spell.GetPattern() is NullPattern) && !spell.IsElement(RollContext.Null, Element.Physical)).ToArray();
	}
}
