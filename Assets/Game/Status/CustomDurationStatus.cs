using System;

public class CustomDurationStatus : DurationStatus {

	private Action<Pawn> add;
	private Action<Pawn> remove;

	public CustomDurationStatus(StatusType type, int duration, Action<Pawn> add, Action<Pawn> remove) : base(type, duration) {
		this.add = add;
		this.remove = remove;
	}

	protected override void OnStatusAdded() {
		add.Invoke(pawn);
	}

	protected override void OnStatusRemoved() {
		remove.Invoke(pawn);
	}
}
