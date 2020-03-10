using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIModule {

	protected Pawn pawn;

	public AIModule(Pawn pawn) {
		this.pawn = pawn;
	}

	public abstract bool DoTurn(ServerBattle battle);

	protected List<Spell> GetPossibleSpells(RollContext context) {
		return pawn.GetKnownSpellIds().Select(Spells.Get).Where(spell => spell.Matches(context)).ToList();
	}

	protected void CastSpell(ServerBattle battle, Spell spell, SpellComponent.TargetGroup targetGroup = SpellComponent.TargetGroup.Any) {
		string spellId = spell.GetId();
		int targetId = -1;
		RollContext context = battle.BuildContext();
		if(spell.DoesRequireTarget(context)) {
			List<int> possibleTargets = battle.GetCurrentTargets().Where(pawn => pawn.IsAlive() && spell.IsValidTarget(pawn, context)).Select(pawn => pawn.GetId()).ToList();
			targetId = REX.Choice(possibleTargets);
		}
		battle.CastSpell(spellId, targetId);
	}
}
