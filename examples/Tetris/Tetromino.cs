using System.Numerics;
using GameUtils.Helpers;

namespace Tetris;

public static class Tetromino
{
    private static readonly Dictionary<TetrominoType, Vector2[][]> _shapes = new()
    {
        [ TetrominoType.I ] =
        [
            [ new Vector2( 0, 1 ), new Vector2( 1, 1 ), new Vector2( 2, 1 ), new Vector2( 3, 1 ) ], // Horizontal
            [ new Vector2( 2, 0 ), new Vector2( 2, 1 ), new Vector2( 2, 2 ), new Vector2( 2, 3 ) ], // Vertical
            [ new Vector2( 0, 2 ), new Vector2( 1, 2 ), new Vector2( 2, 2 ), new Vector2( 3, 2 ) ], // Horizontal
            [ new Vector2( 1, 0 ), new Vector2( 1, 1 ), new Vector2( 1, 2 ), new Vector2( 1, 3 ) ] // Vertical
        ],
        [ TetrominoType.O ] =
        [
            [ new Vector2( 1, 1 ), new Vector2( 2, 1 ), new Vector2( 1, 2 ), new Vector2( 2, 2 ) ],
            [ new Vector2( 1, 1 ), new Vector2( 2, 1 ), new Vector2( 1, 2 ), new Vector2( 2, 2 ) ],
            [ new Vector2( 1, 1 ), new Vector2( 2, 1 ), new Vector2( 1, 2 ), new Vector2( 2, 2 ) ],
            [ new Vector2( 1, 1 ), new Vector2( 2, 1 ), new Vector2( 1, 2 ), new Vector2( 2, 2 ) ]
        ],
        [ TetrominoType.T ] =
        [
            [ new Vector2( 1, 1 ), new Vector2( 0, 1 ), new Vector2( 2, 1 ), new Vector2( 1, 2 ) ], // T pointing up
            [ new Vector2( 1, 1 ), new Vector2( 1, 0 ), new Vector2( 1, 2 ), new Vector2( 2, 1 ) ], // T pointing right
            [ new Vector2( 1, 1 ), new Vector2( 0, 1 ), new Vector2( 2, 1 ), new Vector2( 1, 0 ) ], // T pointing down
            [ new Vector2( 1, 1 ), new Vector2( 1, 0 ), new Vector2( 1, 2 ), new Vector2( 0, 1 ) ] // T pointing left
        ],
        [ TetrominoType.S ] =
        [
            [ new Vector2( 1, 1 ), new Vector2( 2, 1 ), new Vector2( 0, 0 ), new Vector2( 1, 0 ) ],
            [ new Vector2( 1, 1 ), new Vector2( 1, 2 ), new Vector2( 2, 2 ), new Vector2( 2, 3 ) ],
            [ new Vector2( 1, 2 ), new Vector2( 2, 2 ), new Vector2( 0, 1 ), new Vector2( 1, 1 ) ],
            [ new Vector2( 0, 0 ), new Vector2( 0, 1 ), new Vector2( 1, 1 ), new Vector2( 1, 2 ) ]
        ],
        [ TetrominoType.Z ] =
        [
            [ new Vector2( 0, 1 ), new Vector2( 1, 1 ), new Vector2( 1, 0 ), new Vector2( 2, 0 ) ],
            [ new Vector2( 2, 1 ), new Vector2( 2, 2 ), new Vector2( 1, 2 ), new Vector2( 1, 3 ) ],
            [ new Vector2( 0, 2 ), new Vector2( 1, 2 ), new Vector2( 1, 1 ), new Vector2( 2, 1 ) ],
            [ new Vector2( 1, 0 ), new Vector2( 1, 1 ), new Vector2( 0, 1 ), new Vector2( 0, 2 ) ]
        ],
        [ TetrominoType.J ] =
        [
            [ new Vector2( 0, 1 ), new Vector2( 1, 1 ), new Vector2( 2, 1 ), new Vector2( 0, 0 ) ],
            [ new Vector2( 1, 0 ), new Vector2( 1, 1 ), new Vector2( 1, 2 ), new Vector2( 2, 0 ) ],
            [ new Vector2( 0, 1 ), new Vector2( 1, 1 ), new Vector2( 2, 1 ), new Vector2( 2, 2 ) ],
            [ new Vector2( 1, 0 ), new Vector2( 1, 1 ), new Vector2( 1, 2 ), new Vector2( 0, 2 ) ]
        ],
        [ TetrominoType.L ] =
        [
            [ new Vector2( 0, 1 ), new Vector2( 1, 1 ), new Vector2( 2, 1 ), new Vector2( 2, 0 ) ],
            [ new Vector2( 1, 0 ), new Vector2( 1, 1 ), new Vector2( 1, 2 ), new Vector2( 2, 2 ) ],
            [ new Vector2( 0, 1 ), new Vector2( 1, 1 ), new Vector2( 2, 1 ), new Vector2( 0, 2 ) ],
            [ new Vector2( 1, 0 ), new Vector2( 1, 1 ), new Vector2( 1, 2 ), new Vector2( 0, 0 ) ]
        ]
    };

    public static Vector2[] GetBlocks( TetrominoType type, int rotation )
    {
        if ( type == TetrominoType.None || !_shapes.TryGetValue( type, out Vector2[][]? value ) )
            return [ ];

        rotation %= 4;
        return value[ rotation ];
    }

    public static int GetColorIndex( TetrominoType type )
    {
        return type switch
        {
            TetrominoType.I => 1, // Cyan
            TetrominoType.O => 2, // Yellow
            TetrominoType.T => 3, // Purple
            TetrominoType.S => 4, // Green
            TetrominoType.Z => 5, // Red
            TetrominoType.J => 6, // Blue
            TetrominoType.L => 7, // Orange
            _ => 0
        };
    }

    public static Vector4 GetColor( int colorIndex )
    {
        if ( colorIndex == 0 )
            return new Vector4( 0.3f, 0.3f, 0.3f, 1 ); // Gray for empty

        return ColorPalette.GetColor( ColorPalette.Tetris, colorIndex - 1 );
    }

    public static TetrominoType GetRandomType()
    {
        TetrominoType[] types =
        [
            TetrominoType.I,
            TetrominoType.O,
            TetrominoType.T,
            TetrominoType.S,
            TetrominoType.Z,
            TetrominoType.J,
            TetrominoType.L
        ];
        return types[ Random.Shared.Next( types.Length ) ];
    }
}