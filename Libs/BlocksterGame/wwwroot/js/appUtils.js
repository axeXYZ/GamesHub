// wwwroot/js/appUtils.js

window.appUtils = {
    setBodyPausedIndicator: function (isPaused) {
        if (isPaused) {
            document.body.classList.add('game-paused-indicator');
            // Log optionnel pour débogage
            // console.log("JS: Added 'game-paused-indicator' to body.");
        } else {
            document.body.classList.remove('game-paused-indicator');
            // Log optionnel pour débogage
            // console.log("JS: Removed 'game-paused-indicator' from body.");
        }
    }
};

// N'oubliez pas de référencer ce fichier dans votre index.html ou _Host.cshtml:
// <script src="js/appUtils.js"></script>