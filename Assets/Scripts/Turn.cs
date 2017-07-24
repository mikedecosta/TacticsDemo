﻿using UnityEngine;
using System.Collections;

public class Turn : MonoBehaviour {
	
	SortedList units;
	Unit[] unsorted;
	Unit currentUnit;

	// Use this for initialization
	void Start () {
		Unit[] unsorted = FindObjectsOfType<Unit>();
		currentUnit = unsorted[0];
	}
	
	private void FillInitiative() {
		foreach ( Unit unit in unsorted) {
			units.Add(unit.GetInitiative(), unit);
		}
	}
	
	public Unit getCurrentUnit() {
		return currentUnit;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void OnTurnSet(Vector3 newTargetPosition, bool turnSet) {
		if (turnSet) {
			
		}
	}
}
