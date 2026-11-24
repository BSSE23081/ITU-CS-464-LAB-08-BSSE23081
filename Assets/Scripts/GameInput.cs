using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

// Works with both the old Input Manager and the new Input System package.
public static class GameInput
{
    public static bool JumpPressed()
    {
        bool pressed;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        pressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
               || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
               || (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);
#else
        pressed = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
#endif
        // a tap on a UI button (like the pause button) must not also make the player jump
        if (pressed && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return false;
        return pressed;
    }

    public static bool BackPressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
