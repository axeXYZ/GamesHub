using InputManager.Services;

namespace InputManager.Inputs;

/// <summary>
/// Fournit un accès basé sur des propriétés à l'état (Down/Up) des touches courantes.
/// Instance accessible via <see cref="InputHandler.Keys"/>.
/// Usage: InputHandler.Keys.W.IsDown, InputHandler.Keys.W.IsUp
/// Raccourci: if(InputHandler.Keys.W) // vérifie IsDown
/// </summary>
/// <remarks>
/// Cette classe dépend de <see cref="InputHandler"/> pour récupérer l'état actuel.
/// Seules les touches les plus courantes sont exposées ici pour la commodité.
/// </remarks>
public sealed class KeyStateAccess
{
    private readonly Func<string, bool> _getStateFunc;

    internal KeyStateAccess(Func<string, bool> getStateFunc)
    {
        _getStateFunc = getStateFunc ?? throw new ArgumentNullException(nameof(getStateFunc));
    }


    // --- Lettres ---
    public KeyInfo A => new KeyInfo(_getStateFunc("KeyA"));
    public KeyInfo B => new KeyInfo(_getStateFunc("KeyB"));
    public KeyInfo C => new KeyInfo(_getStateFunc("KeyC"));
    public KeyInfo D => new KeyInfo(_getStateFunc("KeyD"));
    public KeyInfo E => new KeyInfo(_getStateFunc("KeyE"));
    public KeyInfo F => new KeyInfo(_getStateFunc("KeyF"));
    public KeyInfo G => new KeyInfo(_getStateFunc("KeyG"));
    public KeyInfo H => new KeyInfo(_getStateFunc("KeyH"));
    public KeyInfo I => new KeyInfo(_getStateFunc("KeyI"));
    public KeyInfo J => new KeyInfo(_getStateFunc("KeyJ"));
    public KeyInfo K => new KeyInfo(_getStateFunc("KeyK"));
    public KeyInfo L => new KeyInfo(_getStateFunc("KeyL"));
    public KeyInfo M => new KeyInfo(_getStateFunc("KeyM"));
    public KeyInfo N => new KeyInfo(_getStateFunc("KeyN"));
    public KeyInfo O => new KeyInfo(_getStateFunc("KeyO"));
    public KeyInfo P => new KeyInfo(_getStateFunc("KeyP"));
    public KeyInfo Q => new KeyInfo(_getStateFunc("KeyQ"));
    public KeyInfo R => new KeyInfo(_getStateFunc("KeyR"));
    public KeyInfo S => new KeyInfo(_getStateFunc("KeyS"));
    public KeyInfo T => new KeyInfo(_getStateFunc("KeyT"));
    public KeyInfo U => new KeyInfo(_getStateFunc("KeyU"));
    public KeyInfo V => new KeyInfo(_getStateFunc("KeyV"));
    public KeyInfo W => new KeyInfo(_getStateFunc("KeyW"));
    public KeyInfo X => new KeyInfo(_getStateFunc("KeyX"));
    public KeyInfo Y => new KeyInfo(_getStateFunc("KeyY"));
    public KeyInfo Z => new KeyInfo(_getStateFunc("KeyZ"));

    // --- Chiffres (ligne supérieure) ---
    public KeyInfo Digit0 => new KeyInfo(_getStateFunc("Digit0"));
    public KeyInfo Digit1 => new KeyInfo(_getStateFunc("Digit1"));
    public KeyInfo Digit2 => new KeyInfo(_getStateFunc("Digit2"));
    public KeyInfo Digit3 => new KeyInfo(_getStateFunc("Digit3"));
    public KeyInfo Digit4 => new KeyInfo(_getStateFunc("Digit4"));
    public KeyInfo Digit5 => new KeyInfo(_getStateFunc("Digit5"));
    public KeyInfo Digit6 => new KeyInfo(_getStateFunc("Digit6"));
    public KeyInfo Digit7 => new KeyInfo(_getStateFunc("Digit7"));
    public KeyInfo Digit8 => new KeyInfo(_getStateFunc("Digit8"));
    public KeyInfo Digit9 => new KeyInfo(_getStateFunc("Digit9"));

    // --- Touches spéciales ---
    public KeyInfo Space => new KeyInfo(_getStateFunc("Space"));
    public KeyInfo Enter => new KeyInfo(_getStateFunc("Enter"));
    public KeyInfo Escape => new KeyInfo(_getStateFunc("Escape"));
    public KeyInfo Tab => new KeyInfo(_getStateFunc("Tab"));
    public KeyInfo Backspace => new KeyInfo(_getStateFunc("Backspace"));
    public KeyInfo Delete => new KeyInfo(_getStateFunc("Delete"));

    // --- Modificateurs ---
    public KeyInfo ShiftLeft => new KeyInfo(_getStateFunc("ShiftLeft"));
    public KeyInfo ShiftRight => new KeyInfo(_getStateFunc("ShiftRight"));
    public KeyInfo ControlLeft => new KeyInfo(_getStateFunc("ControlLeft"));
    public KeyInfo ControlRight => new KeyInfo(_getStateFunc("ControlRight"));
    public KeyInfo AltLeft => new KeyInfo(_getStateFunc("AltLeft"));
    public KeyInfo AltRight => new KeyInfo(_getStateFunc("AltRight"));
    public KeyInfo MetaLeft => new KeyInfo(_getStateFunc("MetaLeft")); // Cmd/Win
    public KeyInfo MetaRight => new KeyInfo(_getStateFunc("MetaRight")); // Cmd/Win

    // --- Flèches ---
    public KeyInfo ArrowUp => new KeyInfo(_getStateFunc("ArrowUp"));
    public KeyInfo ArrowDown => new KeyInfo(_getStateFunc("ArrowDown"));
    public KeyInfo ArrowLeft => new KeyInfo(_getStateFunc("ArrowLeft"));
    public KeyInfo ArrowRight => new KeyInfo(_getStateFunc("ArrowRight"));

    // Ajoutez F1-F12, Numpad, etc. ici si nécessaire...
}