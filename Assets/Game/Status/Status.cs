using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Sirenix.Serialization;

[System.Serializable]
public class Status {

	public enum StatusType { Positive, Negative }

	[OdinSerialize] protected StatusType type;

	[OdinSerialize] protected int age;
	protected Pawn pawn;

	[OdinSerialize] private bool isVisible = false;
	[OdinSerialize] private Color color = Color.white;

	public Status(StatusType type) {
		this.type = type;
	}

	protected void SetVisible(bool visible) {
		isVisible = visible;
	}

	public bool IsVisible() {
		return isVisible;
	}

	protected void SetColor(Color color) {
		this.color = color;
	}

	public Color GetColor() {
		return color;
	}

	public bool IsPositive() {
		return type == StatusType.Positive;
	}

	public bool IsNegative() {
		return type == StatusType.Negative;
	}

	public virtual bool Merge(Status status) {
		return false;
	}

	public void OnAdded(Pawn pawn) {
		this.pawn = pawn;
		age = 0;
		pawn.OnBeginTurn.AddListener(OnTurnStart);
		pawn.OnEndTurn.AddListener(OnTurnEnd);
		OnStatusAdded();
	}

	public void OnRemoved(Pawn pawn) {
		pawn.OnBeginTurn.RemoveListener(OnTurnStart);
		pawn.OnEndTurn.RemoveListener(OnTurnEnd);
		OnStatusRemoved();
	}

	private void OnTurnStart(Battle battle, Pawn pawn) {
		age++;
		OnTurnStarted();
	}

	private void OnTurnEnd(Battle battle, Pawn pawn) {
		OnTurnEnded();
	}

	protected virtual void OnStatusAdded() {

	}

	protected virtual void OnStatusRemoved() {
		
	}

	protected virtual void OnTurnStarted() {

	}

	protected virtual void OnTurnEnded() {
		
	}

	public void Remove() {
		pawn.CmdRemoveStatus(this);
	}

	public virtual int Render(int x, int y) {
		if(IsVisible()) {
			if(IsPositive()) {
				RB.DrawSprite("buff", new Vector2i(x, y));
			} else {
				RB.DrawSprite("debuff", new Vector2i(x, y));
			}
			return 8;
		}
		return 0;
	}

	public void Serialize(NetworkWriter writer) {
		byte[] bytes = SerializationUtility.SerializeValue(this, DataFormat.Binary);
		writer.WriteBytesFull(bytes);
	}

	public static Status DeserializeNew(NetworkReader reader) {
		return SerializationUtility.DeserializeValue<Status>(reader.ReadBytesAndSize(), DataFormat.Binary);
	}
}
