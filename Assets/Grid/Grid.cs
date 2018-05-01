using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public int maxHeight;
	public float nodeRadius;
	public TerrainType[] walkableRegions;
	public bool allowDiagonals = false;
	public float minStandableHeight;
	
	LayerMask walkableMask;
	Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
	Graph graph;
	StateManager stateManager;
	float nodeDiameter;
	int gridSizeX;
	int gridSizeY;
	int gridLevels;
	CameraRaycaster cameraRaycaster;
	Node hoveredNode;
	HashSet<Node> highlightedNodes = new HashSet<Node>();
	bool drawGridSquares = true;
	
	void Awake() {
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
		gridLevels = Mathf.RoundToInt(maxHeight/nodeDiameter/minStandableHeight);
		
		CreateWalkableRegionsDictionary();
		CreateGrid();
	}
	
	private void CreateWalkableRegionsDictionary() {
		foreach (TerrainType region in walkableRegions) {
			walkableMask.value |= region.terrainMask.value;
			walkableRegionsDictionary.Add((int) Mathf.Log(region.terrainMask.value,2), region.terrainPenalty);
		}
	}
	
	void Start() {
		cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();
		cameraRaycaster.nodeHoverChangeObservers += ChangeHighlight;
		stateManager = FindObjectOfType<StateManager>();
		stateManager.stateChangeObservers += HandleStateChange;
	}
	
	public int MaxSize {
		get {
			return gridSizeX * gridSizeY * gridLevels;
		}
	}
	
	private void CreateGrid() {
		setupNodes();
		addNeighbors();
	}
	
	private void setupNodes() {
		graph = new Graph();
		
		for (int x = 0; x < gridSizeX; x++) {
			for (int y = 0; y < gridSizeY; y++) {
				// (xGrid, 0, yGrid)
				Vector3 worldPoint = getWorldPoint(x, y);
				bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius-.1f, unwalkableMask));
				Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
				RaycastHit[] hits = Physics.SphereCastAll(ray, nodeRadius-.1f, 50f, walkableMask);
				Dictionary<Vector3, Node> nodes = createNodesFromHits(hits, worldPoint, walkable, x, y);
								
				foreach(KeyValuePair<Vector3, Node> kvp in nodes) {
					kvp.Value.nodeChangedObservers += OnNodeChange;
					graph.AddNode(kvp.Key, kvp.Value);
				}
			}
		}
	}
	
	private Dictionary<Vector3, Node> createNodesFromHits(RaycastHit[] hits, Vector3 worldPoint, bool walkable, int x, int y) {
		float height = 0;
		int level = 0;
		Node node;
		Dictionary<Vector3, Node> nodes = new Dictionary<Vector3, Node>();
		
		ArrayList hitsList = new ArrayList(hits);
		hitsList.Sort(new ByHeightComparer());
		foreach (RaycastHit hit in hitsList) {
			height = (hit.point.y);
			worldPoint.y = height;
			Vector3 key = new Vector3(x, y, level);
			
			if (level == 0) {
				// (xGrid, height, yGrid)		
				node = new Node(walkable, worldPoint, key, 0);
				nodes[key] = node;
				level++;
			} else {
				// (xGrid, height, yGrid)
				Vector3 previousKey = new Vector3(x, y, level - 1);
				Node previous;
				nodes.TryGetValue(previousKey, out previous);
				
				if (previous == null) {
					continue;
				}				
				
				if ( height - previous.worldPosition.y < minStandableHeight ) {
					node = new Node(walkable, worldPoint, previousKey, 0);
					nodes[previousKey] = node;
				} else {
					node = new Node(walkable, worldPoint, key, 0);
					nodes[key] = node;
					level++;
				}
			}
		}
		
		return nodes;
	}
	
	private Vector3 getWorldBottomLeft() {
		float xDist = gridWorldSize.x / 2;
		float yDist = gridWorldSize.y / 2;
		return transform.position - (Vector3.right * xDist) - (Vector3.forward * yDist);
	}
	
	private Vector3 getWorldPoint (int x, int y) {
		// x full squares + 1 half square;
		float xDist = (x * nodeDiameter) + nodeRadius;
		float yDist = (y * nodeDiameter) + nodeRadius;
		return getWorldBottomLeft() + (Vector3.right * xDist) + (Vector3.forward * yDist);
	}
	
	private void addNeighbors() {
		foreach(Node workingNode in graph) {
			setNorthernNeighbor(workingNode);
			setSouthernNeighbor(workingNode);
			setEasternNeighbor(workingNode);
			setWesternNeighbor(workingNode);
		}
	}
	
	/*    a       a       a       a       a,b     a,b     a       a
	                _     _       _ _   _       _   _   _ _     _ _ _
		_ _ _ X _ _ | X _ | _ X _ | | X | _ _ X | _ | X | | _ X | | |
	
	    0,1     0     0              a,b1     a
	      _   _       _ _   _ _     _   _   _ _   
		_ _ X _ _ X _ | _ X _ | _ X | _ _ X | _ _
	*/
	
	private void setDirectionalNeighbor(Node node, bool modX, bool modY, bool positive) {
		int neighborX;
		int neighborY;
		int increment;
		Node currentWorkingNeighbor;
		float incline;
		
		for (int i = 1; i <= 2; i++) {
			incline = -99f;
			for (int l = 0; l < gridLevels; l++) {
				increment = positive ? i : -i;
				neighborX = modX ? (int) node.key.x + increment : (int) node.key.x;
				neighborY = modY ? (int) node.key.y + increment : (int) node.key.y;
				
				if (!isOnGrid(neighborX, neighborY)) {
					break;
				}
				
				currentWorkingNeighbor = graph.GetNodeFromKey(new Vector3(neighborX, neighborY, l));
				if (currentWorkingNeighbor == null || currentWorkingNeighbor == graph.emptyNode) {
					break;
				}
				incline = currentWorkingNeighbor.worldPosition.y - node.worldPosition.y;
				
				if (roofOverHead(node) && incline >= 0.5f) {
					continue;
				}
				
				if (ledgeInTheWay(node, currentWorkingNeighbor) && incline <= -0.5f) {
					continue;
				}

				graph.AddDirectedEdge(node, currentWorkingNeighbor, getWeight(node, currentWorkingNeighbor));
			}
			
			if (incline >= -0.5f) {
				return;
			}
		}
	}
	
	private bool roofOverHead(Node node) {
		Node aboveHead = graph.GetNodeFromKey(new Vector3(node.key.x, node.key.y, node.key.z + 1));
		if (aboveHead == null || aboveHead == graph.emptyNode) {
			return false;
		}
		
		return aboveHead.worldPosition.y - node.worldPosition.y < 2 * minStandableHeight;
	}
	
	private bool ledgeInTheWay(Node current, Node target) {
		Node ledge = graph.GetNodeFromKey(new Vector3(target.key.x, target.key.y, target.key.z + 1));
		if (ledge == null || ledge == graph.emptyNode) {
			return false;
		}
		
		return ledge.worldPosition.y - current.worldPosition.y < minStandableHeight;
	}
	
	private void setNorthernNeighbor(Node node) {
		setDirectionalNeighbor(node, false, true, true);
	}

	private void setSouthernNeighbor(Node node) {
		setDirectionalNeighbor(node, false, true, false);
	}
	
	private void setEasternNeighbor(Node node) {
		setDirectionalNeighbor(node, true, false, true);
	}
	
	private void setWesternNeighbor(Node node) {
		setDirectionalNeighbor(node, true, false, false);
	}
	
	private float getWeight(Node nodeA, Node nodeB) {
		return (float) Math.Round( (nodeA.worldPosition - nodeB.worldPosition).magnitude, 2);
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
		|-1,1|0,1 |1,1 |
		+----+----+----+
	y	|-1,0|0,0 |1,0 |
		+----+----+----+
		|-1-1|0-1 |1-1 |
		+----+----+----+
	
		           x
		+----+----+----+----+----+
		|-2,2|-1,2|0,2 |1,2 |2,2 |
		+----+----+----+----+----+
	y	|-2,1|-1,1|0,1 |1,1 |2,1 |
		+----+----+----+----+----+
		|-2,0|-1,0|0,0 |1,0 |1,0 |
		+----+----+----+----+----+
		|-2-1|-1-1|0,-1|1,-1|2,-1|
		+----+----+----+----+----+
		|-2-2|-1-2|0,-2|1,-2|2,-2|
		+----+----+----+----+----+
	
	*/
	
	// TODO: fix getNeighbors when allowDiagonals is true and corners are in the way
	// TODO: Allow adding other filters more easily, height filters for instance
	private List<Node> getNeighbors(Node node, bool allowDiagonals) {		
		return node.Neighbors;
	}
	
	private List<Node> setHighestPriorityNeighbor(List<Node> list, Node neighbor) {
		if (list.Count == 0) {
			list.Add(neighbor);
			return list;
		}
		
		return list;
	}
		
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
		if (!isOnGrid(x, y)) {
			throw new ArgumentException("NodeFromWorldPoint " + x + "," + y + " is not on grid.");
		}
		Node node;
		for (int h = 0; h < gridLevels; h++) {
			node = graph.GetNodeFromKey(new Vector3(x, y, h));
			double distance = Math.Abs(node.worldPosition.y - worldPosition.y);
			if (distance < minStandableHeight) {
				return node;
			}
		}
		
		throw new ArgumentException("NodeFromWorldPoint " + worldPosition + " does not exist.");
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
		//graph.UpdateNode(newNode);
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
	
	public class ByHeightComparer : IComparer, IComparer<RaycastHit> {
		public int Compare(RaycastHit a, RaycastHit b) {
			return a.point.y.CompareTo(b.point.y);
		}
		
		int IComparer.Compare(System.Object a, System.Object b) {
			return Compare ( (RaycastHit)a, (RaycastHit)b);
		}
	}
	
	public interface IPreGridValidator {
		bool isValid(int x, int y);
	}
	
	public interface IPostGridFilter {
		List<Node> filterNodes(List<Node> nodes, Node startNode, Node targetNode);
	}
	
}
