using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonInformation : UIObjGroup
{
	private DungeonTemplate template;
	private string[] path;
	private Vector2i pathLineFrom;
	private Vector2i pathLineTo;
	private int currentEncounter;

	public DungeonInformation(Vector2i pos, Vector2i size, DungeonTemplate template, string[] path, int currentEncounter = -1) {
		this.pos = pos;
		this.size = size;
		this.template = template;
		this.path = path;
		this.currentEncounter = currentEncounter;
		Construct();
	}

	public void Construct() {
		Image image = new Image(pos + new Vector2i(4, 4), Game.SPRITEPACK_ENVIRONMENT, RB.PackedSpriteGet(template.GetId(), Game.SPRITEPACK_ENVIRONMENT), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP);
		image.SetOutline(Color.black);
		AddUIObj(image);

		Text title = new Text(pos + new Vector2i(6, 89), new Vector2i(0, 0), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP, template.GetName());
		title.SetEffect(Text.Outline);
		title.SetColor(Color.white);
		title.FitSizeToText();
		AddUIObj(title);

		Text description = new Text(pos + new Vector2i(6, 99), new Vector2i(size.width - 12, size.height - 32), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP | RB.TEXT_OVERFLOW_WRAP, template.GetDescription());
		description.SetColor(Color.black);
		AddUIObj(description);

		Rect2i pathRect = new Rect2i(pos.x + 5, pos.y + 109, size.width - 10, size.height - 125);
		int w = -3;
		for(int j = 0; j < path.Length; j++) {
			if(j.Equals("boss")) {
				w += 17;
			} else {
				w += 15;
			}
		}
		int x = pos.x + size.width / 2 - w / 2;
		int y = pathRect.center.y - 7;
		RB.SpriteSheetSet(Game.SPRITEPACK_UI);
		pathLineFrom = new Vector2i(x + 1, y - 1);
		pathLineTo = new Vector2i(x + w - 2, y - 1);
		for(int i = 0; i < path.Length; i++) {
			int s = path[i].Equals("boss") ? 14 : 12;
			Rect2i rect = new Rect2i(x, y - s / 2, s, s);
			Image img = new Image(rect.min, Game.SPRITEPACK_UI, RB.PackedSpriteGet(path[i], Game.SPRITEPACK_UI), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP);
			img.layer = Screen.Layer.Foreground;
			img.SetOutline(i == currentEncounter ? Color.yellow : Color.black);
			img.SetBackground(Color.white);
			if(i < currentEncounter) {
				img.SetTint(Color.gray);
			}
			TooltipArea tta = new TooltipArea(rect, path[i].Substring(0, 1).ToUpper() + path[i].Substring(1));
			AddUIObj(img);
			AddUIObj(tta);
			x += s + 3;
		}
	}

	public override void Render() {
		RB.DrawLine(pathLineFrom, pathLineTo, Color.black);
		RB.DrawLine(pathLineFrom + new Vector2i(0, 1), pathLineTo + new Vector2i(0, 1), Color.black);
	}
}
