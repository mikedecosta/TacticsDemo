using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Graph : IEnumerable  {
	private Dictionary<Vector3, Node> nodeSet;

	public Graph() : this(null) {}
	public Graph(Dictionary<Vector3, Node> nodeSet) {
		if (nodeSet == null)
			this.nodeSet = new Dictionary<Vector3, Node>();
		else
			this.nodeSet = nodeSet;
	}
	
	public Node getNodeFromKey(Vector3 key) {
		Node node = new Node(false, Vector3.zero, Vector3.zero, 0);
		if (nodeSet.ContainsKey(key)) {
			nodeSet.TryGetValue(key, out node);
		}
		
		return node;
	}
	
	public void AddNode(Vector3 key, Node node) {
		nodeSet.Add(key, node);
	}
	
	public void AddDirectedEdge(Node from, Node to, int cost) {
		from.Neighbors.Add(to);
		from.Costs.Add(cost);
	}
	
	public void AddUndirectedEdge(Node from, Node to, int cost) {
		from.Neighbors.Add(to);
		from.Costs.Add(cost);
		
		to.Neighbors.Add(from);
		to.Costs.Add(cost);
	}
	
	public bool Contains(Node node) {
		return nodeSet.ContainsValue(node);
	}
	
	public bool Remove(Node node) {
		if (!nodeSet.Remove(node.key)) {
			return false;
		}
		
		foreach (Node gnode in nodeSet.Values) {
			int index = gnode.Neighbors.IndexOf(gnode);
			if (index != -1) {
				gnode.Neighbors.RemoveAt(index);
				gnode.Costs.RemoveAt(index);
			}
		}
		
		return true;
	}
	
	public void UpdateNode(Node node) {
		if (!nodeSet.Remove(node.key)) {
			return;
		}
		
		AddNode(node.key, node);
		foreach (Node gnode in nodeSet.Values) {
			int index = gnode.Neighbors.IndexOf(gnode);
			if (index != -1) {
				gnode.Neighbors[index] = node;
			}
		}
	}
	
	public IEnumerator GetEnumerator() {
		return nodeSet.Values.GetEnumerator();
	}
	
	
}
