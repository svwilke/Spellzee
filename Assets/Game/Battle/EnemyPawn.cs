using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class EnemyPawn : Pawn {

	public float spellWeight = 8F;
	public float passWeight = 1F;
	public float useItemWeight = 0F;

	public EnemyPawn(string name, int maxHp) : base(name, maxHp) {
		//DieCount.AddModifier(new AttributeModifier(AttributeModifier.Operation.AddBase, 2));
	}

	public bool DoTurn(ServerBattle battle) {
		/*int action = REX.Weighted(new float[] { spellWeight, passWeight, useItemWeight });
		switch(action) {
			case 0:
				CastSpell(battle);
				break;
			case 1:
				NetworkServer.SendToAll(GameMsg.Pass, new EmptyMessage());
				battle.NextTurn();
				break;
			case 2:
				Debug.Log("Enemy UseItem NYI");
				break;
			default:
				Debug.Log("Unknown AI Action ID");
				break;
		}*/
		List<Spell> castable = GetPossibleSpells(battle.BuildContext());
		if(castable.Count == 0 && battle.rollsLeft > 0) {
			Pawn pawn = battle.GetCurrentPawn();
			int[] rolls = new int[battle.rolls.Length];
			double[] weights = new double[pawn.Affinities.Length];
			for(int i = 0; i < weights.Length; i++) {
				weights[i] = pawn.GetAffinity(i);
			}
			for(int i = 0; i < rolls.Length; i++) {
				if(battle.locks[i]) {
					rolls[i] = battle.rolls[i].GetId();
				} else {
					rolls[i] = Element.All[REX.Weighted(weights)].GetId();
				}
				battle.rolls[i] = Element.All[rolls[i]];
			}
			battle.rollsLeft -= 1;
			NetworkServer.SendToAll(GameMsg.Roll, new GameMsg.MsgIntegerArray() { array = rolls });
		} else
		if(castable.Count > 0) {
			CastSpell(battle, REX.Choice(castable));
			return false;
		} else {
			NetworkServer.SendToAll(GameMsg.Pass, new EmptyMessage());
			battle.NextTurn();
			return false;
		}
		return true;
	}

	public List<Spell> GetPossibleSpells(RollContext context) {
		return GetKnownSpellIds().Select(Spells.Get).Where(spell => spell.Matches(context)).ToList();
	}

	private void CastSpell(ServerBattle battle, Spell spell, SpellComponent.TargetGroup targetGroup = SpellComponent.TargetGroup.Any) {
		string spellId = spell.GetId();
		int targetId = -1;
		RollContext context = battle.BuildContext();
		if(spell.DoesRequireTarget(context)) {
			List<int> possibleTargets = new List<int>();
			for(int i = 0; i < battle.allies.Length; i++) {
				if(battle.allies[i].IsAlive() && spell.IsValidTarget(battle.allies[i], context)) {
					possibleTargets.Add(battle.allies[i].GetId());
				}
			}
			targetId = REX.Choice(possibleTargets);
		}
		battle.CastSpell(spellId, targetId);
	}
}
