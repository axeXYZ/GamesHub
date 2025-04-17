// Engine/Models/KeyState.cs
// (Aucun changement nécessaire - Le code existant est bon)
namespace InputManager.Inputs;

/// <summary>
/// Représente l'état interne simplifié (IsDown) d'une touche.
/// </summary>
internal class KeyState
{
    /// <summary>
    /// Obtient ou définit (en interne) si la touche est considérée comme enfoncée.
    /// </summary>
    public bool IsDown { get; internal set; } = false;
}
