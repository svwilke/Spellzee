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

	public static Equipment Register(string id, Equipment equipment) {
		equipment.SetId(id);
		Registry.Register(equipment);
		return equipment;
	}

	public static Equipment Get(string id) {
		return Registry.Get(id);
	}
}
