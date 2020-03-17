using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class WorldServerHandler : ServerHandler {

	protected Game game;
	private List<Pawn> players;
	private List<Dungeon> dungeons;
	private List<string[]> dungeonPaths;

	public WorldServerHandler(Game game, List<Pawn> players) {
		this.game = game;
		this.players = players;
		dungeons = new List<Dungeon>();
		dungeonPaths = new List<string[]>();
	}

	public void EnterDungeon(int dungeonIndex) {
		NetworkServer.SendToAll(GameMsg.EnterDungeon, new GameMsg.MsgStringArray(new string[] { DungeonTemplates.GetPlayableDungeons()[dungeonIndex].GetId() }.Concat(dungeonPaths[dungeonIndex]).ToArray()));
		game.EnterDungeon(dungeons[dungeonIndex], players);
	}

	public override void Open() {
		base.Open();
		foreach(DungeonTemplate template in DungeonTemplates.GetPlayableDungeons()) {
			Dungeon dungeon = template.Create(players);
			dungeons.Add(dungeon);
			List<Encounter> encounters = dungeon.GetEncounters();
			string[] path = new string[encounters.Count];
			for(int i = 0; i < path.Length; i++) {
				Encounter e = encounters[i];
				if(e is BattleEncounter) {
					BattleEncounter battle = e as BattleEncounter;
					bool isBoss = battle.GetEnemies().Any(pawn => pawn.GetName().Equals("Golem") || pawn.GetName().Equals("Rat King") || pawn.GetName().Equals("Master Thief"));
					path[i] = isBoss ? "boss" : "battle";
				} else
				if(e is VendorEncounter || e is ChoiceEncounter) {
					path[i] = "loot";
				} else {
					path[i] = "mystery";
				}
			}
			dungeonPaths.Add(path);
		}
		foreach(Pawn pawn in players) {
			pawn.FullRestore();
		}
		NetworkServer.SendToAll(GameMsg.OpenWorld, new GameMsg.MsgDungeonList() { dungeonPaths = dungeonPaths });
	}
}
