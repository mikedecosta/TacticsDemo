using UnityEngine;
using System.Collections;

public class SelfieStick : MonoBehaviour {
	private const int ROTATE_DEGREES = 45;
	private const int TILT_UP_MAX = 35;
	private const int TILT_DOWN_MAX = 65;
	private const int TILT_DEGREES = 15;
	private const int ZOOM_IN_MAX = 1;
	private const int ZOOM_OUT_MAX = 3;
	private const float ZOOM_AMOUNT = 0.5f;
	private const float moveTime = 1f;

	private Vector3 stickRotation;
	private StateManager stateManager;
	private Grid grid;
	private Unit currentFocusUnit;
	
	void Start() {
		stickRotation = transform.rotation.eulerAngles;
		stateManager = FindObjectOfType<StateManager>();
		stateManager.activeUnitChangeObservers += focusUnit;
		stateManager.stateChangeObservers += HandleStateChange;
		grid = FindObjectOfType<Grid>();
	}

	void HandleStateChange() {
		if (currentFocusUnit != null) {
			focusUnit(currentFocusUnit);
		}
	}
	
	void focusUnit(Unit unit) {
		StopCoroutine("moveToPosition");
		currentFocusUnit = unit;
		StartCoroutine("moveToPosition", unit.transform.position);
	}
	
	IEnumerator moveToPosition(Vector3 targetPosition) {
		Debug.Log("targetPosition: " + targetPosition);
		targetPosition = ClampTargetByMap(targetPosition);
		Debug.Log("new targetPosition: " + targetPosition);
		Vector3 distance = Vector3.zero;
		float startTime = Time.time;
		
		while (true) {
			distance = transform.position - targetPosition;
			if (distance.magnitude <= .1f) {
				transform.position = targetPosition;
				yield break;
			}
			
			transform.position = Vector3.Lerp(transform.position, targetPosition, .05f);
			yield return null;
		}
	}
	
	private Vector3 ClampTargetByMap(Vector3 targetPosition) {
		int xBounds = (int) (grid.gridWorldSize.x / 2) - 10;
		int yBounds = (int) (grid.gridWorldSize.y / 2) - 10;
		return new Vector3(Mathf.Clamp(targetPosition.x, -xBounds, xBounds), targetPosition.y, Mathf.Clamp(targetPosition.z, -yBounds, yBounds));
	}
	
	public void rotateLeft() {
		rotate(ROTATE_DEGREES);
	}
	
	public void rotateRight() {
		rotate(-ROTATE_DEGREES);
	}
	
	public void tilitUp() {
		tilt (-TILT_DEGREES);
	}
	
	public bool canTiltUp() {
		return stickRotation.x - TILT_DEGREES >= TILT_UP_MAX;
	}
	
	public void tilitDown() {
		tilt(TILT_DEGREES);
	}
	
	public bool canTiltDown() {
		return stickRotation.x + TILT_DEGREES <= TILT_DOWN_MAX;
	}
	
	public void zoomIn() {
		if ( transform.localScale.y <= ZOOM_IN_MAX) {
			return;
		}
		
		transform.localScale -= new Vector3(0, ZOOM_AMOUNT, 0);
	}
	
	public bool canZoomIn() {
		return transform.localScale.y - ZOOM_AMOUNT >= ZOOM_IN_MAX;
	}
	
	public void zoomOut() {
		if ( transform.localScale.y >= ZOOM_OUT_MAX) {
			return;
		}
		
		transform.localScale += new Vector3(0, ZOOM_AMOUNT, 0);
	}
	
	public bool canZoomOut() {
		return transform.localScale.y + ZOOM_AMOUNT <= ZOOM_OUT_MAX;
	}
	
	private void rotate(int degrees) {
		stickRotation.y = Mathf.Round(stickRotation.y + degrees);
		transform.rotation = Quaternion.Euler(stickRotation);	
	}
	
	private void tilt(int degrees) {
		stickRotation.x = Mathf.Round(Mathf.Clamp(stickRotation.x + degrees, TILT_UP_MAX, TILT_DOWN_MAX));
		transform.rotation = Quaternion.Euler(stickRotation);
	}
}
