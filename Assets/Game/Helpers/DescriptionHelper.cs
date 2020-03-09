using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SpellComponent;

public static class DescriptionHelper {

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
			/*case TargetType.Target:
				switch(targetGroup) {
					case TargetGroup.AnyOther:
						suffix += " to someone else";
						break;
					case TargetGroup.Ally:
						suffix += " to an ally";
						break;
					case TargetGroup.AllyOther:
						suffix += " to an ally other than yourself";
						break;
					case TargetGroup.Enemy:
						suffix += " to an enemy";
						break;
					case TargetGroup.Custom:
						suffix += " to a valid target";
						break;
				}
				break;*/
		}
		return suffix;
	}
}
