// TetrisGame/Models/Tetriminos.cs

namespace BlocksterGame.Models;

public class Blocks
{
    // --- Définition statique de toutes les formes et leurs rotations ---
    // (Identique à votre code original)

    // Type O (index 0) - 1 rotation
    private static readonly ushort[][,] O_Shapes = {
        new ushort[,] { { 1, 1 },
                        { 1, 1 } }
    };

    // Type I (index 1) - 2 rotations
    private static readonly ushort[][,] I_Shapes = {
        new ushort[,] { { 2, 2, 2, 2 } }, // Rotation 0
        new ushort[,] { { 2 },
                        { 2 },
                        { 2 },
                        { 2 } } // Rotation 1
    };

    // Type T (index 2) - 4 rotations
    private static readonly ushort[][,] T_Shapes = {
        new ushort[,] { { 0, 3, 0 },
                        { 3, 3, 3 } }, // Rotation 0
        new ushort[,] { { 3, 0 },
                        { 3, 3 },
                        { 3, 0 } }, // Rotation 1
        new ushort[,] { { 3, 3, 3 },
                        { 0, 3, 0 } }, // Rotation 2
        new ushort[,] { { 0, 3 },
                        { 3, 3 },
                        { 0, 3 } }  // Rotation 3
    };

    // Type J (index 3) - 4 rotations
    private static readonly ushort[][,] J_Shapes = {
        new ushort[,] { { 4, 0, 0 },
                        { 4, 4, 4 } }, // Rotation 0
        new ushort[,] { { 0, 4, 4 },
                        { 0, 4, 0 },
                        { 0, 4, 0 } }, // Rotation 1
        new ushort[,] { { 4, 4, 4 },
                        { 0, 0, 4 } }, // Rotation 2
        new ushort[,] { { 0, 4, 0 },
                        { 0, 4, 0 },
                        { 4, 4, 0 } }  // Rotation 3
    };

    // Type L (index 4) - 4 rotations
    private static readonly ushort[][,] L_Shapes = {
        new ushort[,] { { 0, 0, 5 },
                        { 5, 5, 5 } }, // Rotation 0
        new ushort[,] { { 0, 5, 0 },
                        { 0, 5, 0 },
                        { 0, 5, 5 } }, // Rotation 1
        new ushort[,] { { 5, 5, 5 },
                        { 5, 0, 0 } }, // Rotation 2
        new ushort[,] { { 5, 5, 0 },
                        { 0, 5, 0 },
                        { 0, 5, 0 } }  // Rotation 3
    };

    // Type S (index 5) - 2 rotations
    private static readonly ushort[][,] S_Shapes = {
        new ushort[,] { { 0, 6, 6 },
                        { 6, 6, 0 } }, // Rotation 0
        new ushort[,] { { 6, 0 },
                        { 6, 6 },
                        { 0, 6 } }  // Rotation 1
    };

    // Type Z (index 6) - 2 rotations
    private static readonly ushort[][,] Z_Shapes = {
        new ushort[,] { { 7, 7, 0 },
                        { 0, 7, 7 } }, // Rotation 0
        new ushort[,] { { 0, 7 },
                        { 7, 7 },
                        { 7, 0 } }  // Rotation 1
    };

    // Tableau principal statique contenant toutes les formes et leurs rotations
    private static readonly ushort[/*Type*/][/*Rotation*/][,] AllTetriminoShapes = {
        O_Shapes, I_Shapes, T_Shapes, J_Shapes, L_Shapes, S_Shapes, Z_Shapes
    };

    // --- État de l'instance ---
    private readonly Random _random;
    private int _currentTypeIndex;      // Index du type de forme actuel (0-6) dans AllTetriminoShapes
    private int _currentRotationIndex; // Index de la rotation actuelle pour le type courant

    // --- Propriété Publique ---

    /// <summary>
    /// Obtient la forme (tableau ushort[,]) actuelle du Tetrimino actif.
    /// Ne peut être modifié que par les méthodes de cette classe.
    /// </summary>
    public ushort[,] Shape { get; private set; }

    // --- Constructeur ---

    /// <summary>
    /// Initialise une nouvelle instance de Tetriminos avec une forme aléatoire
    /// à sa rotation initiale (index 0).
    /// </summary>
    public Blocks()
    {
        _random = new Random();
        // Assigne une forme aléatoire initiale lors de la création
        SetRandomShape();
    }

    /// <summary>
    /// Initialise une nouvelle instance de Tetriminos avec une forme aléatoire
    /// à sa rotation initiale (index 0), en utilisant une instance fournie de Random.
    /// Utile pour la prévisibilité lors des tests.
    /// </summary>
    /// <param name="randomInstance">L'instance de Random à utiliser.</param>
    public Blocks(Random randomInstance)
    {
        _random = randomInstance ?? new Random(); // Utilise l'instance fournie ou une nouvelle si null
        SetRandomShape();
    }


    // --- Méthodes Publiques ---

    /// <summary>
    /// Assigne une nouvelle forme de Tetrimino aléatoire (à sa rotation initiale, index 0)
    /// à la propriété 'Shape'. Met également à jour l'état interne (type et index de rotation).
    /// </summary>
    private void SetRandomShape()
    {
        _currentTypeIndex = _random.Next(AllTetriminoShapes.Length); // Choisit un type aléatoire (0-6)
        _currentRotationIndex = 0; // Commence toujours par la première rotation

        // Met à jour la propriété publique Shape
        Shape = AllTetriminoShapes[_currentTypeIndex][_currentRotationIndex];
    }

    /// <summary>
    /// Retourne la forme (tableau ushort[,]) de la prochaine rotation
    /// pour le type de Tetrimino actuel, sans modifier l'état courant de l'objet.
    /// </summary>
    /// <returns>Le tableau ushort[,] représentant la forme de la prochaine rotation.</returns>
    public ushort[,] GetNextShape()
    {
        // Récupère le tableau des rotations pour le type actuel
        ushort[][,] currentTypeRotations = AllTetriminoShapes[_currentTypeIndex];

        // Calcule l'index de la prochaine rotation (avec retour au début si nécessaire)
        int nextRotationIndex = (_currentRotationIndex + 1) % currentTypeRotations.Length;

        // Retourne la forme correspondant à la rotation suivante
        return currentTypeRotations[nextRotationIndex];
    }

    /// <summary>
    /// Fait pivoter le Tetrimino actuel à sa prochaine forme de rotation
    /// et met à jour la propriété publique 'Shape' en conséquence.
    /// </summary>
    public void SetToNextShape()
    {
        // Récupère le tableau des rotations pour le type actuel
        ushort[][,] currentTypeRotations = AllTetriminoShapes[_currentTypeIndex];

        // Calcule et met à jour l'index de la rotation actuelle
        _currentRotationIndex = (_currentRotationIndex + 1) % currentTypeRotations.Length;

        // Met à jour la propriété publique Shape avec la nouvelle forme
        Shape = currentTypeRotations[_currentRotationIndex];
    }
}