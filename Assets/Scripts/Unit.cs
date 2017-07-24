using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.ThirdPerson;

public class Unit : MonoBehaviour, IComparable<Unit> {

	public Transform target;
	
	[SerializeField] String CharacterName;
	[SerializeField] int initiativeMin = 1;
	[SerializeField] int initiativeMax = 100;
	[SerializeField] float maxHealth = 1000f;
	[SerializeField] float moveSpeed = 60f;
	[SerializeField] float basicAttackDamage = 250f;
	[SerializeField] float basicAttackDistance = 13f;
	[SerializeField] public float moveDistancePerRound = 60f;
	public Sprite redTeamOutline;
	public Sprite blueTeamOutline;
	
	public int moveSquaresPerRound = 6;
	float currentHealth = 1000f;
	int currentInitiative;
	Vector3 targetPosition;
	Vector3[] path;
	int targetIndex;
	Node targetNode;
	Node currentNode;
	Grid grid;
	StateManager stateManager;
	CameraRaycaster cameraRaycaster;
	float walkStopRadius = 0.3f;
	bool hasMoved;
	bool hasActed;
	
	ThirdPersonCharacter tpcharacter;
	
	/* UI Action Panel */
	GameObject actionUI;
	GameObject statsText;
	GameObject tokenOutline;
	ActionMenu actionMenu;
	
	public delegate void OnTacticsChosen();
	public event OnTacticsChosen tacticsChosenObservers;
	private void TriggerTacticsChosenObservers() {
		if (tacticsChosenObservers != null) {
			tacticsChosenObservers();
		}
	}
	public delegate void OnActionPerformed();
	public event OnActionPerformed actionPerformedObservers;
	private void TriggerActionPerformedObservers() {
		if (actionPerformedObservers != null) {
			actionPerformedObservers();
		}
	}
	public delegate void OnTurnEnd();
	public event OnTurnEnd turnEndObservers;
	private void TriggerTurnEndObservers() {
		if ( turnEndObservers != null) {
			turnEndObservers();
		}
	}
	
	void Start() {
		stateManager = FindObjectOfType<StateManager>();
		stateManager.stateChangeObservers += HandleStateChange;
		stateManager.activeUnitChangeObservers += HandleChooseTactics;
		cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();
		cameraRaycaster.nodeHoverChangeObservers += OnNodeHighlightChange;
		grid = FindObjectOfType<Grid>();
		currentNode = grid.NodeFromWorldPoint(transform.position);
		currentNode.setOccupidUnit(this);
		
		tpcharacter = GetComponent<ThirdPersonCharacter>();
		
		setUIElements();
	}
	
	void setUIElements() {
		actionUI = GameObject.FindGameObjectWithTag("ActionUI");
		statsText = GameObject.FindGameObjectWithTag("StatsText");
		tokenOutline = GameObject.FindGameObjectWithTag("TokenOutline");
		
		actionMenu = GameObject.FindObjectOfType<ActionMenu>();
		List<IButton> buttons = new List<IButton>();
		// Move
		IButton move = new IButton("MOVE", HandleChooseMoveSelection);
		//IButton cancel = new IButton("CANCEL", () => actionMenu.decreaseUiLevel(move));
		//List<IButton> moveChildren = new List<IButton>();
		//moveChildren.Add(cancel);
		//move.setChildren(moveChildren);
		buttons.Add(move);
		// Act
		//IButton act = new IButton("ACT");
		//act.setAction(() => actionMenu.increaseUiLevel(act));
		//cancel = new IButton("CANCEL", () => actionMenu.decreaseUiLevel(act));
		IButton attack = new IButton("ATTACK", HandleChooseAttackSelection);
		//List<IButton> actChildren = new List<IButton>();
		//actChildren.Add(attack);
		//actChildren.Add(cancel);
		//act.setChildren(actChildren);
		buttons.Add(attack);
		// Wait
		buttons.Add(new IButton("WAIT", HandleChooseWait));
		actionMenu.SetButtons(this, buttons);
	}
	
	void Update() {
		CheckDeath();
	}
	
	void CheckDeath() {
		if (currentHealth <= 0) {
			transform.position = transform.position + Vector3.down;
		}
	}
	
	void HandleStateChange() {
		switch(stateManager.getCurrentState()) {
			case StateManager.states.tactics:
				//Debug.Log(gameObject.name + ": state set to tactics");
				//HandleChooseTactics();
				break;
			case StateManager.states.action:
				HandlePerformAction();
				break;
		}
	}
	
	void HandleChooseTactics(Unit activeUnit) {
		Debug.Log(gameObject.name + ": HandleChooseTactics");
		if (!activeUnit.gameObject.Equals(gameObject)) {
			return;
		}
		
		actionUI.GetComponent<CanvasGroup>().alpha = 1;
		setUnitText();
		Image tOutline = tokenOutline.GetComponent<Image>();
		Color color = Color.green;
		if (activeUnit.transform.parent.tag == "RedTeam") {
			color = Color.red;
			tOutline.sprite = redTeamOutline;
		} else if (activeUnit.transform.parent.tag == "BlueTeam") {
			color = Color.blue;
			tOutline.sprite = blueTeamOutline;
		}
		
		actionMenu.ShowButtons(this);
	}
	
	void UnloadActionUI() {
		actionUI.GetComponent<CanvasGroup>().alpha = 0;
		unsetUnitText();
	}
	
	public void HandleChooseMoveSelection() {
		if (stateManager.getActiveUnit().gameObject.Equals(gameObject)) {
			StopCoroutine("ChooseMoveLocation");
			StartCoroutine("ChooseMoveLocation");
		}
	}
	
	IEnumerator ChooseMoveLocation() {
		Vector3 targetLocation = Vector3.zero;
		grid.HighlightSquaresInRange(currentNode, moveDistancePerRound);
		while(true) {
			if (Input.GetMouseButton(0) && targetNode.walkable && !targetNode.isOccupied && targetNode != currentNode) {
				targetLocation = targetNode.worldPosition;
				break;
			}
			yield return null;
		}
		
		PathRequestManager.RequestPath(transform.position, targetLocation, moveDistancePerRound, OnPathFound);
	}
	
	public void HandleChooseAttackSelection() {
		if (stateManager.getActiveUnit().gameObject.Equals(gameObject)) {
			StopCoroutine("ChooseAttackLocation");
			StartCoroutine("ChooseAttackLocation");
		}
	}
	
	IEnumerator ChooseAttackLocation() {
		grid.HighlightSquaresInRange(currentNode, basicAttackDistance);
		while(true) {
			if (Input.GetMouseButton(0) && targetNode.walkable && GetDistance(currentNode, targetNode) <= basicAttackDistance) {
				break;
			}
			yield return null;
		}
		
		BasicAttack(targetNode);
	}
	
	int GetDistance(Node nodeA, Node nodeB) {
		int distX = Mathf.Abs (nodeA.gridX - nodeB.gridX);
		int distY = Mathf.Abs (nodeA.gridY - nodeB.gridY);
		int distHeight = Mathf.RoundToInt(Mathf.Abs (nodeA.worldPosition.y - nodeB.worldPosition.y));
		
		return 10*distX + 10*distY + distHeight;
	}
	
	public void BasicAttack(Node attackNode) {
		if (attackNode.isOccupied) {
			attackNode.getOccupiedUnit().currentHealth -= basicAttackDamage;
		}
		
		hasActed = true;
		if (hasMoved) {
			EndTheTurn();
		} else {
			TriggerActionPerformedObservers();
		}
	}
	
	public void HandleChooseWait() {
		if (stateManager.getActiveUnit().gameObject.Equals(gameObject)) {
			Debug.Log(gameObject.name + ": HandleWaitSelection");
			EndTheTurn();
		}
	}
	
	private void EndTheTurn() {
		hasMoved = false;
		hasActed = false;
		UnloadActionUI();
		TriggerTurnEndObservers();
	}
	
	void HandlePerformAction() {
		StopCoroutine("FollowPath");
		StartCoroutine("FollowPath");
	}
	
	
	public void OnPathFound(Vector3[] newPath, bool pathFound) {
		Debug.Log(gameObject.name + ": Inside OnPathFound");
		if (pathFound) {
			path = newPath;
			Debug.Log(gameObject.name + ": Last waypoint in path = " + path[path.Length - 1].ToString());
			TriggerTacticsChosenObservers();
		} else {
			path = null;
			HandleChooseMoveSelection();
		}
	}
	
	public void OnNodeHighlightChange(Node newNode) {
		targetNode = newNode;
	}
	
	IEnumerator FollowPath() {
		if (path == null) {
			yield break;
		}
		
		Vector3 currentWaypoint = path[0];
		Vector3 previousWaypoint = transform.position;
		//Vector3 movePoint;
		
		while (true) {
			//movePoint = currentWaypoint - transform.position;
			//if (movePoint.magnitude <= walkStopRadius) {
			if (transform.position == currentWaypoint) {
				//transform.position = currentWaypoint;
				Node previousNode = grid.NodeFromWorldPoint(previousWaypoint);				
				previousNode.unsetOccupiedUnit();
				//previousNode.resetCosts();
				previousWaypoint = path[targetIndex];
				
				currentNode = grid.NodeFromWorldPoint(currentWaypoint);
				currentNode.setOccupidUnit(this);
				targetIndex++;
				if (targetIndex >= path.Length) {
					targetIndex = 0;
					path = null;
					TriggerActionPerformedObservers();
					hasMoved = true;
					if (hasActed) {
						EndTheTurn();
					}
					yield break;
				}
				
				currentWaypoint = path[targetIndex];
			}
			
			transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, moveSpeed * Time.deltaTime);
			//tpcharacter.Move(movePoint, false, false);
			yield return null;
		}
	}
	
	public void OnDrawGizmos() {
		if (path != null) {
			for (int i = targetIndex; i < path.Length; i++) {
				Gizmos.color = Color.blue;
				Gizmos.DrawCube(path[i], Vector3.one * (.8f));
				
				if (i == targetIndex) {
					Gizmos.DrawLine(transform.position, path[i]);
				} else {
					Gizmos.DrawLine(path[i-1], path[i]);
				}
			}
		}

		if (stateManager != null && stateManager.getActiveUnit() != null && stateManager.getActiveUnit().gameObject.Equals(gameObject)) {
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(new Vector3(transform.position.x, transform.position.y + .8f, transform.position.z), new Vector3(1.1f, 2f, 1.1f));
		}
	}
	
	public int GetInitiative() {
		if (currentInitiative == null) {
			GenerateInitiative();	
		}
		
		return currentInitiative;
	}
	
	void GenerateInitiative() {
		currentInitiative = (int) UnityEngine.Random.Range(initiativeMin, initiativeMax);
	}
	
	public int CompareTo(Unit other) {
		return GetInitiative().CompareTo(other.GetInitiative());
	}
	
	void unsetUnitText() {
		statsText.GetComponent<Text>().text = "";
	}
	
	void setUnitText() {
		Text text = statsText.GetComponent<Text>();
		String stats = "Name: " + CharacterName + "\n";
		stats += "Health: " + currentHealth + " / " + maxHealth + "\n";
		stats += "Initiative: (" +  initiativeMin + ","+initiativeMax+")\n";
		text.text = stats;
	}
}
