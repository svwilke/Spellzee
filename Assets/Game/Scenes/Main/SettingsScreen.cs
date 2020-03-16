using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsScreen : Screen {

	private Screen parentScreen;

	public SettingsScreen(Game game, Vector2i size, Screen parent) : base(game, size) {
		parentScreen = parent;
	}

	private int volumeValue = 50;
	private int currentResIndex = 1;

	private Text resolutionText;
	private Text volumeText;
	private TextButton resButtonLeft;
	private TextButton resButtonRight;

	public override void OnConstruct() {
		TextButton btn;
		int left = 30;
		int top = RB.DisplaySize.height / 2;
		AddUIObj(btn = new TextButton(new Vector2i(left, top + 80), "Back"));
		btn.SetKeybind(KeyCode.Escape);
		btn.SetOnClick(() => {
			game.OpenScreen(parentScreen);
		});
		if(!(parentScreen is MainScreen)) {
			AddUIObj(btn = new TextButton(new Vector2i(left, size.height - (top + 80)), "Exit to Main Menu"));
			btn.SetOnClick(() => {
				game.OpenScreen(new MainScreen(game, size));
				game.CancelConnection();
			});
		}

		AddUIObj(volumeText = new Text(new Vector2i(RB.DisplaySize.width - 100, top + 76), new Vector2i(16, 10), RB.ALIGN_H_RIGHT | RB.ALIGN_V_CENTER, "Volume:"));
		volumeText.SetColor(Color.white);

		volumeText = new Text(new Vector2i(RB.DisplaySize.width - 60, top + 76), new Vector2i(10, 10), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, "50");
		volumeText.SetColor(Color.white);
		AddUIObj(volumeText);
		
		AddUIObj(btn = new TextButton(new Vector2i(RB.DisplaySize.width - 77, top + 80), "<"));
		btn.SetOnClick(() => {
			volumeValue -= 5;
			if(volumeValue < 0) {
				volumeValue = 0;
			}
			Game.volume = volumeValue / 200F;
			PlayerPrefs.SetInt("sound_volume", volumeValue);
			PlayerPrefs.Save();
			volumeText.SetText(volumeValue.ToString());
		});
		AddUIObj(btn = new TextButton(new Vector2i(RB.DisplaySize.width - 43, top + 80), ">"));
		btn.SetOnClick(() => {
			volumeValue += 5;
			if(volumeValue > 100) {
				volumeValue = 100;
			}
			Game.volume = volumeValue / 200F;
			PlayerPrefs.SetInt("sound_volume", volumeValue);
			PlayerPrefs.Save();
			volumeText.SetText(volumeValue.ToString());
		});

		resolutionText = new Text(new Vector2i(RB.DisplaySize.width - 74, top + 58), new Vector2i(20, 10), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, "(960, 512)");
		resolutionText.SetPosition(resolutionText.pos, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER);
		resolutionText.SetColor(Color.white);
		AddUIObj(resolutionText);
		AddUIObj(btn = new TextButton(new Vector2i(RB.DisplaySize.width - 74, top + 42), "Toggle Fullscreen", RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER));
		btn.SetOnClick(() => {
			bool full = !UnityEngine.Screen.fullScreen;
			int index = currentResIndex;
			if(full) {
				index = GetFullscreenResIndex();
				PlayerPrefs.SetString("fullscreen", "Yes, please!");
			} else {
				PlayerPrefs.DeleteKey("fullscreen");
			}
			PlayerPrefs.Save();
			SetResolution(index, full);
		});

		AddUIObj(resButtonLeft = new TextButton(new Vector2i(btn.pos.x, top + 58), "<"));
		resButtonLeft.SetOnClick(() => {
			if(!UnityEngine.Screen.fullScreen && currentResIndex > 0) {
				int newResIndex = currentResIndex - 1;
				currentResIndex = newResIndex;
				PlayerPrefs.SetInt("resolution_index", currentResIndex);
				PlayerPrefs.Save();
				SetResolution(newResIndex, false);
			}
		});
		AddUIObj(resButtonRight = new TextButton(new Vector2i(btn.pos.x + btn.size.width - resButtonLeft.size.width, top + 58), ">"));
		resButtonRight.SetOnClick(() => {
			if(!UnityEngine.Screen.fullScreen && currentResIndex < Game.Widths.Length - 1) {
				Vector2i screenRes = GetScreenResolution();
				int newResIndex = currentResIndex + 1;
				if(screenRes.width >= Game.Widths[newResIndex] && screenRes.height >= Game.Heights[newResIndex]) {
					currentResIndex = newResIndex;
					PlayerPrefs.SetInt("resolution_index", currentResIndex);
					PlayerPrefs.Save();
					SetResolution(newResIndex, false);
				}
			}
		});

		LoadSettings();
	}

	public override void Render() {
		parentScreen.Render();
		RB.AlphaSet(236);
		RB.DrawRectFill(new Rect2i(0, 0, size.width, size.height), Color.black);
		RB.AlphaSet(255);
		base.Render();
	}

	private void SetResolution(int resIndex, bool full) {
		int width = Game.Widths[resIndex];
		int height = Game.Heights[resIndex];
		resolutionText.SetText("(" + width + ", " + height + ")");
		resButtonLeft.currentState = full ? UIObj.State.Disabled : UIObj.State.Enabled;
		resButtonRight.currentState = full ? UIObj.State.Disabled : UIObj.State.Enabled;
		UnityEngine.Screen.SetResolution(width, height, full);
	}

	private Vector2i GetScreenResolution() {
		int scrWidth = 0;
		int scrHeight = 0;
		Resolution[] ress = UnityEngine.Screen.resolutions;
		int i = 0;
		for(i = 0; i < ress.Length; i++) {
			if(scrWidth < ress[i].width) {
				scrWidth = ress[i].width;
			}
			if(scrHeight < ress[i].height) {
				scrHeight = ress[i].height;
			}
		}
		return new Vector2i(scrWidth, scrHeight);
	}

	private int GetFullscreenResIndex() {
		Vector2i screenRes = GetScreenResolution();
		int i;
		for(i = 0; i < Game.Widths.Length; i++) {
			if(screenRes.width < Game.Widths[i] || screenRes.height < Game.Heights[i]) {
				break;
			}
		}
		return i - 1;
	}

	private void LoadSettings() {
		if(PlayerPrefs.HasKey("sound_volume")) {
			volumeValue = PlayerPrefs.GetInt("sound_volume");
			Game.volume = volumeValue / 200F;
			volumeText.SetText(volumeValue.ToString());
		}
		bool fullscreen = PlayerPrefs.HasKey("fullscreen");
		currentResIndex = 1;
		if(PlayerPrefs.HasKey("resolution_index")) {
			currentResIndex = PlayerPrefs.GetInt("resolution_index");
		}
		int usedResIndex = currentResIndex;
		if(fullscreen) {
			usedResIndex = GetFullscreenResIndex();
		}
		SetResolution(usedResIndex, fullscreen);
	}
}
