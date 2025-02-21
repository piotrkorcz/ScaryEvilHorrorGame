using UnityEngine;

public enum m_lampType { Static, Normal, Flickering }

public class Lamp : MonoBehaviour {

    public m_lampType LampType = m_lampType.Normal;
	public Light switchLight;
    public Animation animationObject;
	public bool LocalEmission;
	public bool isOn;

	[Header("Audio")]
	public AudioClip SwitchOn;
	public AudioClip SwitchOff;

	[HideInInspector]
	public bool canSwitchOn = true;

    void Start()
    {
        if (LampType == m_lampType.Flickering)
        {
            if (isOn)
            {
                animationObject.wrapMode = WrapMode.Loop;
                animationObject.Play();
            }
        }

        if (isOn)
        {
            switchLight.enabled = true;
        }
        else
        {
            switchLight.enabled = false;
        }
    }

    void Update()
    {
        if (LampType == m_lampType.Static) return;

        if (!LocalEmission)
        {
            if (switchLight.enabled)
            {
                switchLight.transform.parent.gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(1f, 1f, 1f));
            }
            else
            {
                switchLight.transform.parent.gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0f, 0f, 0f));
            }
        }
        else
        {
            if (switchLight.enabled)
            {
                GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(1f, 1f, 1f));
            }
            else
            {
                GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0f, 0f, 0f));
            }
        }
    }

    public void UseObject()
	{
		if (!isOn) {
            SwitchLamp(true);
        } else {
            SwitchLamp(false);
        }
	}

    public void SwitchLamp(bool LampState)
    {
        if (LampState)
        {
            if (LampType == m_lampType.Flickering)
            {
                animationObject.wrapMode = WrapMode.Loop;
                animationObject.Play();
            }

            switchLight.enabled = true;

            if (SwitchOn) { AudioSource.PlayClipAtPoint(SwitchOn, transform.position, 0.75f); }
            isOn = true;
        }
        else
        {
            if (LampType == m_lampType.Flickering)
            {
                animationObject.Stop();
            }

            switchLight.enabled = false;

            if (SwitchOff) { AudioSource.PlayClipAtPoint(SwitchOff, transform.position, 0.75f); }
            isOn = false;
        }
    }

    public void OnLoad()
    {
        if (LampType == m_lampType.Flickering)
        {
            if (isOn)
            {
                animationObject.wrapMode = WrapMode.Loop;
                animationObject.Play();
            }
        }
    }
}
