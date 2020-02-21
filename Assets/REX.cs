using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REX {

	public static T Choice<T>(List<T> items) {
		return Choice(items.ToArray());
	}

	public static T Choice<T>(T[] items) {
		return items[Random.Range(0, items.Length)];
	}

	public static T[] Choice<T>(T[] items, int count) {
		List<T> itemList = new List<T>(items);
		List<T> result = new List<T>(count);
		while(result.Count < count && itemList.Count > 0) {
			int index = Random.Range(0, itemList.Count);
			T item = itemList[index];
			result.Add(item);
			itemList.RemoveAt(index);
		}
		return result.ToArray();
	}

	public static int Weighted(double[] weights) {
		double sum = 0f;
		for(int i = 0; i < weights.Length; i++) {
			sum += weights[i];
		}
		double r = Random.value * sum;
		int c = 0;
		while(r >= 0f) {
			r -= weights[c];
			c++;
		}
		return c - 1;
	}

	public static int Weighted(float[] weights) {
		float sum = 0f;
		for(int i = 0; i < weights.Length; i++) {
			sum += weights[i];
		}
		float r = Random.value * sum;
		int c = 0;
		while(r >= 0f) {
			r -= weights[c];
			c++;
		}
		return c - 1;
	}

	public static int Weighted(int[] weights) {
		float sum = 0f;
		for(int i = 0; i < weights.Length; i++) {
			sum += (float)weights[i];
		}
		float r = Random.value * sum;
		int c = 0;
		while(r >= 0f) {
			r -= weights[c];
			c++;
		}
		return c - 1;
	}

	public static T Weighted<T>(double[] weights, T[] items) {
		if(weights.Length != items.Length) {
			throw new System.Exception("Weights and items must have the same amount of entries.");
		}
		return items[Weighted(weights)];
	}

	public static T Weighted<T>(int[] weights, T[] items) {
		if(weights.Length != items.Length) {
			throw new System.Exception("Weights and items must have the same amount of entries.");
		}
		return items[Weighted(weights)];
	}

	public static T Weighted<T>(float[] weights, T[] items) {
		if(weights.Length != items.Length) {
			throw new System.Exception("Weights and items must have the same amount of entries.");
		}
		return items[Weighted(weights)];
	}
}
