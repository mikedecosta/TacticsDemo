using UnityEngine;
using System.Collections;

public class TurnHandler : MonoBehaviour {
	TurnRequestManager turnRequestManager;
	
	void Awake() {
		turnRequestManager = GetComponent<TurnRequestManager>();
	}
	
	public void StartHandleTurn(Unit currentUnit) {
		StartCoroutine(HandleTurn(currentUnit));	
	}
	
	IEnumerator HandleTurn(Unit currentUnit) {
		bool turnFinished = false;
		Vector3 moveTarget = Vector3.zero;
		while (!turnFinished) {
			
			if (true) {
				moveTarget = new Vector3(-8f, 0, -5f);
				turnFinished = true;
				break;
			}
			
			// TODO do stuff
		}
		
		yield return null;
		turnRequestManager.FinishedProcessingTurn(moveTarget, turnFinished);
	}

	public Vector3 MouseRayToCoordinate() {
		RaycastHit hit;
		Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 45f, LayerMask.GetMask("Walkable"));
		return new Vector3 (
			(int) hit.point.x,
			(int) hit.point.y,
			(int) hit.point.z
		);
	}
}
