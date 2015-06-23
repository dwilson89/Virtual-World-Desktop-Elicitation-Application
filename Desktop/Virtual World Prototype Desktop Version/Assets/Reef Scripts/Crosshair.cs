using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*
**Author: Dustin Wilson
**Date: 18/05/2015
**Description: A simple script has been designed to have the functionality that allows the 
**crosshair to be changed, and enable/disable a pop up overlay indicating to the user their 
**available options.
 */
public class Crosshair : MonoBehaviour {

	//The Pop-up Overlay Controller
	public GameObject popUpOverlay;

	//The image for the Crosshair
	private Image crosshair;
	
	// Use this for initialization
	void Start () {

		//Get the image component attach to this object - crosshair image
		crosshair = GetComponent<Image> ();
	
	}

	/* Function: ResetCrosshair
	 * Purpose: Resets the Crosshair, setting it back to the color white
	 * and turns off the popup overlay
	 */
	public void ResetCrosshair(){

		crosshair.color = Color.white;
		popUpOverlay.SetActive(false);
	}

	/* Function: SetCrosshairColor
	 * Param: Color to be set
	 * Purpose: Sets the color of the crosshair image
	 */
	public void SetCrosshairColor (Color col){
		crosshair.color = col;

	}

	/* Function: EnablePopupOverlay
	 * Param: a boolean indicating to enable/disable the object
	 * Purpose: Enables/Disables the Pop-up overlay
	 */
	public void EnablePopupOverlay(bool isOn){
		popUpOverlay.SetActive (isOn);
	}

	/* Function: SetOverlay
	 * Param: an int value indicating the message for the Pop-up overlay; 0 or 1
	 * Purpose: Sets the Pop-up Overlay Label Component, indicating to the user their 
	 * option.
	 */
	public void SetOverlay(int choice){
	
		string text = "";

		//0 indicates Inspecting, 1 indicates Elicitation option
		if (choice == 0) {
			text = "\"Left MouseClick\" to Inspect";
		} else if (choice == 1) {
			text = "\"Left MouseClick\" to Elicitate";
		}

		// Set the Pop-up overlay childs text - this could potentialy be made clearer
		popUpOverlay.GetComponentInChildren<Text> ().text = text;
	}
	
}