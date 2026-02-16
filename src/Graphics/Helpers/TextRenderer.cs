using System.Numerics;
using Aether.Core.Structures;
using Graphics.Text;
using Graphics.Textures;
using CharGlyph = Aether.Core.Structures.Glyph;

namespace Graphics.Helpers;

public static class TextRenderer
{
    private static void RenderText(
        Renderer2D renderer,
        Font font,
        string text,
        Vector3 position,
        float scale,
        Vector4 color )
    {
        Vector3 cursor = position;

        foreach ( char c in text )
        {
            if ( c == '\n' )
            {
                cursor.X = position.X;
                cursor.Y -= font.LineHeight * scale;
                continue;
            }

            if ( c == ' ' )
            {
                ( CharGlyph spaceGlyph, _ ) = font.GetGlyph( ' ' );
                cursor.X += spaceGlyph.Advance * scale;
                continue;
            }

            ( CharGlyph glyph, Texture2D texture ) = font.GetGlyph( c );

            Vector3 glyphPos = new(
                cursor.X,
                cursor.Y - glyph.Size.Y * scale / 2f,
                cursor.Z
            );

            Vector2 size = glyph.Size * scale;
            Span<QuadVertex> vertices =
            [
                new( glyphPos, glyph.UvMin, color ),
                new( glyphPos + new Vector3( size.X, 0, 0 ), new Vector2( glyph.UvMax.X, glyph.UvMin.Y ), color ),
                new( glyphPos + new Vector3( size.X, size.Y, 0 ), glyph.UvMax, color ),
                new( glyphPos + new Vector3( 0, size.Y, 0 ), new Vector2( glyph.UvMin.X, glyph.UvMax.Y ), color )
            ];

            renderer.SubmitQuad( vertices, texture );
            cursor.X += glyph.Advance * scale;
        }
    }

    public static void RenderText(
        Renderer2D renderer,
        Font font,
        string text,
        float x,
        float y,
        float scale,
        Vector4 color )
    {
        RenderText( renderer, font, text, new Vector3( x, y, 0 ), scale, color );
    }

    public static void RenderTextCentered(
        Renderer2D renderer,
        Font font,
        string text,
        float boxX,
        float boxY,
        float boxWidth,
        float boxHeight,
        float scale,
        Vector4 color )
    {
        ( float textWidth, float textHeight ) = font.MeasureText( text );
        textWidth *= scale;
        textHeight *= scale;

        float x = boxX + ( boxWidth - textWidth ) / 2f;
        float y = boxY + ( boxHeight - textHeight ) / 2f;

        RenderText( renderer, font, text, x, y, scale, color );
    }

    public static void RenderTextCentered(
        Renderer2D renderer,
        Font font,
        string text,
        Vector2 boxPosition,
        Vector2 boxSize,
        float scale,
        Vector4 color )
    {
        RenderTextCentered( renderer, font, text,
            boxPosition.X, boxPosition.Y,
            boxSize.X, boxSize.Y,
            scale, color );
    }

    public static void RenderTextAligned(
        Renderer2D renderer,
        Font font,
        string text,
        Vector3 position,
        float scale,
        Vector4 color,
        TextAlignment alignment )
    {
        if ( alignment != TextAlignment.Left )
        {
            ( float width, float _ ) = font.MeasureText( text );
            float offset = alignment == TextAlignment.Center ? width * scale / 2f : width * scale;
            position.X -= offset;
        }

        RenderText( renderer, font, text, position, scale, color );
    }
}

public enum TextAlignment
{
    Left,
    Center,
    Right
}