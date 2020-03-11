using System;

public class CustomStatus : Status {

	private Action<Pawn> add;
	private Action<Pawn> remove;

	public CustomStatus(StatusType type, Action<Pawn> add, Action<Pawn> remove) : base(type) {
		this.add = add;
		this.remove = remove;
	}

	protected override void OnStatusAdded() {
		add.Invoke(pawn);
		pawn.Synchronize();
	}

	protected override void OnStatusRemoved() {
		remove.Invoke(pawn);
		pawn.Synchronize();
	}
}
