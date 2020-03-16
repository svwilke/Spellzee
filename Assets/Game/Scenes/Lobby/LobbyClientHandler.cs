using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class LobbyClientHandler : ClientHandler
{
	private Game game;

	public struct LobbyPlayer
	{
		public int id;
		public string charName;
		public int charClass;
		public bool ready;
	}

	private List<LobbyPlayer> lobbyPlayers = new List<LobbyPlayer>();

	public LobbyClientHandler(Game game) {
		this.game = game;
		AddHandler(GameMsg.Ready, OnClientReady);
		AddHandler(GameMsg.Unready, OnClientUnready);
		AddHandler(GameMsg.PlayerLobbyList, OnLobbyList);
		AddHandler(GameMsg.PlayerLobbyUpdate, OnLobbyPlayerUpdate);
		AddHandler(MsgType.Disconnect, OnDisconnect);
		AddHandler(GameMsg.OpenWorld, OnOpenWorld);
	}

	public void OnDisconnect(NetworkMessage msg) {
		game.OpenScreen(new MainScreen(game, RB.DisplaySize));
	}

	public void OnLobbyList(NetworkMessage msg) {
		GameMsg.MsgPlayerLobbyList list = msg.ReadMessage<GameMsg.MsgPlayerLobbyList>();
		lobbyPlayers.Clear();
		lobbyPlayers.AddRange(list.lobbyPlayerList);
		Game.peerId = list.clientId;
	}

	public void OnLobbyPlayerUpdate(NetworkMessage msg) {
		GameMsg.MsgPlayerLobbyUpdate update = msg.ReadMessage<GameMsg.MsgPlayerLobbyUpdate>();
		LobbyPlayer lpWithId = new LobbyPlayer() { id = -1 };
		foreach(LobbyPlayer lobbyPlayer in lobbyPlayers) {
			if(lobbyPlayer.id == update.lobbyPlayer.id) {
				lpWithId = lobbyPlayer;
			}
		}
		if(lpWithId.id == update.lobbyPlayer.id) {
			lpWithId.charClass = update.lobbyPlayer.charClass;
			lpWithId.charName = update.lobbyPlayer.charName;
			lobbyPlayers[lpWithId.id] = lpWithId;
		} else {
			lobbyPlayers.Add(update.lobbyPlayer);
		}
	}

	public void OnClientReady(NetworkMessage msg) {
		int client = msg.ReadMessage<IntegerMessage>().value;
		LobbyPlayer oldPlayer = lobbyPlayers[client];
		LobbyPlayer readyPlayer = new LobbyPlayer() { ready = true, charClass = oldPlayer.charClass, charName = oldPlayer.charName, id = oldPlayer.id };
		lobbyPlayers[client] = readyPlayer;
	}

	public void OnClientUnready(NetworkMessage msg) {
		int client = msg.ReadMessage<IntegerMessage>().value;
		LobbyPlayer oldPlayer = lobbyPlayers[client];
		LobbyPlayer readyPlayer = new LobbyPlayer() { ready = false, charClass = oldPlayer.charClass, charName = oldPlayer.charName, id = oldPlayer.id };
		lobbyPlayers[client] = readyPlayer;
	}

	public void OnBattleStart(NetworkMessage msg) {
		Battle battle = msg.ReadMessage<GameMsg.MsgStartBattle>().battle;
		game.OpenScreen(new BattleScreen(game, RB.DisplaySize, battle));
		game.OpenClientHandler(new BattleClientHandler(game, battle));
	}

	public void OnOpenWorld(NetworkMessage msg) {
		WorldScreen screen = new WorldScreen(game, RB.DisplaySize);
		game.OpenScreen(screen);
		game.OpenClientHandler(new WorldClientHandler(game, screen));
	}

	public LobbyPlayer GetLobbyPlayer(int id) {
		return lobbyPlayers[id];
	}

	public LobbyPlayer[] GetLobbyPlayers() {
		return lobbyPlayers.ToArray();
	}

	public int GetLobbyPlayerCount() {
		return lobbyPlayers.Count;
	}
}
