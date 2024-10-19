using UnityEngine;

public class BreakpointToggle : MonoBehaviour
{
    private static bool _isReady;

    private void Awake()
    {
        _isReady = false;
        DebugHUD.AddString(IsBreakpointReady);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            _isReady = !_isReady;
        }
    }

    private void OnDestroy()
    {
        DebugHUD.RemoveString(IsBreakpointReady);
    }

    public static void TryBreak()
    {
        if (_isReady)
        {
            Debug.Break();
        }
    }

    private string IsBreakpointReady()
    {
        return _isReady ? "Breakpoint Ready" : "Breakpoint Not Ready";
    }
}