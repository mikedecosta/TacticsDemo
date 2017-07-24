using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GraphNode : Node {
	public GraphNode(bool _walkable, Vector3 _worldPosition, int _gridX, int _gridY, int _movementPenalty) : base(_walkable, _worldPosition, _gridX, _gridY, _movementPenalty) { }
	
	new public List<Node> Neighbors {
		get {
			if (Neighbors == null) {
				Neighbors = new List<Node>();
			}
			
			return Neighbors;
		}
		
		set {
			Neighbors = value;
		}
	}
	
	public List<int> Costs {
		get {
			if (Costs == null) {
				Costs = new List<int>();
			}
			
			return Costs;
		}
		
		set {
			Costs = value;
		}
	}
	
}