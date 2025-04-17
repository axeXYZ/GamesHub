// wwwroot/js/inputManager.js
let dotNetInputHandlerRef = null;

function handleGlobalKeyDown(event) {
    // Log pour voir si l'event JS est capturé
    //console.log("--- JS handleGlobalKeyDown: Code=", event.code);
    if (dotNetInputHandlerRef) {
        try {
            // Ne pas utiliser await ici car on ne bloque pas le thread UI JS
            dotNetInputHandlerRef.invokeMethodAsync('HandleJsKeyDown', event.code)
                // .then(r => console.log("invoke HandleJsKeyDown success")) // Log succès optionnel
                .catch(error => console.error("--- JS Error invoking HandleJsKeyDown:", error));
        } catch (error) {
            console.error("--- JS Exception invoking HandleJsKeyDown:", error);
        }
    } else {
        //console.warn("JS handleGlobalKeyDown: dotNetInputHandlerRef is null!");
    }
    // Empêcher comportement par défaut si nécessaire (ex: flèches, espace)
    if (['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Space'].includes(event.code)) {
        event.preventDefault();
    }
}

function handleGlobalKeyUp(event) {
    //console.log("--- JS handleGlobalKeyUp: Code=", event.code);
    if (dotNetInputHandlerRef) {
        try {
            dotNetInputHandlerRef.invokeMethodAsync('HandleJsKeyUp', event.code)
                // .then(r => console.log("invoke HandleJsKeyUp success"))
                .catch(error => console.error("--- JS Error invoking HandleJsKeyUp:", error));
        } catch (error) {
            console.error("--- JS Exception invoking HandleJsKeyUp:", error);
        }
    } else {
        //console.warn("JS handleGlobalKeyUp: dotNetInputHandlerRef is null!");
    }
}

window.inputManagerGlobal = {
    addListeners: function (dotNetRef) {
        //console.log("--- JS inputManagerGlobal.addListeners called.");
        if (!dotNetInputHandlerRef) {
            dotNetInputHandlerRef = dotNetRef;
            document.addEventListener('keydown', handleGlobalKeyDown);
            document.addEventListener('keyup', handleGlobalKeyUp);
            //console.log("--- JS Global listeners ADDED.");
        } else {
            //console.warn("--- JS addListeners: Listeners already added?");
        }
    },
    removeListeners: function () {
        //console.log("--- JS inputManagerGlobal.removeListeners called.");
        if (dotNetInputHandlerRef) {
            document.removeEventListener('keydown', handleGlobalKeyDown);
            document.removeEventListener('keyup', handleGlobalKeyUp);
            // Ne pas disposer la ref .NET ici, C# s'en charge avec _dotNetObjectReference.Dispose()
            dotNetInputHandlerRef = null;
            //console.log("--- JS Global listeners REMOVED.");
        } else {
            //console.warn("--- JS removeListeners: No listeners seem to be active.");
        }
    }
};