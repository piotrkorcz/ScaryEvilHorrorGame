/*
DragRigidbody.cs ver. 19.10 - wirted by ThunderWire Games * Script for Drag, Drop & Throw Rigidbody Objects
*/

using UnityEngine;
using System.Collections;
using ThunderWire.Parser;

public class DragRigidbody : MonoBehaviour {

    private ParsePrimitives parser = new ParsePrimitives();
    private Camera playerCam;
    private HFPS_GameManager gameManager;
    private InteractManager interact;
	private InputManager inputManager;
	private PlayerFunctions pfunc;
	private DelayEffect delay;

	[Header("Drag")]
	public LayerMask CullLayers;
	public string GrabLayer = "Interact";
	public string GrabTag = "Grab";
    public string OnlyGrabTag = "OnlyGrab";
    public float PickupRange = 3f;
	public float ThrowStrength = 50f;
    public float DragSpeed = 10f;
	public float minDistance = 1.5f;
	public float maxDistance = 3f;
	public float maxDistanceGrab = 4f;
	public float spamWaitTime = 0.5f;
	
	private float distance;
	
	[Header("Other")]
	public float rotateSpeed = 10f;
	public float rotationDeadzone = 0.1f;
	public float objectZoomSpeed = 3f;	
	public bool FreezeRotation = true;
	public bool enableObjectPull = true;
	public bool enableObjectRotation = true;
	public bool enableObjectZooming = true;
	
	private GameObject objectHeld;	
	
	private Ray playerAim;
	private GameObject objectRaycast;
	private bool GrabObject;
	private bool isObjectHeld;
	private bool tryPickupObject;
	private bool isPressed;
    private bool throwPressed = false;
	private bool antiSpam;
	
	private KeyCode rotateObject;
	private KeyCode GrabButton;
	private KeyCode ThrowButton;

	private bool isSet;

    private void Awake()
    {
        delay = gameObject.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).gameObject.GetComponent<DelayEffect>();
        interact = GetComponent<InteractManager>();
        inputManager = GetComponent<ScriptManager>().inputManager;
        gameManager = GetComponent<ScriptManager>().GameManager;
        pfunc = GetComponent<PlayerFunctions>();
        playerCam = Camera.main;
    }

    void SetKeys()
	{
        GrabButton = parser.ParseType<KeyCode>(inputManager.GetInput("Pickup"));
        ThrowButton = parser.ParseType<KeyCode>(inputManager.GetInput("Throw"));
        rotateObject = parser.ParseType<KeyCode>(inputManager.GetInput("Fire"));
        isSet = true;
	}

	void Start () {
		isObjectHeld = false;
		tryPickupObject = false;
		isPressed = false;
		objectHeld = null;
	}
	
	void Update()
	{
        gameManager.isHeld = objectHeld != false;
		interact.isHeld = objectHeld != false;

		if(inputManager.InputsCount() > 0 && !isSet)
		{
			SetKeys();
		}

		if (inputManager.GetRefreshStatus () && isSet) {
			isSet = false;
		}

        if (gameManager.isPaused) return;

		if(objectRaycast && !antiSpam)
		{
			if(ControlFreak2.CF2Input.GetKeyDown(GrabButton) && !isPressed){
				isPressed = true;
				GrabObject = !GrabObject;
			}else if(isPressed){
				isPressed = false;
			}
		}

		if (GrabObject){
			if (!isObjectHeld){
				tryPickObject();
				tryPickupObject = true;
			}else{
				holdObject();
			}
		}else if(isObjectHeld){
			DropObject();
		}

		if (ControlFreak2.CF2Input.GetKeyDown(ThrowButton) && objectHeld)
		{
            throwPressed = true;
            isObjectHeld = false;
			ThrowObject();
		}
        else if (throwPressed)
        {
            Debug.Log("Throw");
            pfunc.enabled = true;
            throwPressed = false;
        }
		
		Ray playerAim = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
		RaycastHit hit;

		float rotationInputX = 0.0f;
		float rotationInputY = 0.0f;

		float x = ControlFreak2.CF2Input.GetAxis("Mouse X");
		float y = ControlFreak2.CF2Input.GetAxis("Mouse Y");

		if(Mathf.Abs(x) > rotationDeadzone){
			rotationInputX = -(x * rotateSpeed);
		}

		if(Mathf.Abs(y) > rotationDeadzone){
			rotationInputY = (y * rotateSpeed);
		}
		
		if (Physics.Raycast (playerAim, out hit, PickupRange, CullLayers)) {
			if (hit.collider.gameObject.layer == LayerMask.NameToLayer (GrabLayer)) {
				if (hit.collider.tag == GrabTag || hit.collider.tag == OnlyGrabTag) {
					objectRaycast = hit.collider.gameObject;
                    gameManager.canGrab = true;
				}
			} else {
				if (!tryPickupObject) {
					objectRaycast = null;
                    gameManager.canGrab = false;
				}
			}
		
			if (objectHeld) {
				if (ControlFreak2.CF2Input.GetKey (rotateObject) && enableObjectRotation) {
					objectHeld.GetComponent<Rigidbody> ().freezeRotation = true;
                    gameManager.LockStates (true, false, false, false, 0);
					objectHeld.transform.Rotate (playerCam.transform.up, rotationInputX, Space.World);
					objectHeld.transform.Rotate (playerCam.transform.right, rotationInputY, Space.World);
				} else {
                    gameManager.LockStates (false, false, false, false, 0);
				}
				if (enableObjectZooming) {
					distance = Mathf.Clamp (distance, minDistance, maxDistance);
					distance += ControlFreak2.CF2Input.GetAxis ("Mouse ScrollWheel") * objectZoomSpeed;
				}
			}
		} else {
			if (!tryPickupObject) {
				objectRaycast = null;
                gameManager.canGrab = false;
			}
		}
	}
	
	private void tryPickObject(){
		StartCoroutine (AntiSpam ());

		objectHeld = objectRaycast;

		if (!(objectHeld.GetComponent<Rigidbody> ())) {
			return;
		}
			
		if (enableObjectPull) {
			if (!objectHeld.GetComponent<DragDistance> ()) {
				float dist = Vector3.Distance (transform.position, objectHeld.transform.position);
				if (dist > maxDistance - 1f) {
					distance = minDistance + 0.25f;
				} else {
					distance = dist;
				}
			} else {
                distance = objectHeld.GetComponent<DragDistance>().dragDistance;
            }
		}

		objectHeld.GetComponent<Rigidbody> ().useGravity = false;

		if (FreezeRotation) {
			objectHeld.GetComponent<Rigidbody> ().freezeRotation = true;
		} else {
			objectHeld.GetComponent<Rigidbody> ().freezeRotation = false;
		}

		delay.isEnabled = false;
        gameManager.UIPreventOverlap(true);
        gameManager.ShowGrabSprites();
        gameManager.HideSprites(spriteType.Examine);
        interact.CrosshairVisible(false);
        pfunc.enabled = false;
        GetComponent<ScriptManager>().SetScriptEnabledGlobal = false;

        Physics.IgnoreCollision (objectHeld.GetComponent<Collider>(), this.transform.root.GetComponent<Collider> (), true);

		isObjectHeld = true;
	}
	
	private void holdObject(){
        Ray playerAim = playerCam.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
		
		Vector3 nextPos = playerCam.transform.position + playerAim.direction * distance;
		Vector3 currPos = objectHeld.transform.position;
		
		objectHeld.GetComponent<Rigidbody>().linearVelocity = (nextPos - currPos) * DragSpeed;
		
		if (Vector3.Distance(objectHeld.transform.position, playerCam.transform.position) > maxDistanceGrab)
		{
           DropObject();
		}
	}

	public bool CheckHold(){
		return isObjectHeld;
	}
	
    private void DropObject()
    {
        gameManager.UIPreventOverlap(false);
        gameManager.HideSprites (spriteType.Grab);
		interact.CrosshairVisible (true);
		objectHeld.GetComponent<Rigidbody>().useGravity = true;
		objectHeld.GetComponent<Rigidbody>().freezeRotation = false;
		Physics.IgnoreCollision(objectHeld.GetComponent<Collider>(), this.transform.root.GetComponent<Collider>(), false);
        GetComponent<ScriptManager>().SetScriptEnabledGlobal = true;
        delay.isEnabled = true;
        pfunc.enabled = true;
        objectRaycast = null;
		objectHeld = null;
		isObjectHeld = false;
		tryPickupObject = false;
		GrabObject = false;
		isPressed = false;
    }
	
    private void ThrowObject()
    {
        gameManager.HideSprites (spriteType.Grab);
        gameManager.UIPreventOverlap(false);
        interact.CrosshairVisible (true);
        objectHeld.GetComponent<Rigidbody>().useGravity = true;
        objectHeld.GetComponent<Rigidbody>().AddForce(playerCam.transform.forward * ThrowStrength * 10);
		objectHeld.GetComponent<Rigidbody>().freezeRotation = false;
		Physics.IgnoreCollision(objectHeld.GetComponent<Collider>(), this.transform.root.GetComponent<Collider>(), false);
        GetComponent<ScriptManager>().SetScriptEnabledGlobal = true;
        delay.isEnabled = true;
		objectRaycast = null;
		objectHeld = null;
		tryPickupObject = false;
		GrabObject = false;
		isPressed = false;
    }

	IEnumerator AntiSpam()
	{
		antiSpam = true;
		yield return new WaitForSeconds (spamWaitTime);
		antiSpam = false;
	}
}
