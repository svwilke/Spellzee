using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class DungeonTemplates {

	private static Registry<DungeonTemplate> Registry = new Registry<DungeonTemplate>();

	public static DungeonTemplate Testing = Register("testing", new DungeonTemplate("Rewards!", "Development mode only. Choice and Vendor. :)")
		.AddEncounter((players) => new ChoiceEncounter())
		.AddEncounter((players) => new VendorEncounter()));

	public static DungeonTemplate Training = Register("training", new DungeonTemplate("Training Grounds", "Test your spells against this training dummy. He won't hurt you!")
		.AddEncounter((players) => new BattleEncounter(new Pawn[] { PawnTemplates.Dummy.Create(players.Count, 0, Pawn.Team.Hostile).SetAI(new PassAI()) })));

	public static DungeonTemplate Forest = Register("forest", new DungeonTemplate("Northern Forest", "A dark forest filled with threats awaits you.")
		.AddEncounter((players) => new BattleEncounter(new Pawn[] { PawnTemplates.Pixie.Create(players.Count, 0, Pawn.Team.Hostile) }))
		.AddEncounter((players) => new BattleEncounter(new Pawn[] { PawnTemplates.Wolf.Create(players.Count, 0, Pawn.Team.Hostile) }))
		.AddEncounter((players) => new BattleEncounter(new Pawn[] { PawnTemplates.Cutpurse.Create(players.Count, 0, Pawn.Team.Hostile) }))
		.AddEncounter((players) => new BattleEncounter(new Pawn[] { PawnTemplates.Golem.Create(players.Count, 0, Pawn.Team.Hostile) }))
		.AddEncounter((players) => new ChoiceEncounter()));

	public static DungeonTemplate Sewer = Register("sewer", new DungeonTemplate("Town Sewers", "What could hide in these filthy waters?")
		.AddEncounter((players) => {
			int lvl = players.Select(p => p.Level).Aggregate((a, b) => a + b);
			List<Pawn> enemies = new List<Pawn>();
			for(int i = 0; i < players.Count; i++) {
				Pawn rat = PawnTemplates.SewerRat.Create(1, lvl, Pawn.Team.Hostile);
				rat.HitChance.AddModifier(new AttributeModifier(AttributeModifier.Operation.SubtractBase, 0.1));
				enemies.Add(rat);
			}
			return new BattleEncounter(enemies);
		})
		.AddEncounter((players) => new BattleEncounter(new Pawn[] { PawnTemplates.RatKing.Create(players.Count, players.Select(p => p.Level).Min(), Pawn.Team.Hostile) })));

	public static DungeonTemplate Cave = Register("cave", new DungeonTemplate("Cavern", "Not many adventurers make it out alive.")
		.AddEncounter((players) => new BattleEncounter(new Pawn[] { PawnTemplates.ScarySpider.Create(players.Count, 0, Pawn.Team.Hostile) }))
		.AddEncounter((players) => new BattleEncounter(new Pawn[] { PawnTemplates.Golem.Create(players.Count, 2, Pawn.Team.Hostile) }))
		.AddEncounter((players) => new VendorEncounter())
		.AddEncounter((players) => new BattleEncounter(new Pawn[] { PawnTemplates.Golem.Create(players.Count, 2, Pawn.Team.Hostile) }))
		.AddEncounter((players) => new ChoiceEncounter())
		.AddEncounter((players) => new VendorEncounter()));

	public static DungeonTemplate Register(string id, DungeonTemplate dungeonTemplate) {
		dungeonTemplate.SetId(id);
		Registry.Register(dungeonTemplate);
		return dungeonTemplate;
	}

	public static DungeonTemplate Get(string id) {
		return Registry.Get(id);
	}

	public static DungeonTemplate[] GetPlayableDungeons() {
		return new DungeonTemplate[] { Forest, Sewer, Cave };
	}
}
