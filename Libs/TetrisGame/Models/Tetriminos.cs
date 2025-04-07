namespace TetrisGame.Models;

public class Tetriminos
{
    //
    // UInt16[nbRotations, hauteur, largeur]
    //
    public ushort[,,] Z = new ushort[2, 3, 3]{
        {// 0
            { 0, 0, 7 },
            { 0, 7, 7 },
            { 0, 7, 0 }
        },
        {// 1
            { 7 ,7 ,0 },
            { 0 ,7 ,7 },
            { 0 ,0 ,0 }
        }
    };
    public ushort[,,] S = new ushort[2, 3, 3]{
        {// 0
            { 0, 6, 6 },
            { 6, 6, 0 },
            { 0, 0, 0 }
        },
        {// 1
            { 0 ,6 ,0 },
            { 0 ,6 ,6 },
            { 0 ,0 ,6 }
        }
    };
    public ushort[,,] T = new ushort[4, 3, 3]{
        {// 0
            { 0, 5, 0 },
            { 5, 5, 5 },
            { 0, 0, 0 }
        },
        {// 1
            { 0 ,5 ,0 },
            { 0 ,5 ,5 },
            { 0 ,5 ,0 }
        },
        {// 2
            { 0, 0, 0 },
            { 5, 5, 5 },
            { 0, 5, 0 }
        },
        {// 3
            { 0 ,5 ,0 },
            { 5 ,5 ,0 },
            { 0 ,5 ,0 }
        }
    };
    public ushort[,,] O = new ushort[1, 2, 2]{
        {// 0
            { 4, 4 },
            { 4, 4 }
        }
    };
    public ushort[,,] J = new ushort[4, 3, 3]{
        {// 0
            { 0, 3, 0 },
            { 0, 3, 0 },
            { 3, 3, 0 }
        },
        {// 1
            { 3 ,0 ,0 },
            { 3 ,3 ,3 },
            { 0 ,0 ,0 }
        },
        {// 2
            { 0, 3, 3 },
            { 0, 3, 0 },
            { 0, 3, 0 }
        },
        {// 3
            { 0 ,0 ,0 },
            { 3 ,3 ,3 },
            { 0 ,0 ,3 }
        }
    };
    public ushort[,,] L = new ushort[4, 3, 3]{
        {// 0
            { 0, 2, 0 },
            { 0, 2, 0 },
            { 2, 2, 0 }
        },
        {// 1
            { 2 ,0 ,0 },
            { 2 ,2 ,2 },
            { 0 ,0 ,0 }
        },
        {// 2
            { 0, 2, 2 },
            { 0, 2, 0 },
            { 0, 2, 0 }
        },
        {// 3
            { 0 ,0 ,0 },
            { 2 ,2 ,2 },
            { 0 ,0 ,2 }
        }
    };
    public ushort[,,] I = new ushort[2, 4, 4]{
        {// 0
            { 0, 0, 0, 0 },
            { 1, 1, 1, 1 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        },
        {// 1
            { 0 ,1 ,0 ,0 },
            { 0 ,1 ,0 ,0 },
            { 0 ,1 ,0 ,0 },
            { 0 ,1 ,0 ,0 }
        }
    };

    public ushort[,,] GetRandomTetriminos()
    {
        int r = Random.Shared.Next(0, 7);
        ushort[,,] tetriminos = r switch
        {
            0 => Z,
            1 => S,
            2 => T,
            3 => O,
            4 => J,
            5 => L,
            6 => I,
            _ => Z
        };
        return tetriminos;
    }
}
