using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile {

	public static Tile Wall = new Tile("Wall", Color.white);
	public static Tile Floor = new Tile("Floor", Color.gray);
	public static Tile Water = new Tile("Waves", Color.blue);
	public static Tile Gate = new Tile("Gate", Color.yellow);
	public static Tile CableH = new Tile("Horizontal", Color.yellow);
	public static Tile CableV = new Tile("Vertical", Color.yellow);
	private string sprite;
	private Color color;

	private Tile(string sprite, Color color) {
		this.sprite = sprite;
		this.color = color;
	}

	public string GetSprite() {
		return sprite;
	}

	public Color GetColor() {
		return color;
	}
}
