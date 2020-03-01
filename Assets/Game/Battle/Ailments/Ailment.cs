using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ailment : RegistryEntry<Ailment> {

	private string fullName;
	private string shortName;

	private Color color;

	protected GainType gainType;

	public Ailment(string fullName, string shortName, Color color) {
		this.fullName = fullName;
		this.shortName = shortName;
		this.color = color;
		gainType = GainType.Stacking;
	}

	public string GetFullName() {
		return fullName;
	}

	public string GetShortName() {
		return shortName;
	}

	public Color GetColor() {
		return color;
	}

	public virtual void OnIntensityChange(Pawn pawn, int oldIntensity, int newIntensity) {
		if(oldIntensity == 0 && newIntensity > 0) {
			OnGain(pawn, newIntensity);
		} else
		if(oldIntensity > 0 && newIntensity == 0) {
			OnLose(pawn, oldIntensity);
		} else
		if(gainType == GainType.Stacking) {
			if(oldIntensity != newIntensity) {
				OnLose(pawn, oldIntensity);
				OnGain(pawn, newIntensity);
			}
		}
	}

	public virtual void ApplyToPawn(Pawn pawn, int intensity) {
		pawn.SetAilment(this, intensity);
	}

	public void RemoveFromPawn(Pawn pawn) {
		pawn.SetAilment(this, 0);
	}

	public abstract void OnGain(Pawn pawn, int intensity);
	public abstract void OnLose(Pawn pawn, int intensity);

	protected enum GainType {
		Stacking, Once
	}
}
