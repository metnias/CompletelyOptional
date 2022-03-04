using UnityEngine;

namespace OptionalUI
{
    public interface ICanBeFocused
    {
        /// <summary>
        /// Whether this is focused
        /// </summary>
        bool Focused { get; set; }

        /// <summary>
        /// Whether its change is prevented
        /// </summary>
        bool GreyedOut { get; }

        /// <summary>
        /// How the Focus Pointer should surround this. Left, Bottom(NOT Top), Width, Height.
        /// <code>Rect ICanBeFocused.FocusRect => new Rect(pos.x, pos.y, size.x, size.y);</code>
        /// </summary>
        Rect FocusRect { get; }

        /// <summary>
        /// Whether this can be focused with Mouse pointer
        /// </summary>
        bool CurrentlyFocusableMouse { get; }

        /// <summary>
        /// Whether this can be focused with Gamepad/Keyboard
        /// </summary>
        bool CurrentlyFocusableNonMouse { get; }
    }
}