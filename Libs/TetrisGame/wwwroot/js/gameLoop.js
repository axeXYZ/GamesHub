let gameLoopCallbackRef = null;
let animationFrameId = null;
let lastTimestamp = 0;

// Fonction appelée par Blazor pour démarrer la boucle
window.startGameLoop = (dotnetHelper) => {
    gameLoopCallbackRef = dotnetHelper;
    lastTimestamp = performance.now(); // Initialiser le timestamp
    animationFrameId = requestAnimationFrame(gameLoopInternal);
};

// Fonction interne récursive
function gameLoopInternal(timestamp) {
    if (!gameLoopCallbackRef) return; // Arrêté ?

    const deltaTime = (timestamp - lastTimestamp) / 1000.0; // DeltaTime en secondes
    lastTimestamp = timestamp;

    // Appelle la méthode C# dans Blazor
    gameLoopCallbackRef.invokeMethodAsync('JsGameTick', deltaTime)
        .then(() => {
            // Demande le prochain frame UNIQUEMENT si la boucle n'a pas été arrêtée
            if (gameLoopCallbackRef) {
                animationFrameId = requestAnimationFrame(gameLoopInternal);
            }
        })
        .catch(err => {
            console.error("Error during game tick or requesting next frame:", err);
            // Peut-être arrêter la boucle ici ?
            // stopGameLoop(); 
        });
}

// Fonction appelée par Blazor pour arrêter la boucle
window.stopGameLoop = () => {
    if (animationFrameId) {
        cancelAnimationFrame(animationFrameId);
    }
    if (gameLoopCallbackRef) {
        // Important: Libérer la référence côté JS pour permettre le garbage collection côté .NET
        // gameLoopCallbackRef.dispose(); // Ne pas appeler dispose() ici, Blazor le fait.
        gameLoopCallbackRef = null; // Simplement nullifier la référence JS
    }
    animationFrameId = null;
    console.log("Game loop stopped.");
};