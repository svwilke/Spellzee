using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine;

public class LockingAI : AIModule {

	private Spell chosenSpell = null;

	public LockingAI(Pawn pawn) : base(pawn) { }

	public override bool DoTurn(ServerBattle battle) {
		
		Spell[] knownSpells = pawn.GetSpells();
		List<Spell> castable = GetPossibleSpells(battle.BuildContext());
		if(castable.Count == 0 && battle.rollsLeft > 0 && knownSpells.Length > 0) {
			Spell spellToLockFor = null;
			int minDistance = int.MaxValue;
			List<Spell> closestSpells = new List<Spell>();
			foreach(Spell spell in knownSpells) {
				int d = spell.GetPattern().GetDistance();
				if(d < minDistance) {
					closestSpells.Clear();
					closestSpells.Add(spell);
					minDistance = d;
				} else
				if(d == minDistance) {
					closestSpells.Add(spell);
				}
			}
			if(chosenSpell == null || !closestSpells.Contains(chosenSpell)) {
				chosenSpell = spellToLockFor = REX.Choice(closestSpells);
			} else {
				spellToLockFor = chosenSpell;
			}
			HashSet<int> matchingDice = spellToLockFor.GetPattern().GetMatchingDiceIndices();
			bool hasChangedLocks = false;
			for(int i = 0; i < battle.locks.Length; i++) {
				if(!battle.locks[i] && matchingDice.Contains(i)) {
					if(battle.ToggleLock(i)) {
						hasChangedLocks = true;
					}
				} else
				if(battle.locks[i] && !matchingDice.Contains(i)) {
					if(battle.ToggleLock(i)) {
						hasChangedLocks = true;
					}
				}
			}
			if(!hasChangedLocks) {
				battle.Roll();
				chosenSpell = null;
			}
		} else
		if(castable.Count > 0) {
			int[] weights = castable.Select(s => (int)Mathf.Pow(s.GetPattern().GetMatchingDiceIndices().Count, 4)).ToArray();
			CastSpell(battle, REX.Weighted(weights, castable.ToArray()));
			chosenSpell = null;
			return false;
		} else {
			NetworkServer.SendToAll(GameMsg.Pass, new EmptyMessage());
			battle.NextTurn();
			chosenSpell = null;
			return false;
		}
		return true;
	}
}
