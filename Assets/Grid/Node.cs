using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node : IHeapItem<Node> {

	public bool walkable;
	public bool occupied;
	public bool highlighted;
	public Vector3 worldPosition;
	public int movementPenalty;
	public int gCost;
	public int hCost;
	public Node parent;
	public Vector3 key;
	int heapIndex;
	Unit occupiedUnit;
	StateManager stateManager;
	List<Node> neighbors;
	List<float> costs;
	
	public Node(bool _walkable, Vector3 _worldPosition, Vector3 _key, int _movementPenalty) {
		walkable = _walkable;
		worldPosition = _worldPosition;
		key = _key;
		movementPenalty = _movementPenalty;
	}
	
	public Node(bool _walkable, Vector3 _worldPosition, Vector3 _key, int _movementPenalty, Unit _occupiedUnit) {
		walkable = _walkable;
		worldPosition = _worldPosition;
		movementPenalty = _movementPenalty;
		occupiedUnit = _occupiedUnit;
	}
	
	public List<Node> Neighbors {
		get {
			if (neighbors == null) {
				neighbors = new List<Node>();
			}
			
			return neighbors;
		}
		
		set {
			neighbors = value;
		}
	}
	
	public List<float> Costs {
		get {
			if (costs == null) {
				costs = new List<float>();
			}
			
			return costs;
		}
		
		set {
			costs = value;
		}
	}
	
	/*
	void Start() {
		stateManager = FindObjectOfType<StateManager>();
		stateManager.activeUnitChangeObservers += HandleactiveUnitChangeObservers;
	}
	
	*/

	void HandleactiveUnitChangeObservers (Unit activeUnit)
	{
		resetCosts();
	}
	
	public delegate void OnNodeChanged(Node node);
	public event OnNodeChanged nodeChangedObservers;
	private void TriggerNodeChangedObservers(Node node) {
		if (nodeChangedObservers != null) {
			nodeChangedObservers(node);
		}
	}
	
	public int fCost {
		get {
			return gCost + hCost;
		}
	}
	
	public bool isOccupied {
		get {
			return occupiedUnit != null;
		}
	}
	
	public Unit getOccupiedUnit() {
		return occupiedUnit;
	}
	
	public void setOccupidUnit(Unit unit) {
		if (occupiedUnit == null || !occupiedUnit.Equals(unit)) {
			occupiedUnit = unit;
			TriggerNodeChangedObservers(this);
		}
	}
	
	public void unsetOccupiedUnit() {
		occupiedUnit = null;
		TriggerNodeChangedObservers(this);
	}
	
	public void resetCosts() {
		gCost = 0;
		hCost = 0;
		TriggerNodeChangedObservers(this);
	}
	
	public int HeapIndex {
		get {
			return heapIndex;
		}
		set {
			heapIndex = value;
			if (!heapIndex.Equals(value)) {
				TriggerNodeChangedObservers(this);
			}
		}
	}
	
	public int CompareTo(Node other) {
		int compare = fCost.CompareTo(other.fCost);
		
		if (compare == 0) {
			compare = hCost.CompareTo(other.hCost);
		}
		
		return -compare;
	}
	
	public override string ToString() {
		string jsString = "{ \"walkable\" : ";
		jsString += walkable ? "true, " : "false, ";
		jsString += "\"highlighted\" : ";
		jsString += highlighted ? "true, " : "false, ";
		jsString += "\"occupied\" : ";
		if (isOccupied) {
			jsString += "true, ";
			Unit unit = occupiedUnit;
			jsString += "\"occupiedUnit\" : \"" + unit.name + "\", ";
		} else {
			jsString += "false, ";
		}
		jsString += "\"worldPosition\" : " + worldPosition.ToString() + ", ";
		jsString += "\"key\" : " + key.ToString() + ", ";
		jsString += "\"gCost\" : " + gCost + ", ";
		jsString += "\"hCost\" : " + hCost + ", ";
		jsString += "\"fCost\" : " + fCost + ", ";
		if (Neighbors != null) {
			jsString += "\n\t\"neighbors\" : [";
			for(int i = 0; i < neighbors.Count; i++) {
				Node neighbor = neighbors[i];
				jsString += costs[i];
				jsString += " => " + neighbor.key + ", ";
			}
			jsString += "]\n";
		}
		jsString += "}";
		return jsString;
	}
}
