using Microsoft.JSInterop;

namespace GameLoop.Services;
public interface IGameLoopService
{
    // Événement déclenché à chaque frame par requestAnimationFrame
    // Le double représente le deltaTime en secondes.
    event Action<double> TickOccurred;

    // Méthode pour s'assurer que la boucle est démarrée (si nécessaire)
    // Peut être appelée par les composants de jeu lors de leur initialisation
    Task EnsureLoopStartedAsync();

    // On pourrait aussi ajouter une méthode pour arrêter explicitement si besoin,
    // mais l'arrêt automatique basé sur les abonnements est souvent préférable.
    // Task StopLoopAsync(); 
}

public class GameLoopService : IGameLoopService, IAsyncDisposable // IDisposable est ok aussi si StopLoopAsync est synchrone
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<GameLoopService> _dotNetHelper;
    private bool _isLoopRunning = false;
    private int _subscriberCount = 0; // Pour démarrer/arrêter automatiquement

    public event Action<double> TickOccurred;

    public GameLoopService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _dotNetHelper = DotNetObjectReference.Create(this);
    }

    public async Task EnsureLoopStartedAsync()
    {
        // Démarre la boucle JS uniquement si elle ne tourne pas déjà
        if (!_isLoopRunning)
        {
            Console.WriteLine("GameLoopService: Requesting JS loop start.");
            try
            {
                // Assurez-vous que gameLoop.js est chargé (peut être fait globalement dans index.html ou _Host.cshtml)
                await _jsRuntime.InvokeVoidAsync("startGameLoop", _dotNetHelper);
                _isLoopRunning = true;
            }
            catch (JSException ex)
            {
                Console.Error.WriteLine($"Error starting JS game loop: {ex.Message}");
                // Gérer l'erreur - peut-être que le JS n'est pas chargé ?
            }
        }
    }

    // Méthode appelée par JavaScript à chaque frame
    [JSInvokable]
    public void JsGameTick(double deltaTime)
    {
        // Déclencher l'événement pour tous les abonnés (les jeux)
        TickOccurred?.Invoke(deltaTime);
    }

    // Méthode pour arrêter la boucle (appelée par Dispose ou explicitement)
    private async Task StopLoopInternalAsync()
    {
        if (_isLoopRunning)
        {
            _isLoopRunning = false; // Marquer comme arrêté immédiatement
            Console.WriteLine("GameLoopService: Requesting JS loop stop.");
            try
            {
                // Peut échouer si le contexte JS est déjà parti (fermeture onglet)
                await _jsRuntime.InvokeVoidAsync("stopGameLoop");
            }
            catch (JSException ex)
            {
                // C'est souvent normal lors de la fermeture rapide de l'application/onglet
                Console.WriteLine($"GameLoopService: Info - JS stop command failed (maybe page closed): {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                // Peut arriver si le JSRuntime n'est plus disponible
                Console.WriteLine($"GameLoopService: Info - JS stop command failed (InvalidOperation): {ex.Message}");
            }
        }
    }


    // Logique pour gérer les abonnements et démarrer/arrêter automatiquement (Optionnel mais recommandé)
    // Note : L'ajout/retrait d'event handler n'est pas async, donc on ne peut pas démarrer/arrêter la boucle ici directement de manière fiable.
    // Il est plus simple de laisser le composant de jeu appeler EnsureLoopStartedAsync et gérer l'arrêt dans Dispose.
    // Une alternative plus complexe impliquerait de wrapper l'event pour compter les abonnés.

    // Nettoyage
    public async ValueTask DisposeAsync() // Utiliser IAsyncDisposable
    {
        Console.WriteLine("GameLoopService: Disposing...");
        await StopLoopInternalAsync();
        _dotNetHelper?.Dispose();
        Console.WriteLine("GameLoopService: Disposed.");
    }
}