using Microsoft.JSInterop;
using System;
using System.Diagnostics; // Ajout pour Stopwatch
using System.Threading.Tasks;

namespace TetrisGame.Models;
public sealed class Engine : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    // Renommé pour clarté et ajout de TimeSpan deltaTime en paramètre
    private readonly Action<TimeSpan> _onFrameUpdateAction;
    // Nouvelle action pour le tick basé sur le temps
    private readonly Action? _onTimeTickAction;
    // Intervalle pour le onTimeTickAction
    private readonly TimeSpan _tickInterval;

    private DotNetObjectReference<Engine>? _dotNetObjectReference;
    private bool _isRunning = false;
    private readonly string _instanceId;

    // Pour mesurer le temps écoulé
    private Stopwatch _stopwatch = new Stopwatch();
    private TimeSpan _lastFrameTime = TimeSpan.Zero;
    private TimeSpan _timeSinceLastTick = TimeSpan.Zero;


    // --- CONSTRUCTEUR MODIFIÉ ---
    public Engine(
        IJSRuntime jsRuntime,
        Action<TimeSpan> onFrameUpdateAction, // Attend maintenant un TimeSpan
        Action? onTimeTickAction = null,      // Action pour le tick (optionnelle)
        TimeSpan? tickInterval = null)        // Intervalle pour le tick ( requis si onTimeTickAction est fourni)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _onFrameUpdateAction = onFrameUpdateAction ?? throw new ArgumentNullException(nameof(onFrameUpdateAction));

        if (onTimeTickAction != null)
        {
            if (tickInterval == null || tickInterval.Value <= TimeSpan.Zero)
            {
                throw new ArgumentException("Un intervalle de temps positif doit être fourni si onTimeTickAction est défini.", nameof(tickInterval));
            }
            _onTimeTickAction = onTimeTickAction;
            _tickInterval = tickInterval.Value;
        }
        else
        {
            // Si pas d'action de tick, on met un intervalle invalide pour ne jamais déclencher
            _tickInterval = TimeSpan.MinValue;
        }

        _instanceId = Guid.NewGuid().ToString("N");
    }

    public async Task StartAsync()
    {
        if (_isRunning) return;

        _dotNetObjectReference = DotNetObjectReference.Create(this);
        _isRunning = true;

        // Réinitialiser et démarrer le chronomètre et les temps
        _timeSinceLastTick = TimeSpan.Zero;
        _lastFrameTime = TimeSpan.Zero;
        _stopwatch.Restart(); // Redémarre le chronomètre

        Console.WriteLine($"Starting engine {_instanceId}...");
        try
        {
            await _jsRuntime.InvokeVoidAsync("gameLoopManager.start", _dotNetObjectReference, _instanceId);
        }
        catch (Exception ex) // Simplifié pour l'exemple
        {
            Console.Error.WriteLine($"Error starting game loop for {_instanceId}: {ex.Message}");
            _isRunning = false;
            _dotNetObjectReference?.Dispose();
            _dotNetObjectReference = null;
            _stopwatch.Stop();
        }
    }

    public async Task StopAsync()
    {
        if (!_isRunning) return;

        _isRunning = false; // Important: doit être défini avant l'appel JS
        _stopwatch.Stop(); // Arrête le chronomètre
        Console.WriteLine($"Stopping engine {_instanceId}...");

        try
        {
            await _jsRuntime.InvokeVoidAsync("gameLoopManager.stop", _instanceId);
        }
        catch (Exception ex) // Simplifié
        {
            Console.Error.WriteLine($"Error stopping JS game loop for {_instanceId}: {ex.Message}");
        }

        _dotNetObjectReference?.Dispose();
        _dotNetObjectReference = null;
        Console.WriteLine($"Engine {_instanceId} stopped and reference disposed.");
    }

    // --- GAMELOOPTICK MODIFIÉ ---
    [JSInvokable]
    public void GameLoopTick()
    {
        if (!_isRunning) return;

        // --- Calcul du Delta Time ---
        TimeSpan currentFrameTime = _stopwatch.Elapsed;
        TimeSpan deltaTime = currentFrameTime - _lastFrameTime;
        _lastFrameTime = currentFrameTime;

        // Sécurité : Si deltaTime est trop grand (ex: débogage, longue pause), on le limite
        // pour éviter des sauts énormes dans la logique ou des boucles infinies dans le while ci-dessous.
        // 1/10ème de seconde est une limite raisonnable.
        if (deltaTime > TimeSpan.FromMilliseconds(100))
        {
            deltaTime = TimeSpan.FromMilliseconds(100);
        }


        try
        {
            // --- Appel de l'Update de Frame ---
            // Passe le deltaTime calculé à l'action de mise à jour par frame
            _onFrameUpdateAction?.Invoke(deltaTime);

            // --- Logique pour le Tick basé sur le Temps ---
            if (_onTimeTickAction != null && _tickInterval > TimeSpan.Zero)
            {
                _timeSinceLastTick += deltaTime;

                // Utilise 'while' au cas où plusieurs ticks se seraient écoulés
                // (si une frame a pris beaucoup de temps ou si l'intervalle est très court)
                while (_timeSinceLastTick >= _tickInterval)
                {
                    _onTimeTickAction.Invoke(); // Appelle l'action de tick
                    _timeSinceLastTick -= _tickInterval; // Réduit l'accumulateur
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error during GameLoopTick for instance {_instanceId}: {ex.Message}");
            // Envisager d'arrêter le moteur ici si l'erreur est critique
            // Task.Run(async () => await StopAsync()); // Attention, ne pas faire await directement ici
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        GC.SuppressFinalize(this);
    }
}