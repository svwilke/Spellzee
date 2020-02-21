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

	public static Element None = new Element(7, "None", Color.white, "FFFFFF");

	public static Element[] All = new Element[] {
		Fire = new Element(0, "Fire", new Color(239F / 255F, 45F / 255F, 31F / 255F), "EF2D1F"),
		Water = new Element(1, "Water", new Color(31F / 255F, 45F / 255F, 239F / 255F), "1F2DEF"),
		Earth = new Element(2, "Earth", new Color(31F / 255F, 128F / 255F, 31F / 255F), "1F801F"),
		Air = new Element(3, "Air", new Color(131F / 255F, 219F / 255F, 219F / 255F), "83DBDB"),
		Light = new Element(4, "Light", new Color(239F / 255F, 239F / 255F, 31F / 255F), "EFEF1F"),
		Dark = new Element(5, "Dark", new Color(44F / 255F, 44F / 255F, 55F / 255F), "2C2C37"),
		Chaos = new Element(6, "Chaos", new Color(128F / 255F, 31F / 255F, 128F / 255F), "801F80"),
		None
	};

	private int id;
	private string name;
	private Color color;
	private string colorHex;

	private Element(int id, string name, Color color, string colorHex) {
		this.id = id;
		this.name = name;
		this.color = color;
		this.colorHex = colorHex;
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

	public int GetId() {
		return id;
	}
}
