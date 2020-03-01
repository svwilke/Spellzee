using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class EnemyPawn : Pawn {

	public float spellWeight = 8F;
	public float passWeight = 1F;
	public float useItemWeight = 0F;

	public EnemyPawn(string name, int maxHp) : base(name, maxHp) {
		
	}

	public void DoTurn(ServerBattle battle) {
		int action = REX.Weighted(new float[] { spellWeight, passWeight, useItemWeight });
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
		}
		
	}

	public override void Update(Pawn pawn) {
		base.Update(pawn);
		if(pawn is EnemyPawn) {
			EnemyPawn other = pawn as EnemyPawn;
			spellWeight = other.spellWeight;
			passWeight = other.passWeight;
			useItemWeight = other.useItemWeight;
		}
	}

	private void CastSpell(ServerBattle battle) {
		Spell spell = REX.Choice(GetSpells());
		string spellId = spell.GetId();
		int targetId = -1;
		if(spell.DoesRequireTarget()) {
			List<int> possibleTargets = new List<int>();
			for(int i = 0; i < battle.allies.Length; i++) {
				if(battle.allies[i].IsAlive()) {
					possibleTargets.Add(battle.allies[i].GetId());
				}
			}
			targetId = REX.Choice(possibleTargets);
		}
		battle.CastSpell(spellId, targetId);
	}
}
