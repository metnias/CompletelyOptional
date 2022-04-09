namespace OptionalUI
{
    /// <summary>
    /// Event handler for <see cref="UIelement.OnChange"/>
    /// </summary>
    public delegate void OnChangeHandler();

    /// <summary>
    /// Event handler for <see cref="UIfocusable.OnHeld"/>.
    /// </summary>
    /// <param name="held">New <see cref="UIfocusable.held"/> value</param>
    public delegate void OnHeldHandler(bool held);

    /// <summary>
    /// Event handler for generic Signals
    /// </summary>
    /// <param name="trigger"><see cref="UIfocusable"/> instance that triggered event</param>
    public delegate void OnSignalHandler(UIfocusable trigger);

    /// <summary>
    /// Event handler for <see cref="UIconfig.OnValueChange"/>.
    /// </summary>
    /// <param name="config"><see cref="UIconfig"/> instance</param>
    /// <param name="value">New value</param>
    /// <param name="oldValue">Old value before the change</param>
    public delegate void OnValueChangeHandler(UIconfig config, string value, string oldValue);
}