﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Parser;

public class ExamineManager : MonoBehaviour {

    private ParsePrimitives parser = new ParsePrimitives();
    private HFPS_GameManager gameManager;
    private InputManager inputManager;
	private InteractManager interact;
	private PlayerFunctions pfunc;
	private GameObject paperUI;
	private Text paperText;
	private DelayEffect delay;

	[Header("Raycast")]
	public LayerMask CullLayers;
	public string InteractLayer = "Interact";
	public string TagExamine = "Examine";
	public string TagExaminePaper = "Paper";
	public float PickupRange = 3f; 
	public float rotationDeadzone = 0.1f;
	public float rotateSpeed = 10f;
	public float spamWaitTime = 0.5f;
    public bool isPaper;

    [Header("Layering")]
    public LayerMask ExamineMainCamMask;
    public LayerMask ExamineArmsCamMask;

    private LayerMask DefaultMainCamMask;
    private LayerMask DefaultArmsCamMask;
    private Camera ArmsCam;

    private string examineName;

	private bool rotSet;
	private bool isPressedRead;
	private bool isReaded;

	private bool antiSpam;
	private bool isPressed;
	private bool isObjectHeld;
	private bool isExaminig;
	private bool tryExamine;
	private bool otherHeld;
	private bool objectUsable;

	private Vector3 objectPosition;
	private Quaternion objectRotation;
	private float distance;

	private Vector3 rotateAngle;

	private GameObject objectRaycast;
	private GameObject objectHeld;	
	private Camera playerCam;

	private Ray playerAim;

	private KeyCode rotateKey;
	private KeyCode grabKey;
	private KeyCode examineKey;

	private ExamineItem examinedItem;
	private bool isSet;

	void SetKeys()
	{
        grabKey = parser.ParseType<KeyCode>(inputManager.GetInput("Pickup"));
        rotateKey = parser.ParseType<KeyCode>(inputManager.GetInput("Fire"));
        examineKey = parser.ParseType<KeyCode>(inputManager.GetInput("Examine"));
    }

    private void Awake()
    {

        if (GetComponent<ScriptManager>() && GetComponent<InteractManager>() && GetComponent<PlayerFunctions>())
        {
            inputManager = GetComponent<ScriptManager>().inputManager;
            gameManager = GetComponent<ScriptManager>().GameManager;
            interact = GetComponent<InteractManager>();
            pfunc = GetComponent<PlayerFunctions>();
            paperUI = gameManager.PaperTextUI;
            paperText = gameManager.PaperReadText;
        }
        else
        {
            Debug.LogError("Missing one or more scripts in " + gameObject.name);
        }

        delay = gameObject.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).gameObject.GetComponent<DelayEffect>();
        playerCam = Camera.main;
        ArmsCam = GetComponent<ScriptManager>().ArmsCameraBlur.GetComponent<Camera>();
        DefaultMainCamMask = Camera.main.cullingMask;
        DefaultArmsCamMask = ArmsCam.cullingMask;
    }
	

	void Update ()
	{
		if(inputManager.InputsCount() > 0 && !isSet)
		{
			SetKeys();
		}

		if (inputManager.GetRefreshStatus () && isSet) {
			isSet = false;
		}

		//Prevent Interact Dynamic Object when player is holding other object
		otherHeld = GetComponent<DragRigidbody> ().CheckHold ();

        if (gameManager.isPaused) return;

		if (objectRaycast && !antiSpam && examinedItem) {
			if (ControlFreak2.CF2Input.GetKeyDown (grabKey) && !isPressed && !otherHeld) {
				isPressed = true;
				isExaminig = !isExaminig;
			} else if (isPressed) {			
				isPressed = false;
			}
		}

		if (isExaminig){
			if (!isObjectHeld){
				firstGrab();
				tryExamine = true;
			}else{
				holdObject();
			}
		}else if(isObjectHeld){
			dropObject();
		}

		RaycastHit hit;
		playerAim = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

		if (Physics.Raycast (playerAim, out hit, PickupRange, CullLayers)) {
			if (hit.collider.gameObject.layer == LayerMask.NameToLayer (InteractLayer)) {
				if (hit.collider.tag == TagExamine || hit.collider.tag == TagExaminePaper) {
					objectRaycast = hit.collider.gameObject;
					if (objectRaycast.GetComponent<ExamineItem> ()) {
						examinedItem = objectRaycast.GetComponent<ExamineItem> ();
					}
				} else {
					if (!tryExamine) {
						objectRaycast = null;
						examinedItem = null;
					}
				}
			} else {
				if (!tryExamine) {
					objectRaycast = null;
					examinedItem = null;
				}
			}
		} else {
			if (!tryExamine) {
				objectRaycast = null;
				examinedItem = null;
			}
		}

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
			
		if (objectHeld && isObjectHeld) {
			if (ControlFreak2.CF2Input.GetKey (rotateKey)) {
				objectHeld.transform.Rotate (playerCam.transform.up, rotationInputX, Space.World);
				objectHeld.transform.Rotate (playerCam.transform.right, rotationInputY, Space.World);
			}

			if (ControlFreak2.CF2Input.GetKey (examineKey) && !isPaper) {
                gameManager.ShowHint (examineName);
			}

			if (isPaper) {
				if(ControlFreak2.CF2Input.GetKeyDown(examineKey) && !isPressedRead){
					isPressedRead = true;
					isReaded = !isReaded;
				}else if(isPressedRead){
					isPressedRead = false;
				}

				if (isReaded) {
					string text = objectRaycast.GetComponent<ExamineItem>().PaperReadTexts;
					paperText.text = text;
					paperUI.SetActive (true);
				} else {
					paperUI.SetActive (false);
				}
			}
		}
	}

	void firstGrab()
	{
		if(objectRaycast.tag == TagExamine || objectRaycast.tag == TagExaminePaper)
		{
			StartCoroutine (AntiSpam ());
			objectHeld = objectRaycast.gameObject;
			if (objectRaycast.tag == TagExaminePaper) {
				isPaper = true;
			} else {
				isPaper = false;
			}

            if (!(objectHeld.GetComponent<Rigidbody>()))
            {
                Debug.LogError(objectHeld.name + " need Rigidbody Component to pickup");
                return;
            }

            if (objectHeld.GetComponent<ExamineItem> ()) {
				examinedItem = objectHeld.GetComponent<ExamineItem> ();
				distance = examinedItem.examineDistance;
				examineName = examinedItem.examineObjectName;
				if (examinedItem.examineSound) {
					AudioSource.PlayClipAtPoint(examinedItem.examineSound, objectRaycast.transform.position, 0.75f);
				}
			}

			if (!isObjectHeld) {
				objectPosition = objectHeld.transform.position;
				objectRotation = objectHeld.transform.rotation;
				objectHeld.GetComponent<Collider> ().isTrigger = true;
			}

			if (gameManager.gameObject.GetComponent<UIFloatingItem> ().AllItemsList.Contains (objectHeld)) {
                gameManager.gameObject.GetComponent<UIFloatingItem> ().SetItemVisible (objectHeld, false);
			}

			if (objectHeld.transform.childCount > 0) {
				objectHeld.layer = LayerMask.NameToLayer ("Examine");
				foreach (Transform child in objectHeld.transform) {
					if (child.GetComponent<MeshFilter> ()) {
						child.gameObject.layer = LayerMask.NameToLayer ("Examine");
					}
				}
			} else {
                objectHeld.layer = LayerMask.NameToLayer ("Examine");
			}

            playerCam.cullingMask = ExamineMainCamMask;
            ArmsCam.cullingMask = ExamineArmsCamMask;

			delay.isEnabled = false;
            pfunc.enabled = false;
            gameManager.UIPreventOverlap(true);
            gameManager.ShowExamineSprites(grabKey.ToString(), examineKey.ToString());
            gameManager.HideSprites(spriteType.Interact);
            GetComponent<ScriptManager>().SetScriptEnabledGlobal = false;

            Physics.IgnoreCollision(objectRaycast.GetComponent<Collider>(), transform.root.GetComponent<Collider>(), true);

            isObjectHeld = true;
        }
	}

	void holdObject()
	{
		Vector3 nextPos = playerCam.transform.position + playerAim.direction * distance;
		Vector3 currPos = objectRaycast.transform.position;

		interact.CrosshairVisible (false);
        gameManager.LockStates (true, true, true, false, 1);

		objectHeld.GetComponent<Rigidbody> ().isKinematic = false;
		objectHeld.GetComponent<Rigidbody> ().useGravity = false;
		objectHeld.GetComponent<Rigidbody> ().linearVelocity = (nextPos - currPos) * 10;

		if (!rotSet && isPaper) {
			Vector3 rotation = objectRaycast.GetComponent<ExamineItem> ().paperRotation;
			objectRaycast.transform.rotation = Quaternion.LookRotation (nextPos - currPos) * Quaternion.Euler (rotation);
			rotSet = true;
		}
	}
		

	void dropObject()
	{
		if (gameManager.gameObject.GetComponent<UIFloatingItem> ().AllItemsList.Contains (objectHeld)) {
            gameManager.gameObject.GetComponent<UIFloatingItem> ().SetItemVisible (objectHeld, true);
		}
		distance = 0;

		if (objectHeld.transform.childCount > 0) {
			objectHeld.layer = LayerMask.NameToLayer ("Interact");
			foreach (Transform child in objectHeld.transform) {
				if (child.GetComponent<MeshFilter> ()) {
					child.gameObject.layer = LayerMask.NameToLayer ("Interact");
				}
			}
		} else {
			objectHeld.layer = LayerMask.NameToLayer ("Interact");
		}

        playerCam.cullingMask = DefaultMainCamMask;
        ArmsCam.cullingMask = DefaultArmsCamMask;

        pfunc.enabled = true;
        gameManager.UIPreventOverlap(false);
        gameManager.HideSprites(spriteType.Examine);
        examinedItem = null;
        examineName = null;
        isObjectHeld = false;
		isExaminig = false;
		rotSet = false;
		isReaded = false;
		paperUI.SetActive (false);
        interact.CrosshairVisible (true);
        gameManager.LockStates (false, true, true, false, 1);
		objectHeld.transform.position = objectPosition;
		objectHeld.transform.rotation = objectRotation;
		if (!isPaper) {
			objectHeld.GetComponent<Collider> ().isTrigger = false;
			objectHeld.GetComponent<Rigidbody> ().isKinematic = false;
			objectHeld.GetComponent<Rigidbody> ().useGravity = true;
		} else {
			objectHeld.GetComponent<Rigidbody> ().isKinematic = true;
			objectHeld.GetComponent<Rigidbody> ().useGravity = false;
		}
		tryExamine = false;
		objectRaycast = null;
		objectHeld = null;
		delay.isEnabled = true;
        GetComponent<ScriptManager>().SetScriptEnabledGlobal = true;
        StartCoroutine (AntiSpam ());
	}

	IEnumerator AntiSpam()
	{
		antiSpam = true;
		yield return new WaitForSeconds (spamWaitTime);
		antiSpam = false;
	}
}