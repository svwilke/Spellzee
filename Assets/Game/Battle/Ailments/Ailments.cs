using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ailments {

	private static Registry<Ailment> Registry = new Registry<Ailment>();

	public static Ailment Burn = Register("burn", new BurnAilment("Burn", "Brn", Color.red));
	public static Ailment Blind = Register("blind", new BlindAilment("Blind", "Bld", Element.Light.GetColor()));
	public static Ailment Regen = Register("regen", new RegenAilment("Regen", "Rgn", Element.Earth.GetColor()));
	public static Ailment Protect = Register("protect", new ProtectAilment("Protect", "Prt", Element.Water.GetColor()));

	public static Ailment Register(string id, Ailment ailment) {
		ailment.SetId(id);
		Registry.Register(ailment);
		return ailment;
	}

	public static Ailment Get(string id) {
		return Registry.Get(id);
	}
}
