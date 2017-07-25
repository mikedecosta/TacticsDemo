using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(Grid))]
public class Pathfinder : MonoBehaviour {

	[SerializeField] bool allowDiagonals = true;
	[SerializeField] bool simplifyPaths = false;
	
	PathRequestManager pathRequestManager;
	Grid grid;
	
	void Awake() {
		grid = GetComponent<Grid>();
		pathRequestManager = GetComponent<PathRequestManager>();
	}
	
	public void StartFindPath(Vector3 startPos, Vector3 targetPos, float maxDistance) {
		StartCoroutine(FindPath(startPos, targetPos, maxDistance));	
	}
	
	public void StartHighlightPaths(Vector3 startPos, float maxDistance) {
		StartCoroutine(HighlightPaths(startPos, maxDistance));	
	}

	IEnumerator FindPath(Vector3 startPos, Vector3 targetPos, float maxDistance) {
		Vector3[] waypoints = new Vector3[0];
		bool pathFound = false;
		
		Node startNode = grid.NodeFromWorldPoint(startPos);
		Node targetNode = grid.NodeFromWorldPoint(targetPos);
		
		//Debug.Log ("startNode = " + startNode.ToString());
		//Debug.Log ("targetNode = " + targetNode.ToString());
		
		Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);
		
		while (openSet.Count > 0) {
			Node currentNode = openSet.RemoveFirst();
			closedSet.Add(currentNode);
			
			if (currentNode == targetNode) {
				pathFound = currentNode.gCost <= maxDistance;
				break;
			}
			
			List<Node> neighbors = allowDiagonals ? grid.GetNeighborsAllowDiaganols(currentNode) : grid.GetNighborsDissallowDiaganols(currentNode);
			foreach (Node neighbor in neighbors) {
				if (!neighbor.walkable || neighbor.isOccupied || closedSet.Contains(neighbor)) {
					continue;
				}
				
				int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor) + neighbor.movementPenalty;
				if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor)) {
					neighbor.gCost = newMovementCostToNeighbor;
					neighbor.hCost = GetDistance(neighbor, targetNode);
					neighbor.parent = currentNode;
					
					if (!openSet.Contains(neighbor)) {
						openSet.Add(neighbor);
					} else {
						openSet.UpdateItem(neighbor);
					}
				}
			}
		}
		yield return null;
		if (pathFound) {
			waypoints = RetracePath(startNode, targetNode);
		}
		
		foreach(Node node in closedSet) {
			node.resetCosts();
		}
		while(openSet.Count > 0) {
			Node node = openSet.RemoveFirst();
			node.resetCosts();
		}
		pathRequestManager.FinishedProcessingPath(waypoints, pathFound);
	}
	
	IEnumerator HighlightPaths(Vector3 startPos, float magnitude) {
		HashSet<Node> highlight = new HashSet<Node>();
		
		Node startNode = grid.NodeFromWorldPoint(startPos);
		
		Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);
		
		while (openSet.Count > 0) {
			Node currentNode = openSet.RemoveFirst();
			closedSet.Add(currentNode);
			
			List<Node> neighbors = allowDiagonals ? grid.GetNeighborsAllowDiaganols(currentNode) : grid.GetNighborsDissallowDiaganols(currentNode);
			foreach (Node neighbor in neighbors) {
				if (!neighbor.walkable || neighbor.isOccupied || closedSet.Contains(neighbor)) {
					continue;
				}
				
				int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor) + neighbor.movementPenalty;
				if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor)) {
					neighbor.gCost = newMovementCostToNeighbor;
					neighbor.hCost = 0;
					neighbor.parent = currentNode;
					
					if (newMovementCostToNeighbor > magnitude) {
						neighbor.gCost = 0;
						continue;
					}
					
					if (!highlight.Contains(neighbor)) {
						highlight.Add(neighbor);
					} else {
						//Debug.Log("highlight.Contains(neighbor)");
						//Debug.Log("neighbor = "+neighbor.ToString());
					}
					
					if (!openSet.Contains(neighbor)) {
						openSet.Add(neighbor);
					} else {
						openSet.UpdateItem(neighbor);
					}
				}
			}
		}
		yield return null;
		
		foreach(Node node in closedSet) {
			node.resetCosts();
		}
		while(openSet.Count > 0) {
			Node node = openSet.RemoveFirst();
			node.resetCosts();
		}
		
		pathRequestManager.FinishedProcessingHighlight(highlight);
	}
	
	Vector3[] RetracePath(Node startNode, Node endNode) {
		List<Node> path = new List<Node>();
		Node currentNode = endNode;
		
		while (currentNode != startNode) {
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		
		Vector3[] waypoints;
		if (simplifyPaths) {
			waypoints = SimplifyPath(path);
		} else {
			waypoints = NodeListToVector3Array(path);
		}
		
		Array.Reverse(waypoints);
		return waypoints;
	}
	
	Vector3[] NodeListToVector3Array(List<Node> path) {
		List<Vector3> waypoints = new List<Vector3>();
		for (int i = 0; i < path.Count; i++) {
			waypoints.Add(path[i].worldPosition);
		}
		
		return waypoints.ToArray();
	}
	
	// TODO: fix for dissallow diaganols
	// TODO: fix always add path[0] and path[1]
	Vector3[] SimplifyPath(List<Node> path) {
		List<Vector3> waypoints = new List<Vector3>();
		Vector2 directionOld = Vector2.zero;
		waypoints.Add(path[0].worldPosition);
		
		for (int i = 1; i < path.Count; i++) {
			Vector2 directionNew = new Vector2(path[i-1].gridX - path[i].gridX, path[i-1].gridY - path[i].gridY);
			if (directionNew != directionOld) {
				waypoints.Add(path[i].worldPosition);
				directionOld = directionNew;
			}
		}
		
		return waypoints.ToArray();
	}
	
	int GetDistance(Node nodeA, Node nodeB) {
		if (allowDiagonals) {
		 return GetDistanceAllowDiagonals(nodeA, nodeB);
		}
		
		return GetDistanceDisallowDiagonals(nodeA, nodeB);
	}
	
	int GetDistanceAllowDiagonals(Node nodeA, Node nodeB) {
		int distX = Mathf.Abs (nodeA.gridX - nodeB.gridX);
		int distY = Mathf.Abs (nodeA.gridY - nodeB.gridY);
		
		if (distX > distY) {
			return 14*distY + 10*(distX - distY);
		}
		
		return 14*distX + 10*(distY - distX);
	}
	
	int GetDistanceDisallowDiagonals(Node nodeA, Node nodeB) {
		int distX = Mathf.Abs (nodeA.gridX - nodeB.gridX);
		int distY = Mathf.Abs (nodeA.gridY - nodeB.gridY);
		int distHeight = Mathf.RoundToInt(Mathf.Abs (nodeA.worldPosition.y - nodeB.worldPosition.y));
		
		return 10*distX + 10*distY + distHeight;
	}
}
