using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;

public class BattleScreen : Screen {


	public Battle battle;

	private ImageButton rollButton;
	private TextButton passButton;
	private DieButton[] dieButtons;
	private Vector2i[] lockPositions;
	private Image[] viewPawnImages;
	private SpellButton[] spellButtons;
	private int[] spellButtonOwnership;
	public TabbedPane spellPane;
	private TabbedPane bottomPane;
	private TabbedPane infoPane;
	private Text battleLog;

	private Dictionary<Pawn, UIObj> pawnCards;

	private List<Text> currentItemTexts = new List<Text>();

	// Info Panes
	// Player
	private Text playerName;
	private Text affinities;
	// Spell
	private Spell viewedSpell;
	private Text spellName;
	private Text description;

	// Targetting
	private bool renderTargeting;
	private UIObj originButton;
	private Spell targetSpell;
	private Vector2i originPoint;
	private Vector2i targetPoint;
	private Rect2i targetRect;
	private Pawn targetPawn;

	public BattleScreen(Game game, Vector2i size, Battle battle) : base(game, size) {
		this.battle = battle;
		CreateBattleButtons();
		UpdateContext();
	}

	public void Rebuild() {
		game.OpenScreen(new BattleScreen(game, size, battle));
	}

	public void OnPawnDamage(Battle battle, Pawn pawn, int damage) {
		battle.log.Add(pawn.GetName() + " takes @FF0000" + damage + "@- damage.");
	}

	public void OnPawnHeal(Battle battle, Pawn pawn, int heal) {
		battle.log.Add(pawn.GetName() + " heals for @00FF00" + heal + "@- health.");
	}

	public void OnBeforeCast(Battle battle, Pawn pawn, Pawn target, int spellId) {
		Spell spell = DB.SpellList[spellId];
		string end = ".";
		if(target != null) {
			end = " on " + target.GetName() + end;
		}
		battle.log.Add(pawn.GetName() + " casts " + spell.GetName() + end);
	}

	public override void OnConstruct() {
		AddUIObj(rollButton = new ImageButton(new Vector2i(size.x / 5, size.y / 2 + 32), RB.PackedSpriteGet("RollButton", Game.SPRITEPACK_BATTLE), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER));
		rollButton.SetOnClick(() => {
			if(battle.rollsLeft > 0) {
				Game.client.Send(GameMsg.Roll, new EmptyMessage());
			}
		});
		AddUIObj(passButton = new TextButton(new Vector2i(size.x / 5, size.y / 2 + 48), "Pass", RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER));
		passButton.SetOnClick(() => {
			if(battle.rollsLeft == 0) {
				Game.client.Send(GameMsg.Pass, new EmptyMessage());
				passButton.isVisible = false;
			}
		});
		passButton.isVisible = false;
		AddUIObj(bottomPane = new TabbedPane(new Vector2i(0, size.height - 72), new Vector2i(size.width / 2 + 1, 72)));
		bottomPane.SetTabs(new string[] { "Battle Log", "Inventory", "Bestiary" });
		AddUIObj(battleLog = new Text(new Vector2i(4, size.height - 64), new Vector2i(size.width / 3 * 2, 60), RB.ALIGN_H_LEFT | RB.ALIGN_V_BOTTOM | RB.TEXT_OVERFLOW_CLIP));
		bottomPane.AddToTab(0, battleLog);

		// Infopane
		int infoPaneWidth = (size.width - 102) - (size.width / 2 + 1);
		AddUIObj(infoPane = new TabbedPane(new Vector2i(size.width / 2 + 1, size.height - 62), new Vector2i(infoPaneWidth, 62), true));
		infoPane.SetTabs(new string[] { "Player", "Enemy", "Spell", "Item" });
		// Infopane: Pawn info
		AddUIObj(playerName = new Text(new Vector2i(size.width / 2 + 5, size.height - 58), new Vector2i(infoPaneWidth / 2, 10), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP));
		playerName.SetEffect(Text.Outline);
		playerName.SetColor(Color.white);
		infoPane.AddToTab(0, playerName);
		AddUIObj(affinities = new Text(new Vector2i(size.width / 2 + 5 + infoPaneWidth / 2, size.height - 58), new Vector2i(infoPaneWidth / 2 - 6, 40), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP));
		affinities.SetText("Affinities:");
		affinities.SetColor(Color.white);
		affinities.SetEffect(Text.Outline);
		infoPane.AddToTab(0, affinities);
		AddUIObj(affinities = new Text(new Vector2i(size.width / 2 + 5 + infoPaneWidth / 2, size.height - 58), new Vector2i(infoPaneWidth / 2 - 6, 40), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP));
		affinities.SetColor(Color.white);
		string affText = "";
		for(int i = 0; i < 6; i++) {
			affText += "\n " + Element.All[i].GetColorHex() + Element.All[i].GetName();
		}
		affinities.SetText(affText);
		infoPane.AddToTab(0, affinities);
		AddUIObj(affinities = new Text(new Vector2i(size.width / 2 + 5 + infoPaneWidth / 2, size.height - 58), new Vector2i(infoPaneWidth / 2 - 6, 40), RB.ALIGN_H_RIGHT | RB.ALIGN_V_TOP));
		affinities.SetColor(Color.black);
		infoPane.AddToTab(0, affinities);
		TooltipArea tta = new TooltipArea(new Vector2i(size.width / 2 + 5 + infoPaneWidth / 2, size.height - 58), new Vector2i(infoPaneWidth / 2 - 6, 54), "Affinities determine how likely\nyou are to roll a certain aspect.");
		AddUIObj(tta);
		infoPane.AddToTab(0, tta);
		// Infopane: Spell info
		AddUIObj(spellName = new Text(new Vector2i(size.width / 2 + 5, size.height - 58), new Vector2i(infoPaneWidth / 2, 10), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP));
		spellName.SetEffect(Text.Outline);
		spellName.SetColor(Color.white);
		infoPane.AddToTab(1, spellName);
		AddUIObj(description = new Text(new Vector2i(size.width / 2 + 5, size.height - 32), new Vector2i(infoPaneWidth - 6, 30), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP | RB.TEXT_OVERFLOW_WRAP));
		description.SetColor(Color.white);
		infoPane.AddToTab(1, description);
	}

	private void CreateBattleButtons() {
		// set up die buttons
		UpdateDieButtons();

		pawnCards = new Dictionary<Pawn, UIObj>();

		EnemyPawnCard epc = new EnemyPawnCard(new Vector2i(size.x / 2, 72), new Vector2i(128, 128), battle.enemy, battle, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER);
		AddUIObj(epc);
		pawnCards.Add(battle.enemy, epc);

		viewPawnImages = new Image[battle.allies.Length + 1];
		string[] tabNames = new string[battle.allies.Length];
		int spellCount = 0;
		PlayerPawnCard ppc;
		PackedSprite viewSprite = RB.PackedSpriteGet("Eye", Game.SPRITEPACK_BATTLE);
		for(int i = 0; i < battle.allies.Length; i++) {
			AddUIObj(ppc = new PlayerPawnCard(new Vector2i(8, 8 + 36 * i), new Vector2i(96, 32), battle.allies[i], battle));
			pawnCards.Add(battle.allies[i], ppc);
			AddUIObj(viewPawnImages[i] = new Image(new Vector2i(108, 24 + 36 * i), viewSprite));
			if(i > 0) viewPawnImages[i].isVisible = false;
			int x = i;
			ppc.SetOnClick(() => {
				ViewSpellTab(x);
			});
			tabNames[i] = battle.allies[i].GetName();
			spellCount += battle.allies[i].GetSpells().Length;
		}

		spellPane = new TabbedPane(new Vector2i(size.width - 102, 0), new Vector2i(102, size.height), true);
		spellPane.SetTabs(tabNames);
		AddUIObj(spellPane);
		spellButtons = new SpellButton[spellCount];
		spellButtonOwnership = new int[spellCount];
		int currentSpellIndex = 0;
		for(int i = 0; i < battle.allies.Length; i++) {
			currentSpellIndex = FillSpellPane(spellPane, i, currentSpellIndex);
		}
		/*
		Spell[] knownSpells = battle.GetClientPawn().GetSpells();
		spellButtons = new SpellButton[knownSpells.Length];
		for(int i = 0; i < knownSpells.Length; i++) {
			AddUIObj(spellButtons[i] = new SpellButton(battle, knownSpells[i], new Vector2i(size.width - 100, 8 + i * 36)));
		}*/
		ViewSpellTab(0);
	}

	public void UpdateDieButtons() {
		if(dieButtons != null) {
			for(int i = 0; i < dieButtons.Length; i++) {
				RemoveUIObj(dieButtons[i]);
			}
		}
		int rollsMinX = size.x / 5;
		int rollsMaxX = size.x / 5 * 4;
		int rollsW = rollsMaxX - rollsMinX;
		int step = rollsW / (battle.rolls.Length + 1);
		int start = rollsMinX + step;
		dieButtons = new DieButton[battle.rolls.Length];
		lockPositions = new Vector2i[battle.rolls.Length];
		int y = size.y / 2 + 16;
		for(int i = 0; i < battle.rolls.Length; i++) {
			Vector2i buttonTopLeft = new Vector2i(-16 + start + step * i, y);
			AddUIObj(dieButtons[i] = new DieButton(battle, i, buttonTopLeft));
			lockPositions[i] = new Vector2i(buttonTopLeft.x + 16, buttonTopLeft.y + 40);
		}
	}

	public void BeginTargeting(Spell spell, SpellButton button) {
		targetSpell = spell;
		renderTargeting = true;
		targetPawn = null;
		originPoint = targetPoint = new Vector2i(button.pos.x - 2, button.pos.y + button.size.height / 2);
		originButton = button;
	}

	public int GetCurrentTargetPawnId() {
		if(targetPawn == null) {
			return -1;
		} else {
			return targetPawn.GetId();
		}
	}

	private int FillSpellPane(TabbedPane pane, int index, int buttonIndexStart) {
		Spell[] knownSpells = battle.allies[index].GetSpells();
		for(int i = 0; i < knownSpells.Length; i++) {
			SpellButton sb = new SpellButton(this, battle, knownSpells[i], new Vector2i(size.width - 97, 5 + i * 31), index != Game.peerId);
			spellButtons[i + buttonIndexStart] = sb;
			spellButtonOwnership[i + buttonIndexStart] = index;
			AddUIObj(sb);
			pane.AddToTab(index, sb);
		}
		return buttonIndexStart + knownSpells.Length;
	}

	public override void RenderForeground() {

		for(int i = 0; i < battle.rolls.Length; i++) {
			if(battle.locks[i]) {
				RB.DrawSprite("Lock", lockPositions[i] - new Vector2i(6, 6));
				//RB.Print(new Rect2i(lockPositions[i] - new Vector2i(16, 8), new Vector2i(32, 16)), Color.black, RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, "Locked");
			}
		}

		for(int i = 0; i < battle.rollsLeft; i++) {
			RB.DrawEllipseFill(new Vector2i(size.x / 5 - 10 + i * 10, size.y / 2 + 44), new Vector2i(4, 4), Color.white);
			RB.DrawEllipse(new Vector2i(size.x / 5 - 10 + i * 10, size.y / 2 + 44), new Vector2i(4, 4), Color.black);
		}

		if(infoPane.GetOpenTabIndex() == 1) {
			ElementDisplay[] displays = viewedSpell.GetElementDisplays(battle.BuildContext());
			for(int i = 0; i < displays.Length; i++) {
				displays[i].Render(size.width / 2 + 5 + i * 18, size.height - 50);
			}
		}
		
		if(renderTargeting) {
			if(targetPawn != null) {
				RB.AlphaSet(96);
				RB.DrawRectFill(new Rect2i(0, 0, size), Color.black);
				RB.AlphaSet(255);
				pawnCards[targetPawn].Render();
				RB.DrawRect(targetRect, Color.yellow);
				RB.DrawPixel(targetPoint - new Vector2i(1, 0), Color.white);
				//RB.DrawRect(targetRect.Offset(new Vector2i(0, 0)).Expand(1), Color.black);
				originButton.Render();
			}
			RB.DrawPixel(originPoint + new Vector2i(1, 0), Color.white);
			//RB.DrawRect(new Rect2i(originButton.pos, originButton.size).Expand(2), Color.black);
			RB.DrawLine(originPoint + new Vector2i(0, 1), targetPoint + new Vector2i(0, 1), Color.yellow);
			RB.DrawLine(originPoint, targetPoint, Color.white);
			RB.DrawLine(originPoint - new Vector2i(0, 1), targetPoint - new Vector2i(0, 1), Color.yellow);
		}
	}

	public override void Update(bool hasFocus = true) {
		if(renderTargeting) {
			Rect2i onRect = new Rect2i(0, 0, 0, 0);
			UIObj on = null;
			Pawn pawnTarget = null;
			Vector2i mouse = RB.PointerPos();
			foreach(KeyValuePair<Pawn, UIObj> cardPair in pawnCards) {
				onRect = new Rect2i(cardPair.Value.pos, cardPair.Value.size);
				if(onRect.Contains(mouse)) {
					on = cardPair.Value;
					pawnTarget = cardPair.Key;
					break;
				}
			}
			if(on != null && pawnTarget != null) {
				if(pawnTarget != targetPawn) {
					SetTooltip(targetSpell.GetShortDescription(battle.BuildContext(pawnTarget.GetId())));
				}
				targetPawn = pawnTarget;
				targetPoint = new Vector2i(onRect.x + onRect.width, onRect.y + onRect.height / 2);
				targetRect = new Rect2i(onRect.x - 1, onRect.y - 1, onRect.width + 1, onRect.height + 1);
				if(RB.ButtonPressed(RB.BTN_POINTER_A)) {
					Game.client.Send(GameMsg.CastSpell, new GameMsg.MsgIntegerArray(targetSpell.GetId(), targetPawn.GetId()));
					renderTargeting = false;
					targetSpell = null;
				}
			} else {
				targetPawn = null;
				targetPoint = mouse;
				if(RB.ButtonPressed(RB.BTN_POINTER_ANY)) {
					renderTargeting = false;
					targetSpell = null;
				}
			}
		} else {
			base.Update(hasFocus);
		}
	}

	public void ViewSpellTab(int pawnId) {
		for(int j = 0; j < viewPawnImages.Length; j++) {
			if(viewPawnImages[j] != null) {
				viewPawnImages[j].isVisible = j == pawnId;
			}
		}
		spellPane.OpenTab(pawnId);
		infoPane.OpenTab(0);
		PlayerPawn pawn = battle.allies[pawnId] as PlayerPawn;
		playerName.SetText(pawn.GetName());
		string affText = "";
		FastString perc = new FastString(3);

		double affTotal = pawn.GetAffinityTotal();
		for(int i = 0; i < 6; i++) {
			perc.Clear();
			double aff = pawn.GetAffinity(i);
			perc.Append((int)(aff / affTotal * 100), 2, FastString.FILL_SPACES);
			affText += "\n" /*+ Element.All[i].GetColorHex()*/ + pawn.GetAffinity(i) + " (" + perc + "%)";
		}
		affinities.SetText(affText);

		foreach(Text text in currentItemTexts) {
			RemoveUIObj(text);
		}
		int[] eq = pawn.GetEquipment();
		int currentY = size.height - 58;
		for(int i = 0; i < eq.Length; i++) {
			Equipment e = DB.Equipments[eq[i]];
			Text text = new Text(new Vector2i(5, currentY), new Vector2i(), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, e.GetName());
			text.FitSizeToText(1);
			text.SetTooltip(e.GetDescription());
			currentY += text.size.height + 2;
			AddUIObj(text);
			currentItemTexts.Add(text);
			bottomPane.AddToTab(1, text);
		}
	}

	public void UpdateContext() {
		RollContext context = battle.BuildContext();
		bool anyCastable = false;
		for(int i = 0; i < spellButtons.Length; i++) {
			SpellButton b = spellButtons[i];
			b.castable = b.spell.Matches(context);
			if(!b.castable && b.currentState != UIObj.State.Disabled) {
				b.currentState = UIObj.State.Disabled;
			} else
			if(b.castable && b.currentState == UIObj.State.Disabled) {
				b.currentState = UIObj.State.Enabled;
			}
			if(b.castable && spellButtonOwnership[i] == battle.currentTurn) {
				anyCastable = true;
			}
		}

		if(battle.currentTurn != Game.peerId || battle.rollsLeft == 0) {
			if(!(battle.currentTurn == Game.peerId && battle.rollsLeft == 0 && !anyCastable)) {
				rollButton.currentState = UIObj.State.Disabled;
			}
			foreach(DieButton db in dieButtons) {
				db.currentState = UIObj.State.Disabled;
			}
		} else {
			if(rollButton.currentState == UIObj.State.Disabled) {
				rollButton.currentState = UIObj.State.Enabled;
			}
			foreach(DieButton db in dieButtons) {
				if(db.currentState == UIObj.State.Disabled) {
					db.currentState = UIObj.State.Enabled;
				}
			}
		}

		if(battle.currentTurn == Game.peerId && battle.rollsLeft == 0) {
			//rollButton.SetText("Pass"); TODO
			passButton.isVisible = true;
			rollButton.currentState = UIObj.State.Disabled;
		} else {
			passButton.isVisible = false;
		}
		/* OLD Pass button behaviour (appears only when no castable spells)
		if(battle.currentTurn == Game.peerId && battle.rollsLeft == 0 && !anyCastable) {
			//rollButton.SetText("Pass"); TODO
			passButton.isVisible = true;
			rollButton.currentState = UIObj.State.Disabled;
		}
		*/

		if(battle.log.Count > 0) {
			int start = Mathf.Max(0, battle.log.Count - 7);
			int end = battle.log.Count - 1;
			string text = "";
			for(int i = start; i <= end; i++) {
				if(i > start) {
					text += "\n";
				}
				text += battle.log[i];
			}

			battleLog.SetText(text);
		}
	}

	public void SetInformation(UIObj hovered) {
		if(hovered is SpellButton) {
			viewedSpell = ((SpellButton)hovered).spell;
			spellName.SetText(viewedSpell.GetName());
			description.SetText(viewedSpell.GetLongDescription());
			infoPane.OpenTab(1);
		}
	}

	public void ResetInformation(UIObj unhovered) {
		if(unhovered is SpellButton) {
			infoPane.OpenTab(0);
		}
	}

	public void OnPawnUpdate(Battle battle, Pawn newPawn) {
		Pawn oldPawn = battle.GetPawn(newPawn.GetId());
		if(oldPawn.GetId() == battle.allies.Length) {
			EnemyPawnCard epc = pawnCards[oldPawn] as EnemyPawnCard;
			pawnCards.Remove(oldPawn);
			epc.pawn = newPawn;
			pawnCards.Add(newPawn, epc);
		} else {
			PlayerPawnCard ppc = pawnCards[oldPawn] as PlayerPawnCard;
			pawnCards.Remove(oldPawn);
			ppc.pawn = newPawn;
			pawnCards.Add(newPawn, ppc);
		}
	}

	public override void OnOpen() {
		EventBus.UIMouseEnter.AddListener(SetInformation);
		EventBus.UIMouseExit.AddListener(ResetInformation);
		EventBus.PawnDamage.AddListener(OnPawnDamage);
		EventBus.PawnHeal.AddListener(OnPawnHeal);
		EventBus.CastSpellPre.AddListener(OnBeforeCast);
		EventBus.PawnUpdate.AddListener(OnPawnUpdate);
	}

	public override void OnClose() {
		EventBus.UIMouseEnter.RemoveListener(SetInformation);
		EventBus.UIMouseExit.RemoveListener(ResetInformation);
		EventBus.PawnDamage.RemoveListener(OnPawnDamage);
		EventBus.PawnHeal.RemoveListener(OnPawnHeal);
		EventBus.CastSpellPre.RemoveListener(OnBeforeCast);
		EventBus.PawnUpdate.RemoveListener(OnPawnUpdate);
	}
}
