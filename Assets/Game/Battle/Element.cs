using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Element {

	public static Element Fire;
	public static Element Water;
	public static Element Earth;
	public static Element Air;
	public static Element Light;
	public static Element Dark;
	public static Element Chaos;

	public const int Count = 9;

	public static Element Physical;
	public static Element None;

	public static Element[] All = new Element[] {
		Fire = new Element(0, "Fire", new Color(239F / 255F, 45F / 255F, 31F / 255F), "EF2D1F"),
		Water = new Element(1, "Water", new Color(31F / 255F, 45F / 255F, 239F / 255F), "1F2DEF"),
		Earth = new Element(2, "Earth", new Color(31F / 255F, 128F / 255F, 31F / 255F), "1F801F"),
		Air = new Element(3, "Air", new Color(131F / 255F, 219F / 255F, 219F / 255F), "83DBDB"),
		Light = new Element(4, "Light", new Color(239F / 255F, 239F / 255F, 31F / 255F), "EFEF1F"),
		Dark = new Element(5, "Dark", new Color(44F / 255F, 44F / 255F, 55F / 255F), "2C2C37"),
		Chaos = new Element(6, "Chaos", new Color(128F / 255F, 31F / 255F, 128F / 255F), "801F80", baseAffinity: 0),
		Physical = new Element(7, "Phys", Color.white, "FFFFFF", baseAffinity: 0),
		None = new Element(8, "None", Color.white, "FFFFFF", baseAffinity: 0)
	};

	private int id;
	private string name;
	private Color color;
	private string colorHex;
	private double baseAffinity;

	private Element(int id, string name, Color color, string colorHex, double baseAffinity = 10) {
		this.id = id;
		this.name = name;
		this.color = color;
		this.colorHex = colorHex;
		this.baseAffinity = baseAffinity;
	}

	public string GetName() {
		return name;
	}

	public Color GetColor() {
		return color;
	}

	public string GetColorHex() {
		return "@" + colorHex;
	}

	public string GetColoredName() {
		return GetColorHex() + GetName() + "@-";
	}

	public double GetBaseAffinity() {
		return baseAffinity;
	}

	public int GetId() {
		return id;
	}
}
