using UnityEngine;
using ThunderWire.Parser;

public class ExampleInputManager : MonoBehaviour {
    private ParsePrimitives parser = new ParsePrimitives();

    public InputManager inputManager;

    private KeyCode useKey;
    private bool isSet = false;
    private bool isPressed = false;

    void Update()
    {
        if (inputManager.InputsCount() > 0 && !isSet)
        {
            useKey = parser.ParseType<KeyCode>(inputManager.GetInput("Use"));
            isSet = true;
        }

        if (inputManager.GetRefreshStatus() && isSet)
        {
            isSet = false;
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
