﻿using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PawnCard : UIObj
{
	private static Color XPBarBackground = new Color(64F / 255F, 64F / 255F, 64F / 255F);
	private static Color XPBarForeground = Color.cyan;

	private Battle battle;
	public Pawn pawn;

	private System.Action onClick;

	public PawnCard(Vector2i pos, Vector2i size, Pawn pawn, Battle battle) {
		this.battle = battle;
		this.pawn = pawn;
		this.pos = pos;
		this.size = size;
	}

	public void SetOnClick(System.Action onClick) {
		this.onClick = onClick;
	}

	public override void Render() {
		if(!isVisible) {
			return;
		}
		int off = 0;
		if(currentState != State.Pressed) {
			RB.DrawRectFill(new Rect2i(pos.x + 1, pos.y + 1, size.x - 1, size.y - 1), Color.black);
		} else {
			off = 1;
		}
		Color backgroundHighlight = Color.white;
		Color backgroundDefault = Color.gray;
		Color foregroundHighlight = Color.yellow;
		Color foregroundDefault = Color.white;
		Color turnHighlightColor = Color.blue;
		bool highlight = currentState == State.Hovered || currentState == State.Pressed;
		if(pawn.GetId() == battle.currentTurn) {
			RB.DrawRectFill(new Rect2i(pos.x - 1 + off, pos.y - 1 + off, size.x + 1, size.y + 1), turnHighlightColor);
		}
		RB.DrawRectFill(new Rect2i(pos.x + off, pos.y + off, size.x - 1, size.y - 1), highlight ? backgroundHighlight : backgroundDefault);
		RB.DrawRect(new Rect2i(pos.x + 1 + off, pos.y + 1 + off, size.width - 3, size.height - 3), highlight ? foregroundHighlight : foregroundDefault);
		Rect2i titleRect = new Rect2i(pos.x + 30 + off, pos.y + 3 + off, new Vector2i(size.x - 21, 12));
		for(int dx = -1; dx <= 1; dx++) {
			for(int dy = -1; dy <= 1; dy++) {
				RB.Print(titleRect.Offset(new Vector2i(dx, dy)), Color.black, RB.ALIGN_H_LEFT | RB.ALIGN_V_CENTER, pawn.GetName());
			}
		}
		RB.Print(titleRect, Color.white, RB.ALIGN_H_LEFT | RB.ALIGN_V_CENTER, pawn.GetName());

		Rect2i imageRect = new Rect2i(pos.x + 4 + off, pos.y + 4 + off, new Vector2i(22, 22));
		RB.DrawRectFill(imageRect, Color.white);
		RB.DrawRect(imageRect, Color.black);
		if(pawn.GetSprite() == null) {
			RB.Print(imageRect, Color.black, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, "Img");
		} else {
			RB.DrawSprite(pawn.GetSprite(), imageRect.Expand(-1), pawn.team == Pawn.Team.Hostile ? RB.FLIP_H : 0);
		}

		Rect2i levelRect = new Rect2i(imageRect.x, imageRect.y + imageRect.height - 1, new Vector2i(imageRect.width, size.height - 22 - 4));
		RB.Print(levelRect, Color.black, RB.ALIGN_H_LEFT | RB.ALIGN_V_CENTER, "Lv.");
		RB.Print(levelRect, Color.black, RB.ALIGN_H_RIGHT | RB.ALIGN_V_CENTER, (1 + pawn.Level).ToString());

		int ailY = pos.y + size.height - 11;
		int ailX = pos.x + 8 + imageRect.width;
		List<Status> statuses = pawn.GetStatuses();
		foreach(Status s in statuses) {
			ailX += s.Render(ailX, ailY);
		}

		Vector2i hpSize = RB.PrintMeasure("HP: ");
		int hpX = pos.x + 30 + hpSize.x;
		Rect2i healthBar = new Rect2i(hpX + off, pos.y + 16 + off, size.x - 6 - (hpX - pos.x), hpSize.y + 2);
		RB.Print(new Vector2i(hpX - hpSize.x + off, pos.y + 17 + off), Color.black, "HP:");
		RB.DrawRectFill(healthBar, Color.red);
		int fill = (int)(healthBar.width * (pawn.CurrentHp / (float)pawn.GetMaxHp()));
		RB.DrawRectFill(new Rect2i(healthBar.x, healthBar.y, fill, healthBar.height), pawn.IsAlive() ? Color.green : Color.gray);
		RB.Print(healthBar, Color.white, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, pawn.CurrentHp + "/" + pawn.GetMaxHp());
		RB.DrawRect(healthBar.Expand(1), Color.black);

		RB.DrawLine(new Vector2i(healthBar.x, healthBar.y + healthBar.height), new Vector2i(healthBar.x + healthBar.width - 1, healthBar.y + healthBar.height), XPBarBackground);
		if(pawn.XPProgress > 0) {
			RB.DrawLine(new Vector2i(healthBar.x, healthBar.y + healthBar.height), new Vector2i(healthBar.x + (healthBar.width - 1) * pawn.XPProgress, healthBar.y + healthBar.height), XPBarForeground);
		}
	}

	private StatusBox statusDescriptionBox;
	public override void OnMouseEnter() {
		List<Status> statuses = pawn.GetStatuses().Where(status => status.GetDescription() != null && status.GetDescription().Length > 0).ToList();
		if(statuses.Count > 0) {
			statusDescriptionBox = new StatusBox(new Vector2i(pos.x, pos.y + size.height), (size.width - 1) * 2, statuses);
			statusDescriptionBox.layer = Screen.Layer.Foreground;
			screen.AddUIObj(statusDescriptionBox);
		}
	}

	public override void OnMouseExit() {
		if(statusDescriptionBox != null) {
			screen.RemoveUIObj(statusDescriptionBox);
			statusDescriptionBox = null;
		}
	}

	public override void OnClick() {
		if(onClick != null) {
			onClick.Invoke();
			Game.PlaySound(Game.AUDIO_BUTTON);
		}
	}
}
