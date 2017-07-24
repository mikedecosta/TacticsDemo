using UnityEngine;
using System.Collections;

public class TurnManager : MonoBehaviour {
	private Pawn[] pawns;
	private StateManager stateManager;
	private int currentTactics;
	private Pawn currentTacticsPawn;

	// Use this for initialization
	void Start () {
		stateManager = FindObjectOfType<StateManager>();
		pawns = FindObjectsOfType<Pawn>();
		currentTactics = 0;
	}
	
	// Update is called once per frame
	void Update () {
		switch (stateManager.getCurrentState()) {
			case StateManager.states.loading:
				loadingUpdate();
				break;
			case StateManager.states.tactics:
				tacticsUpdate();
				break;
			case StateManager.states.action:
				actionUpdate();
				break;
			case StateManager.states.end:
				endUpdate();
				break;
			default:
				break;
		}
	}
	
	void loadingUpdate() {
	}
	
	void tacticsUpdate() {
		if (pawns.Length - 1 <= currentTactics) {
			currentTactics = 0;
			//stateManager.setReadyForAction();
		} else {
			currentTacticsPawn = pawns[currentTactics] as Pawn;
		}
		
		if (currentTacticsPawn != null) {
			if (!currentTacticsPawn.getTurnSet()) {
				currentTacticsPawn.setHighlight(true);
			} else {
				currentTacticsPawn.setHighlight(false);
				currentTactics++;
			}
		}
	}
	
	void actionUpdate() {
	
	}
	
	void endUpdate() {
	
	}
}
