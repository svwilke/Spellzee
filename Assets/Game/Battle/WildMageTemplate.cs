using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildMageTemplate : PlayerTemplate {

	public WildMageTemplate(string name) : base(name) {

	}

	public override PlayerPawn Create(LobbyClientHandler.LobbyPlayer lobbyPlayer) {
		PlayerPawn player = new PlayerPawn(lobbyPlayer.charName, maxHp);
		player.SetId(lobbyPlayer.id);
		int[] affinityCounts = new int[Element.Count];
		Spell[] spellsToLearn = REX.Choice(Spells.GetCastableSpells(), 4);
		for(int i = 0; i < spellsToLearn.Length; i++) {
			ElementDisplay[] d = spellsToLearn[i].GetElementDisplays(RollContext.Null);
			for(int j = 0; j < d.Length; j++) {
				affinityCounts[d[j].element.GetId()]++;
			}
		}
		foreach(Spell spell in spellsToLearn) {
			player.AddSpell(spell);
		}
		for(int i = 0; i < Element.Count; i++) {
			if(affinityCounts[i] > 0) {
				player.Affinities[i].AddModifier(new AttributeModifier(GetName(), AttributeModifier.Operation.AddBase, affinityCounts[i]));
			}
		}
		return player;
	}
}
