using InputManager.Inputs;
using Microsoft.JSInterop;
using System.Collections.Concurrent;

namespace InputManager.Services;


/// <summary>
/// Définit l'interface pour le gestionnaire d'entrées clavier.
/// Fournit l'accès à l'état des touches et aux événements KeyDown/KeyUp.
/// Doit être libéré (Dispose) lorsqu'il n'est plus utilisé, idéalement via DI.
/// </summary>
public interface IInputHandler : IAsyncDisposable // IDisposable est approprié ici
{
    /// <summary>
    /// Obtient un objet permettant de vérifier l'état actuel (Down/Up) des touches courantes.
    /// Usage : Keys.W.IsDown, Keys.Space.IsUp
    /// </summary>
    KeyStateAccess Keys { get; }

    /// <summary>
    /// Obtient un objet permettant de s'abonner aux événements KeyDown des touches courantes.
    /// Usage : OnKeyDown.W += MyHandler;
    /// </summary>
    KeyEventAccess OnKeyDown { get; }

    /// <summary>
    /// Obtient un objet permettant de s'abonner aux événements KeyUp des touches courantes.
    /// Usage : OnKeyUp.W += MyHandler;
    /// </summary>
    KeyEventAccess OnKeyUp { get; }

    Task InitializeAsync(); // Méthode pour démarrer l'écoute
}

/// <summary>
/// Gère et centralise l'état et les événements des touches du clavier pour une instance.
/// Fournit un accès SANS STRING à l'état via la propriété 'Keys' (ex: instance.Keys.W)
/// et aux événements via 'OnKeyDown'/'OnKeyUp' (ex: instance.OnKeyDown.W += handler).
/// Implémente IDisposable pour assurer le nettoyage de l'état des touches (via ClearAllKeys).
/// </summary>
public sealed class InputHandler : IInputHandler
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<InputHandler>? _dotNetObjectReference;

    // Stockage interne de l'état simple (juste si IsDown)
    private readonly ConcurrentDictionary<string, KeyState> _keyStates = new ConcurrentDictionary<string, KeyState>();

    // Singletons pour l'accès externe
    private readonly Lazy<KeyStateAccess> _lazyKeys;
    private readonly Lazy<KeyEventAccess> _lazyOnKeyDown = new Lazy<KeyEventAccess>(() => new KeyEventAccess());
    private readonly Lazy<KeyEventAccess> _lazyOnKeyUp = new Lazy<KeyEventAccess>(() => new KeyEventAccess());

    private bool _isInitialized = false;
    private bool _disposed = false;

    /// <summary>
    /// Permet de vérifier l'état actuel des touches courantes (ex: InputHandler.Keys.W).
    /// </summary>
    public KeyStateAccess Keys => _lazyKeys.Value;

    /// <summary>
    /// Permet de s'abonner aux événements KeyDown des touches courantes (ex: InputHandler.OnKeyDown.W += MyHandler).
    /// </summary>
    public KeyEventAccess OnKeyDown => _lazyOnKeyDown.Value;

    /// <summary>
    /// Permet de s'abonner aux événements KeyUp des touches courantes (ex: InputHandler.OnKeyUp.W += MyHandler).
    /// </summary>
    public KeyEventAccess OnKeyUp => _lazyOnKeyUp.Value;


    public InputHandler(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _lazyKeys = new Lazy<KeyStateAccess>(() => new KeyStateAccess(this.GetKeyState));
        // La référence .NET est créée ici pour être passée à JS
        _dotNetObjectReference = DotNetObjectReference.Create(this);
        Console.WriteLine($"InputHandler Singleton Instance Created (HashCode: {this.GetHashCode()})");
    }
    public async Task InitializeAsync()
    {
        if (_isInitialized || _disposed) return;
        try
        {
            // Log avant l'appel JS
            Console.WriteLine($"InputHandler: Attempting to call inputManagerGlobal.addListeners...");
            await _jsRuntime.InvokeVoidAsync("inputManagerGlobal.addListeners", _dotNetObjectReference);
            _isInitialized = true;
            Console.WriteLine($"InputHandler: Global Listeners Initialized Successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InputHandler: ERROR initializing global listeners: {ex.Message}");
            // Propager ou gérer l'erreur
        }
    }

    internal bool GetKeyState(string code)
    {
        // ObjectDisposedException.ThrowIf(_disposed, this); // Peut causer pb si lu après dispose partiel
        if (_disposed) return false;
        bool result = _keyStates.TryGetValue(code, out var state) && state.IsDown;
        // Log intensif pour le débogage:
        // Console.WriteLine($"GetKeyState('{code}') -> {result}");
        return result;
    }

    [JSInvokable] // Important pour l'appel depuis JS
    public void HandleJsKeyDown(string code)
    {
        if (string.IsNullOrEmpty(code) || _disposed) return;
        //Console.WriteLine($"-- C# HandleJsKeyDown received: {code}"); // <-- Log de réception C#
        var state = _keyStates.GetOrAdd(code, _ => new KeyState());
        if (!state.IsDown)
        {
            state.IsDown = true;
            //Console.WriteLine($"---- C# State Update: {code} -> IsDown = {state.IsDown}"); // <-- Log de mise à jour état
                                                                                           // Optionnel: Déclencher l'event C# global
            TriggerKeyEvent(OnKeyDown, code);
        }
    }

    [JSInvokable] // Important
    public void HandleJsKeyUp(string code)
    {
        if (string.IsNullOrEmpty(code) || _disposed) return;
        //Console.WriteLine($"-- C# HandleJsKeyUp received: {code}"); // <-- Log de réception C#
        if (_keyStates.TryGetValue(code, out var state))
        {
            if (state.IsDown)
            {
                state.IsDown = false;
                //Console.WriteLine($"---- C# State Update: {code} -> IsDown = {state.IsDown}"); // <-- Log de mise à jour état
                                                                                               // Optionnel: Déclencher l'event C# global
                TriggerKeyEvent(OnKeyUp, code);
            }
        }
    }

    /// <summary>
    /// Réinitialise l'état de toutes les touches à "relâchée" et déclenche les événements OnKeyUp correspondants.
    /// Utile typiquement lors de la perte de focus de l'élément d'input.
    /// </summary>
    private void ClearAllKeys()
    {
        // Ne rien faire si déjà libéré.
        if (_disposed) return;

#if DEBUG
        Console.WriteLine("InputHandler: Clearing all keys..."); // Log de débogage
#endif
        int clearedCount = 0;
        foreach (var kvp in _keyStates)
        {
            if (kvp.Value.IsDown)
            {
                clearedCount++;

                kvp.Value.IsDown = false;
                // Déclencher l'événement KeyUp correspondant
                TriggerKeyEvent(OnKeyUp, kvp.Key);
            }
        }
#if DEBUG
        Console.WriteLine($"InputHandler: Cleared state for {clearedCount} keys."); // Log de débogage
#endif
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            Console.WriteLine($"InputHandler Disposing (Instance: {this.GetHashCode()})...");
            _disposed = true;
            if (_isInitialized)
            {
                try
                {
                    Console.WriteLine($"InputHandler: Attempting to call inputManagerGlobal.removeListeners...");
                    await _jsRuntime.InvokeVoidAsync("inputManagerGlobal.removeListeners");
                    Console.WriteLine($"InputHandler: Global Listeners Removed Successfully.");
                }
                catch (Exception ex) { Console.WriteLine($"InputHandler: Error removing listeners: {ex.Message}"); }
            }
            _dotNetObjectReference?.Dispose();
            _keyStates.Clear();
            Console.WriteLine($"InputHandler Disposed (Instance: {this.GetHashCode()}).");
        }
    }


    /// <summary>
    /// Méthode interne pour trouver et déclencher l'événement approprié
    /// sur l'instance de KeyEventAccess fournie, basé sur le code de touche.
    /// Utilise un 'switch' pour mapper le code de touche à la méthode TriggerXXX correspondante.
    /// </summary>
    /// <param name="eventAccess">L'instance de KeyEventAccess (soit OnKeyDown.Value, soit OnKeyUp.Value).</param>
    /// <param name="code">Le code de touche (ex: "KeyW") provenant de KeyboardEventArgs.</param>
    /// <remarks>
    /// Ce switch est la partie la plus verbeuse à maintenir manuellement.
    /// Pour une couverture complète et une maintenance simplifiée, l'utilisation
    /// de Générateurs de Source (.NET 5+) serait préférable pour générer ce mapping.
    /// Les erreurs éventuelles DANS les gestionnaires d'événements utilisateur sont capturées
    /// dans les méthodes TriggerXXX de KeyEventAccess.
    /// </remarks>
    private void TriggerKeyEvent(KeyEventAccess eventAccess, string code)
    {
        // Utilisation d'un switch pour la performance (plus rapide que réflexion).
        // !! C'est la partie la plus verbeuse à maintenir manuellement !!
        // !! Un générateur de source serait idéal ici !!
        switch (code)
        {
            // Lettres
            case "KeyA": eventAccess.TriggerA(); break;
            case "KeyB": eventAccess.TriggerB(); break;
            case "KeyC": eventAccess.TriggerC(); break;
            case "KeyD": eventAccess.TriggerD(); break;
            case "KeyE": eventAccess.TriggerE(); break;
            case "KeyF": eventAccess.TriggerF(); break;
            case "KeyG": eventAccess.TriggerG(); break;
            case "KeyH": eventAccess.TriggerH(); break;
            case "KeyI": eventAccess.TriggerI(); break;
            case "KeyJ": eventAccess.TriggerJ(); break;
            case "KeyK": eventAccess.TriggerK(); break;
            case "KeyL": eventAccess.TriggerL(); break;
            case "KeyM": eventAccess.TriggerM(); break;
            case "KeyN": eventAccess.TriggerN(); break;
            case "KeyO": eventAccess.TriggerO(); break;
            case "KeyP": eventAccess.TriggerP(); break;
            case "KeyQ": eventAccess.TriggerQ(); break;
            case "KeyR": eventAccess.TriggerR(); break;
            case "KeyS": eventAccess.TriggerS(); break;
            case "KeyT": eventAccess.TriggerT(); break;
            case "KeyU": eventAccess.TriggerU(); break;
            case "KeyV": eventAccess.TriggerV(); break;
            case "KeyW": eventAccess.TriggerW(); break;
            case "KeyX": eventAccess.TriggerX(); break;
            case "KeyY": eventAccess.TriggerY(); break;
            case "KeyZ": eventAccess.TriggerZ(); break;

            // Chiffres
            case "Digit0": eventAccess.TriggerDigit0(); break;
            case "Digit1": eventAccess.TriggerDigit1(); break;
            case "Digit2": eventAccess.TriggerDigit2(); break;
            case "Digit3": eventAccess.TriggerDigit3(); break;
            case "Digit4": eventAccess.TriggerDigit4(); break;
            case "Digit5": eventAccess.TriggerDigit5(); break;
            case "Digit6": eventAccess.TriggerDigit6(); break;
            case "Digit7": eventAccess.TriggerDigit7(); break;
            case "Digit8": eventAccess.TriggerDigit8(); break;
            case "Digit9": eventAccess.TriggerDigit9(); break;

            // Spéciales
            case "Space": eventAccess.TriggerSpace(); break;
            case "Enter": eventAccess.TriggerEnter(); break;
            case "Escape": eventAccess.TriggerEscape(); break;
            case "Tab": eventAccess.TriggerTab(); break;
            case "Backspace": eventAccess.TriggerBackspace(); break;
            case "Delete": eventAccess.TriggerDelete(); break;

            // Modificateurs
            case "ShiftLeft": eventAccess.TriggerShiftLeft(); break;
            case "ShiftRight": eventAccess.TriggerShiftRight(); break;
            case "ControlLeft": eventAccess.TriggerControlLeft(); break;
            case "ControlRight": eventAccess.TriggerControlRight(); break;
            case "AltLeft": eventAccess.TriggerAltLeft(); break;
            case "AltRight": eventAccess.TriggerAltRight(); break;
            case "MetaLeft": eventAccess.TriggerMetaLeft(); break;
            case "MetaRight": eventAccess.TriggerMetaRight(); break;

            // Flèches
            case "ArrowUp": eventAccess.TriggerArrowUp(); break;
            case "ArrowDown": eventAccess.TriggerArrowDown(); break;
            case "ArrowLeft": eventAccess.TriggerArrowLeft(); break;
            case "ArrowRight": eventAccess.TriggerArrowRight(); break;

            default:
#if DEBUG
                // Optionnel: Logguer les touches non gérées seulement en Debug
                // System.Diagnostics.Debug.WriteLine($"Unhandled key code in TriggerKeyEvent: {code}");
#endif
                break;
        }
    }
}