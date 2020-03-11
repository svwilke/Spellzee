using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipments {

	private static Registry<Equipment> Registry = new Registry<Equipment>();

	public static Equipment CharmOfPerfection = Register("charm_of_perfection", new PerfectionCharm("Charm of Perfection"));
	public static Equipment SolarOrb = Register("solar_orb", new AffinityEquipment("Solar Orb", AttributeModifier.Operation.AddBase, 0, 0, 0, 0, 5, 0, 0));
	public static Equipment LunarOrb = Register("lunar_orb", new AffinityEquipment("Lunar Orb", AttributeModifier.Operation.AddBase, 0, 0, 0, 0, 0, 5, 0));
	public static Equipment ElementalFocus = Register("elemental_focus", new AffinityEquipment("Elemental Focus", AttributeModifier.Operation.AddBase, 2, 2, 2, 2, 0, 0, 0));
	public static Equipment TalismanOfDisorder = Register("talisman_of_disorder", new DieTalisman("Talisman of Disorder"));
	public static Equipment RingOfRestoration = Register("ring_of_restoration", new RestoreRing("Ring of Restoration"));
	public static Equipment HealCharm = Register("heal_charm", new SpellEquipment("Heal Charm", "Spells restoring life to you restore 1 additional life.", false, (spell, context, sc) => {
		if(sc is HealComponent) {
			(sc as HealComponent).AddModifier(AttributeModifier.Operation.AddTotal, 1);
		}
	}));
	public static Equipment IgneousRock = Register("igneous_rock", new IgniteRock("Igneous Rock", "Convert 1 damage of your Fire spells to 1 Burn."));
	public static Equipment ChaosGift = Register("chaos_gift", new ChaosGift("Gift of Chaos"));
	public static Equipment HeadOfTheHydra = Register("head_of_the_hydra", new Equipment("Head of the Hydra", "When you die, you are resurrected with 50% of your maximum life. Works only once."));

	public static Equipment Register(string id, Equipment equipment) {
		equipment.SetId(id);
		Registry.Register(equipment);
		return equipment;
	}

	public static Equipment Get(string id) {
		return Registry.Get(id);
	}
}
