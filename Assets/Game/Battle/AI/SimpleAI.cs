using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class SimpleAI : AIModule {

	public SimpleAI(Pawn pawn) : base(pawn) { }

	public override bool DoTurn(ServerBattle battle) {
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
}
