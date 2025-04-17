using Microsoft.JSInterop;
using System;
using System.Diagnostics; // Pour #if DEBUG seulement
using System.Threading;
using System.Threading.Tasks;

// Namespace pour les services du moteur de jeu
namespace Engine.Services;

/// <summary>
/// Définit l'interface pour la boucle de jeu principale, gérant les cycles de mise à jour
/// pour la logique de jeu, la physique et le rendu.
/// </summary>
/// <remarks>
/// Implémente <see cref="IAsyncDisposable"/> pour une gestion correcte des ressources.
/// Il est recommandé d'enregistrer ce service avec une portée 'Scoped' dans l'injection de dépendances
/// pour assurer un nettoyage automatique via <see cref="DisposeAsync"/>.
/// </remarks>
public interface IGameLoop : IAsyncDisposable
{
    /// <summary>
    /// Événement déclenché en synchronisation avec le cycle de rendu du navigateur (via requestAnimationFrame).
    /// La fréquence effective dépend du taux de rafraîchissement de l'écran et des performances (vise 60Hz, 120Hz, etc.).
    /// Fournit un delta time (temps écoulé depuis le dernier appel Update en secondes) VARIABLE, basé sur les timestamps du navigateur.
    /// </summary>
    /// <remarks>
    /// À utiliser pour : préparation des données de rendu, animations visuelles fluides, logique de jeu générale non-physique.
    /// S'exécute généralement dans le contexte de synchronisation Blazor principal. Pour Blazor Server,
    /// <c>InvokeAsync</c> peut rester nécessaire pour garantir les mises à jour UI depuis cet événement.
    /// Le delta time est limité (clamped) pour éviter des sauts trop importants après une période d'inactivité.
    /// </remarks>
    event Action<float>? Update;

    /// <summary>
    /// Événement déclenché à une fréquence FIXE (cible configurable, défaut 60 FPS) via un timer .NET
    /// pour les calculs de physique et la logique de jeu déterministe.
    /// Fournit un delta time (intervalle de temps en secondes) FIXE, indépendamment des fluctuations du rendu.
    /// </summary>
    /// <remarks>
    /// À utiliser pour : simulation physique (mouvements, collisions), logique de jeu nécessitant une stabilité temporelle.
    /// **ATTENTION :** Cet événement s'exécute sur un thread d'arrière-plan (<see cref="ThreadPool"/>).
    /// Utilisez <c>InvokeAsync()</c> pour interagir avec l'état ou l'UI Blazor depuis les abonnés à cet événement.
    /// Une compensation de lag est appliquée si le traitement prend trop de temps, avec une limite d'itérations par tick.
    /// </remarks>
    event Action<float>? PhysicsUpdate;

    /// <summary>
    /// Événement générique pour d'autres types d'entrées (ex: souris, gamepad) qui pourraient nécessiter
    /// un traitement centralisé via la boucle de jeu. Non utilisé pour le clavier (géré par InputManager).
    /// </summary>
    /// <remarks>
    /// L'objet passé en argument (<see cref="object"/>) contient les arguments de l'événement d'origine (ex: <see cref="MouseEventArgs"/>).
    /// Si cet événement n'est pas utilisé dans votre application, il peut être ignoré (ou l'événement et
    /// la méthode <see cref="TriggerInputUpdate"/> peuvent être supprimés de l'implémentation pour simplifier).
    /// </remarks>
    event Action<object>? InputUpdate;

    /// <summary>
    /// Événement déclenché lorsque le jeu devrait se mettre en pause
    /// (par exemple, onglet caché ou fenêtre inactive).
    /// Les abonnés doivent implémenter leur propre logique de pause.
    /// </summary>
    event Action? PauseRequested;

    /// <summary>
    /// Événement déclenché lorsque le jeu devrait reprendre
    /// (par exemple, onglet redevenu visible ou fenêtre réactivée).
    /// Les abonnés doivent implémenter leur propre logique de reprise.
    /// </summary>
    event Action? ResumeRequested;

    /// <summary>
    /// Obtient une valeur indiquant si la boucle de jeu est actuellement en cours d'exécution (<c>StartAsync</c> appelée et pas encore <c>StopAsync</c> ou <c>DisposeAsync</c>).
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Démarre les boucles de jeu (Update via JS et PhysicsUpdate via Timer .NET).
    /// Ne fait rien si la boucle est déjà démarrée ou si l'objet est libéré (disposed).
    /// </summary>
    /// <returns>Une <see cref="Task"/> représentant l'opération de démarrage asynchrone.</returns>
    /// <exception cref="JSException">Peut être levée si l'appel JavaScript initial échoue.</exception>
    Task StartAsync();

    /// <summary>
    /// Arrête les boucles de jeu de manière asynchrone et sécurisée.
    /// Ne fait rien si la boucle est déjà arrêtée ou si l'objet est libéré (disposed).
    /// </summary>
    /// <returns>Une <see cref="Task"/> représentant l'opération d'arrêt asynchrone.</returns>
    Task StopAsync();

    /// <summary>
    /// Déclenche l'événement <see cref="InputUpdate"/> avec les arguments fournis.
    /// Principalement destiné à être appelé depuis des gestionnaires d'événements Blazor
    /// pour des types d'entrées non-clavier (ex: <c>@onmousemove</c>).
    /// </summary>
    /// <param name="eventArgs">Les arguments de l'événement d'origine (ex: <see cref="MouseEventArgs"/>).</param>
    void TriggerInputUpdate(object eventArgs);
}


/// <summary>
/// Implémente une boucle de jeu (<see cref="IGameLoop"/>) hybride pour Blazor utilisant:
/// 1. JavaScript <c>requestAnimationFrame</c> pour la boucle <see cref="Update"/> (synchronisée avec le rendu).
/// 2. Un <see cref="PeriodicTimer"/> .NET pour la boucle <see cref="PhysicsUpdate"/> (à pas de temps fixe).
/// </summary>
/// <remarks>
/// Assure la libération correcte des ressources via <see cref="IAsyncDisposable"/>.
/// Pour une utilisation optimale, enregistrez cette classe avec une portée 'Scoped' dans votre conteneur DI.
/// </remarks>
public sealed class GameLoop : IGameLoop // sealed: pas d'héritage prévu
{
    // Services et état interne
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<GameLoop>? _dotNetObjectReference; // Référence .NET passée à JS
    private PeriodicTimer? _physicsTimer;                            // Timer pour la boucle physique
    private CancellationTokenSource? _physicsCts;                    // Pour annuler la boucle physique
    private long _lastRenderTimestamp = 0;                           // Pour calculer le delta time de Update
    private volatile bool _isRunning = false;                        // Indique si la boucle est active (volatile pour la visibilité entre threads)
    private bool _isDisposed = false;                                // Indicateur pour IAsyncDisposable
    private IJSObjectReference? _engineLoopModule;                   // Stocker la référence au module

    // --- Champs pour l'état de visibilité/focus ---
    private bool _isPageVisible = true; // Suppose visible au début
    private bool _hasWindowFocus = true; // Suppose focus au début
    private bool _isCurrentlyPausedBySystem = false; // État de pause interne basé sur les événements système

    // --- Configuration ---
    // Ces constantes pourraient être passées via un objet de configuration ou des options si nécessaire
    private const double TARGET_PHYSICS_FPS = 60.0;                  // FPS cible pour la physique
    private const float FIXED_PHYSICS_DELTA_TIME = (float)(1.0 / TARGET_PHYSICS_FPS); // Delta time fixe pour la physique
    private static readonly TimeSpan _physicsInterval = TimeSpan.FromSeconds(1.0 / TARGET_PHYSICS_FPS); // Intervalle du timer physique
    private const float MAX_UPDATE_DELTA_TIME = 0.1f;                // Clamp max pour le delta time de Update (100ms)
    private const int MAX_PHYSICS_ITERATIONS_PER_TICK = 5;           // Limite pour la compensation de lag physique

    // --- Événements ---
    /// <inheritdoc />
    public event Action<float>? Update;
    /// <inheritdoc />
    public event Action<float>? PhysicsUpdate;
    /// <inheritdoc />
    public event Action<object>? InputUpdate;
    /// <inheritdoc />
    public event Action? PauseRequested; // Implémentation du nouvel événement
    /// <inheritdoc />
    public event Action? ResumeRequested; // Implémentation du nouvel événement

    // --- Propriétés ---
    /// <inheritdoc />
    public bool IsRunning => _isRunning && !_isDisposed; // Plus précis: ne tourne pas si disposed

    /// <summary>
    /// Initialise une nouvelle instance de la classe <see cref="GameLoop"/>.
    /// </summary>
    /// <param name="jsRuntime">Le service IJSRuntime injecté pour l'interopérabilité JavaScript.</param>
    /// <exception cref="ArgumentNullException">Lancé si <paramref name="jsRuntime"/> est null.</exception>
    public GameLoop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
#if DEBUG
        Console.WriteLine($"GameLoop Instance Created (HashCode: {this.GetHashCode()}). Mode: Hybrid JS/NET.");
#endif
    }

    /// <inheritdoc />
    public async Task StartAsync()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_isRunning) return;

        _isRunning = true;
        _lastRenderTimestamp = 0;
        // Réinitialiser l'état de pause au démarrage
        _isPageVisible = true; // On suppose qu'on démarre visible/focus
        _hasWindowFocus = true;
        _isCurrentlyPausedBySystem = false;


        try
        {
            _engineLoopModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Engine/js/engineGameLoop.js");

            _dotNetObjectReference = DotNetObjectReference.Create(this); // Créé ici

            if (_engineLoopModule != null)
            {
                using var startCts = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // Timeout pour start JS
                await _engineLoopModule.InvokeVoidAsync("start", startCts.Token, _dotNetObjectReference); // Passe la réf .NET à JS
                Console.WriteLine($"GameLoop: JS Module Started.");
            }
            else
            {
                throw new InvalidOperationException("Failed to import engineGameLoop.js module.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GameLoop: Failed to import or start JS module. Error: {ex.Message}");
            _isRunning = false;
            _dotNetObjectReference?.Dispose(); // Nettoyer si créé
            _dotNetObjectReference = null;
            if (_engineLoopModule != null) { await _engineLoopModule.DisposeAsync(); _engineLoopModule = null; }
            // On pourrait vouloir propager l'exception ici ou retourner un booléen d'échec
            return; // Ne pas démarrer la boucle physique
        }

        // Démarrer la boucle Physique .NET (inchangé)
        _physicsCts = new CancellationTokenSource();
        _physicsTimer = new PeriodicTimer(_physicsInterval);
        _ = Task.Run(() => PhysicsTimerLoopAsync(_physicsCts.Token));

        Console.WriteLine($"GameLoop: Started successfully.");

        // Optionnel : Vérifier l'état initial (nécessite appel JS supplémentaire)
        // await RequestInitialStateFromJs();
    }

    /// <inheritdoc />
    public async Task StopAsync()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!_isRunning) return;
        Console.WriteLine($"GameLoop: Stopping...");
        _isRunning = false; // Signaler l'arrêt logique

        // Arrêter la boucle Update JS et les écouteurs focus/visibility via le module
        if (_engineLoopModule != null)
        {
            try
            {
                Console.WriteLine($"GameLoop: Calling JS stop()...");
                using var stopCts = new CancellationTokenSource(TimeSpan.FromSeconds(1)); // Timeout pour stop JS
                await _engineLoopModule.InvokeVoidAsync("stop", stopCts.Token);
                Console.WriteLine($"GameLoop: JS stop() called.");
            }
            catch (Exception ex) when (ex is JSDisconnectedException || ex is OperationCanceledException || ex is JSException)
            {
                Console.WriteLine($"GameLoop: Error stopping JS module loop (or browser closed): {ex.Message}");
            }
            // La référence au module sera disposée dans DisposeAsync
        }

        // Arrêter la boucle Physique .NET (inchangé)
        Console.WriteLine($"GameLoop: Stopping physics loop...");
        _physicsCts?.Cancel(); // Demande l'arrêt de la boucle Task.Run
        _physicsTimer?.Dispose(); // Libère le timer
        // Attendre la fin de la tâche de la boucle physique pourrait être plus propre ici si nécessaire
        // _physicsTask?.Wait(); // Si on gardait une référence à la Task

        _physicsTimer = null;
        _physicsCts?.Dispose(); // Libère le CancellationTokenSource
        _physicsCts = null;

        Console.WriteLine($"GameLoop: Stopped.");
    }

    /// <summary>
    /// Méthode de callback [JSInvokable] appelée par JavaScript via <c>requestAnimationFrame</c>.
    /// (Inchangée)
    /// </summary>
    [JSInvokable("OnAnimationFrame")]
    public void OnAnimationFrameCallback(long currentTimestamp)
    {
        if (!_isRunning || _isDisposed) return;
        // Ne pas exécuter Update si le système demande une pause
        if (_isCurrentlyPausedBySystem) return;

        float deltaTime = 0;
        if (_lastRenderTimestamp != 0)
        {
            deltaTime = (float)(currentTimestamp - _lastRenderTimestamp) / 1000.0f;
        }
        _lastRenderTimestamp = currentTimestamp;
        deltaTime = MathF.Min(deltaTime, MAX_UPDATE_DELTA_TIME);
        try
        {
            Update?.Invoke(deltaTime);
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine($"GameLoop (Instance: {this.GetHashCode()}): Error during Update event handler: {ex}");
#endif
        }
    }

    /// <summary>
    /// Boucle asynchrone pour <see cref="PhysicsUpdate"/> à intervalle fixe.
    /// (Modifiée pour vérifier la pause système)
    /// </summary>
    private async Task PhysicsTimerLoopAsync(CancellationToken cancellationToken)
    {
#if DEBUG
        Console.WriteLine($"GameLoop (Instance: {this.GetHashCode()}): Physics loop ({Thread.CurrentThread.ManagedThreadId}) started.");
#endif
        double accumulatedLag = 0;

        try
        {
            while (await _physicsTimer!.WaitForNextTickAsync(cancellationToken))
            {
                if (!_isRunning) break; // Vérifier l'état global aussi
                                        // Ne pas exécuter PhysicsUpdate si le système demande une pause
                if (_isCurrentlyPausedBySystem)
                {
                    accumulatedLag = 0; // Réinitialiser le lag pendant la pause
                    continue; // Passer au prochain tick sans exécuter la physique
                }

                accumulatedLag += FIXED_PHYSICS_DELTA_TIME;
                int iterations = 0;
                while (accumulatedLag >= FIXED_PHYSICS_DELTA_TIME && iterations < MAX_PHYSICS_ITERATIONS_PER_TICK)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    try
                    {
                        PhysicsUpdate?.Invoke(FIXED_PHYSICS_DELTA_TIME);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Console.WriteLine($"GameLoop (Instance: {this.GetHashCode()}): Error during PhysicsUpdate event handler: {ex}");
#endif
                    }
                    accumulatedLag -= FIXED_PHYSICS_DELTA_TIME;
                    iterations++;
                }
                if (cancellationToken.IsCancellationRequested) break;
                if (iterations >= MAX_PHYSICS_ITERATIONS_PER_TICK)
                {
#if DEBUG
                    Console.WriteLine($"GameLoop (Instance: {this.GetHashCode()}): Warning - Physics update lagged ({iterations} iterations). Resetting lag {accumulatedLag:F4}s.");
#endif
                    accumulatedLag = 0;
                }
            }
        }
        catch (OperationCanceledException)
        {
#if DEBUG
            Console.WriteLine($"GameLoop (Instance: {this.GetHashCode()}): Physics loop cancelled.");
#endif
        }
        catch (ObjectDisposedException)
        {
#if DEBUG
            Console.WriteLine($"GameLoop (Instance: {this.GetHashCode()}): Physics loop timer was disposed during operation.");
#endif
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine($"GameLoop (Instance: {this.GetHashCode()}): Critical error in Physics loop: {ex}");
#endif
            _ = StopAsync();
        }
        finally
        {
#if DEBUG
            Console.WriteLine($"GameLoop (Instance: {this.GetHashCode()}): Physics loop ({Thread.CurrentThread.ManagedThreadId}) stopped.");
#endif
        }
    }

    [JSInvokable("HandleVisibilityChangeJsCallback")]
    public void HandleVisibilityChangeJsCallback(bool isHidden)
    {
        if (_isDisposed) return; // Ne rien faire si disposed
        _isPageVisible = !isHidden;
#if DEBUG
        Console.WriteLine($"GameLoop JSCallback: VisibilityChange - IsHidden: {isHidden}, IsPageVisible: {_isPageVisible}");
#endif
        CheckPauseStateAndRaiseEvents(); // Vérifie si l'état de pause global doit changer
    }

    [JSInvokable("HandleWindowFocusChangeJsCallback")]
    public void HandleWindowFocusChangeJsCallback(bool hasFocus)
    {
        if (_isDisposed) return; // Ne rien faire si disposed
        _hasWindowFocus = hasFocus;
#if DEBUG
        Console.WriteLine($"GameLoop JSCallback: WindowFocusChange - HasFocus: {hasFocus}");
#endif
        CheckPauseStateAndRaiseEvents(); // Vérifie si l'état de pause global doit changer
    }

    /// <summary>
    /// Évalue si le jeu doit être en pause en fonction de la visibilité et du focus,
    /// et déclenche les événements PauseRequested ou ResumeRequested si l'état change.
    /// </summary>
    private void CheckPauseStateAndRaiseEvents()
    {
        // Le jeu doit être en pause si la page n'est pas visible OU si la fenêtre n'a pas le focus
        bool shouldBePaused = !_isPageVisible || !_hasWindowFocus;

        // Vérifier si l'état de pause a changé
        if (shouldBePaused && !_isCurrentlyPausedBySystem)
        {
            _isCurrentlyPausedBySystem = true;
            Console.WriteLine("GameLoop: Pause Requested due to visibility/focus change.");
            try
            {
                PauseRequested?.Invoke(); // Déclenche l'événement pour les abonnés
            }
            catch (Exception ex) { Console.WriteLine($"GameLoop: Error in PauseRequested handler: {ex}"); }
            // La boucle physique/update vérifiera _isCurrentlyPausedBySystem pour s'arrêter
        }
        else if (!shouldBePaused && _isCurrentlyPausedBySystem)
        {
            _isCurrentlyPausedBySystem = false;
            Console.WriteLine("GameLoop: Resume Requested due to visibility/focus change.");
            try
            {
                ResumeRequested?.Invoke(); // Déclenche l'événement pour les abonnés
            }
            catch (Exception ex) { Console.WriteLine($"GameLoop: Error in ResumeRequested handler: {ex}"); }
            // La boucle physique/update reprendra automatiquement car _isCurrentlyPausedBySystem est false
        }
        // Si l'état de pause ne change pas, on ne fait rien (pas d'événements répétés)
    }

    /// <inheritdoc />
    public void TriggerInputUpdate(object eventArgs) // Inchangé
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!_isRunning || _isCurrentlyPausedBySystem) return; // Ne pas traiter input si en pause systeme
        try
        {
            InputUpdate?.Invoke(eventArgs);
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine($"GameLoop (Instance: {this.GetHashCode()}): Error during InputUpdate event handler: {ex}");
#endif
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() // Légèrement modifié pour être sûr que Stop est appelé
    {
        if (_isDisposed) return;
        _isDisposed = true;
        Console.WriteLine($"GameLoop Disposing (Instance: {this.GetHashCode()})...");

        // Assure l'arrêt propre des boucles et le retrait des écouteurs JS
        await StopAsync();

        _dotNetObjectReference?.Dispose();
        _dotNetObjectReference = null;

        if (_engineLoopModule != null)
        {
            try { await _engineLoopModule.DisposeAsync(); } catch { /* Ignorer erreurs de dispose JS */ }
            _engineLoopModule = null;
        }

        // Nettoyer les abonnements pour libérer la mémoire
        Update = null;
        PhysicsUpdate = null;
        InputUpdate = null;
        PauseRequested = null;
        ResumeRequested = null;

        Console.WriteLine($"GameLoop Disposed (Instance: {this.GetHashCode()}).");
        GC.SuppressFinalize(this);
    }

    // Optionnel: Ajouter un finaliseur pour une sécurité supplémentaire (rarement nécessaire avec IAsyncDisposable bien utilisé)
    // ~GameLoop()
    // {
    //     // Ne pas appeler StopAsync ici (peut bloquer le thread finaliseur)
    //     // Nettoyer seulement les ressources non managées directement si nécessaire
    //     // ou signaler un problème de non-libération.
    // #if DEBUG
    //     Console.WriteLine($"GameLoop (Instance: {this.GetHashCode()}) Finalizer called! DisposeAsync() was likely missed.");
    // #endif
    // }
}