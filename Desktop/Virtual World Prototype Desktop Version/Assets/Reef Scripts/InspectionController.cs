using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*
**Author: Dustin Wilson
**Date: 18/05/2015
**Description: A script that Sets up the data displayed on the inspection overlay
**Note: Remove this script and add its functionality to the OverlayController Script
**/
public class InspectionController : MonoBehaviour {
	

	public GameObject siteViewer;// The Reef inspector Camera
	public GameObject svText;//Scrollview text
	public GameObject contentBox;//Content parent of Inspect Text
	public string elicitedInfo; //Current Selected elicitation information

	//Elicited object data used in this class
	private GameObject currentObject; //Current Selected Object
	private Transform TransformInfo;// The elicited transform information
	private ObjectDataProperties currentObjectData;//Current objectcs object data

	/** Function: SetInspectionData
	 ** Param: GameObject, element being inspected 
	 ** Purpose: Set up the inspection overlay, with data obtained from the element to be inspected
	 */
	public void SetInspectionData(GameObject inspectElement){
		//Get the objectDataProperties that stores the information
		currentObjectData = inspectElement.GetComponent<ObjectDataProperties> ();
		//Get and assign the stored values
		TransformInfo = currentObjectData.elicitedTransformInfo;
		elicitedInfo = currentObjectData.elicitedInformation;
		currentObject = inspectElement;
		UpdateCameraPosition (TransformInfo);
		UpdateOverlayText (elicitedInfo);

	}

	/** Function: UpdateOverlayText
	 ** Param: string, updated elicited data 
	 ** Purpose: Update the text displayed, with the edit/added information
	 */
	public void UpdateOverlayText(string text){
		svText.GetComponent<Text> ().text = text;//update the scroll viewer text
		elicitedInfo = text; //update the elicitedInfo
		currentObjectData.elicitedInformation = text;//update the current object data elicited information
		UpdateScrollViewSize ();
	}

	/** Function: UpdateScrollViewSize
	 ** Purpose: Update the size of the scrollview when text informationed is added or removed
	 */
	private void UpdateScrollViewSize () {
		
		//Get the size and update the content size
		Vector2 info = svText.GetComponent<RectTransform>().sizeDelta;
		contentBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, info.y + 40f);
		
		// need to offset the y position for the child
		Vector2 textPos = svText.GetComponent<RectTransform> ().anchoredPosition;
		svText.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (textPos.x, -info.y / 2f);
		
	}

	/** Function: UpdateCameraPosition
	 ** Param: The new transform for the reef inspector camera
	 ** Purpose: Update the reef inspection camera, to the current objects position
	 */
	private void UpdateCameraPosition(Transform camPos){
		siteViewer.transform.position = camPos.position;
		TransformInfo = siteViewer.transform;//update the TransformInfo
	}
}
