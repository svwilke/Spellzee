using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SpellComponent;

public static class DescriptionHelper {

	public static string GetDescriptionPrefix(Target target) {
		return GetDescriptionPrefix(target.GetTargetType(), target.GetTargetGroup());
	}

	public static string GetDescriptionSuffix(Target target) {
		return GetDescriptionSuffix(target.GetTargetType(), target.GetTargetGroup());
	}

	public static string GetDescriptionInfix(Target target) {
		return GetDescriptionInfix(target.GetTargetType(), target.GetTargetGroup());
	}

	public static string GetDescriptionOnfix(Target target) {
		return GetDescriptionOnfix(target.GetTargetType(), target.GetTargetGroup());
	}

	public static string GetDescriptionPrefix(TargetType targetType, TargetGroup targetGroup) {
		string prefix = "";
		switch(targetType) {
			case TargetType.Caster:
				prefix = "To you: ";
				break;
			case TargetType.Allies:
				prefix = "To allies: ";
				break;
			case TargetType.Enemies:
				prefix = "To enemies: ";
				break;
			case TargetType.All:
				prefix = "To all: ";
				break;
			case TargetType.Random:
				prefix = "To a random ";
				switch(targetGroup) {
					case TargetGroup.Any:
						prefix += "target";
						break;
					case TargetGroup.AnyOther:
						prefix += "target other than yourself";
						break;
					case TargetGroup.Ally:
						prefix += "ally";
						break;
					case TargetGroup.AllyOther:
						prefix += "ally other than yourself";
						break;
					case TargetGroup.Enemy:
						prefix += "enemy";
						break;
					case TargetGroup.Custom:
						prefix += "valid target";
						break;
				}
				prefix += ":";
				break;
			case TargetType.None:
				prefix = "To noone: ";
				break;
		}
		return prefix;
	}

	public static string GetDescriptionSuffix(TargetType targetType, TargetGroup targetGroup) {
		string suffix = "";
		switch(targetType) {
			case TargetType.Caster:
				suffix += " to yourself";
				break;
			case TargetType.Allies:
				suffix += " to all allies";
				break;
			case TargetType.Enemies:
				suffix += " to all enemies";
				break;
			case TargetType.All:
				suffix += " to everyone";
				break;
			case TargetType.Random:
				suffix += " to a random ";
				switch(targetGroup) {
					case TargetGroup.Any:
						suffix += "target";
						break;
					case TargetGroup.AnyOther:
						suffix += "target other than yourself";
						break;
					case TargetGroup.Ally:
						suffix += "ally";
						break;
					case TargetGroup.AllyOther:
						suffix += "ally other than yourself";
						break;
					case TargetGroup.Enemy:
						suffix += "enemy";
						break;
					case TargetGroup.Custom:
						suffix += "valid target";
						break;
				}
				break;
			case TargetType.None:
				suffix += " to noone";
				break;
		}
		return suffix;
	}

	public static string GetDescriptionInfix(TargetType targetType, TargetGroup targetGroup) {
		string infix = "";
		switch(targetType) {
			case TargetType.Caster:
				infix += " yourself";
				break;
			case TargetType.Allies:
				infix += " all allies";
				break;
			case TargetType.Enemies:
				infix += " all enemies";
				break;
			case TargetType.All:
				infix += " everyone";
				break;
			case TargetType.Random:
				infix += " a random ";
				switch(targetGroup) {
					case TargetGroup.Any:
						infix += "target";
						break;
					case TargetGroup.AnyOther:
						infix += "target other than yourself";
						break;
					case TargetGroup.Ally:
						infix += "ally";
						break;
					case TargetGroup.AllyOther:
						infix += "ally other than yourself";
						break;
					case TargetGroup.Enemy:
						infix += "enemy";
						break;
					case TargetGroup.Custom:
						infix += "valid target";
						break;
				}
				break;
			case TargetType.None:
				infix += " noone";
				break;
		}
		return infix;
	}

	public static string GetDescriptionOnfix(TargetType targetType, TargetGroup targetGroup) {
		string suffix = "";
		switch(targetType) {
			case TargetType.Caster:
				suffix += " on yourself";
				break;
			case TargetType.Allies:
				suffix += " on all allies";
				break;
			case TargetType.Enemies:
				suffix += " on all enemies";
				break;
			case TargetType.All:
				suffix += " on everyone";
				break;
			case TargetType.Random:
				suffix += " on a random ";
				switch(targetGroup) {
					case TargetGroup.Any:
						suffix += "target";
						break;
					case TargetGroup.AnyOther:
						suffix += "target other than yourself";
						break;
					case TargetGroup.Ally:
						suffix += "ally";
						break;
					case TargetGroup.AllyOther:
						suffix += "ally other than yourself";
						break;
					case TargetGroup.Enemy:
						suffix += "enemy";
						break;
					case TargetGroup.Custom:
						suffix += "valid target";
						break;
				}
				break;
			case TargetType.None:
				suffix += " on noone";
				break;
		}
		return suffix;
	}
}
