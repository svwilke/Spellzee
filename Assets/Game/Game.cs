﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using UnityEngine.Networking.NetworkSystem;
using System.Collections;

public class Game : RB.IRetroBlitGame
{
	public static int[] Widths = new int[] { 480, 960, 1440, 1920, 2400, 2880, 3360, 3840 };
	public static int[] Heights = new int[] { 270, 540, 810, 1080, 1350, 1620, 1890, 2160 };

	private ClientHandler clientHandler;
	private ServerHandler serverHandler;

	public static int peerId;

	public static NetworkClient client;

	private Dungeon currentDungeon;

	private ConnectionConfig networkConfig = new ConnectionConfig();

	private MonoBehaviour gameBinding;

	public Game(MonoBehaviour binding) {
		gameBinding = binding;
	}

	public RB.HardwareSettings QueryHardware() {
		RB.HardwareSettings settings = new RB.HardwareSettings();

		settings.DisplaySize = new Vector2i(480, 270);

		return settings;
	}

	public const int SPRITEPACK_BATTLE = 0;
	public const int SPRITEPACK_UI = 1;
	public const int SPRITEPACK_ENVIRONMENT = 2;

	public const int AUDIO_BUTTON = 0;
	public const int AUDIO_ROLL_MIN = 1;
	public const int AUDIO_ROLL_COUNT = 3;
	public const int AUDIO_HURT = 4;
	public const int AUDIO_HEAL = 5;

	private List<UIObj> uiObjs = new List<UIObj>();

	private static Queue<int> soundQueue = new Queue<int>();

	public static float volume = 0.25F;

	public bool Initialize() {
		networkConfig.AddChannel(QosType.ReliableSequenced);

		RB.EffectSet(RB.Effect.Desaturation, 0.5F);
		
		RB.SpriteSheetSetup(SPRITEPACK_BATTLE, "Sprites/Battle", new Vector2i(12, 12));
		RB.SpriteSheetSetup(SPRITEPACK_ENVIRONMENT, "Sprites/Environment", new Vector2i(212, 82));
		RB.SpriteSheetSet(SPRITEPACK_BATTLE);

		RB.SoundSetup(AUDIO_BUTTON, "Audio/Select");
		for(int i = 0; i < AUDIO_ROLL_COUNT; i++) {
			RB.SoundSetup(AUDIO_ROLL_MIN + i, "Audio/Roll" + (i + 1));
		}
		RB.SoundSetup(AUDIO_HURT, "Audio/Hurt");
		RB.SoundSetup(AUDIO_HEAL, "Audio/Heal");

		OpenScreen(new MainScreen(this, RB.DisplaySize));
		return true;
	}

	private Vector2i mousePos;
	private Screen currentScreen;

	private Color background = new Color(0.9F, 0.9F, 0.76F);

	public void Render() {
		RB.EffectApplyNow();
		RB.EffectSet(RB.Effect.Noise, .2F);
		RB.Clear(background);
		RB.EffectSet(RB.Effect.Noise, 0F);
		if(currentScreen != null) {
			currentScreen.Render();
		}
		if(message.Length > 0) {
			RB.DrawRectFill(new Rect2i(0, 0, RB.DisplaySize.x, RB.DisplaySize.y), Color.Lerp(Color.black, Color.clear, 0.5F));
			Vector2i msgSize = RB.PrintMeasure(message);
			Rect2i msgRect = new Rect2i(RB.DisplaySize.x / 2 - msgSize.x / 2, RB.DisplaySize.y / 2 - msgSize.y / 2, msgSize.x, msgSize.y).Expand(2);
			RB.DrawRectFill(msgRect, Color.white);
			RB.DrawRect(msgRect, Color.black);
			RB.Print(msgRect, Color.black, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, message);
		}
	}

	public void Update() {
		mousePos = RB.PointerPos();
		if(currentScreen != null) {
			bool openedSettings = false;
			if(!(currentScreen is MainScreen || currentScreen is SettingsScreen)) {
				if(RB.KeyPressed(KeyCode.Escape)) {
					PlaySound(AUDIO_BUTTON);
					OpenScreen(new SettingsScreen(this, RB.DisplaySize, currentScreen));
					openedSettings = true;
				}
			}
			if(!openedSettings) {
				currentScreen.Update(message.Length == 0);
			}
		}
		if(message.Length > 0) {
			if(RB.ButtonPressed(RB.BTN_POINTER_A)) {
				message = "";
				if(onClickMessage != null) {
					onClickMessage.Invoke();
					onClickMessage = null;
				}
			}
		}
		HashSet<int> soundSet = new HashSet<int>();
		while(soundQueue.Count > 0) {
			soundSet.Add(soundQueue.Dequeue());
		}
		foreach(int sound in soundSet) {
			RB.SoundPlay(sound, volume);
		}
	}

	public static void PlaySound(int sound) {
		soundQueue.Enqueue(sound);
	}

	private string message = "";
	private System.Action onClickMessage = null;
	
	public void ShowMessage(string message, System.Action onClick) {
		this.message = message;
		this.onClickMessage = onClick;
	}

	public void OpenScreen(Screen screen) {
		if(currentScreen != null) {
			currentScreen.OnClose();
		}
		currentScreen = screen;
		screen.OnOpen();
	}

	public Screen GetOpenScreen() {
		return currentScreen;
	}

	public void StartSingleplayer() {
		CancelConnection();
		StartServer(1);
		client = ClientScene.ConnectLocalServer();
		LobbyClientHandler lobby = new LobbyClientHandler(this);
		OpenClientHandler(lobby);
		OpenScreen(new LobbyScreen(this, RB.DisplaySize, lobby));
	}

	public void StartMultiplayerHost() {
		CancelConnection();
		StartServer(3);
		client = ClientScene.ConnectLocalServer();
		LobbyClientHandler lobby = new LobbyClientHandler(this);
		OpenClientHandler(lobby);
		OpenScreen(new LobbyScreen(this, RB.DisplaySize, lobby));
	}

	private void StartServer(int playerCount) {
		NetworkServer.Configure(networkConfig, playerCount);
		OpenServerHandler(new LobbyServerHandler());
		NetworkServer.Listen(4789);
	}

	public void JoinMultiplayerHost(string ip) {
		CancelConnection();
		client = new NetworkClient();
		client.Configure(networkConfig, 1);
		client.RegisterHandler(MsgType.Connect, (msg) => {
			client.UnregisterHandler(MsgType.Connect);
			client.UnregisterHandler(MsgType.Error);
			LobbyClientHandler lobby = new LobbyClientHandler(this);
			OpenClientHandler(lobby);
			OpenScreen(new LobbyScreen(this, RB.DisplaySize, lobby));
		});
		client.RegisterHandler(MsgType.Error, (msg) => {
			client.UnregisterHandler(MsgType.Connect);
			client.UnregisterHandler(MsgType.Error);
			ShowMessage("Failed to connect: " + Enum.GetName(typeof(NetworkError), msg.ReadMessage<ErrorMessage>().errorCode), null);
		});
		client.Connect(ip, 4789);
	}

	public void EnterDungeon(Dungeon dungeon, List<Pawn> players) {
		currentDungeon = dungeon;
		dungeon.EnterDungeon(this, players);
	}

	public Dungeon GetCurrentDungeon() {
		return currentDungeon;
	}

	public void StartGame(LobbyClientHandler.LobbyPlayer[] lobbyPlayers) {
		/*Dungeon d = new Dungeon(new Encounter[] {
			new BattleEncounter(new Pawn[] {
				PawnTemplates.Wizard.Create(lobbyPlayers.Length, 0, Pawn.Team.Hostile)
			}),
			new BattleEncounter(new Pawn[] {
				PawnTemplates.Pixie.Create(lobbyPlayers.Length, 0, Pawn.Team.Hostile)
			}),
			new BattleEncounter(new Pawn[] {
				PawnTemplates.Cutpurse.Create(lobbyPlayers.Length, 0, Pawn.Team.Hostile)
			}),
			new BattleEncounter(new Pawn[] {
				PawnTemplates.FieryBat.Create(lobbyPlayers.Length, 0, Pawn.Team.Hostile),
				PawnTemplates.FieryBat.Create(lobbyPlayers.Length, 0, Pawn.Team.Hostile)
			}),
			new BattleEncounter(new Pawn[] {
				PawnTemplates.Golem.Create(lobbyPlayers.Length, 0, Pawn.Team.Hostile)
			}),
			new ChoiceEncounter(),
			new VendorEncounter(),
			new BattleEncounter(new Pawn[] {
				PawnTemplates.Pixie.Create(lobbyPlayers.Length, 1, Pawn.Team.Hostile)
			}),
			new BattleEncounter(new Pawn[] {
				PawnTemplates.Cutpurse.Create(lobbyPlayers.Length, 1, Pawn.Team.Hostile)
			}),
			new BattleEncounter(new Pawn[] {
				PawnTemplates.FieryBat.Create(lobbyPlayers.Length, 1, Pawn.Team.Hostile),
				PawnTemplates.FieryBat.Create(lobbyPlayers.Length, 1, Pawn.Team.Hostile)
			}),
			new BattleEncounter(new Pawn[] {
				PawnTemplates.Golem.Create(lobbyPlayers.Length, 1, Pawn.Team.Hostile)
			}),
			new ChoiceEncounter(),
			new VendorEncounter(),
			new ChoiceEncounter(),
			new VendorEncounter(),
			new BattleEncounter(new Pawn[] {
				PawnTemplates.Golem.Create(lobbyPlayers.Length, 10, Pawn.Team.Hostile)
			})
		});*/
		List<Pawn> players = new List<Pawn>();
		for(int i = 0; i < lobbyPlayers.Length; i++) {
			players.Add(Pawn.CreatePlayer(lobbyPlayers[i]));
		}
		OpenServerHandler(new WorldServerHandler(this, players));
		//EnterDungeon(d, players);
	}

	public void CancelConnection() {
		if(serverHandler != null) {
			serverHandler.Close();
			serverHandler = null;
			NetworkServer.Shutdown();
		}
		if(client != null) {
			if(clientHandler != null) {
				clientHandler.Close();
				clientHandler = null;
			}
			client.Shutdown();
			client = null;
		}
	}

	public ServerHandler GetServerHandler() {
		return serverHandler;
	}

	public void OpenServerHandler(ServerHandler serverHandler) {
		if(this.serverHandler != null) {
			this.serverHandler.Close();
		}
		this.serverHandler = serverHandler;
		serverHandler.Open();
	}

	public ClientHandler GetClientHandler() {
		return clientHandler;
	}

	public void OpenClientHandler(ClientHandler clientHandler) {
		if(this.clientHandler != null) {
			this.clientHandler.Close();
		}
		this.clientHandler = clientHandler;
		clientHandler.Open();
	}

	public bool IsHost() {
		return serverHandler != null;
	}

	public int GetPlayerCount() {
		return NetworkServer.connections.Count;
	}

	public void StartCoroutine(IEnumerator coroutine) {
		gameBinding.StartCoroutine(coroutine);
	}
}
