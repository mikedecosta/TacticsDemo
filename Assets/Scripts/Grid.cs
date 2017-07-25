using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Grid : MonoBehaviour {

	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public int maxHeight;
	public float nodeRadius;
	public TerrainType[] walkableRegions;
	public bool allowDiagonals = false;
	
	LayerMask walkableMask;
	Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
	Graph graph;
	StateManager stateManager;
	float nodeDiameter;
	int gridSizeX;
	int gridSizeY;
	int gridHeight;
	CameraRaycaster cameraRaycaster;
	Node hoveredNode;
	HashSet<Node> highlightedNodes = new HashSet<Node>();
	bool drawGridSquares = true;
	
	void Awake() {
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
		gridHeight = Mathf.RoundToInt(maxHeight/nodeDiameter);
		
		foreach (TerrainType region in walkableRegions) {
			walkableMask.value |= region.terrainMask.value;
			walkableRegionsDictionary.Add((int) Mathf.Log(region.terrainMask.value,2), region.terrainPenalty);
		}
		CreateGrid();
	}
	
	void Start() {
		cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();
		cameraRaycaster.nodeHoverChangeObservers += ChangeHighlight;
		stateManager = FindObjectOfType<StateManager>();
		stateManager.stateChangeObservers += HandleStateChange;
	}
	
	public int MaxSize {
		get {
			return gridSizeX * gridSizeY * 1; // TODO add height
		}
	}
	
	void CreateGrid() {
		setupNodes();
		addNeighbors();
	}
	
	private void setupNodes() {
		graph = new Graph();
		Vector3 worldBottomLeft = getWorldBottomLeft();
		Node node;
		
		for (int x = 0; x < gridSizeX; x++) {
			for (int y = 0; y < gridSizeY; y++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius-.1f, unwalkableMask));
				//hits = Physics.(worldPoint + Vector3.up * (maxHeight + 1), nodeRadius-.05f, Vector3.down, 100f, unwalkableMask);
				float height = 0;
				Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
				RaycastHit hit;
				if (Physics.SphereCast(ray, nodeRadius-.1f, out hit, 100f, walkableMask)) {
					height = (hit.point.y);
					worldPoint.y = height;
				}
				
				Vector3 key = new Vector3(x, y, 0);
				node = new Node(walkable, worldPoint, key, 0);
				node.nodeChangedObservers += OnNodeChange;
				graph.AddNode(key, node);
			}
		}
	}
	
	private Vector3 getWorldBottomLeft() {
		return transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;
	}
	
	private void addNeighbors() {
		Vector3 neighborKey;
		foreach(Node workingNode in graph) {
			for (int i = -2; i <= 2; i++) {
				for (int j = -2; j <= 2; j++) {
					int neighborX = (int) workingNode.key.x + i;
					int neighborY = (int) workingNode.key.y + j;
					if ( 
					    isSelf(i, j) || 
					    !isOnGrid(neighborX, neighborY) ||
					    (!allowDiagonals && isDiagonal(i, j))
					    ) {
						continue;
					}
					
					neighborKey = new Vector3(neighborX, neighborY, 0);
					float weight = (float) Math.Round( (workingNode.worldPosition - graph.getNodeFromKey(neighborKey).worldPosition).magnitude, 2);
					workingNode.Neighbors.Add(graph.getNodeFromKey(neighborKey));
					workingNode.Costs.Add(weight);
				}
			}
		}
	}
	
	public List<Node> GetNeighborsAllowDiaganols(Node node) {
		return node.Neighbors;
		//return getNeighbors(node, true);
	}

	public List<Node> GetNighborsDissallowDiaganols(Node node) {
		return getNeighbors(node, false);
	}
	
	/*
	           x
		+----+----+----+
		|-1-1|0,-1|1,-1|
		+----+----+----+
	y	|-1,0|0,0 |1,0 |
		+----+----+----+
		|-1,1|0,1 |1,1 |
		+----+----+----+
	
		           x
		+----+----+----+----+----+
		|-2-2|-1-2|0,-2|1,-2|2,-2|
		+----+----+----+----+----+
	y	|-2-1|-1-1|0,-1|1,-1|2,-1|
		+----+----+----+----+----+
		|-2,0|-1,0|0,0 |1,0 |1,0 |
		+----+----+----+----+----+
		|-2,1|-1,1|0,1 |1,1 |2,1 |
		+----+----+----+----+----+
		|-2,2|-1,2|0,2 |1,2 |2,2 |
		+----+----+----+----+----+
	
	*/
	
	// TODO: fix getNeighbors when allowDiagonals is true and corners are in the way
	// TODO: Allow adding other filters more easily, height filters for instance
	private List<Node> getNeighbors(Node node, bool allowDiagonals) {
		List<Node> neighbors = new List<Node>();
		
		//neighbors.AddRange(getNorthernNeighbor(node));
		//neighbors.AddRange(getEasternNeighbor(node));
		//neighbors.AddRange(getSouthernNeighbor(node));
		//neighbors.AddRange(getWesternNeighbor(node));
		
/*		
		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				int neighborX = node.gridX + x;
				int neighborY = node.gridY + y;
				if ( 
					isSelf(x, y) || 
					!isOnGrid(neighborX, neighborY) ||
					(!allowDiagonals && isDiagonal(x, y))
				) {
					continue;
				}
				
				neighbors.Add(grid[neighborX, neighborY]);
			}
		}
*/
		return node.Neighbors;
	}
/*	
	private List<Node> getNorthernNeighbor(Node node) {
		List<Node> northernNeighbor = new List<Node>();
		int neighborY;
		Node currentWorkingNeighbor;
		for (int i = 1; i < 3; i++) {
			neighborY = node.gridY + i;
			if (!isOnGrid(node.gridX, neighborY)) {
				return northernNeighbor;
			}
			
			currentWorkingNeighbor = grid[node.gridX, neighborY];
			float incline = node.worldPosition.y - currentWorkingNeighbor.worldPosition.y;
			
			if (incline <= 0.1f) {
				northernNeighbor.Add(currentWorkingNeighbor);
				return northernNeighbor;
			}
			
			if (northernNeighbor.Count == 0) {
				northernNeighbor.Add(currentWorkingNeighbor);
			} else {
				Node currentReturnNeighbor = northernNeighbor[0];
				if ( currentReturnNeighbor.worldPosition.y < currentWorkingNeighbor.worldPosition.y) {
					northernNeighbor.Remove(currentReturnNeighbor);
					northernNeighbor.Add(currentWorkingNeighbor);
				}
			}
		}
		
		return northernNeighbor;
	}
	
	private List<Node> getSouthernNeighbor(Node node) {
		List<Node> northernNeighbor = new List<Node>();
		int neighborY;
		Node currentWorkingNeighbor;
		for (int i = 1; i < 3; i++) {
			neighborY = node.gridY - i;
			if (!isOnGrid(node.gridX, neighborY)) {
				return northernNeighbor;
			}
			
			currentWorkingNeighbor = grid[node.gridX, neighborY];
			float incline = node.worldPosition.y - currentWorkingNeighbor.worldPosition.y;
			
			if (incline <= 0.1f) {
				northernNeighbor.Add(currentWorkingNeighbor);
				return northernNeighbor;
			}
			
			if (northernNeighbor.Count == 0) {
				northernNeighbor.Add(currentWorkingNeighbor);
			} else {
				Node currentReturnNeighbor = northernNeighbor[0];
				if ( currentReturnNeighbor.worldPosition.y < currentWorkingNeighbor.worldPosition.y) {
					northernNeighbor.Remove(currentReturnNeighbor);
					northernNeighbor.Add(currentWorkingNeighbor);
				}
			}
		}
		
		return northernNeighbor;
	}
	
	private List<Node> getEasternNeighbor(Node node) {
		List<Node> northernNeighbor = new List<Node>();
		int neighborX;
		Node currentWorkingNeighbor;
		for (int i = 1; i < 3; i++) {
			neighborX = node.gridX + i;
			if (!isOnGrid(neighborX, node.gridY)) {
				return northernNeighbor;
			}
			
			currentWorkingNeighbor = grid[neighborX, node.gridY];
			float incline = node.worldPosition.y - currentWorkingNeighbor.worldPosition.y;
			
			if (incline <= 0.1f) {
				northernNeighbor.Add(currentWorkingNeighbor);
				return northernNeighbor;
			}
			
			if (northernNeighbor.Count == 0) {
				northernNeighbor.Add(currentWorkingNeighbor);
			} else {
				Node currentReturnNeighbor = northernNeighbor[0];
				if ( currentReturnNeighbor.worldPosition.y < currentWorkingNeighbor.worldPosition.y) {
					northernNeighbor.Remove(currentReturnNeighbor);
					northernNeighbor.Add(currentWorkingNeighbor);
				}
			}
		}
		
		return northernNeighbor;
	}
	
	private List<Node> getWesternNeighbor(Node node) {
		List<Node> northernNeighbor = new List<Node>();
		int neighborX;
		Node currentWorkingNeighbor;
		for (int i = 1; i < 3; i++) {
			neighborX = node.gridX - i;
			if (!isOnGrid(neighborX, node.gridY)) {
				return northernNeighbor;
			}
			
			currentWorkingNeighbor = grid[neighborX, node.gridY];
			float incline = node.worldPosition.y - currentWorkingNeighbor.worldPosition.y;
			
			if (incline <= 0.1f) {
				northernNeighbor.Add(currentWorkingNeighbor);
				return northernNeighbor;
			}
			
			if (northernNeighbor.Count == 0) {
				northernNeighbor.Add(currentWorkingNeighbor);
			} else {
				Node currentReturnNeighbor = northernNeighbor[0];
				if ( currentReturnNeighbor.worldPosition.y < currentWorkingNeighbor.worldPosition.y) {
					northernNeighbor.Remove(currentReturnNeighbor);
					northernNeighbor.Add(currentWorkingNeighbor);
				}
			}
		}
		
		return northernNeighbor;
	}
*/	
	public void HighlightSquaresInRange(Node origin, float range) {
		PathRequestManager.RequestHighlights(origin.worldPosition, range, HighlightSquare);
	}
	
	public void HighlightSquare(HashSet<Node> highlight) {
		highlightedNodes = highlight;
		foreach(Node node in highlightedNodes) {
			node.highlighted = true;
		}
	}
	
	private bool isOnGrid(int x, int y) {
		return 
			x >= 0 && 
			x < gridSizeX &&
			y >= 0 &&
			y < gridSizeY;
		
	}
	
	private bool isSelf(int x, int y) {
		return x == 0 && y == 0;
	}
	
	private bool isDiagonal(int x, int y) {
		return x != 0 && y != 0;
	}
	
	public Node NodeFromWorldPoint(Vector3 worldPosition) {		
		int x = worldPointXToKeyInt(worldPosition.x);
		int y = worldPointYToKeyInt(worldPosition.z);
		int z = worldPointZToKeyInt(worldPosition.y);
		
		return graph.getNodeFromKey(new Vector3(x,y,z));
	}
	
	private int worldPointXToKeyInt(float worldPointX) {
		float percentX = Mathf.Clamp01( (worldPointX + gridWorldSize.x/2) / gridWorldSize.x );
		return Mathf.RoundToInt( (gridSizeX-1) * percentX );
	}
	
	private int worldPointYToKeyInt(float worldPointZ) {
		float percentY = Mathf.Clamp01( (worldPointZ + gridWorldSize.y/2) / gridWorldSize.y );
		return Mathf.RoundToInt( (gridSizeY-1) * percentY );
	}
	
	private int worldPointZToKeyInt(float worldPointY) {
		//float percentZ = Mathf.Clamp01( (worldPointY + maxHeight/2) / maxHeight );
		//return Mathf.RoundToInt( (maxHeight-1) * percentZ );
		return 0;
	}
	
	void OnNodeChange(Node newNode) {
		graph.UpdateNode(newNode);
	}
	
	void ChangeHighlight(Node newHoveredNode) {
		hoveredNode = newHoveredNode;
	}
	
	void HandleStateChange() {
		foreach(Node node in highlightedNodes) {
			node.highlighted = false;
		}
	}
	
	void OnDrawGizmos() {
		float halfHeight = ((float) maxHeight / 2);
		Vector3 gridOrigin = new Vector3(transform.position.x, halfHeight, transform.position.z); 
		Gizmos.DrawWireCube(gridOrigin, new Vector3(gridWorldSize.x, maxHeight, gridWorldSize.y));
		float size = (float) (1 * (nodeDiameter - .07));
		
		if (graph != null) {
			if (drawGridSquares) {
				foreach (Node n in graph) {
					if (n.isOccupied) {
						Gizmos.color = Color.red;
					} else if (hoveredNode == n) {
						Gizmos.color = Color.green;
					} else if (n.highlighted) {
						Gizmos.color = Color.blue;
					} else if (n.walkable) {
						float hue = n.gCost;
						Gizmos.color = hue == 0 ? new Color(255f, 255f, 255f, .3f) : new Color(0f, 0f, hue, .6f);
					} else {
						Gizmos.color = Color.black;
					}
					Gizmos.DrawCube(n.worldPosition, new Vector3(size, .2f, size));
				}
			} else if(hoveredNode != null) {
				if (hoveredNode.isOccupied) {
					Gizmos.color = Color.red;
				} else {
					Gizmos.color = Color.blue;
				}
				Gizmos.DrawCube(hoveredNode.worldPosition, new Vector3(size, .2f, size));				
				foreach (Node neighbor in getNeighbors(hoveredNode, allowDiagonals)) {
					if (neighbor.isOccupied) {
						Gizmos.color = Color.red;
					} else {
						//int index = highlightedNode.Neighbors.IndexOf(neighbor);
						//float color = index == -1 ? 255f : highlightedNode.Costs[index];
						Gizmos.color = Color.cyan;
					}
					Gizmos.DrawCube(neighbor.worldPosition, new Vector3(size, .2f, size));
				}
			}
		}
	}
	
	[System.Serializable]
	public class TerrainType {
		public LayerMask terrainMask;
		public int terrainPenalty;
	}
	
	public interface IPreGridValidator {
		bool isValid(int x, int y);
	}
	
	public interface IPostGridFilter {
		List<Node> filterNodes(List<Node> nodes, Node startNode, Node targetNode);
	}
	
}
