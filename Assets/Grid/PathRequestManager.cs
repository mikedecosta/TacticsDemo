using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PathRequestManager : MonoBehaviour {

	Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
	Queue<HighlightRequest> highlightRequestQueue = new Queue<HighlightRequest>();
	PathRequest currentPathRequest;
	HighlightRequest currentHighlightRequest;
	
	static PathRequestManager instance;
	Pathfinder pathfinder;
	bool isProcessingJob;
	
	void Awake() {
		instance = this;
		pathfinder = GetComponent<Pathfinder>();
	}

	public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, float maxDistance, Action<Vector3[], bool> callback) {
		PathRequest newRequest = new PathRequest(pathStart, pathEnd, maxDistance, callback);
		instance.pathRequestQueue.Enqueue(newRequest);
		instance.TryProcessNext();
	}
	
	public static void RequestHighlights(Vector3 pathStart, float maxDistance, Action<HashSet<Node>> callback) {
		HighlightRequest newRequest = new HighlightRequest(pathStart, maxDistance, callback);
		instance.highlightRequestQueue.Enqueue(newRequest);
		instance.TryProcessNext();
	}
	
	void TryProcessNext() {
		if (!isProcessingJob) {
			if (pathRequestQueue.Count > 0) {
				isProcessingJob = true;
				currentPathRequest = pathRequestQueue.Dequeue();
				pathfinder.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd, currentPathRequest.maxDistance);
			} else if (highlightRequestQueue.Count > 0) {
				isProcessingJob = true;
				currentHighlightRequest = highlightRequestQueue.Dequeue();
				pathfinder.StartHighlightPaths(currentHighlightRequest.pathStart, currentHighlightRequest.maxDistance);
			}
		}
	}
	
	public void FinishedProcessingPath(Vector3[] path, bool success) {
		currentPathRequest.callback(path, success);
		isProcessingJob = false;
		TryProcessNext();
	}
	
	public void FinishedProcessingHighlight(HashSet<Node> highlight) {
		currentHighlightRequest.callback(highlight);
		isProcessingJob = false;
		TryProcessNext();
	}
	
	struct PathRequest {
		public Vector3 pathStart;
		public Vector3 pathEnd;
		public float maxDistance;
		public Action<Vector3[], bool> callback;
		
		public PathRequest(Vector3 _start, Vector3 _end, float _maxDistance, Action<Vector3[], bool> _callback) {
			pathStart = _start;
			pathEnd = _end;
			maxDistance = _maxDistance;
			callback = _callback;
		}
	}
	
	struct HighlightRequest {
		public Vector3 pathStart;
		public float maxDistance;
		public Action<HashSet<Node>> callback;
		
		public HighlightRequest(Vector3 _start, float _maxDistance, Action<HashSet<Node>> _callback) {
			pathStart = _start;
			maxDistance = _maxDistance;
			callback = _callback;
		}
	}
}
