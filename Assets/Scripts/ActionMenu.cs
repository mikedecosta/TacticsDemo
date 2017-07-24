using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ActionMenu : MonoBehaviour {
	public Button actionButtonPrefab;
	GameObject hiddenActions;
	StateManager stateManager;
	Dictionary<Unit, List<Button>> buttons = new Dictionary<Unit, List<Button>>();
	Button parentButton;
	float buttonOffset = 35f;

	// Use this for initialization
	void Awake () {
		stateManager = FindObjectOfType<StateManager>();
		stateManager.activeUnitChangeObservers += HideButtons;
		hiddenActions = GameObject.FindGameObjectWithTag("HiddenActions");
	}
	
	public void SetButtons(Unit owningUnit, List<IButton> ibuttons) {
		Button working;
		RectTransform rectTransform;
		List<Button> toAdd = new List<Button>();
		for(int i = 0; i < ibuttons.Count; i++) {
			working = createButton(ibuttons[i]);
			rectTransform = working.GetComponent<RectTransform>();
			float xPos = rectTransform.anchoredPosition.x;
			float yPos = rectTransform.anchoredPosition.y;
			rectTransform.anchoredPosition = new Vector3(xPos, yPos - (buttonOffset * i), -1f);
			toAdd.Add(working);
		}
		
		buttons.Add(owningUnit, toAdd);
	}
	
	void HideButtons(Unit activeUnit) {
		CanvasGroup cg;
		foreach (KeyValuePair<Unit, List<Button>> kvp in buttons) {
			if (!kvp.Key.Equals(activeUnit)) {
				foreach (Button button in kvp.Value) {
					cg = button.GetComponent<CanvasGroup>();
					cg.interactable = false;
					cg.alpha = 0;
					button.transform.SetParent(hiddenActions.transform);
				}
			}
		}
	}
	
	public void ShowButtons(Unit activeUnit) {
		CanvasGroup cg;
		foreach (KeyValuePair<Unit, List<Button>> kvp in buttons) {
			if (kvp.Key.Equals(activeUnit)) {
				foreach (Button button in kvp.Value) {
					cg = button.GetComponent<CanvasGroup>();
					cg.interactable = true;
					cg.alpha = 1;
					button.transform.SetParent(transform);
				}
			}
		}
	}
	
	void hideAllButtons() {
		CanvasGroup cg;
		foreach (KeyValuePair<Unit, List<Button>> kvp in buttons) {
			foreach (Button button in kvp.Value) {
				cg = button.GetComponent<CanvasGroup>();
				cg.interactable = true;
				cg.alpha = 1;
				button.transform.SetParent(transform);
			}
		}
	}
	
	void showChildButtons(IButton parent) {
/* it's an IButton, not a Button	
		CanvasGroup cg;
		foreach (Button button in parent.children) {
			cg = button.GetComponent<CanvasGroup>();
			cg.interactable = true;
			cg.alpha = 1;
			button.transform.SetParent(transform);
		}
*/
	}
	
	public void increaseUiLevel(IButton parent) {
		Debug.Log(parent.text);
		//hideAllButtons();
		//showChildButtons(parent);
	}
	
	public void decreaseUiLevel(IButton parent) {
		Debug.Log(parent.text);
		//hideAllButtons();
		//showChildButtons(parent);
	}
	
	Button createButton(IButton info) {
		Button clone = Instantiate(actionButtonPrefab);
		clone.name = info.text;
		Text text = clone.GetComponentInChildren<Text>();
		text.text = info.text;
		clone.onClick.AddListener(info.action);
		Debug.Log(clone);
		Debug.Log(clone.transform);
		Debug.Log(hiddenActions);
		Debug.Log(hiddenActions.transform);
		clone.transform.SetParent(hiddenActions.transform);
		
		return clone;
	}

}

public class IButton {
	public string text;
	public UnityAction action;
	public IButton parent;
	public List<IButton> children;
	
	public IButton(string _text, UnityAction _action) {
		text = _text;
		action = _action;
		children = new List<IButton>();
	}
	
	public IButton(string _text) {
		text = _text;
		children = new List<IButton>();
	}
	
	public void setAction(UnityAction _action) {
		action = _action;
	}
	
	public void setChildren(List<IButton> _children) {
		foreach(IButton child in _children) {
			child.parent = this;
		}
		
		children = _children;
	}
}

