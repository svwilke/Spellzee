using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;

public class ChoiceBox : UIObjGroup
{
	private string description;
	private List<Option> options;
	private bool areOptionsUsable = false;

	public ChoiceBox(Vector2i pos, Vector2i size, string description, List<Option> options, bool usable) {
		this.pos = pos;
		this.size = size;
		this.description = description;
		this.options = options;
		this.areOptionsUsable = usable;
		Construct();
	}

	public void Construct() {
		//Image image = new Image(pos + new Vector2i(4, 4), Game.SPRITEPACK_ENVIRONMENT, RB.PackedSpriteGet(template.GetId(), Game.SPRITEPACK_ENVIRONMENT), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP);
		//image.SetOutline(Color.black);
		//AddUIObj(image);
		AddUIObj(new TabbedPane(pos, size, true));
		Text desc = new Text(pos + new Vector2i(6, 6), new Vector2i(size.width - 6, 40), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP, description);
		AddUIObj(desc);

		int y = size.height - 6 - options.Count * 16;
		for(int i = 0; i < options.Count; i++) {
			Option o = options[i];
			TextButton oButton = new TextButton(pos + new Vector2i(4, y + i * 16), o.text, RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP);
			if(o.isLocked || !areOptionsUsable) {
				oButton.currentState = State.Disabled;
			}
			oButton.SetOnClick(() => {
				Game.client.Send(GameMsg.SelectChoice, new IntegerMessage(o.actionId));
			});
			AddUIObj(oButton);
		}
	}

	public void UpdateOptions(List<Option> options, bool usable) {
		this.options = options;
		this.areOptionsUsable = usable;
		Clear();
		Construct();
	}
}
