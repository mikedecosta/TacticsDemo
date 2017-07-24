using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TiltUpDisabler : Button {
	private SelfieStick selfieStick;

	
	protected override void Start () {
		selfieStick = (SelfieStick) FindObjectOfType<SelfieStick>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!selfieStick.canTiltUp()) {
			interactable = false;
		}
		
		interactable = true;
	}
}
