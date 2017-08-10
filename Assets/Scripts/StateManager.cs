using UnityEngine;
using System.Collections;

public class StateManager : MonoBehaviour {
	
	public enum states { loading, tactics, action, end};
	public states currentState;
	
	private float loadingDuration;
	private Unit[] units;
	private Unit activeUnit;
	private int unitIndex = 0;
	private LevelManager levelManager;
	
	public states getCurrentState() {
		return currentState;
	}
	
	public Unit getActiveUnit() {
		return activeUnit;
	}

	public delegate void OnStateChange();
	public event OnStateChange stateChangeObservers;
	private void TriggerStateChangeObservers() {
		if (stateChangeObservers != null) {
			stateChangeObservers();
		}
	}
	public delegate void OnActiveUnitChange(Unit activeUnit);
	public event OnActiveUnitChange activeUnitChangeObservers;
	private void TriggerActiveUnitChangeObservers(Unit activeUnit) {
		if (activeUnitChangeObservers != null) {
			activeUnitChangeObservers(activeUnit);
		}
	}
	
	// Use this for initialization
	void Start () {
		levelManager = GetComponent<LevelManager>();
		currentState = states.loading;
		loadingDuration = .5f;
		units = FindObjectsOfType<Unit>();
		foreach (Unit unit in units) {
			unit.tacticsChosenObservers += HandleTacticsChosen;
			unit.actionPerformedObservers += HandleActionPerformed;
			unit.turnEndObservers += HandleTurnEnded;
		}
	}
	
	void HandleTacticsChosen() {
		Debug.Log("TacticsChoosen");
		currentState = states.action;
		TriggerStateChangeObservers();
	}
	
	void HandleActionPerformed() {
		Debug.Log("Action Performed");
		if (EndConditionMet()) {
			currentState = states.end;
		} else {
			currentState = states.tactics;
		}
		
		TriggerStateChangeObservers();
	}
	
	void HandleTurnEnded() {
		if (EndConditionMet()) {
			currentState = states.end;
		} else {
			currentState = states.tactics;
			setNextActiveUnit();
		}
		
		TriggerStateChangeObservers();
	}
	
	private bool EndConditionMet() {
		return false;
	}
	
	private void setNextActiveUnit() {
		Debug.Log("setNextActiveUnit: " + unitIndex);
		int startIndex = unitIndex;
		while (true) {
			unitIndex++;
			if (unitIndex == units.Length) {
				unitIndex = 0;
			}
			if (startIndex == unitIndex) {
				triggerSceneEnd();
			}
			
			activeUnit = units[unitIndex];
			if (activeUnit.isDead()) {
				continue;
			}
			
			TriggerActiveUnitChangeObservers(activeUnit);
			break;
		}
	}
	
	private void triggerSceneEnd() {
		levelManager.LoadNextLevel();
	}
	
	// Update is called once per frame
	void Update () {
		switch (currentState) {
			case states.loading:
				//Debug.Log ("Loading...");
				if (Time.time >= loadingDuration) {
					currentState = states.tactics;
					TriggerStateChangeObservers();
					setNextActiveUnit();
				}
				break;
			case states.tactics:
				//Debug.Log ("Tactics...");
				break;
			case states.action:
				//Debug.Log ("Action...");
				break;
			default:
				//Debug.Log ("default...");
				break;
		}
	}
	
}
