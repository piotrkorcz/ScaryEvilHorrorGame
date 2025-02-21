using UnityEngine;
using ThunderWire.Parser;

public class ExampleInput : MonoBehaviour
{
    private ParsePrimitives parser = new ParsePrimitives();

    public ConfigActionManager configActions;

    private KeyCode useKey;
    private bool isSet = false;
    private bool isPressed = false;

    void Update()
    {
        if (configActions.GetKeysCount() > 0 && !isSet)
        {
            useKey = parser.ParseType<KeyCode>(configActions.Deserialize("Input", "Use"));
            isSet = true;
        }

        if (ControlFreak2.CF2Input.GetKeyDown(useKey) && !isPressed)
        {
            Debug.Log("Use Key Pressed!");
            isPressed = true;
        }
        else if (isPressed)
        {
            isPressed = false;
        }
    }
}