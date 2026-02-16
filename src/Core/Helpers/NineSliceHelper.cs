using System.Numerics;
using Aether.Core.Structures;

namespace Aether.Core.Helpers;

public static class NineSliceHelper
{
    public static SliceQuad[] CalculateSlices( Vector2 position, Vector2 size, int borderPixels, int textureSize )
    {
        float borderUv = ( float )borderPixels / textureSize;

        // World space border size
        float worldBorder = ( float )borderPixels / textureSize * size.X;

        // Clamp border if size is too small
        if ( worldBorder * 2 > size.X ) worldBorder = size.X / 2;
        if ( worldBorder * 2 > size.Y ) worldBorder = size.Y / 2;

        // Position coordinates
        float x0 = position.X;
        float x1 = position.X + worldBorder;
        float x2 = position.X + size.X - worldBorder;
        float x3 = position.X + size.X;

        float y0 = position.Y;
        float y1 = position.Y + worldBorder;
        float y2 = position.Y + size.Y - worldBorder;
        float y3 = position.Y + size.Y;

        // UV coordinates
        const float u0 = 0f;
        float u2 = 1f - borderUv;
        const float u3 = 1f;

        const float v0 = 0f;
        float v2 = 1f - borderUv;
        const float v3 = 1f;

        // Return 9 quads in order: top-left, top-center, top-right,
        // middle-left, center, middle-right,
        // bottom-left, bottom-center, bottom-right
        return
        [
            // Top row
            new SliceQuad( new Vector2( x0, y2 ), new Vector2( x1, y3 ), new Vector2( u0, v2 ),
                new Vector2( borderUv, v3 ) ),
            new SliceQuad( new Vector2( x1, y2 ), new Vector2( x2, y3 ), new Vector2( borderUv, v2 ),
                new Vector2( u2, v3 ) ),
            new SliceQuad( new Vector2( x2, y2 ), new Vector2( x3, y3 ), new Vector2( u2, v2 ), new Vector2( u3, v3 ) ),

            // Middle row
            new SliceQuad( new Vector2( x0, y1 ), new Vector2( x1, y2 ), new Vector2( u0, borderUv ),
                new Vector2( borderUv, v2 ) ),
            new SliceQuad( new Vector2( x1, y1 ), new Vector2( x2, y2 ), new Vector2( borderUv, borderUv ),
                new Vector2( u2, v2 ) ),
            new SliceQuad( new Vector2( x2, y1 ), new Vector2( x3, y2 ), new Vector2( u2, borderUv ),
                new Vector2( u3, v2 ) ),

            // Bottom row
            new SliceQuad( new Vector2( x0, y0 ), new Vector2( x1, y1 ), new Vector2( u0, v0 ),
                new Vector2( borderUv, borderUv ) ),
            new SliceQuad( new Vector2( x1, y0 ), new Vector2( x2, y1 ), new Vector2( borderUv, v0 ),
                new Vector2( u2, borderUv ) ),
            new SliceQuad( new Vector2( x2, y0 ), new Vector2( x3, y1 ), new Vector2( u2, v0 ),
                new Vector2( u3, borderUv ) )
        ];
    }
}