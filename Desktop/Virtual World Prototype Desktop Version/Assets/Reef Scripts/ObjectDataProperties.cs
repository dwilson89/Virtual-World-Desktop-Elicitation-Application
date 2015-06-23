using UnityEngine;
using System.Collections;
using System;

/*
**Author: Dustin Wilson
**Date: 18/05/2015
**Description: A script that purpose is to hold the information of the elicitation.
**Note: Is to be attached to the elicitation object
**/
public class ObjectDataProperties:MonoBehaviour{
	

	// In future possibly add the name of the author

	//currently public will need to be reverted to private, with getters and setters, when loading from a savefile implementation

	// The dictated information from the user
	public string elicitedInformation= "";

	// The elicited transform information
	public Transform elicitedTransformInfo;//the gameobject transform on which this script is attached to

	public Transform elicitedAreaTransform;//projector transform
	public float projectorRadius; //projector orthographic radius

	// The object this information is attached to
	//public GameObject subjectObject;

	// The required distance to interact with this object
	//public float interactionDistance = 5.0f;
	
	// Use this for initialization
	void Start () {

		//subjectObject = gameObject;
		elicitedTransformInfo = gameObject.transform;
	}

}
