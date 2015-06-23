using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

/*
**Author: Dustin Wilson
**Date: 18/05/2015
**Description: A script that that contains the functionality to update, enable/disable the different HUDs found in the application.
**Note: Remove the update function and add it functionality to the PlayerActor script
**/
public class OverlayController : MonoBehaviour {

	//Overlays
	public GameObject InspectionOverlay; //Inspection overlay
	public GameObject MainGuiOverlay; // Crosshair overlay
	public GameObject EditModeOverlay; //Edit mode overlay
	public GameObject MainMenuOverlay; //Exit menu overlay
	public GameObject ElicitationMainOverlay;//Elicitation overlay

	//Controllers
	private PlayerActor userManager; // remove functionality that calls this and added it to the PlayerActor script
	private InspectionController inspectOver;
	
	private bool isEdit;
	private InputField editField;
	private List<GameObject> overlayList;//Keeps track of the current progression of overlays, to allow for easy backwards traversal
	private int currentIndex;

	// Use this for initialization
	void Start () {
		userManager = GameObject.FindWithTag ("Player").GetComponent<PlayerActor> ();
		overlayList = new List <GameObject>();
		currentIndex = 0;
		overlayList.Add (MainGuiOverlay);
		inspectOver = gameObject.GetComponent<InspectionController> ();;
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown (KeyCode.Escape)) {
			//if in one of the modes
			if(currentIndex>0){
				EscapeOverlay();
				userManager.exitMode = false;
			} else {
				EnableExitMenu(true);
				userManager.exitMode = true;
				EnableMainOverlay(false);
				userManager.EnableControl(false,false);
			}
		}
	}

	/** Function: EnableInspectionOverlay
	 ** Param: Boolean, true enable, false disable 
	 ** Purpose: Is to turn on/off the cursor and the inspection overlay
	 */
	public void EnableInspectionOverlay(bool enable){
		//userManager.EnableCursor (enable);
		EnableOverlay (InspectionOverlay, enable);

	}

	/** Function: EnableMainOverlay
	 ** Param: Boolean, true enable, false disable 
	 ** Purpose: Its purpose is turn on/off the main overlay (crosshair)
	 */
	public void EnableMainOverlay(bool enable){
		
		EnableOverlay (MainGuiOverlay, enable);
	}

	/** Function: EnableEditModeOverlay
	 ** Param: Boolean, true enable, false disable 
	 ** Purpose: Is to turn on/off the edit mode overlay
	 */
	public void EnableEditModeOverlay(bool enable){
		//userManager.EnableCursor (enable);
		EnableOverlay (EditModeOverlay, enable);

	}

	/** Function: EnableElicitationOverlay
	 ** Param: Boolean, true enable, false disable 
	 ** Purpose: Is to turn on/off the Elicitation overlay
	 */
	public void EnableElicitationOverlay(bool enable){
		
		EnableOverlay (ElicitationMainOverlay, enable);
	}
	
	/** Function: EnableExitMenu
	 ** Param: Boolean, true enable, false disable 
	 ** Purpose: Is turn on/off the cursor and the Main Menu (exit) overlay
	 */
	public void EnableExitMenu(bool enable){
		userManager.EnableCursor (enable);
		EnableOverlay (MainMenuOverlay, enable);
		DataContoller.control.inExitMenu = true;	
	}

	/** Function: EnableOverlay
	 ** Param1: GameObject, the overlay to be enable/disable
	 ** Param2: Boolean, true enable, false disable 
	 ** Purpose: Is turn on/off the chosen overlay
	 */
	public void EnableOverlay(GameObject overlay, bool enable){

		//Enable/Disable the chosen overlay gameobject
		overlay.SetActive (enable);

		//If enable and not the current overlay does not equal the selected overlay
		if (enable && !(overlayList[currentIndex].Equals(overlay))) {
			//Add the overlay to the list to keep track of how far deep
			overlayList.Add (overlay);
			currentIndex ++;
		} 
	}

	/** Function: RemoveElement
	 ** Purpose: Is the remove the current element from the array
	 */
	private void RemoveElement(){
		overlayList.RemoveAt (currentIndex);
		currentIndex--;
	}

	/** Function: EscapeOverlay
	 ** Purpose: Function is to escape from the current overlay, and enable/disable controls and modes
	 */
	public void EscapeOverlay(){
		//Disable the current
		EnableOverlay (overlayList[currentIndex], false);
		RemoveElement();
		EnableOverlay(overlayList[currentIndex], true);

		//leave mode and bring up mainmode
		if (overlayList [currentIndex] == MainGuiOverlay) {
			userManager.EnableCursor (false);
			userManager.EnableControl (true, true);
			//Disable all modes
			if (userManager.IsInspecting ()) {
				userManager.EnableInspectionMode (false);
				//Disable elicitationmode 
			} else if (userManager.IsElicitating ()) {
				userManager.EnableElicitationMode (false);
				userManager.ResetProjector ();
			}	
		} 
	}

	/** Function: UpdateButton
	 ** Param: boolean, indicates which button is pressed, false its add, true its edit
	 ** Purpose: Calls for the edit mode overlay to be enable and for the inspection overlay to be turned off,
	 ** in addition to if isUpdate is true, the edit input field is filled with current data for editing
	 */
	public void UpdateButton(bool isUpdate){

		isEdit = isUpdate;
		EnableEditModeOverlay (true);
		EnableInspectionOverlay (false);
		editField = GameObject.FindWithTag ("Edit content").GetComponent<InputField> ();

		//set the text to be equal to the inspector text
		if (isEdit) {
			string info = inspectOver.elicitedInfo;
			editField.text = info;
		} else {
			editField.text = "";
		}

		//Set focus for the input field
		editField.Select();
		editField.ActivateInputField();

	}

	/** Function: UpdateInformation
	 ** Param: Text, text component used by the input field
	 ** Purpose: Update the elicited Infomation obtained from the input field, for the inspection overlay
	 */
	public void UpdateInformation(Text info){

		//If on edit mode or text is blank
		if (isEdit || inspectOver.elicitedInfo == "") {
			inspectOver.elicitedInfo = info.text;
		} else { //else add a separator
			inspectOver.elicitedInfo = inspectOver.elicitedInfo +"\n\n" + info.text;
		}
		//update the inspector overlay
		inspectOver.UpdateOverlayText (inspectOver.elicitedInfo);
	}
}
