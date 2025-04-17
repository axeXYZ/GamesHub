namespace InputManager.Inputs;

/// <summary>
/// Fournit un accès aux événements KeyDown ou KeyUp pour les touches courantes.
/// Instances accessibles via InputHandler.OnKeyDown et InputHandler.OnKeyUp.
/// </summary>
public sealed class KeyEventAccess
{
    /// <summary>
    /// Constructeur interne pour garantir l'instanciation unique via <see cref="InputHandler"/>.
    /// </summary>
    internal KeyEventAccess() { }

    // --- Lettres ---
    public event Action? A; internal void TriggerA() => A?.Invoke();
    public event Action? B; internal void TriggerB() => B?.Invoke();
    public event Action? C; internal void TriggerC() => C?.Invoke();
    public event Action? D; internal void TriggerD() => D?.Invoke();
    public event Action? E; internal void TriggerE() => E?.Invoke();
    public event Action? F; internal void TriggerF() => F?.Invoke();
    public event Action? G; internal void TriggerG() => G?.Invoke();
    public event Action? H; internal void TriggerH() => H?.Invoke();
    public event Action? I; internal void TriggerI() => I?.Invoke();
    public event Action? J; internal void TriggerJ() => J?.Invoke();
    public event Action? K; internal void TriggerK() => K?.Invoke();
    public event Action? L; internal void TriggerL() => L?.Invoke();
    public event Action? M; internal void TriggerM() => M?.Invoke();
    public event Action? N; internal void TriggerN() => N?.Invoke();
    public event Action? O; internal void TriggerO() => O?.Invoke();
    public event Action? P; internal void TriggerP() => P?.Invoke();
    public event Action? Q; internal void TriggerQ() => Q?.Invoke();
    public event Action? R; internal void TriggerR() => R?.Invoke();
    public event Action? S; internal void TriggerS() => S?.Invoke();
    public event Action? T; internal void TriggerT() => T?.Invoke();
    public event Action? U; internal void TriggerU() => U?.Invoke();
    public event Action? V; internal void TriggerV() => V?.Invoke();
    public event Action? W; internal void TriggerW() => W?.Invoke();
    public event Action? X; internal void TriggerX() => X?.Invoke();
    public event Action? Y; internal void TriggerY() => Y?.Invoke();
    public event Action? Z; internal void TriggerZ() => Z?.Invoke();

    // --- Chiffres ---
    public event Action? Digit0; internal void TriggerDigit0() => Digit0?.Invoke();
    public event Action? Digit1; internal void TriggerDigit1() => Digit1?.Invoke();
    public event Action? Digit2; internal void TriggerDigit2() => Digit2?.Invoke();
    public event Action? Digit3; internal void TriggerDigit3() => Digit3?.Invoke();
    public event Action? Digit4; internal void TriggerDigit4() => Digit4?.Invoke();
    public event Action? Digit5; internal void TriggerDigit5() => Digit5?.Invoke();
    public event Action? Digit6; internal void TriggerDigit6() => Digit6?.Invoke();
    public event Action? Digit7; internal void TriggerDigit7() => Digit7?.Invoke();
    public event Action? Digit8; internal void TriggerDigit8() => Digit8?.Invoke();
    public event Action? Digit9; internal void TriggerDigit9() => Digit9?.Invoke();

    // --- Touches spéciales ---
    public event Action? Space; internal void TriggerSpace() => Space?.Invoke();
    public event Action? Enter; internal void TriggerEnter() => Enter?.Invoke();
    public event Action? Escape; internal void TriggerEscape() => Escape?.Invoke();
    public event Action? Tab; internal void TriggerTab() => Tab?.Invoke();
    public event Action? Backspace; internal void TriggerBackspace() => Backspace?.Invoke();
    public event Action? Delete; internal void TriggerDelete() => Delete?.Invoke();

    // --- Modificateurs ---
    public event Action? ShiftLeft; internal void TriggerShiftLeft() => ShiftLeft?.Invoke();
    public event Action? ShiftRight; internal void TriggerShiftRight() => ShiftRight?.Invoke();
    public event Action? ControlLeft; internal void TriggerControlLeft() => ControlLeft?.Invoke();
    public event Action? ControlRight; internal void TriggerControlRight() => ControlRight?.Invoke();
    public event Action? AltLeft; internal void TriggerAltLeft() => AltLeft?.Invoke();
    public event Action? AltRight; internal void TriggerAltRight() => AltRight?.Invoke();
    public event Action? MetaLeft; internal void TriggerMetaLeft() => MetaLeft?.Invoke();
    public event Action? MetaRight; internal void TriggerMetaRight() => MetaRight?.Invoke();

    // --- Flèches ---
    public event Action? ArrowUp; internal void TriggerArrowUp() => ArrowUp?.Invoke();
    public event Action? ArrowDown; internal void TriggerArrowDown() => ArrowDown?.Invoke();
    public event Action? ArrowLeft; internal void TriggerArrowLeft() => ArrowLeft?.Invoke();
    public event Action? ArrowRight; internal void TriggerArrowRight() => ArrowRight?.Invoke();


    /// <summary>
    /// Logue une erreur survenue dans un gestionnaire d'événement utilisateur.
    /// </summary>
    private static void LogHandlerError(string keyName, Exception exception)
    {
        // Utiliser ILogger si disponible, sinon Console.WriteLine conditionnel
#if DEBUG
        Console.WriteLine($"Error in user handler for key '{keyName}': {exception.Message}");
        // Pourrait logguer l'exception complète: Console.WriteLine(exception);
#endif
        // En production, logger via ILogger: _logger.LogError(exception, "Error in user handler for key {KeyName}", keyName);
    }
}
