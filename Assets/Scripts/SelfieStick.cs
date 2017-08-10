using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class SelfieStick : MonoBehaviour {
	private const int ROTATE_DEGREES = 45;
	private const int TILT_UP_MAX = 35;
	private const int TILT_DOWN_MAX = 65;
	private const int TILT_DEGREES = 15;
	private const float ZOOM_IN_MAX = 1f;
	private const float ZOOM_OUT_MAX = 6f;
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
	
	void Update() {
		float update = CrossPlatformInputManager.GetAxis("Mouse ScrollWheel");
		if (update != 0f) {
			zoom(-update);
		}
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
		targetPosition = ClampTargetByMap(targetPosition);
		Vector3 distance = Vector3.zero;
		
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
	
	public bool canZoomIn(float amount) {
		return transform.localScale.y - amount >= ZOOM_IN_MAX;
	}
	
	public bool canZoomOut(float amount) {
		return transform.localScale.y + amount <= ZOOM_OUT_MAX;
	}
	
	public void zoom(float amount) {
		if ( (amount > 0 && canZoomOut(amount)) || amount < 0 && canZoomIn(amount) ) {
			transform.localScale += new Vector3(0, amount, 0);	
		}
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
