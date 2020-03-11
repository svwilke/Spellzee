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
	private Text affinityNames;
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
	private int pawnSelectedByKey;

	private Dictionary<int, int> pawnIdToIndex;
	private Dictionary<int, int> pawnIndexToId;

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

	public void OnPawnDied(Battle battle, Pawn pawn) {
		battle.log.Add(pawn.GetName() + " dies.");
	}

	public void OnBeforeCast(Battle battle, Pawn pawn, Pawn target, string spellId) {
		Spell spell = Spells.Get(spellId);
		string end = ".";
		if(target != null) {
			end = " on " + target.GetName() + end;
		}
		battle.log.Add(pawn.GetName() + " casts " + spell.GetName() + end);
	}

	public override void OnConstruct() {
		int rollsW = size.width / 5 * 3;
		int rollsMinX = size.width - 102 - rollsW;
		int y = size.height - 62 - 44;
		AddUIObj(rollButton = new ImageButton(new Vector2i(rollsMinX + 1, y + 22), RB.PackedSpriteGet("RollButton", Game.SPRITEPACK_BATTLE), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER));
		rollButton.SetOnClick(() => {
			if(battle.rollsLeft > 0) {
				Game.client.Send(GameMsg.Roll, new EmptyMessage());
			}
		});
		rollButton.SetKeybind(KeyCode.Space);
		AddUIObj(passButton = new TextButton(new Vector2i(rollsMinX - 32, y + 22), "Pass", RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER));
		passButton.SetOnClick(() => {
			if(battle.rollsLeft == 0) {
				Game.client.Send(GameMsg.Pass, new EmptyMessage());
				passButton.isVisible = false;
			}
		});
		passButton.isVisible = false;
		passButton.SetKeybind(KeyCode.Return);
		AddUIObj(bottomPane = new TabbedPane(new Vector2i(0, size.height - 72), new Vector2i(size.width / 2 + 1, 72)));
		bottomPane.SetTabs(new string[] { "Battle", "Inventory" });
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
		AddUIObj(affinityNames = new Text(new Vector2i(size.width / 2 + 5 + infoPaneWidth / 2, size.height - 58), new Vector2i(infoPaneWidth / 2 - 6, 40), RB.ALIGN_H_LEFT | RB.ALIGN_V_TOP));
		affinityNames.SetColor(Color.white);
		infoPane.AddToTab(0, affinityNames);
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

		pawnIdToIndex = new Dictionary<int, int>();
		pawnIndexToId = new Dictionary<int, int>();

		pawnCards = new Dictionary<Pawn, UIObj>();
		List<int> pawnIds = battle.GetPawnIds();
		viewPawnImages = new Image[pawnIds.Count];
		string[] tabNames = new string[pawnIds.Count];
		int spellCount = 0;
		PawnCard ppc;
		PackedSprite viewSprite = RB.PackedSpriteGet("Eye", Game.SPRITEPACK_BATTLE);
		for(int i = 0; i < pawnIds.Count; i++) {
			pawnIdToIndex.Add(pawnIds[i], i);
			pawnIndexToId.Add(i, pawnIds[i]);
			int pawnId = pawnIds[i];
			Pawn pawn = battle.GetPawn(pawnId);
			int y = pawnId % 4;
			int x = pawnId / 4;
			int off = x > 1 ? 5 : 0;
			Vector2i pos = new Vector2i(4 + + off + 92 * x, 4 + 40 * y);
			AddUIObj(ppc = new PawnCard(pos, new Vector2i(90, 38), pawn, battle));
			pawnCards.Add(pawn, ppc);
			AddUIObj(viewPawnImages[i] = new Image(pos + new Vector2i(3, 14), viewSprite));
			viewPawnImages[i].SetAlpha(180);
			if(i > 0) viewPawnImages[i].isVisible = false;
			int spellTabId = i;
			ppc.SetOnClick(() => {
				ViewSpellTabIndex(spellTabId);
			});
			tabNames[i] = pawn.GetName();
			spellCount += pawn.GetSpells().Length;
		}

		spellPane = new TabbedPane(new Vector2i(size.width - 102, 0), new Vector2i(102, size.height), true);
		spellPane.SetTabs(tabNames);
		AddUIObj(spellPane);
		spellButtons = new SpellButton[spellCount];
		spellButtonOwnership = new int[spellCount];
		int currentSpellIndex = 0;
		for(int i = 0; i < pawnIds.Count; i++) {
			currentSpellIndex = FillSpellPane(spellPane, pawnIds[i], i, currentSpellIndex);
		}
		ViewSpellTab(0);
	}

	public void UpdateDieButtons() {
		if(dieButtons != null) {
			for(int i = 0; i < dieButtons.Length; i++) {
				RemoveUIObj(dieButtons[i]);
			}
		}
		int rollsW = size.width / 5 * 3;
		int rollsMinX = size.width - 102 - rollsW; // spellPane left - width
		int step = rollsW / (battle.rolls.Length + 1);
		int start = rollsMinX + step;
		dieButtons = new DieButton[battle.rolls.Length];
		lockPositions = new Vector2i[battle.rolls.Length];
		int y = size.height - 62 - 44 + 3 + 3; // infoPane top - 44 (top of die pane) + 3 (border) + 6 (half of lock)
		for(int i = 0; i < battle.rolls.Length; i++) {
			Vector2i buttonTopLeft = new Vector2i(-16 + start + step * i, y);
			AddUIObj(dieButtons[i] = new DieButton(battle, i, buttonTopLeft));
			lockPositions[i] = new Vector2i(buttonTopLeft.x + 7, buttonTopLeft.y + 1);
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

	private int FillSpellPane(TabbedPane pane, int pawnId, int pawnIndex, int buttonIndexStart) {
		Spell[] knownSpells = battle.GetPawn(pawnId).GetSpells();
		for(int i = 0; i < knownSpells.Length; i++) {
			SpellButton sb = new SpellButton(this, battle, knownSpells[i], new Vector2i(size.width - 97, 5 + i * 31), pawnId != Game.peerId);
			if(pawnId == Game.peerId) {
				sb.SetKeybind(KeyCode.Alpha1 + i);
			}
			spellButtons[i + buttonIndexStart] = sb;
			spellButtonOwnership[i + buttonIndexStart] = pawnId;
			AddUIObj(sb);
			pane.AddToTab(pawnIndex, sb);
		}
		return buttonIndexStart + knownSpells.Length;
	}

	private Vector2i rollCircleCenter;
	private int rollCircleRadius;
	public override void RenderBackground() {
		int rollsW = size.width / 5 * 3;
		int rollsMinX = size.width - 102 - rollsW; // spellPane left - width
		int step = rollsW / (battle.rolls.Length);
		int start = rollsMinX + step;
		int y = size.height - 62 - 44; // infoPane top - 44
		Rect2i diceBackgroundRect = new Rect2i(rollsMinX, y, rollsW, 44);
		RB.DrawRectFill(diceBackgroundRect, Color.gray);
		RB.DrawRect(diceBackgroundRect.Expand(-1), Color.white);
		rollCircleCenter = diceBackgroundRect.min + new Vector2i(0, diceBackgroundRect.height / 2);
		rollCircleRadius = diceBackgroundRect.height / 2 - 8;
		Vector2i circleRadius = new Vector2i(rollCircleRadius, rollCircleRadius);
		RB.DrawEllipseFill(rollCircleCenter, circleRadius, Color.gray);
		RB.DrawEllipse(rollCircleCenter, circleRadius - new Vector2i(1, 1), Color.white);
		RB.DrawRectFill(new Rect2i(diceBackgroundRect.min + new Vector2i(1, 1), new Vector2i(circleRadius.x, diceBackgroundRect.height - 2)), Color.gray);
		RB.DrawPixel(new Vector2i(rollCircleCenter + new Vector2i(0, circleRadius.y)), Color.white);
		RB.DrawPixel(new Vector2i(rollCircleCenter - new Vector2i(0, circleRadius.y)), Color.white);
	}

	public override void RenderForeground() {

		for(int i = 0; i < battle.rolls.Length; i++) {
			if(battle.locks[i]) {
				RB.DrawSprite("Lock", lockPositions[i] - new Vector2i(6, 6));
			}
		}

		float step = Mathf.PI / (battle.rollsHad + 1);
		float start = -step;
		for(int i = 0; i < battle.rollsLeft; i++) {
			Vector2i pos = rollCircleCenter + new Vector2i(Mathf.RoundToInt(Mathf.Sin(start - step * i) * rollCircleRadius), Mathf.RoundToInt(Mathf.Cos(start - step * i) * rollCircleRadius));
			RB.DrawEllipseFill(pos, new Vector2i(4, 4), Color.white);
			RB.DrawEllipse(pos, new Vector2i(4, 4), Color.black);
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
			RB.DrawRect(new Rect2i(originButton.pos, originButton.size).Expand(1), Color.yellow);
			RB.DrawPixel(originPoint + new Vector2i(1, 0), Color.white);
			RB.DrawLine(originPoint + new Vector2i(0, 1), targetPoint + new Vector2i(0, 1), Color.yellow);
			RB.DrawLine(originPoint, targetPoint, Color.white);
			RB.DrawLine(originPoint - new Vector2i(0, 1), targetPoint - new Vector2i(0, 1), Color.yellow);
		}
	}

	private Vector2i lastMousePos;

	public override void Update(bool hasFocus = true) {
		if(renderTargeting) {
			Rect2i onRect = new Rect2i(0, 0, 0, 0);
			UIObj on = null;
			Pawn pawnTarget = null;
			Vector2i mouse = RB.PointerPos();
			if((mouse - lastMousePos).SqrMagnitude() > 2) {
				pawnSelectedByKey = -1;
			}
			List<int> pawnIds = battle.GetPawnIds();
			for(int i = 0; i < pawnIds.Count; i++) {
				if(RB.KeyPressed(KeyCode.Alpha1 + i)) {
					lastMousePos = mouse;
					pawnSelectedByKey = pawnIds[i];
				}
			}
			if(RB.KeyPressed(KeyCode.Escape) || RB.KeyPressed(KeyCode.Backspace)) {
				renderTargeting = false;
				targetSpell = null;
				targetPawn = null;
				SetTooltip("");
				return;
			}
			if(pawnSelectedByKey >= 0) {
				Pawn p = battle.GetPawn(pawnSelectedByKey);
				UIObj card = pawnCards[p];
				mouse = card.pos + card.size / 2;
				on = card;
				pawnTarget = p;
				onRect = new Rect2i(on.pos, on.size);
			}
			foreach(KeyValuePair<Pawn, UIObj> cardPair in pawnCards) {
				onRect = new Rect2i(cardPair.Value.pos, cardPair.Value.size);
				if(onRect.Contains(mouse)) {
					on = cardPair.Value;
					pawnTarget = cardPair.Key;
					break;
				}
			}
			if(on != null && pawnTarget != null) {
				if(targetSpell.IsValidTarget(pawnTarget, battle.BuildContext())) {
					if(pawnTarget != targetPawn) {
						SetTooltip(targetSpell.GetShortDescription(battle.BuildContext(pawnTarget.GetId())));
					}
					targetPawn = pawnTarget;
					targetPoint = new Vector2i(onRect.x + onRect.width, onRect.y + onRect.height / 2);
					targetRect = new Rect2i(onRect.x - 1, onRect.y - 1, onRect.width + 1, onRect.height + 1);
					if(RB.ButtonPressed(RB.BTN_POINTER_A) || RB.KeyPressed(KeyCode.Return)) {
						Game.client.Send(GameMsg.CastSpell, new GameMsg.MsgCastSpell() { spellId = targetSpell.GetId(), targetId = targetPawn.GetId() });
						renderTargeting = false;
						targetSpell = null;
						targetPawn = null;
						SetTooltip("");
					}
				} else {
					targetPawn = null;
					SetTooltip(targetSpell.GetShortDescription(battle.BuildContext(-1)));
					targetPoint = mouse;
					if(RB.ButtonPressed(RB.BTN_POINTER_ANY)) {
						renderTargeting = false;
						targetSpell = null;
						targetPawn = null;
						SetTooltip("");
					}
				}
			} else {
				targetPawn = null;
				SetTooltip(targetSpell.GetShortDescription(battle.BuildContext(-1)));
				targetPoint = mouse;
				if(RB.ButtonPressed(RB.BTN_POINTER_ANY)) {
					renderTargeting = false;
					targetSpell = null;
					targetPawn = null;
					SetTooltip("");
				}
			}
		} else {
			base.Update(hasFocus);
		}
	}

	public void ViewSpellTab(int pawnId) {
		ViewSpellTabIndex(pawnIdToIndex[pawnId]);
	}

	public void ViewSpellTabIndex(int pawnIndex) {
		for(int j = 0; j < viewPawnImages.Length; j++) {
			if(viewPawnImages[j] != null) {
				viewPawnImages[j].isVisible = j == pawnIndex;
			}
		}
		spellPane.OpenTab(pawnIndex);
		infoPane.OpenTab(0);
		Pawn pawn = battle.GetPawn(pawnIndexToId[pawnIndex]);
		playerName.SetText(pawn.GetName());
		string affText = "";
		FastString perc = new FastString(3);

		double affTotal = pawn.GetAffinityTotal();
		string affTitles = "";
		for(int i = 0; i < Element.Count; i++) {
			perc.Clear();
			double aff = pawn.GetAffinity(i);
			perc.Append((int)(aff / affTotal * 100), 2, FastString.FILL_SPACES);
			if(aff > 0) {
				affText += "\n" /*+ Element.All[i].GetColorHex()*/ + aff + " (" + perc + "%)";
				affTitles += "\n " + Element.All[i].GetColoredName();
			}
		}
		affinityNames.SetText(affTitles);
		affinities.SetText(affText);

		foreach(Text text in currentItemTexts) {
			RemoveUIObj(text);
		}
		string[] eq = pawn.GetEquipment();
		int currentY = size.height - 58;
		for(int i = 0; i < eq.Length; i++) {
			Equipment e = Equipments.Get(eq[i]);
			Text text = new Text(new Vector2i(5, currentY), new Vector2i(), RB.ALIGN_H_CENTER | RB.ALIGN_V_CENTER, e.GetName());
			text.FitSizeToText(1);
			text.SetTooltip(e.GetDescription());
			currentY += text.size.height + 2;
			AddUIObj(text);
			currentItemTexts.Add(text);
			bottomPane.AddToTab(1, text);
		}
		UpdateContext();
	}

	public void UpdateContext() {
		RollContext context = battle.BuildContext();
		bool anyCastable = false;
		for(int i = 0; i < spellButtons.Length; i++) {
			SpellButton b = spellButtons[i];
			b.castable = b.spell.Matches(context) && b.spell.IsCastable(context);
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
		PawnCard ppc;
		if(oldPawn != null) {
			ppc = pawnCards[oldPawn] as PawnCard;
			pawnCards.Remove(oldPawn);
		} else {
			ppc = new PawnCard(new Vector2i(0, 0), new Vector2i(100, 50), newPawn, battle);
		}
		ppc.pawn = newPawn;
		pawnCards.Add(newPawn, ppc);
	}

	public override void OnOpen() {
		RB.SpriteSheetSet(Game.SPRITEPACK_BATTLE);
		EventBus.UIMouseEnter.AddListener(SetInformation);
		EventBus.UIMouseExit.AddListener(ResetInformation);
		EventBus.PawnDamage.AddListener(OnPawnDamage);
		EventBus.PawnHeal.AddListener(OnPawnHeal);
		EventBus.PawnDied.AddListener(OnPawnDied);
		EventBus.CastSpellPre.AddListener(OnBeforeCast);
		EventBus.PawnUpdate.AddListener(OnPawnUpdate);
	}

	public override void OnClose() {
		EventBus.UIMouseEnter.RemoveListener(SetInformation);
		EventBus.UIMouseExit.RemoveListener(ResetInformation);
		EventBus.PawnDamage.RemoveListener(OnPawnDamage);
		EventBus.PawnHeal.RemoveListener(OnPawnHeal);
		EventBus.PawnDied.RemoveListener(OnPawnDied);
		EventBus.CastSpellPre.RemoveListener(OnBeforeCast);
		EventBus.PawnUpdate.RemoveListener(OnPawnUpdate);
	}
}
