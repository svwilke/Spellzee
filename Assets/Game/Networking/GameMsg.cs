using System.Collections;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine.Networking;

public class GameMsg {

	public static short Ready = MsgType.Highest + 1;
	public static short Unready = MsgType.Highest + 2;
	public static short PlayerJoined = MsgType.Highest + 3;
	public static short PlayerLobbyList = MsgType.Highest + 4;
	public static short PlayerLobbyUpdate = MsgType.Highest + 5;
	public static short ClassChange = MsgType.Highest + 6;

	public static short OpenWorld = MsgType.Highest + 7;
	public static short EndEncounter = MsgType.Highest + 8;
	public static short EnterDungeon = MsgType.Highest + 9;
	public static short ShowDungeon = MsgType.Highest + 10;

	public static short StartBattle = MsgType.Highest + 12;
	public static short Pass = MsgType.Highest + 13;
	public static short Roll = MsgType.Highest + 14;
	public static short ToggleDieLock = MsgType.Highest + 15;
	public static short CastSpell = MsgType.Highest + 16;
	public static short EndBattle = MsgType.Highest + 17;
	public static short TakeDamage = MsgType.Highest + 18;
	public static short Heal = MsgType.Highest + 19;
	public static short NextTurn = MsgType.Highest + 20;
	public static short UpdatePawn = MsgType.Highest + 21;
	public static short CastSpellEnd = MsgType.Highest + 22;
	public static short UpdateAilment = MsgType.Highest + 23;
	public static short SetupTurn = MsgType.Highest + 24;
	public static short EndGame = MsgType.Highest + 25;
	public static short Miss = MsgType.Highest + 26;
	public static short ShowMessage = MsgType.Highest + 27;
	public static short AddPawn = MsgType.Highest + 28;
	public static short RemovePawn = MsgType.Highest + 29;

	public static short OpenChoice = MsgType.Highest + 30;
	public static short OpenVendor = MsgType.Highest + 31;
	public static short ShopList = MsgType.Highest + 32;
	public static short DropSpell = MsgType.Highest + 33;
	public static short BuySpell = MsgType.Highest + 34;
	public static short SwapSpells = MsgType.Highest + 35;

	public class MsgPawn : MessageBase {
		public Pawn pawn;

		public override void Serialize(NetworkWriter writer) {
			pawn.Serialize(writer);
		}

		public override void Deserialize(NetworkReader reader) {
			pawn = Pawn.DeserializeNew(reader);
		}
	}

	public class MsgPlayerLobbyUpdate : MessageBase {
		public LobbyClientHandler.LobbyPlayer lobbyPlayer;
	}

	public class MsgPlayerLobbyList : MessageBase {
		public int clientId;
		public LobbyClientHandler.LobbyPlayer[] lobbyPlayerList;
	}

	public class MsgStartBattle : MessageBase {
		public Battle battle;

		public override void Serialize(NetworkWriter writer) {
			battle.Serialize(writer);
		}

		public override void Deserialize(NetworkReader reader) {
			battle = Battle.DeserializeNew(reader);
		}
	}

	public class MsgIntegerArray : MessageBase {
		public int[] array;

		public MsgIntegerArray() { }

		public MsgIntegerArray(params int[] values) {
			array = values;
		}
	}

	public class MsgStringArray : MessageBase {
		public string[] array;

		public MsgStringArray() { }

		public MsgStringArray(params string[] values) {
			array = values;
		}
	}

	public class MsgStatusList : MessageBase {
		public int pawnId;
		public List<Status> statuses;

		public override void Serialize(NetworkWriter writer) {
			writer.Write(pawnId);
			writer.Write(statuses.Count);
			foreach(Status status in statuses) {
				status.Serialize(writer);
			}
		}

		public override void Deserialize(NetworkReader reader) {
			pawnId = reader.ReadInt32();
			int statusCount = reader.ReadInt32();
			statuses = new List<Status>(statusCount);
			for(int i = 0; i < statusCount; i++) {
				statuses.Add(Status.DeserializeNew(reader));
			}
		}
	}

	public class MsgCastSpell : MessageBase {
		public string spellId;
		public int targetId = -1;
	}

	public class MsgDungeonList : MessageBase {
		public List<string[]> dungeonPaths;

		public override void Serialize(NetworkWriter writer) {
			writer.WriteBytesFull(SerializationUtility.SerializeValue(dungeonPaths, DataFormat.Binary));
		}

		public override void Deserialize(NetworkReader reader) {
			dungeonPaths = SerializationUtility.DeserializeValue<List<string[]>>(reader.ReadBytesAndSize(), DataFormat.Binary);
		}
	}
}
