using System.Linq;
using System.Collections.Generic;

public class WildMageProgression : PawnProgression {

	public override List<Spell> GetAccumulatedSpells(int level) {
		return Spells.GetCastableSpells().ToList();
	}

	public override List<Spell> GetSpells(int level) {
		return Spells.GetCastableSpells().ToList();
	}
}
