using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleItem : MonoBehaviour {

	[Header("Setup")]
	public ScriptManager scriptManager;
	private InputManager inputManager;

	[Header("Candle Others")]
	public int InventoyID;
	public AudioClip BlowOut;
    public GameObject Candle;
    public GameObject CandleFlame;
	public GameObject CandleLight;
    public Transform FlamePosition;

	[Header("Candle Animations")]
	public GameObject CandleGO;
	public string DrawAnimation;
	public string HideAnimation;
    public string BlowOutAnimation;
    public string IdleAnimation;

	public float DrawSpeed = 1f;
	public float HideSpeed = 1f;

    [Header("Candle Settings")]
    public bool candleReduction;
    public float reductionRate;
    public float minScale;

    private KeyCode BlowOutKey;

    private bool isSelected;
    private bool IsPressed;

	void Start () {
		inputManager = scriptManager.inputManager;
        if (isSelected)
        {
            Select();
        }
    }

	public void Select() {
		isSelected = true;
		CandleGO.SetActive (true);
		CandleGO.GetComponent<Animation>().Play(DrawAnimation);
        CandleFlame.SetActive (true);
		CandleLight.SetActive (true);
        if (candleReduction)
        {
            StartCoroutine(Scale());
        }
	}

	public void Deselect()
	{
		if (CandleGO.activeSelf) {
			CandleGO.GetComponent<Animation>().Play(HideAnimation);
            StopAllCoroutines();
			IsPressed = true;
		}
	}

    public void LoaderSetItemEnabled()
    {
        isSelected = true;
        CandleGO.SetActive(true);
        CandleGO.GetComponent<Animation>().Play(IdleAnimation);
        CandleFlame.SetActive(true);
        CandleLight.SetActive(true);
        if (candleReduction)
        {
            StartCoroutine(Scale());
        }
    }

    public void BlowOut_Event()
	{
		AudioSource.PlayClipAtPoint (BlowOut, Camera.main.transform.position, 0.35f);
		CandleFlame.SetActive (false);
		CandleLight.SetActive (false);
	}

	void Update () {
		if(inputManager.InputsCount() > 0)
		{
			BlowOutKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), inputManager.GetInput("Flashlight"));
		}

        CandleFlame.transform.position = FlamePosition.position;

        if (ControlFreak2.CF2Input.GetKeyDown (BlowOutKey) && !IsPressed && isSelected && !(CandleGO.GetComponent<Animation> ().isPlaying)) {
			CandleGO.GetComponent<Animation>().Play(BlowOutAnimation);
            StopAllCoroutines();
			IsPressed = true;
		}

		if (IsPressed && !(CandleGO.GetComponent<Animation> ().isPlaying)) {
			CandleGO.SetActive (false);
			IsPressed = false;
		}
	}

    IEnumerator Scale()
    {
        while (minScale <= Candle.transform.localScale.y)
        {
            Vector3 temp = Candle.transform.localScale;
            temp.y -= temp.y * Time.deltaTime * reductionRate;
            Candle.transform.localScale = temp;
            yield return null;
        }

        FlameBurnOut();

        yield return new WaitForSeconds(1f);

        Deselect();
    }

    void FlameBurnOut()
    {
        CandleFlame.SetActive(false);
        CandleLight.SetActive(false);
    }

    public void SendValues()
    {
        if (GetComponent<SaveHelper>())
        {
            GetComponent<SaveHelper>().SetValues(
                new List<string>() {
                    Candle.transform.localScale.y.ToString()
                });
        }
    }

    public void SetSavedValues(List<string> values)
    {
        Vector3 scale = Candle.transform.localScale;
        scale.y = float.Parse(values[0]);
        Candle.transform.localScale = scale;
    }
}
