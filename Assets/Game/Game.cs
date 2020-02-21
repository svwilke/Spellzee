using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using UnityEngine.Networking.NetworkSystem;
using System.Collections;

public class Game : RB.IRetroBlitGame
{
	private ClientHandler clientHandler;
	private ServerHandler serverHandler;

	public static int peerId;

	public static NetworkClient client;

	public static int enemy = -1;

	private ConnectionConfig networkConfig = new ConnectionConfig();

	private MonoBehaviour gameBinding;

	public Game(MonoBehaviour binding) {
		gameBinding = binding;
	}

	public RB.HardwareSettings QueryHardware() {
		RB.HardwareSettings settings = new RB.HardwareSettings();

		settings.DisplaySize = new Vector2i(480, 256);
		//settings.DisplaySize = new Vector2i(480 + 240, 256 + 128);

		settings.MapSize = new Vector2i(16, 16);
		settings.MapLayers = 4;

		return settings;
	}

	public const int SPRITEPACK_BATTLE = 0;

	public const int AUDIO_BUTTON = 0;
	public const int AUDIO_ROLL = 1;
	public const int AUDIO_HURT = 2;
	public const int AUDIO_HEAL = 3;

	private List<UIObj> uiObjs = new List<UIObj>();

	public static float volume = 0.25F;

	public bool Initialize() {
		networkConfig.AddChannel(QosType.ReliableSequenced);

		RB.EffectSet(RB.Effect.Desaturation, 0.5F);
		
		RB.SpriteSheetSetup(SPRITEPACK_BATTLE, "Sprites/Battle", new Vector2i(12, 12));
		RB.SpriteSheetSet(SPRITEPACK_BATTLE);

		RB.SoundSetup(AUDIO_BUTTON, "Audio/Select");
		RB.SoundSetup(AUDIO_ROLL, "Audio/Roll");
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
			currentScreen.Update(message.Length == 0);
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

	public void StartGame(LobbyClientHandler.LobbyPlayer[] lobbyPlayers) {
		ServerBattle battle = new ServerBattle(this);
		battle.allies = new Pawn[lobbyPlayers.Length];
		for(int i = 0; i < battle.allies.Length; i++) {
			battle.allies[i] = Pawn.CreatePlayer(lobbyPlayers[i]);
		}
		battle.enemy = CreateNextEnemy();
		battle.enemy.SetId(battle.allies.Length);
		GameMsg.MsgStartBattle msg = new GameMsg.MsgStartBattle() { battle = battle };
		NetworkServer.SendToAll(GameMsg.StartBattle, msg);
		OpenServerHandler(new BattleServerHandler(this, battle));
	}

	public Pawn CreateNextEnemy() {
		enemy = (enemy + 1); //DB.EnemyNames.Length;
		/*Pawn pawn = new Pawn(DB.EnemyNames[enemy], DB.EnemyHPs[enemy] * GetPlayerCount());
		pawn.AddSpell(DB.EnemySpells[enemy]);
		return pawn;*/
		int level = Mathf.FloorToInt((enemy / DB.Enemies.Length));
		return DB.Enemies[enemy % DB.Enemies.Length].Create(GetPlayerCount(), level);
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
