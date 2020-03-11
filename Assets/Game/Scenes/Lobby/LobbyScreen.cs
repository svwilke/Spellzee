using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;
using static LobbyClientHandler;

public class LobbyScreen : Screen {

	private int lastPlayerCount;
	private int[] playerCardPos;
	private int[] desiredPlayerCardPos;

	private TextButton readyButton;
	private TextButton startButton;
	private InputField nameInput;
	private TextButton prevClassButton;
	private TextButton nextClassButton;

	private LobbyClientHandler lobby;

	private PawnTemplate[] playerTemplates;

	public LobbyScreen(Game game, Vector2i size, LobbyClientHandler lobby) : base(game, size) {
		playerTemplates = PawnTemplates.GetPlayableClasses();
		playerCardPos = new int[4];
		desiredPlayerCardPos = new int[4];
		lastPlayerCount = 0;
		for(int i = 0; i < 4; i++) {
			playerCardPos[i] = desiredPlayerCardPos[i] = size.x / 2;
		}
		this.lobby = lobby;
	}

	public override void OnConstruct() {
		TextButton btn;
		AddUIObj(btn = new TextButton(new Vector2i(30, RB.DisplaySize.height - 30), "Back to menu"));
		btn.SetOnClick(() => {
			game.CancelConnection();
			game.OpenScreen(new MainScreen(game, size));
		});
		AddUIObj(startButton = new TextButton(new Vector2i(RB.DisplaySize.width - 30, RB.DisplaySize.height - 30), "Start Game", RB.ALIGN_H_RIGHT | RB.ALIGN_V_CENTER));
		startButton.currentState = UIObj.State.Disabled;
		startButton.SetOnClick(() => {
			game.StartGame(lobby.GetLobbyPlayers());
		});
		if(!game.IsHost()) {
			startButton.isVisible = false;
		}
		AddUIObj(readyButton = new TextButton(new Vector2i(30, RB.DisplaySize.height - 30), "Ready?"));
		readyButton.isVisible = false;
		readyButton.SetOnClick(() => {
			if(lobby.GetLobbyPlayer(Game.peerId).ready) {
				Game.client.Send(GameMsg.Unready, new EmptyMessage());
			} else {
				Game.client.Send(GameMsg.Ready, new EmptyMessage());
			}
		});
		AddUIObj(nameInput = new InputField(new Vector2i(-100, 82), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, 54));
		nameInput.isVisible = false;
		nameInput.SetOnCompleteEdit((name) => {
			LobbyPlayer oldPlayer = lobby.GetLobbyPlayer(Game.peerId);
			LobbyPlayer updatedPlayer = new LobbyPlayer() { charClass = oldPlayer.charClass, charName = name.ToString(), id = oldPlayer.id, ready = false };
			Game.client.Send(GameMsg.PlayerLobbyUpdate, new GameMsg.MsgPlayerLobbyUpdate() { lobbyPlayer = updatedPlayer });
		});

		AddUIObj(prevClassButton = new TextButton(new Vector2i(0, 0), "<", RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER));
		prevClassButton.isVisible = false;
		prevClassButton.SetOnClick(() => {
			int cls = lobby.GetLobbyPlayer(Game.peerId).charClass;
			cls = (cls - 1 + playerTemplates.Length) % playerTemplates.Length;
			LobbyPlayer oldPlayer = lobby.GetLobbyPlayer(Game.peerId);
			LobbyPlayer updatedPlayer = new LobbyPlayer() { charClass = cls, charName = oldPlayer.charName, id = oldPlayer.id, ready = false };
			Game.client.Send(GameMsg.PlayerLobbyUpdate, new GameMsg.MsgPlayerLobbyUpdate() { lobbyPlayer = updatedPlayer });
		});

		AddUIObj(nextClassButton = new TextButton(new Vector2i(0, 0), ">", RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER));
		nextClassButton.isVisible = false;
		nextClassButton.SetOnClick(() => {
			int cls = lobby.GetLobbyPlayer(Game.peerId).charClass;
			cls = (cls + 1) % playerTemplates.Length;
			LobbyPlayer oldPlayer = lobby.GetLobbyPlayer(Game.peerId);
			LobbyPlayer updatedPlayer = new LobbyPlayer() { charClass = cls, charName = oldPlayer.charName, id = oldPlayer.id, ready = false };
			Game.client.Send(GameMsg.PlayerLobbyUpdate, new GameMsg.MsgPlayerLobbyUpdate() { lobbyPlayer = updatedPlayer });
		});
	}

	public override void Render() {
		int cardWidth = size.x / 5 - 6;
		for(int i = 0; i < lastPlayerCount && i < lobby.GetLobbyPlayerCount(); i++) {
			RenderCard(playerCardPos[i], cardWidth, lobby.GetLobbyPlayer(i), i == Game.peerId);
		}
		base.Render();
	}

	private void RenderCard(int xPos, int w, LobbyPlayer player, bool self) {
		int topLeftX = xPos - w / 2 + 2;
		int topLeftY = 32;
		RB.DrawRectFill(new Rect2i(topLeftX - 1, topLeftY - 1, new Vector2i(w, size.height - 90)), Color.black);
		RB.DrawRectFill(new Rect2i(topLeftX - 2, topLeftY - 2, new Vector2i(w, size.height - 90)), Color.green);
		RB.DrawRect(new Rect2i(topLeftX, topLeftY, new Vector2i(w - 4, size.height - 94)), Color.gray);
		if(self) {
			if(!nameInput.isVisible) {
				nameInput.SetText(player.charName);
			}
			readyButton.isVisible = true;
			nameInput.isVisible = true;
			prevClassButton.isVisible = true;
			nextClassButton.isVisible = true;
		} else {
			RB.Print(new Rect2i(topLeftX + 1, topLeftY + 49, w - 4, 12), Color.black, RB.ALIGN_H_CENTER | RB.ALIGN_V_TOP, player.charName);
			RB.Print(new Rect2i(topLeftX, topLeftY + 48, w - 4, 12), Color.white, RB.ALIGN_H_CENTER | RB.ALIGN_V_TOP, player.charName);
			RB.Print(new Vector2i(xPos - 26, topLeftY + size.height - 115), Color.black, RB.NO_INLINE_COLOR, "Ready?");
			RB.Print(new Vector2i(xPos - 27, topLeftY + size.height - 116), Color.white, "Ready?");
		}

		RB.Print(new Rect2i(topLeftX + 11, topLeftY + 71, w - 24, 20), Color.black, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, playerTemplates[player.charClass].GetName());
		RB.Print(new Rect2i(topLeftX + 10, topLeftY + 70, w - 24, 20), Color.white, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, playerTemplates[player.charClass].GetName());
		RB.DrawEllipseFill(new Vector2i(xPos + 16, topLeftY - 2 + size.height - 110), new Vector2i(5, 5), Color.black);
		RB.DrawEllipseFill(new Vector2i(xPos + 15, topLeftY - 3 + size.height - 110), new Vector2i(5, 5), player.ready ? Color.green : Color.red);
		RB.DrawEllipse(new Vector2i(xPos + 15, topLeftY - 3 + size.height - 110), new Vector2i(5, 5), Color.white);
	}

	public override void Update(bool hasFocus = true) {
		base.Update(hasFocus);
		int playerCount = lobby.GetLobbyPlayerCount();
		if(playerCount != lastPlayerCount) {
			int spacing = size.width / (playerCount + 1);
			for(int i = 0; i < playerCount; i++) {
				desiredPlayerCardPos[i] = spacing + (i * spacing);
				if(Game.peerId == i) {
					int top = 32;
					nameInput.SetPosition(new Vector2i(playerCardPos[i], top + 50), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER);
					readyButton.SetPosition(new Vector2i(playerCardPos[i] - 30, top - 2 + size.height - 110));
				}
			}
			for(int i = lastPlayerCount + 1; i < playerCount; i++) {
				playerCardPos[i] = size.width + size.width / 5;
			}
			lastPlayerCount = playerCount;
		} else {
			bool isHostAndAllReady = game.IsHost();
			for(int i = 0; i < playerCount; i++) {
				isHostAndAllReady &= lobby.GetLobbyPlayer(i).ready;
				if(desiredPlayerCardPos[i] != playerCardPos[i]) {
					playerCardPos[i] += (desiredPlayerCardPos[i] - playerCardPos[i]) / 10;
				}
				if(Game.peerId == i) {
					int top = 32;
					nameInput.SetPosition(new Vector2i(playerCardPos[i], top + 50), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER);
					readyButton.SetPosition(new Vector2i(playerCardPos[i] - 30, top - 2 + size.height - 110));
					prevClassButton.SetPosition(new Vector2i(playerCardPos[i] - 30, top + 80), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER);
					nextClassButton.SetPosition(new Vector2i(playerCardPos[i] + 30, top + 80), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER);
				}
			}
			if(isHostAndAllReady && startButton.currentState == UIObj.State.Disabled) {
				startButton.currentState = UIObj.State.Enabled;
			} else
			if(!isHostAndAllReady && startButton.currentState != UIObj.State.Disabled) {
				startButton.currentState = UIObj.State.Disabled;
			}
		}
	}
}
