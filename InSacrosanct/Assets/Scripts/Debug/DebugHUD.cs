using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugHUD : MonoBehaviour
{
    private static readonly List<Func<string>> _strings = new();

    private void OnDestroy()
    {
        _strings.Clear();
    }

    private void OnGUI()
    {
        var y = 10;
        for (var i = 0; i < _strings.Count; i++)
        {
            string str = _strings[i]();
            int h = 20 * str.Split('\n').Length;
            GUI.Label(new Rect(10, y, 400, h), str);
            y += h;
        }
    }

    private void OnPostRender()
    {
        _strings.Clear();
    }

    public static void AddString(Func<string> text)
    {
        _strings.Add(text);
    }

    public static void RemoveString(Func<string> text)
    {
        _strings.Remove(text);
    }
}