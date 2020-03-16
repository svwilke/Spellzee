using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;

public class VendorScreen : Screen
{

	private TextButton readyButton;
	private Pawn pawn;

	private TabbedPane infoPane;
	private Text spellName;
	private Text description;

	private Text vendorText;

	private Text buyHeader;
	private VendorButton[] buyButtons;
	private Text sellHeader;
	private VendorButton[] sellButtons;
	private List<ImageButton> swapButtons = new List<ImageButton>();

	public VendorScreen(Game game, Vector2i size, Pawn pawn) : base(game, size) {
		this.pawn = pawn;
		buyButtons = new VendorButton[0];
		sellButtons = new VendorButton[0];
		UpdateSell(pawn.GetKnownSpellIds());
		vendorText.SetText(vendorText.GetText() + "\n\nYou can currently know @FFFFFF" + pawn.SpellSlotCount.GetValue() + "@- spells.");
	}

	public override void OnConstruct() {
		AddUIObj(readyButton = new TextButton(new Vector2i(size.width / 2, size.height - 76), "Ready?", RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER));
		readyButton.isVisible = true;
		readyButton.SetOnClick(() => {
			Game.client.Send(GameMsg.Ready, new EmptyMessage());
			readyButton.currentState = UIObj.State.Disabled;
			MessageBox waitingMsg = new MessageBox("Waiting for other players...");
			ShowMessageBox(waitingMsg);
		});

		int infoPaneWidth = (size.width - 102) - (size.width / 2 + 1);
		AddUIObj(infoPane = new TabbedPane(new Vector2i(size.width / 2 - infoPaneWidth / 2, size.height - 62), new Vector2i(infoPaneWidth, 62), true));
		infoPane.SetTabs(new string[] { "Shop Info", "Spell" });
		vendorText = new Text(new Vector2i(size.width / 2 - infoPaneWidth / 2 + 4, size.height - 58), new Vector2i(infoPaneWidth - 8, 54), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP | RB.TEXT_OVERFLOW_WRAP);
		vendorText.SetText("Yoda:\n\"A new spell I can teach.\"");
		AddUIObj(vendorText);
		infoPane.AddToTab(0, vendorText);
		// Infopane: Spell info
		AddUIObj(spellName = new Text(new Vector2i(size.width / 2 - infoPaneWidth / 2 + 4, size.height - 58), new Vector2i(infoPaneWidth / 2, 10), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP));
		spellName.SetEffect(Text.Outline);
		spellName.SetColor(Color.white);
		infoPane.AddToTab(1, spellName);
		AddUIObj(description = new Text(new Vector2i(size.width / 2 - infoPaneWidth / 2 + 4, size.height - 32), new Vector2i(infoPaneWidth - 6, 30), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP | RB.TEXT_OVERFLOW_WRAP));
		description.SetColor(Color.white);
		infoPane.AddToTab(1, description);

		buyHeader = new Text(new Vector2i(size.width / 4, -10), new Vector2i(100, 20), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, "@w214Learn");
		buyHeader.SetEffect(Text.Outline);
		buyHeader.SetColor(Color.white);
		buyHeader.FitSizeToText();
		buyHeader.alignment = RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER;
		AddUIObj(buyHeader);
		sellHeader = new Text(new Vector2i(3 * size.width / 4, -10), new Vector2i(100, 20), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, "@w214Forget");
		sellHeader.SetEffect(Text.Outline);
		sellHeader.SetColor(Color.white);
		sellHeader.FitSizeToText();
		sellHeader.alignment = RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER;
		AddUIObj(sellHeader);
	}

	public void SetInformation(UIObj hovered) {
		if(hovered is VendorButton) {
			Spell viewedSpell = ((VendorButton)hovered).spell;
			spellName.SetText(viewedSpell.GetName());
			description.SetText(viewedSpell.GetLongDescription());
			infoPane.OpenTab(1);
		}
	}

	public void ResetInformation(UIObj unhovered) {
		if(unhovered is VendorButton) {
			infoPane.OpenTab(0);
		}
	}

	public void BlockBuying() {
		foreach(VendorButton ib in buyButtons) {
			ib.currentState = UIObj.State.Disabled;
		}
	}

	public void UpdateBuy(List<string> buyableSpells) {
		foreach(VendorButton ib in buyButtons) {
			RemoveUIObj(ib);
		}
		buyButtons = new VendorButton[buyableSpells.Count];
		bool buyPossible = pawn.GetSpells().Length < pawn.SpellSlotCount.GetValue();
		int height = 28 * buyableSpells.Count + (4 * (buyableSpells.Count - 1));
		int yStart = size.height / 2 - height / 2;
		for(int i = 0; i < buyButtons.Length; i++) {
			string spellId = buyableSpells[i];
			buyButtons[i] = new VendorButton(Spells.Get(spellId), new Vector2i(size.width / 4, yStart + i * 32), true, !buyPossible || pawn.DoesKnowSpell(spellId));
			AddUIObj(buyButtons[i]);
		}
		buyHeader.SetPosition(new Vector2i(size.width / 4, yStart - 10));
	}

	public void UpdateSell(List<string> sellableSpells) {
		foreach(VendorButton ib in sellButtons) {
			RemoveUIObj(ib);
		}
		foreach(ImageButton ib in swapButtons) {
			RemoveUIObj(ib);
		}
		sellButtons = new VendorButton[sellableSpells.Count];
		bool buyPossible = pawn.GetSpells().Length < pawn.SpellSlotCount.GetValue();
		for(int i = 0; i < buyButtons.Length; i++) {
			buyButtons[i].blocked = !buyPossible || pawn.DoesKnowSpell(buyButtons[i].spell);
		}
		int height = 28 * sellableSpells.Count + (4 * (sellableSpells.Count - 1));
		int yStart = size.height / 2 - height / 2;
		for(int i = 0; i < sellButtons.Length; i++) {
			sellButtons[i] = new VendorButton(Spells.Get(sellableSpells[i]), new Vector2i(3 * (size.width / 4), yStart + i * 32));
			AddUIObj(sellButtons[i]);
			int x = i;
			if(i > 0) {
				ImageButton up = new ImageButton(new Vector2i(3 * (size.width / 4) + 50, yStart + 7 + i * 32), Game.SPRITEPACK_BATTLE, RB.PackedSpriteGet("ButtonUp", Game.SPRITEPACK_BATTLE));
				swapButtons.Add(up);
				AddUIObj(up);
				up.SetOnClick(() => {
					Game.client.Send(GameMsg.SwapSpells, new GameMsg.MsgIntegerArray(x - 1, x));
					Spell temp = sellButtons[x - 1].spell;
					sellButtons[x - 1].spell = sellButtons[x].spell;
					sellButtons[x].spell = temp;
				});
			}
			if(i < sellButtons.Length - 1) {
				ImageButton down = new ImageButton(new Vector2i(3 * (size.width / 4) + 50, yStart + 19 + i * 32), Game.SPRITEPACK_BATTLE, RB.PackedSpriteGet("ButtonDown", Game.SPRITEPACK_BATTLE));
				swapButtons.Add(down);
				AddUIObj(down);
				down.SetOnClick(() => {
					Game.client.Send(GameMsg.SwapSpells, new GameMsg.MsgIntegerArray(x, x + 1));
					Spell temp = sellButtons[x].spell;
					sellButtons[x].spell = sellButtons[x + 1].spell;
					sellButtons[x + 1].spell = temp;
				});
			}
			

		}
		sellHeader.SetPosition(new Vector2i(3 * size.width / 4, yStart - 10));
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
