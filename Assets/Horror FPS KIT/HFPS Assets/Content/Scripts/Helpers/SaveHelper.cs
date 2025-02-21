using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveHelper : MonoBehaviour
{
    private List<string> Values = new List<string>();

    public void SetValues(List<string> valueList)
    {
        Values = valueList;
    }

    public List<string> HandlerGetValues()
    {
        return Values;
    }

    public void CallScriptGetValues()
    {
        SendMessage("SendValues", SendMessageOptions.DontRequireReceiver);
    }

    public void LoadSavedValues(List<string> values)
    {
        SendMessage("SetSavedValues", values, SendMessageOptions.DontRequireReceiver);
    }

    public int ValuesCount()
    {
        return Values.Count;
    }
}
