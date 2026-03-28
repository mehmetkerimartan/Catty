using UnityEngine;

/// <summary>
/// Global static class to hold mobile inputs.
/// </summary>
public static class MobileInput
{
    public static Vector2 Joystick = Vector2.zero;
    
    // State bools
    public static bool DashHeld = false;
    public static bool TearHeld = false;
    
    // Trigger bools
    public static bool JumpPressed = false;

    /// <summary>
    /// Call this instead of checking JumpPressed directly to consume the event just like GetKeyDown.
    /// </summary>
    public static bool ConsumeJump()
    {
        if (JumpPressed)
        {
            JumpPressed = false;
            return true;
        }
        return false;
    }
}
