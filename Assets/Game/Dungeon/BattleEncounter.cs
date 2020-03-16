using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;

public class BattleEncounter : Encounter {

	protected List<Pawn> enemies;

	public BattleEncounter(IEnumerable<Pawn> enemies) {
		this.enemies = enemies.ToList();
	}

	public List<Pawn> GetEnemies() {
		return new List<Pawn>(enemies);
	}

	protected override void OnEncounterBegin() {
		ServerBattle battle = new ServerBattle(game);
		foreach(Pawn p in players) {
			battle.AddPawn(p);
		}
		foreach(Pawn p in enemies) {
			battle.AddPawn(p);
		}
		NetworkServer.SendToAll(GameMsg.StartBattle, new GameMsg.MsgStartBattle() { battle = battle });
		game.OpenServerHandler(new BattleServerHandler(game, this, battle));
	}

	protected override void OnEncounterEnd() {
		
	}
}
