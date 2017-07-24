using UnityEngine;
using System;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class Pawn : MonoBehaviour, IComparable {
	public float reactionModifier;
	public float landSpeed;
	
	private BoardManager boardManager;
	private StateManager stateManager;
	private float currentInitiative;
	private bool turnSet;
	private bool highlight;
	private Vector3 moveTo;
	
	public float getInitiative() {
		if (this.currentInitiative == null) {
			this.setInititative();
		}
		
		return this.currentInitiative;
	}
	
	public float setInititative() {
		currentInitiative = UnityEngine.Random.Range(0f, reactionModifier);
		turnSet = false;
		return getInitiative();
	}
	
	public Vector3 getMoveTo() {
		return moveTo;
	}
	
	public Vector3 setMoveTo(Vector3 value) {
		moveTo = value;
		return getMoveTo();
	}
	
	public bool getTurnSet() {
		return turnSet;
	}
	
	public bool getHighlight() {
		return highlight;
	}
	
	public bool setHighlight(bool hl) {
		highlight = hl;
		return getHighlight();
	}

	// Use this for initialization
	void Start () {
		boardManager = FindObjectOfType<BoardManager>();
		stateManager = FindObjectOfType<StateManager>();
	}
	
	// Update is called once per frame
	void Update () {
		if (stateManager.getCurrentState().Equals(StateManager.states.tactics)) {
			if (highlight) {
				InvokeRepeating("doHighlight", 0f, 0.3f);
				if (CrossPlatformInputManager.GetButtonUp("Fire1")) {
					setMoveTo(boardManager.mouseRayToCoordinate());
					turnSet = true;
				}
			} else {
				cancelHighlight();
			}
		} else if (stateManager.getCurrentState().Equals(StateManager.states.action)) {
			cancelHighlight();
			if ( moveTo != null) {
				float step = landSpeed * Time.deltaTime;
				transform.position = Vector3.MoveTowards(transform.position, moveTo, step);
			}
		}
	}
	
	void doHighlight() {
		Renderer renderer = GetComponent<Renderer>();
		if (Time.fixedTime % .5 < .2) {
			renderer.material.shader = Shader.Find("Self-Illumin/Outlined Diffuse");
		} else {
			renderer.material.shader = Shader.Find("Diffuse");
		}
	}
	
	void cancelHighlight() {
		CancelInvoke(); 
		Renderer renderer = GetComponent<Renderer>();
		renderer.material.shader = Shader.Find("Standard");
	}
	
	public int CompareTo(object other) {
		if (other == null) {
			return 1;
		}
		
		Pawn otherPawn = other as Pawn;
		if (otherPawn == null) {
			return 1;
		}
		
		return this.getInitiative().CompareTo(otherPawn.getInitiative());
	}
	
}
