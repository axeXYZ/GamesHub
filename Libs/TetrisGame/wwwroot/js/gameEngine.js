// js/gameEngine.js
window.gameLoopManager = {
    loopInstances: {}, // Pour suivre les différentes instances de boucle

    start: function (dotNetHelper, instanceId) {
        // Stocke la référence pour pouvoir l'arrêter plus tard
        this.loopInstances[instanceId] = {
            dotNetHelper: dotNetHelper,
            isRunning: true,
            animationFrameId: null // Pour stocker l'ID de requestAnimationFrame
        };

        // Définit la fonction de boucle interne pour cette instance
        const loop = () => {
            const instance = this.loopInstances[instanceId];
            // Vérifie si l'instance existe toujours et si elle est censée tourner
            if (!instance || !instance.isRunning) {
                delete this.loopInstances[instanceId]; // Nettoyage si arrêté
                return; // Sort de la boucle
            }

            // Appelle la méthode C# pour faire le travail d'une frame
            instance.dotNetHelper.invokeMethodAsync('GameLoopTick')
                .then(() => {
                    // Une fois le travail C# terminé (ou au moins invoqué),
                    // demande la prochaine frame SI toujours en cours d'exécution
                    if (instance.isRunning) {
                        instance.animationFrameId = requestAnimationFrame(loop);
                    } else {
                        delete this.loopInstances[instanceId]; // Nettoyage
                    }
                })
                .catch(err => {
                    console.error(`Error in game loop instance ${instanceId}:`, err);
                    // Arrête la boucle en cas d'erreur grave avec l'appel .NET
                    instance.isRunning = false;
                    delete this.loopInstances[instanceId];
                    // Vous pourriez vouloir informer C# ici via un autre invokeMethodAsync
                });
        };

        // Démarre la boucle pour la première fois
        this.loopInstances[instanceId].animationFrameId = requestAnimationFrame(loop);
        console.log(`Game loop started for instance ${instanceId}`);
    },

    stop: function (instanceId) {
        const instance = this.loopInstances[instanceId];
        if (instance) {
            instance.isRunning = false;
            // Bien que le flag isRunning suffise souvent, cancelAnimationFrame
            // est plus propre pour arrêter immédiatement la demande au navigateur.
            if (instance.animationFrameId) {
                cancelAnimationFrame(instance.animationFrameId);
            }
            // Le nettoyage final se fait dans la boucle elle-même ou ici
            delete this.loopInstances[instanceId];
            console.log(`Game loop stopped for instance ${instanceId}`);
            // La référence dotNetHelper est conservée par C# et sera disposée là-bas
        }
    }
};