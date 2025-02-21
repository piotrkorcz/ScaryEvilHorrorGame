using System;
using UnityEngine;

namespace ThunderWire.Parser
{
    public class ParsePrimitives
    {
        public T ParseType<T>(string value)
        {
            return (T)ChangeType(typeof(T), value);
        }

        public object ChangeType(Type type, string value)
        {
            object ret = null;
            if (type == typeof(int)) ret = int.Parse(value);
            if (type == typeof(uint)) ret = uint.Parse(value);
            if (type == typeof(long)) ret = long.Parse(value);
            if (type == typeof(ulong)) ret = ulong.Parse(value);
            if (type == typeof(float)) ret = float.Parse(value);
            if (type == typeof(double)) ret = double.Parse(value);
            if (type == typeof(bool)) ret = bool.Parse(value);
            if (type == typeof(char)) ret = char.Parse(value);
            if (type == typeof(short)) ret = short.Parse(value);
            if (type == typeof(byte)) ret = byte.Parse(value);
            if (type == typeof(Color)) { Color newColor; ColorUtility.TryParseHtmlString(value, out newColor); ret = newColor; }
            if (type == typeof(KeyCode)) { ret = (KeyCode)Enum.Parse(typeof(KeyCode), value); }
            return ret;
        }

        public Vector2 ParseVector2(string x, string y)
        {
            return new Vector2(float.Parse(x), float.Parse(y));
        }

        public Vector3 ParseVector3(string x, string y, string z)
        {
            return new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
        }

        public Vector4 ParseVector4(string x, string y, string z, string w)
        {
            return new Vector4(float.Parse(x), float.Parse(y), float.Parse(z), float.Parse(w));
        }

        public Quaternion ParseQuaternion(string x, string y, string z, string w)
        {
            return new Quaternion(float.Parse(x), float.Parse(y), float.Parse(z), float.Parse(w));
        }
    }
}
