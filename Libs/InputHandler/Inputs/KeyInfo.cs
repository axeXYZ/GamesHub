// Engine/Models/KeyInfo.cs
// (Aucun changement nécessaire - Le code existant est bon)
namespace InputManager.Inputs;

/// <summary>
/// Représente l'état d'une touche clavier (enfoncée et relâchée).
/// </summary>
public readonly struct KeyInfo
{
    /// <summary>
    /// Obtient une valeur indiquant si la touche est actuellement enfoncée.
    /// </summary>
    public bool IsDown { get; }

    /// <summary>
    /// Obtient une valeur indiquant si la touche est actuellement relâchée.
    /// (C'est l'inverse de IsDown).
    /// </summary>
    public bool IsUp => !IsDown;

    /// <summary>
    /// Constructeur interne utilisé par le système InputHandler.
    /// </summary>
    /// <param name="isDown">L'état actuel de la touche.</param>
    internal KeyInfo(bool isDown)
    {
        IsDown = isDown;
    }

    /// <summary>
    /// Permet une conversion implicite de KeyInfo vers bool, retournant la valeur de IsDown.
    /// </summary>
    /// <param name="keyInfo">L'instance de KeyInfo à convertir.</param>
    /// <returns>La valeur de IsDown.</returns>
    /// <example>
    /// Usage : if (InputHandler.Keys.W) { ... } // Vérifie IsDown
    /// </example>
    public static implicit operator bool(KeyInfo keyInfo) => keyInfo.IsDown;
}
