// Engine/wwwroot/js/engineGameLoop.js (version module)

let dotNetReference = null;
let animationFrameId = null;
let isRunning = false;

function handleVisibilityChange() {
    if (!dotNetReference) return;
    const isHidden = document.hidden;
    console.log(`JS: Visibility changed. Hidden: ${isHidden}`);
    try {
        // Appelle une méthode C# spécifique [JSInvokable]
        dotNetReference.invokeMethodAsync('HandleVisibilityChangeJsCallback', isHidden);
    } catch (error) {
        console.error("JS Error invoking HandleVisibilityChangeJsCallback:", error);
        // Peut-être arrêter la boucle JS si .NET est déconnecté ? stop();
    }
}

function handleWindowBlur() {
    if (!dotNetReference) return;
    console.log("JS: Window lost focus (blur)");
    try {
        // Appelle une méthode C# spécifique [JSInvokable]
        dotNetReference.invokeMethodAsync('HandleWindowFocusChangeJsCallback', false); // false = hasFocus
    } catch (error) {
        console.error("JS Error invoking HandleWindowFocusChangeJsCallback:", error);
    }
}

function handleWindowFocus() {
    if (!dotNetReference) return;
    console.log("JS: Window gained focus");
    try {
        // Appelle une méthode C# spécifique [JSInvokable]
        dotNetReference.invokeMethodAsync('HandleWindowFocusChangeJsCallback', true); // true = hasFocus
    } catch (error) {
        console.error("JS Error invoking HandleWindowFocusChangeJsCallback:", error);
    }
}
// Exporter la fonction 'start'
export function start(dotNetRef) {
    if (isRunning) {
        console.warn("Engine Game Loop Module: start() called but already running.");
        return;
    }
    if (!dotNetRef) {
        console.error("Engine Game Loop Module: start() called with null dotNetRef.");
        return;
    }
    dotNetReference = dotNetRef;
    isRunning = true;
    console.log("Engine Game Loop Module: Starting JavaScript loop (requestAnimationFrame).");

    // --- AJOUT des écouteurs ---
    document.addEventListener('visibilitychange', handleVisibilityChange);
    window.addEventListener('blur', handleWindowBlur);
    window.addEventListener('focus', handleWindowFocus);
    console.log("JS: Visibility and focus listeners ADDED.");

    // Démarre la boucle de rendu (requestAnimationFrame)
    requestLoop();

    // Optionnel : Appeler C# immédiatement avec l'état initial ?
    // handleVisibilityChange(); // Pour envoyer l'état initial de visibilité
    // handleWindowFocus(); // Ne fonctionne pas car document.hasFocus() est nécessaire
    // Mieux géré côté C# si nécessaire au démarrage.
}

// Exporter la fonction 'stop'
export function stop() {
    if (!isRunning) {
        return;
    }
    isRunning = false;
    if (animationFrameId) {
        cancelAnimationFrame(animationFrameId);
        animationFrameId = null;
    }

    // --- RETRAIT des écouteurs ---
    document.removeEventListener('visibilitychange', handleVisibilityChange);
    window.removeEventListener('blur', handleWindowBlur);
    window.removeEventListener('focus', handleWindowFocus);
    console.log("JS: Visibility and focus listeners REMOVED.");

    // La référence .NET (dotNetReference) est gérée par C# lors du Dispose
    console.log("Engine Game Loop Module: Stopped JavaScript loop.");
}

// Fonction interne (pas besoin d'exporter si non appelée directement par C#)
function requestLoop() {
    if (!isRunning) return;
    animationFrameId = requestAnimationFrame(gameLoopStep);
}

// Fonction interne
function gameLoopStep(timestamp) {
    if (!isRunning || !dotNetReference) {
        console.error("Engine Game Loop Module: Stopping loop due to missing reference or stopped state.");
        stop(); // Assure le nettoyage des écouteurs si la référence est perdue
        return;
    }
    try {
        // Appel C# pour Update (inchangé)
        dotNetReference.invokeMethodAsync('OnAnimationFrame', Math.floor(timestamp));
    } catch (error) {
        console.error("Engine Game Loop Module: Error invoking .NET 'OnAnimationFrame'.", error);
        stop(); // Arrêter si l'appel échoue (ex: déconnexion)
        return;
    }
    requestLoop(); // Continue la boucle
}

// Log de chargement (optionnel)
console.log("engineGameLoop.js module loaded");