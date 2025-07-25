﻿using UnityEngine;
using System.Collections;

public class PlayerFunctions : MonoBehaviour {

    private InputManager inputManager;

	[Header("Player Lean")]
	public LayerMask LeanMask;
	public float LeanAngle;
	public float LeanPos;
	public float LeanMaxRay;
	public float LeanSpeed;
	public float LeanBackSpeed;

    [Header("Zoom Effects")]
	public Camera WeaponCamera;
    public float ZoomSpeed = 5f;
    public float NormalFOV;
    public float ZoomFOV;

    private KeyCode ZoomKey = KeyCode.Mouse1;
	private KeyCode LeanRight;
	private KeyCode LeanLeft;

    private Camera MainCamera;

    private bool isSet;

	void SetKeys()
	{
		LeanLeft = (KeyCode)System.Enum.Parse(typeof(KeyCode), inputManager.GetInput("LeanLeft"));
		LeanRight = (KeyCode)System.Enum.Parse(typeof(KeyCode), inputManager.GetInput("LeanRight"));
		isSet = true;
	}

	void Start () {
        MainCamera = Camera.main;
        inputManager = GetComponent<ScriptManager>().inputManager;
    }

    void Update()
    {
        if (inputManager.InputsCount() > 0 && !isSet)
        {
            SetKeys();
        }

        if (inputManager.GetRefreshStatus() && isSet)
        {
            isSet = false;
        }

        TryLean(1);
        TryLean(2);

        if (ControlFreak2.CF2Input.GetKey(ZoomKey))
        {
            MainCamera.fieldOfView = Mathf.Lerp(MainCamera.fieldOfView, ZoomFOV, ZoomSpeed * Time.deltaTime);
            if (WeaponCamera)
            {
                WeaponCamera.fieldOfView = Mathf.Lerp(MainCamera.fieldOfView, ZoomFOV, ZoomSpeed * Time.deltaTime);
            }
        }
        else
        {
            MainCamera.fieldOfView = Mathf.Lerp(MainCamera.fieldOfView, NormalFOV, ZoomSpeed * Time.deltaTime);
            if (WeaponCamera)
            {
                WeaponCamera.fieldOfView = Mathf.Lerp(MainCamera.fieldOfView, NormalFOV, ZoomSpeed * Time.deltaTime);
            }
        }
    }

    void Lean(int Direction)
    {
        switch (Direction)
        {
            case 0:
                MainCamera.transform.localRotation = Quaternion.Slerp(MainCamera.transform.localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * LeanBackSpeed);
                MainCamera.transform.localPosition = Vector3.Lerp(MainCamera.transform.localPosition, new Vector3(0, 0, 0), Time.deltaTime * LeanSpeed);
                break;
            case 1:
                float leanAngle = -LeanAngle;
                MainCamera.transform.localRotation = Quaternion.Slerp(MainCamera.transform.localRotation, Quaternion.Euler(0, 0, leanAngle), Time.deltaTime * LeanSpeed);
                MainCamera.transform.localPosition = Vector3.Lerp(MainCamera.transform.localPosition, new Vector3(LeanPos, 0, 0), Time.deltaTime * LeanSpeed);
                break;
            case 2:
                float leanPos = -LeanPos;
                MainCamera.transform.localRotation = Quaternion.Slerp(MainCamera.transform.localRotation, Quaternion.Euler(0, 0, LeanAngle), Time.deltaTime * LeanSpeed);
                MainCamera.transform.localPosition = Vector3.Lerp(MainCamera.transform.localPosition, new Vector3(leanPos, 0, 0), Time.deltaTime * LeanSpeed);
                break;
        }
    }

	void TryLean(int dir)
    {
        RaycastHit raycastHit;
        switch (dir)
        {
            case 1:
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.right * 1f), out raycastHit, LeanMaxRay, LeanMask)){
                    Lean(0);
                }else if (ControlFreak2.CF2Input.GetKey(LeanRight)){
                    Lean(1);
                }else{
                    Lean(0);
                }
                break;
            case 2:
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.left * 1f), out raycastHit, LeanMaxRay, LeanMask)){
                    Lean(0);
                }else if (ControlFreak2.CF2Input.GetKey(LeanLeft)){
                    Lean(2);
                }else{
                    Lean(0);
                }
                break;
        }
    }
}