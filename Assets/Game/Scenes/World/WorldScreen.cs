using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class WorldScreen : Screen {

	private TabbedPane dungeonInfo;
	private DungeonTemplate[] dungeons;
	private TextButton[] dungeonButtons;

	public WorldScreen(Game game, Vector2i size) : base(game, size) {
		
	}

	public override void OnConstruct() {
		dungeons = DungeonTemplates.GetPlayableDungeons();

		dungeonInfo = new TabbedPane(new Vector2i(size.width / 4 + 10, 40), new Vector2i(size.width / 2 - 20, size.height - 80), true);
		dungeonInfo.SetTabs(dungeons.Select(d => d.GetName()).ToArray());

		
		AddUIObj(dungeonInfo);

		dungeonButtons = new TextButton[dungeons.Length];
		for(int i = 0; i < dungeons.Length; i++) {
			Image image = new Image(dungeonInfo.pos + new Vector2i(4, 4), Game.SPRITEPACK_ENVIRONMENT, RB.PackedSpriteGet(dungeons[i].GetId(), Game.SPRITEPACK_ENVIRONMENT), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP);
			image.SetOutline(Color.black);
			dungeonInfo.AddToTab(i, image);
			AddUIObj(image);

			Text title = new Text(dungeonInfo.pos + new Vector2i(6, 89), new Vector2i(0, 0), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP, dungeons[i].GetName());
			title.SetEffect(Text.Outline);
			title.SetColor(Color.white);
			title.FitSizeToText();
			dungeonInfo.AddToTab(i, title);
			AddUIObj(title);

			Text description = new Text(dungeonInfo.pos + new Vector2i(6, 99), new Vector2i(dungeonInfo.size.width - 12, dungeonInfo.size.height - 32), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP | RB.TEXT_OVERFLOW_WRAP, dungeons[i].GetDescription());
			description.SetColor(Color.black);
			dungeonInfo.AddToTab(i, description);
			AddUIObj(description);

			TextButton enter = new TextButton(new Vector2i(dungeonInfo.pos.x + dungeonInfo.size.width / 2, dungeonInfo.pos.y + dungeonInfo.size.height - 16), "Enter", RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER);
			if(!game.IsHost()) {
				enter.currentState = UIObj.State.Disabled;
			}
			int x = i;
			enter.SetOnClick(() => {
				(game.GetServerHandler() as WorldServerHandler).EnterDungeon(dungeons[x]);
			});
			dungeonInfo.AddToTab(i, enter);
			AddUIObj(enter);

			TextButton select = new TextButton(new Vector2i(size.width / 8 - 32, size.height / 3 + 20 * i), dungeons[i].GetName(), RB.ALIGN_H_LEFT | RB.ALIGN_V_CENTER);
			select.SetOnClick(() => {
				NetworkServer.SendToAll(GameMsg.ShowDungeon, new IntegerMessage(x));
			});
			if(!game.IsHost()) {
				select.currentState = UIObj.State.Disabled;
			}
			AddUIObj(select);
		}
	}

	public void ShowDungeon(int id) {
		dungeonInfo.OpenTab(id);
	}
}
