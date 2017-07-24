using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Graph : IEnumerable  {
	private List<Node> nodeSet;

	public Graph() : this(null) {}
	public Graph(List<Node> nodeSet) {
		if (nodeSet == null)
			this.nodeSet = new List<Node>();
		else
			this.nodeSet = nodeSet;
	}
	
	public void AddNode(Node node) {
		nodeSet.Add(node);
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
		return nodeSet.Contains(node);
	}
	
	public bool Remove(Node node) {
		if (!nodeSet.Remove(node)) {
			return false;
		}
		
		foreach (Node gnode in nodeSet) {
			int index = gnode.Neighbors.IndexOf(gnode);
			if (index != -1) {
				gnode.Neighbors.RemoveAt(index);
				gnode.Costs.RemoveAt(index);
			}
		}
		
		return true;
	}
	
	public IEnumerator GetEnumerator() {
		return nodeSet.GetEnumerator();
	}
	
	
}
