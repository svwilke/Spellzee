using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;

public class VendorScreen : Screen
{

	private Button readyButton;
	private PlayerPawn pawn;

	private TabbedPane infoPane;
	private Text spellName;
	private Text description;

	private Text buyHeader;
	private ItemButton[] buyButtons;
	private Text sellHeader;
	private ItemButton[] sellButtons;

	public VendorScreen(Game game, Vector2i size, PlayerPawn pawn) : base(game, size) {
		this.pawn = pawn;
		buyButtons = new ItemButton[0];
		sellButtons = new ItemButton[0];
		UpdateSell(pawn.GetKnownSpellIds());
	}

	public override void OnConstruct() {
		AddUIObj(readyButton = new Button(new Vector2i(size.width / 2, size.height - 76), "Ready?", UIObj.ALIGN_CENTER));
		readyButton.isVisible = true;
		readyButton.SetOnClick(() => {
			Game.client.Send(GameMsg.Ready, new EmptyMessage());
			readyButton.currentState = UIObj.State.Disabled;
		});

		int infoPaneWidth = (size.width - 102) - (size.width / 2 + 1);
		AddUIObj(infoPane = new TabbedPane(new Vector2i(size.width / 2 - infoPaneWidth / 2, size.height - 62), new Vector2i(infoPaneWidth, 62), true));
		infoPane.SetTabs(new string[] { "Shop Info", "Spell" });
		description = new Text(new Vector2i(size.width / 2 - infoPaneWidth / 2 + 4, size.height - 58), new Vector2i(infoPaneWidth - 8, 54), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP | RB.TEXT_OVERFLOW_WRAP);
		description.SetText("Yoda:\n\"I can teach you a new spell!\"");
		AddUIObj(description);
		infoPane.AddToTab(0, description);
		// Infopane: Spell info
		AddUIObj(spellName = new Text(new Vector2i(size.width / 2 - infoPaneWidth / 2 + 4, size.height - 58), new Vector2i(infoPaneWidth / 2, 10), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP));
		spellName.SetEffect(Text.Outline);
		spellName.SetColor(Color.white);
		infoPane.AddToTab(1, spellName);
		AddUIObj(description = new Text(new Vector2i(size.width / 2 - infoPaneWidth / 2 + 4, size.height - 32), new Vector2i(infoPaneWidth - 6, 30), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP | RB.TEXT_OVERFLOW_WRAP));
		description.SetColor(Color.white);
		infoPane.AddToTab(1, description);

		buyHeader = new Text(new Vector2i(size.width / 4 - 46, -10), new Vector2i(100, 20), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, "@w214Learn");
		buyHeader.SetEffect(Text.Outline);
		buyHeader.SetColor(Color.white);
		AddUIObj(buyHeader);
		sellHeader = new Text(new Vector2i(3 * size.width / 4 - 46, -10), new Vector2i(100, 20), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, "@w214Forget");
		sellHeader.SetEffect(Text.Outline);
		sellHeader.SetColor(Color.white);
		AddUIObj(sellHeader);
	}

	public void SetInformation(UIObj hovered) {
		if(hovered is ItemButton) {
			Spell viewedSpell = ((ItemButton)hovered).spell;
			spellName.SetText(viewedSpell.GetName());
			description.SetText(viewedSpell.GetLongDescription());
			infoPane.OpenTab(1);
		}
	}

	public void ResetInformation(UIObj unhovered) {
		if(unhovered is ItemButton) {
			infoPane.OpenTab(0);
		}
	}

	public void UpdateBuy(List<int> buyableSpells) {
		foreach(ItemButton ib in buyButtons) {
			RemoveUIObj(ib);
		}
		buyButtons = new ItemButton[buyableSpells.Count];
		bool buyPossible = pawn.GetSpells().Length < pawn.SpellSlotCount.GetValue();
		int height = 28 * buyableSpells.Count + (4 * (buyableSpells.Count - 1));
		int yStart = size.height / 2 - height / 2;
		for(int i = 0; i < buyButtons.Length; i++) {
			int spellId = buyableSpells[i];
			buyButtons[i] = new ItemButton(DB.SpellList[spellId], new Vector2i(size.width / 4 - 46, yStart + i * 32), true, !buyPossible || pawn.DoesKnowSpell(spellId));
			AddUIObj(buyButtons[i]);
		}
		buyHeader.pos = new Vector2i(size.width / 4 - 46, yStart - 20);
	}

	public void UpdateSell(List<int> sellableSpells) {
		foreach(ItemButton ib in sellButtons) {
			RemoveUIObj(ib);
		}
		sellButtons = new ItemButton[sellableSpells.Count];
		bool buyPossible = pawn.GetSpells().Length < pawn.SpellSlotCount.GetValue();
		for(int i = 0; i < buyButtons.Length; i++) {
			buyButtons[i].blocked = !buyPossible || pawn.DoesKnowSpell(buyButtons[i].spell);
		}
		int height = 28 * sellableSpells.Count + (4 * (sellableSpells.Count - 1));
		int yStart = size.height / 2 - height / 2;
		for(int i = 0; i < sellButtons.Length; i++) {
			sellButtons[i] = new ItemButton(DB.SpellList[sellableSpells[i]], new Vector2i(3 * (size.width / 4) - 46, yStart + i * 32));
			AddUIObj(sellButtons[i]);
		}
		sellHeader.pos = new Vector2i(3 * size.width / 4 - 46, yStart - 20);
	}

	public override void OnOpen() {
		EventBus.UIMouseEnter.AddListener(SetInformation);
		EventBus.UIMouseExit.AddListener(ResetInformation);
	}

	public override void OnClose() {
		EventBus.UIMouseEnter.RemoveListener(SetInformation);
		EventBus.UIMouseExit.RemoveListener(ResetInformation);
	}
}
