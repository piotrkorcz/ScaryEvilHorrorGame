using UnityEngine;
using System.Collections;

public class DelayEffect : MonoBehaviour
{
    public float amount = 0.02f;
    public float maxAmount = 0.03f;
    public float smooth = 3;
    private Vector3 def;

	[HideInInspector]
	public bool isEnabled;

    void Start()
    {
        isEnabled = true;
        def = transform.localPosition;
    }

    void Update()
    {
			if (ControlFreak2.CFCursor.lockState == CursorLockMode.None)
				return;

			float factorX = -ControlFreak2.CF2Input.GetAxis ("Mouse X") * amount;
			float factorY = -ControlFreak2.CF2Input.GetAxis ("Mouse Y") * amount;

			if (factorX > maxAmount)
				factorX = maxAmount;

			if (factorX < -maxAmount)
				factorX = -maxAmount;

			if (factorY > maxAmount)
				factorY = maxAmount;

			if (factorY < -maxAmount)
				factorY = -maxAmount;
		
		if (isEnabled) {
			Vector3 Final = new Vector3 (def.x + factorX, def.y + factorY, def.z);
			transform.localPosition = Vector3.Lerp (transform.localPosition, Final, Time.deltaTime * smooth);
		}
    }
}