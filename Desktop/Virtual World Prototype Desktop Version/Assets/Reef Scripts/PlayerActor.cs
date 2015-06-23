using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using UnityStandardAssets.Characters.FirstPerson;
/*
**Author: Dustin Wilson
**Date: 18/05/2015
**Description: A script that that contains the main functionality for the user, and the objects in the scene
**/
public class PlayerActor : MonoBehaviour {

	public static GameObject userAvatar;

	// Constants
	const int EXPLORATION = 0;
	const int ELICITATION = 1;
	const float INITIAL_R = 0.5f;
	const float MAX_SIGHT_DISTANCE = 100;// Maximum user sight range
	const float INTERACTION_DISTANCE = 5.0f;
	const float ZOOM = 10f;//Camera maximum zoom
	const float NORMAL = 60f;//Camera normal
	const float R_INCREMENT = 0.01f;

	// Whether or not the use has movement and/or camera controls
	private bool hasMovementControls;
	private bool hasCameraControls;
	//Indicates if exit menu is up
	public bool exitMode;

	public GameObject hoveredObject;//Current hovered object (Reef or Elicited Object)
	public GameObject siteViewer;//Reef inspection camera
	public GameObject elicitorObj;//Prefab for the Elicitor Object
	public OverlayController overlayCont;//Overlay Controller in the scene
	public InspectionController inspectCont;//Inspection Controller in the scene
	public Crosshair playerCross;

	//Projector variables - use for referencing the setup
	public GameObject projectorController;//Parent of the projector object - used for moving
	public Projector elicitPro;//projector component

	private ObjectDataProperties objectProps;//object properties of hovered object
	private int currentMode;//current mode exploration = 0, elicitation = 1
	private float range;

	//References CustomFirstPersonController for the FPScontroller
	private CustomFirstPersonController fpsController;

	//Indicators
	private bool elicitationInProgress;
	private bool isInspecting;
	private bool isPositioning;
	private bool isScaling;
	private bool currentlyHovering;

	//Variables for the positioning
	//private Vector2 p_Input;
	//private Vector3 m_MoveDir = Vector3.zero;
	//private float speed = 0.1f;
	
	//Camera Zooming Vairables
	private float smooth = 5f;
	private bool isZoomed = false;
	public float zoomFactor = 0.1f;
		

	// Use this for initialization
	void Start () {
		exitMode = false;
		currentMode = EXPLORATION;
		range = 10;
		userAvatar = gameObject;
		isInspecting = false;
		elicitationInProgress = false;
		fpsController = userAvatar.GetComponent<CustomFirstPersonController> ();
		EnableCursor (false);
		ProjectorColorChange(Color.green);
	}
	
	// Update is called once per frame
	void Update () {
	
		//disable updates if in inspection or exit modes of operation
		if (!isInspecting && !exitMode) {

			// If elicitation is in progress, look for specific key input
			if (elicitationInProgress) {
				
				if(Input.GetKeyDown (KeyCode.LeftShift)){//hotkey for scaling
					isScaling = true;
					ProjectorColorChange(Color.red);
				} else if (Input.GetKeyDown (KeyCode.LeftControl)){//Hotkey for positioning
					isPositioning = true;
					ProjectorColorChange(Color.yellow);
				} else if (Input.GetKeyUp (KeyCode.LeftShift)){//on key let go resets projector color and turns of scaling
					isScaling = false;
					ProjectorColorChange(Color.green);
				} else if (Input.GetKeyUp (KeyCode.LeftControl)){//on key let go resets projector color and turns of position
					isPositioning = false;
					ProjectorColorChange(Color.green);
				}

				//if scaling look for scale input
				if(isScaling){
					GetScaleInput();//scroll wheel commands
				} else if (isPositioning){//if positioning get and update new position
					PositionElicitation();
				} 

				//Get player confirmation
				if ((Input.GetKeyDown (KeyCode.Return) ||(Input.GetMouseButtonDown (0))) &&(!isScaling &&!isPositioning)){
					ConfirmElicitObjectValues();
				}
				
			} else {// else it performs normal operations if not eliciting

				// Perform a Raycast - to look for collision with objects of interest
				Vector3 v = new Vector3 (Camera.main.pixelWidth / 2.0f, Camera.main.pixelHeight / 2.0f, 0.0f);
				Ray ray = Camera.main.ScreenPointToRay (v); 
				RaycastHit hitInfo = new RaycastHit ();
				Physics.Raycast (ray, out hitInfo, MAX_SIGHT_DISTANCE);
			
				// If the raycast hits an object, select the object
				if (hitInfo.collider != null && hitInfo.transform.tag == "Inspect Element") {
					Refresh();
					hoveredObject = hitInfo.collider.gameObject;
					objectProps = hoveredObject.GetComponent<ObjectDataProperties>();
					UpdateCrosshairOverlay (0);
					TurnOnCameraAndProjector ();
				
				//if the raycast hits the reef
				} else if (hitInfo.collider != null && hitInfo.transform.tag == "Reef") {
					hoveredObject = hitInfo.collider.gameObject;
					EnableProjector(false);
					Refresh();
					// Need to check if in elicitation mode
					if (currentMode == ELICITATION) {
						UpdateCrosshairOverlay (1);
					}

				//if it doesnt hit anything
				} else {

					Refresh();
					hoveredObject = null;
					SetCurrentlyHovering (false, hoveredObject);
					//hoverControls.SetActive(false);
				}

				//Check if the user has changed modes
				if (Input.GetKeyDown (KeyCode.Alpha1)) {
					SetCurrentMode (0);
				} else if (Input.GetKeyDown (KeyCode.Alpha2)) {
					SetCurrentMode (1);
				}

				//Checks to see if the user wants to inspect or elicit
				if (((Input.GetMouseButtonDown (0))) && currentlyHovering && (hoveredObject.tag == "Inspect Element")) {

					LoadInspector(hoveredObject);
				
				} else if (((Input.GetMouseButtonDown (0))) && currentlyHovering && (hoveredObject.tag == "Reef")) {
					overlayCont.EnableElicitationOverlay (true);
					EnableElicitationMode (true);
					Refresh();
					Elicitation (hitInfo);
				}
			}
			//check to see iof the user wants to zoom in or out 
			if (!isScaling) {//as they share the same key bindings we dont want to be zooming in when scaling
				if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
					isZoomed = true;
					ZoomCamera ();
				} else if ((Input.GetAxis ("Mouse ScrollWheel") < 0)) {
					isZoomed = false;
					ZoomCamera ();
				}
			}
		
		} 

	}

	/** Function: IsObjectInRange
	 ** Purpose: Is to check, if the hoverObject object does exist, is it in range of the user for interaction
	 */
	private bool IsObjectInRange(){
		
		if (hoveredObject == null) {
			
			return false;
		} 
		
		return(Vector3.Distance (hoveredObject.transform.position, Camera.main.transform.position) <=
		       INTERACTION_DISTANCE || Vector3.Distance (hoveredObject.transform.position, Camera.main.transform.position) < range);
	}

//	private void GetPositionInput(){
//		// Read input
//		float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
//		float vertical = CrossPlatformInputManager.GetAxis("Vertical");
//		
//		p_Input = new Vector2(horizontal, vertical);
//		
//		m_MoveDir.x = p_Input.x * speed;
//		m_MoveDir.z = p_Input.y * speed;
//		
//		projectorController.transform.Translate(m_MoveDir * Time.fixedDeltaTime, Camera.main.transform);
//		UpdateCameraPosition (projectorController.transform);
//	}

	/** Function: PositionElicitation()
	 ** Purpose: Is to update the position of the elicitation area, via the use of raycasting
	 ** Note: Should be refactored as this is duplicated code from the update function 
	 */
	private void PositionElicitation(){
		//Cast a ray
		Vector3 v = new Vector3 (Camera.main.pixelWidth / 2.0f, Camera.main.pixelHeight / 2.0f, 0.0f);
		Ray ray = Camera.main.ScreenPointToRay (v); 
		RaycastHit hitInfo = new RaycastHit ();
		Physics.Raycast (ray, out hitInfo, MAX_SIGHT_DISTANCE);

		//get the current position
		Vector3 curPos = projectorController.transform.position;

		//if it collided with the reef, update the position of the 
		if (hitInfo.collider != null && hitInfo.transform.tag == "Reef") {
			//update current position
			curPos = Vector3.MoveTowards(curPos, hitInfo.point, 5*Time.deltaTime);
			projectorController.transform.position = curPos;
			UpdateCameraPosition (projectorController.transform.position);
		}
	}

	/** Function: ProjectorColorChange
	 ** Param: Color, the new color for the projector
	 ** Purpose: Is to change the color of the projector
	 */
	private void ProjectorColorChange(Color col){
		elicitPro.material.color = col;
	}
	
	/** Function: EnableCursor
	 ** Param: boolean, indicates whether or not it is active
	 ** Purpose: Is to turn off/on the Cursor
	 */
	public void EnableCursor(bool isOn){
		Cursor.visible = isOn;
	}

	/** Function: ZoomCamera()
	 ** Purpose: Is to zoom the camera in or out depending on if isZoomed boolean
	 */
	private void ZoomCamera(){
		
		if(isZoomed == true){

			Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView,ZOOM,Time.deltaTime * smooth);

		} else {

			Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView,NORMAL,Time.deltaTime * smooth);
		}
	}

	/** Function: UpdateCameraHeight
	 ** Param: Vector3, the position to be used for the camera
	 ** Purpose: Is to update the Reef Inspection camera position, in addition to keep it at a desired height
	 ** Note: Should possibly be refactored
	 */
	private void UpdateCameraPosition(Vector3 pos){

		//Perform a raycast
		Camera sV = siteViewer.GetComponent<Camera> ();

		Vector3 v = new Vector3 (sV.pixelWidth / 2.0f, sV.pixelHeight / 2.0f, 0.0f);
		Ray ray = sV.ScreenPointToRay (v); 
		RaycastHit hitInfo = new RaycastHit ();
		Physics.Raycast (ray, out hitInfo, 10.0f);
		//Update the height with an offset of 1.5
		siteViewer.transform.position = new Vector3(pos.x, hitInfo.point.y + 1.5f, pos.z);

	}

	/** Function: GetScaleInput()
	 ** Purpose: Is to get input from the user to update the area size for the elicitation 
	 */
	private void GetScaleInput(){
		if (Input.GetKey (KeyCode.UpArrow) || (Input.GetAxis ("Mouse ScrollWheel") > 0)) {
			elicitPro.orthographicSize += R_INCREMENT;
		} else if (Input.GetKey(KeyCode.DownArrow) || (Input.GetAxis ("Mouse ScrollWheel") < 0)) {
			elicitPro.orthographicSize -= R_INCREMENT;
		} 
	}
	
	/** Function: ResetProjector()
	 ** Purpose: Is to reset the projector back default, which entails disabling it, changing 
	 ** its color back to green and setting the radius back to the intial value 
	 */
	public void ResetProjector(){
		EnableProjector (false);
		ProjectorColorChange(Color.green);
		SetProjectorRadius (INITIAL_R);
	}

	/** Function: LoadInspector
	 ** Param: GameObject, the GameObject holding the information to be used for inspection
	 ** Purpose: Is to set up the application inspection mode, by setting control variables, enabling/disabling the specific overlays
	 ** and the setting of the inspection data.
	 */
	public void LoadInspector(GameObject inspecObject){
		//disable all player controls, 
		EnableCursor(true);
		EnableControl (false, false);
		//disable the main gui
		overlayCont.EnableMainOverlay (false);
		//enable the inspection window
		overlayCont.EnableInspectionOverlay (true);
		inspectCont.SetInspectionData (inspecObject);
		isInspecting = true;
	}

	/** Function: EnableInspectionMode
	 ** Param: boolean, the current state for inspection
	 ** Purpose: Is to enable/disable, the isInspecting variable.
	 */
	public void EnableInspectionMode(bool isOn){
		isInspecting = isOn;
	}

	/** Function: EnableElicitationMode
	 ** Param: boolean, the current state for elicitation
	 ** Purpose: Is to enable/disable, the elicitationInProgress variable.
	 */
	public void EnableElicitationMode(bool isOn){
		elicitationInProgress = isOn;
	}

	/** Function: EnableControl
	 ** Param1: boolean, the current state for camera controls
	 ** Param2: boolean, the current state for movement controls
	 ** Purpose: Is to enable/disable, the camera and movement modes of operations.
	 */
	public void EnableControl(bool isCam, bool isMove){

		if (currentMode == 0) {
			EnableCamera (isCam);
			EnableMovement (isMove);
		} else if (currentMode == 1) {
			EnableCamera(isCam);
		}
	}

	/** Function: TurnOnCameraAndProjector
	 ** Purpose: Turns on the projector and Reef Inspector camera, sets their positions and the projectors radius
	 */
	private void TurnOnCameraAndProjector ()
	{
		EnableProjector (true);
		siteViewer.SetActive (true);
		PositionProjector (hoveredObject.transform.position);
		SetProjectorRadius (objectProps.projectorRadius);
		UpdateCameraPosition (projectorController.transform.position);
	}

	/** Function: updateCrosshairOverlay
	 ** Param: integer, the overlay to be loaded (0 - exploration, 1 - elicitation)
	 ** Purpose: Updates the crosshair overlay accordingly to whether it is in range or not,
	 ** by enabling a popup overlay and changing the crosshair color, also setting the variables to 
	 ** indicate a current hovered object.
	 */
	public void UpdateCrosshairOverlay(int overlayType){
		
		if (IsObjectInRange ()) {
			playerCross.EnablePopupOverlay (true);
			playerCross.SetOverlay (overlayType);
			playerCross.SetCrosshairColor (Color.green);
			SetCurrentlyHovering (true, hoveredObject);
		} else {	
			playerCross.SetCrosshairColor (Color.red);
		}

	}

	/** Function: EnableProjector
	 ** Param: boolean, the current active state for the projector
	 ** Purpose: Is to enable/disable, the current active state for the projector.
	 */
	private void EnableProjector(bool isOn){
		projectorController.SetActive(isOn); 
	}

	/** Function: PositionProjector
	 ** Param: Vector3, the position to be used to update the projector
	 ** Purpose: Is to set the projector to the new position, with a height of 2.
	 ** Note: Height may need to be changed in the future to account for higher meshes
	 */
	private void PositionProjector(Vector3 pos){
		projectorController.transform.position = new Vector3 (pos.x, 2.0f, pos.z); 
	}

	/** Function: SetProjectorRadius
	 ** Param: float, the value of the radius to be set
	 ** Purpose: Is to set the projector orthographic size with the new radius value.
	 */
	private void SetProjectorRadius(float radius){
		elicitPro.orthographicSize = radius;
	}

	/** Function: Refresh
	 ** Purpose: Is to reset the crosshair, the projector, and disable the reef inspector camera.
	 */
	private void Refresh(){
		ResetCrosshair ();
		ResetProjector ();
		siteViewer.SetActive (false);
	}

	/** Function: ResetCrosshair
	 ** Purpose: Is to reset the crosshair, by setting it to its default color and disabling the pop-up overlay.
	 */
	private void ResetCrosshair ()
	{
		playerCross.EnablePopupOverlay (false);
		playerCross.SetCrosshairColor (Color.white);
	}

	/** Function: SetCurrentMode
	 ** Param: integer, the mode of operation 0 - exploration, 1 - elicitation
	 ** Purpose: Is to set up the current mode indicated, by setting the label up the top left-hand corner
	 ** and turning off/on the controls used in that mode
	 */
	private void SetCurrentMode(int mode){
		Refresh ();
		currentMode = mode;
		GameObject modeText = GameObject.FindWithTag("Mode Display");
		if (mode == EXPLORATION) {
			modeText.GetComponent<Text> ().text = "Mode: Exploration";
			EnableMovement(true);
		} else if (mode == ELICITATION){
			modeText.GetComponent<Text>().text = "Mode: Elicitation";
			EnableMovement(false);
		}
	}

	/** Function: EnableCamera
	 ** Param: boolean, indicates the state of the camera controls
	 ** Purpose: Is to disable/enable the camera controls, by setting the first person controller is camera moving variable
	 */
	private void EnableCamera(bool isCameraMoving){
		hasCameraControls = isCameraMoving;
		fpsController.isCameraMoving = hasCameraControls;
	}

	/** Function: EnableMovement
	 ** Param: boolean, indicates the state of the movement controls
	 ** Purpose: Is to disable/enable the player movement controls, by setting the first person controller is moving variable
	 */
	private void EnableMovement(bool isMoving){
		hasMovementControls = isMoving;
		fpsController.isMoving = hasMovementControls;
		
	}

	/** Function: SetCurrentlyHovering
	 ** Param1: boolean, indicates the hovered state
	 ** Param2: GameObject, the game object being hovered over
	 ** Purpose: Is to set the current hovered object, and the currently hovering state
	 */
	public void SetCurrentlyHovering(bool isHover, GameObject hoverObject){
		currentlyHovering = isHover;	
		hoveredObject = hoverObject;
	}

	/** Function: Elicitation
	 ** Param1: RaycastHit, the current ray cast hit info
	 ** Purpose: Is to set up the Elicitation mode, by setting up the projector and reef inspection camera
	 */
	public void Elicitation(RaycastHit hitInfo){

		Vector3 pos = new Vector3 (hitInfo.point.x, 2f, hitInfo.point.z);
		EnableProjector (true);
		siteViewer.SetActive (true);
		PositionProjector(pos);

		Vector3 pcPos = projectorController.transform.position;

		//currentElicPos = new Vector3(pcPos.x,pcPos.y,pcPos.z);
		UpdateCameraPosition (hitInfo.point);
		ProjectorColorChange(Color.green);
	}
	

	//function to destroy object if canceled
	//public void DestroyElicitationObject(){

	//	Destroy (currentElicitedObject,0.0f);
	//}

	/** Function: ConfirmElicitObjectValues
	 ** Purpose: Is to create an Elicitation object, assign the gather values to, disable the current mode, and load the inspector
	 ** for the purpose of adding reef information to the newly created object
	 */
	public void ConfirmElicitObjectValues(){

		ProjectorColorChange(Color.green);
		Vector3 pos = new Vector3 (projectorController.transform.position.x, 1.0f, projectorController.transform.position.z); 

		//create an item at the position obtained via hitInfo
		GameObject currentElicitedObject = Instantiate (elicitorObj, pos, Quaternion.identity) as GameObject;

		//u get the object
		GameObject capChild = currentElicitedObject.transform.FindChild ("Capsule").gameObject;
		capChild.SetActive (true);

		//grab the script
		ObjectDataProperties currentObjectData = capChild.GetComponent<ObjectDataProperties> ();

		// Set the transform information for each child
		currentObjectData.elicitedAreaTransform = projectorController.transform; 
		currentObjectData.elicitedTransformInfo = capChild.transform;
		currentObjectData.projectorRadius = elicitPro.orthographicSize;

		EnableElicitationMode (false);

		//escape out of current overlay
		overlayCont.EscapeOverlay ();

		// load the inspector for data input
		LoadInspector (capChild);
	}

	/** Function: IsInspecting
	 ** Purpose: Is to get the current value for the isInspecting variable
	 */
	public bool IsInspecting(){
		return isInspecting;
	}

	/** Function: IsElicitating
	 ** Purpose: Is to get the current value for the elicitationInProgress variable
	 */
	public bool IsElicitating(){
		return elicitationInProgress;
	}
}
