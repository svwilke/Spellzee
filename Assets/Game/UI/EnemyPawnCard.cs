using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPawnCard : UIObj {

	private Battle battle;
	private Pawn pawn;
	private int align;
	private Vector2i originalPos;

	public EnemyPawnCard(Vector2i pos, Vector2i size, Pawn pawn, Battle battle, int align = RB.ALIGN_H_LEFT) {
		this.battle = battle;
		this.pawn = pawn;
		this.size = size;
		SetPosition(this.originalPos = pos, alignment = align);
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
		Rect2i titleRect = new Rect2i(pos.x, pos.y + 1, new Vector2i(size.x, 15));
		for(int dx = -1; dx <= 1; dx++) {
			for(int dy = -1; dy <= 1; dy++) {
				RB.Print(titleRect.Offset(new Vector2i(dx, dy)), Color.black, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, pawn.GetName());
			}
		}
		RB.Print(titleRect, Color.white, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, pawn.GetName());

		Rect2i imageRect = new Rect2i(pos.x + size.width / 2 - 32, pos.y + 16, new Vector2i(64, 64));
		RB.DrawRectFill(imageRect, Color.white);
		RB.DrawRect(imageRect, Color.black);
		RB.Print(imageRect, Color.black, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, "Image");

		int ailX = pos.x + 8;
		for(int i = 0; i < DB.Ailments.Length; i++) {
			Ailment ail = DB.Ailments[i];
			int intensity = pawn.GetAilment(i);
			if(intensity > 0) {
				string text = ail.GetShortName() + " " + intensity;
				int w = RB.PrintMeasure(text).width;
				RB.Print(new Vector2i(ailX, pos.y + 84), ail.GetColor(), text);
				ailX += w + 3;
			}
		}

		Vector2i hpSize = RB.PrintMeasure("HP: ");
		int hpX = pos.x + 8 + hpSize.x;
		Rect2i healthBar = new Rect2i(hpX, pos.y + 96, size.x - 8 - (hpX - pos.x), hpSize.y + 2);
		RB.Print(new Vector2i(hpX - hpSize.x, pos.y + 97), Color.black, "HP:");
		RB.DrawRectFill(healthBar, Color.red);
		int fill = (int)(healthBar.width * ((float)pawn.CurrentHp / (float)pawn.MaxHp));
		RB.DrawRectFill(new Rect2i(healthBar.x, healthBar.y, fill, healthBar.height), Color.green);
		RB.DrawRect(healthBar.Expand(1), Color.black);
		RB.Print(healthBar, Color.white, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, pawn.CurrentHp + "/" + pawn.MaxHp);
	}
}
