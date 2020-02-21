using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using static LobbyClientHandler;

public class LobbyServerHandler : ServerHandler {

	private List<LobbyPlayer> lobbyPlayers = new List<LobbyClientHandler.LobbyPlayer>();

	public LobbyServerHandler() {
		AddHandler(MsgType.Connect, OnConnect);
		AddHandler(MsgType.Disconnect, OnDisconnect);
		AddHandler(GameMsg.Ready, OnClientReady);
		AddHandler(GameMsg.Unready, OnClientUnready);
		AddHandler(GameMsg.PlayerLobbyUpdate, OnPlayerLobbyUpdate);
	}

	public void OnConnect(NetworkMessage msg) {
		LobbyPlayer lobbyPlayer = new LobbyPlayer() { id = msg.conn.connectionId, charName = REX.Choice(DB.CharNames) };
		lobbyPlayers.Add(lobbyPlayer);
		GameMsg.MsgPlayerLobbyUpdate playerLobbyUpdate = new GameMsg.MsgPlayerLobbyUpdate() { lobbyPlayer = lobbyPlayer };
		for(int i = 0; i < NetworkServer.connections.Count; i++) {
			if(i == msg.conn.connectionId) {
				LobbyPlayer[] clients = lobbyPlayers.ToArray();
				GameMsg.MsgPlayerLobbyList lobbyList = new GameMsg.MsgPlayerLobbyList() { lobbyPlayerList = clients, clientId = i };
				NetworkServer.connections[i].Send(GameMsg.PlayerLobbyList, lobbyList);
			} else {
				NetworkServer.connections[i].Send(GameMsg.PlayerLobbyUpdate, playerLobbyUpdate);
			}
		}
	}

	public void OnDisconnect(NetworkMessage msg) {
		LobbyPlayer discPlayer = new LobbyPlayer() { id = -1 };
		foreach(LobbyPlayer lp in lobbyPlayers) {
			if(lp.id == msg.conn.connectionId) {
				discPlayer = lp;
			}
		}
		if(discPlayer.id > -1) {
			lobbyPlayers.Remove(discPlayer);
		}
		LobbyPlayer[] clients = lobbyPlayers.ToArray();
		for(int i = 0; i < NetworkServer.connections.Count; i++) {
			GameMsg.MsgPlayerLobbyList lobbyList = new GameMsg.MsgPlayerLobbyList() { lobbyPlayerList = clients, clientId = i };
			if(NetworkServer.connections[i] != null) {
				NetworkServer.connections[i].Send(GameMsg.PlayerLobbyList, lobbyList);
			}
		}
	}

	public void OnClientReady(NetworkMessage msg) {
		int client = msg.conn.connectionId;
		LobbyPlayer oldPlayer = lobbyPlayers[client];
		LobbyPlayer readyPlayer = new LobbyPlayer() { ready = true, charClass = oldPlayer.charClass, charName = oldPlayer.charName, id = oldPlayer.id };
		lobbyPlayers[client] = readyPlayer;
		NetworkServer.SendToAll(GameMsg.Ready, new IntegerMessage(msg.conn.connectionId));
	}

	public void OnClientUnready(NetworkMessage msg) {
		int client = msg.conn.connectionId;
		LobbyPlayer oldPlayer = lobbyPlayers[client];
		LobbyPlayer readyPlayer = new LobbyPlayer() { ready = false, charClass = oldPlayer.charClass, charName = oldPlayer.charName, id = oldPlayer.id };
		lobbyPlayers[client] = readyPlayer;
		NetworkServer.SendToAll(GameMsg.Unready, new IntegerMessage(msg.conn.connectionId));
	}

	public void OnPlayerLobbyUpdate(NetworkMessage msg) {
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
		NetworkServer.SendToAll(GameMsg.PlayerLobbyUpdate, update);
	}
}
