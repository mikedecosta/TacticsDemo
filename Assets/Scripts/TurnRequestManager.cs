using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TurnRequestManager : MonoBehaviour {
	
	Queue<TurnRequest> turnRequestQueue = new Queue<TurnRequest>();
	TurnRequest currentTurnRequest;
	
	static TurnRequestManager instance;
	TurnHandler turnHandler;
	bool isProcessingTurn;
	
	void Awake() {
		instance = this;
		turnHandler = GetComponent<TurnHandler>();
	}
	
	public static void RequestTurn(Unit unit, Action<Vector3, bool> callback) {
		Debug.Log(unit.name + " requested turn");
		TurnRequest newRequest = new TurnRequest(unit, callback);
		instance.turnRequestQueue.Enqueue(newRequest);
		instance.TryProcessNext();
	}
	
	void TryProcessNext() {
		if (!isProcessingTurn && turnRequestQueue.Count > 0) {
			currentTurnRequest = turnRequestQueue.Dequeue();
			isProcessingTurn = true;
			turnHandler.StartHandleTurn(currentTurnRequest.currentUnit);
		}
	}
	
	public void FinishedProcessingTurn(Vector3 moveTarget, bool success) {
		currentTurnRequest.callback(moveTarget, success);
		isProcessingTurn = false;
		TryProcessNext();
	}
	
	struct TurnRequest {
		public Unit currentUnit;
		public Action<Vector3, bool> callback;
		
		public TurnRequest(Unit _currentUnit, Action<Vector3, bool> _callback) {
			currentUnit = _currentUnit;
			callback = _callback;
		}
	}
}
