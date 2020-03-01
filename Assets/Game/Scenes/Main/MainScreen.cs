using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScreen : Screen {

	public MainScreen(Game game, Vector2i size) : base(game, size) {

	}

	private string ip = "localhost";
	private int volumeValue = 50;

	public override void OnConstruct() {
		TextButton btn;
		int left = 30;
		int top = RB.DisplaySize.height / 2;
		AddUIObj(btn = new TextButton(new Vector2i(left, top), "Singleplayer"));
		btn.SetOnClick(() => {
			game.StartSingleplayer();
		});
		AddUIObj(btn = new TextButton(new Vector2i(left, top + 30), "Host Game"));
		btn.SetOnClick(() => {
			game.StartMultiplayerHost();
		});
		AddUIObj(btn = new TextButton(new Vector2i(left, top + 50), "Join Game"));
		btn.SetOnClick(() => {
			game.JoinMultiplayerHost(ip);
		});
		InputField input;
		AddUIObj(input = new InputField(new Vector2i(left + 56, top + 50)));
		input.SetText("localhost");
		input.SetOnCompleteEdit((txt) => ip = txt.ToString());
		AddUIObj(btn = new TextButton(new Vector2i(left, top + 80), "Exit Game"));
		btn.SetOnClick(() => {
			Application.Quit();
		});
		Text text = new Text(new Vector2i(RB.DisplaySize.width - 60, top + 76), new Vector2i(10, 10), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, "50");
		AddUIObj(text);
		AddUIObj(new Text(new Vector2i(RB.DisplaySize.width - 100, top + 76), new Vector2i(16, 10), RB.ALIGN_H_RIGHT | RB.ALIGN_V_CENTER, "Volume:"));
		AddUIObj(btn = new TextButton(new Vector2i(RB.DisplaySize.width - 77, top + 80), "<"));
		btn.SetOnClick(() => {
			volumeValue -= 5;
			if(volumeValue < 0) {
				volumeValue = 0;
			}
			Game.volume = volumeValue / 200F;
			text.SetText(volumeValue.ToString());
		});
		AddUIObj(btn = new TextButton(new Vector2i(RB.DisplaySize.width - 43, top + 80), ">"));
		btn.SetOnClick(() => {
			volumeValue += 5;
			if(volumeValue > 100) {
				volumeValue = 100;
			}
			Game.volume = volumeValue / 200F;
			text.SetText(volumeValue.ToString());
		});

		Text resText = new Text(new Vector2i(RB.DisplaySize.width - 60, top + 66), new Vector2i(20, 10), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, "(960, 512)");
		resText.SetPosition(resText.pos, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER);
		AddUIObj(resText);
		AddUIObj(btn = new TextButton(new Vector2i(RB.DisplaySize.width - 60, top + 54), "Toggle Fullscreen", RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER));
		btn.SetOnClick(() => {
			bool full = !UnityEngine.Screen.fullScreen;
			int width = Game.Widths[1];
			int height = Game.Heights[1];
			int scrWidth = 0;
			int scrHeight = 0;
			Resolution[] ress = UnityEngine.Screen.resolutions;
			for(int i = 0; i < ress.Length; i++) {
				if (scrWidth < ress[i].width) {
					scrWidth = ress[i].width;
				}
				if(scrHeight < ress[i].height) {
					scrHeight = ress[i].height;
				}
			}
			if(full) {
				int i = 1;
				for(i = 0; i < Game.Widths.Length; i++) {
					if (scrWidth < Game.Widths[i] || scrHeight < Game.Heights[i]) {
						break;
					}
				}
				width = Game.Widths[i - 1];
				height = Game.Heights[i - 1];
			}
			resText.SetText("(" + width + ", " + height + ")");
			UnityEngine.Screen.SetResolution(width, height, full);
		});
		
	}
}
