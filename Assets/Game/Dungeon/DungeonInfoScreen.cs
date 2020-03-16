using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonInfoScreen : Screen {

	private Screen parentScreen;
	private TabbedPane dungeonInfo;

	public DungeonInfoScreen(Game game, Vector2i size, Screen parent) : base(game, size) {
		parentScreen = parent;
		EncounterClientHandler ech = game.GetClientHandler() as EncounterClientHandler;
		if(ech != null) {
			DungeonInformation di = new DungeonInformation(dungeonInfo.pos, dungeonInfo.size, ech.GetDungeonTemplate(), ech.GetDungeonPath(), ech.GetCurrentEncounter());
			dungeonInfo.AddToTab(0, di);
			AddUIObj(di);
		}
		AddKeybinding(KeyCode.I, () => game.OpenScreen(parentScreen));
	}

	public override void OnConstruct() {
		int w = size.width / 2 - 20;
		dungeonInfo = new TabbedPane(new Vector2i(size.width / 2 - w / 2, 40), new Vector2i(w, size.height - 80), true);
		dungeonInfo.SetTabs(new string[] { "Dungeon" });
		AddUIObj(dungeonInfo);
		TextButton btn = new TextButton(new Vector2i(30, RB.DisplaySize.height / 2 + 80), "Back");
		AddUIObj(btn);
		btn.SetKeybind(KeyCode.Escape);
		btn.SetOnClick(() => {
			game.OpenScreen(parentScreen);
		});
	}

	public override void Render() {
		RB.SpriteSheetSet(Game.SPRITEPACK_BATTLE);
		parentScreen.Render();
		RB.AlphaSet(236);
		RB.DrawRectFill(new Rect2i(0, 0, size.width, size.height), Color.black);
		RB.AlphaSet(255);
		base.Render();
	}
}
