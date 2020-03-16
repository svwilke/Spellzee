using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageButton : Image
{

	public ImageButton(Vector2i pos, int sheet, PackedSprite sprite, int align = RB.ALIGN_H_LEFT | RB.ALIGN_V_CENTER) : base(pos, sheet, sprite, align) {

	}

	public override void Render() {
		if(!isVisible) {
			return;
		}
		int off = 0;
		if(currentState != State.Pressed) {
			RB.TintColorSet(Color.black);
			RB.DrawSprite(sprite, pos + new Vector2i(1, 1));
		} else {
			off = 1;
		}
		bool highlight = currentState == State.Hovered || currentState == State.Pressed;
		if(highlight) {
			RB.TintColorSet(Color.yellow);
		} else
		if(currentState == State.Disabled) {
			RB.TintColorSet(Color.gray);
		} else {
			RB.TintColorSet(Color.white);
		}
		RB.DrawSprite(sprite, pos + new Vector2i(off, off));
		RB.TintColorSet(Color.white);
	}
}
