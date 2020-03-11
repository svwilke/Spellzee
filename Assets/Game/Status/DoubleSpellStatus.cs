using System;

public class DoubleSpellStatus : Status {

	public DoubleSpellStatus() : base(StatusType.Positive) {
		
	}

	protected override void OnStatusAdded() {
		pawn.OnBeforeSpellCast.AddListener(CastSpellAgain);
	}

	protected override void OnStatusRemoved() {
		pawn.OnBeforeSpellCast.RemoveListener(CastSpellAgain);
	}

	public void CastSpellAgain(Spell spell, RollContext context) {
		pawn.CmdRemoveStatus(this);
		(context.GetBattle() as ServerBattle).DoCastSpell(spell.GetId(), context.GetTarget() != null ? context.GetTarget().GetId() : -1, true, false);
	}
}
