using Aether.Core;
using Silk.NET.Maths;
using Graphics.Components;
using Graphics.Structures;

namespace Graphics.Systems;

public class TextRenderSystem : SystemBase
{
    public List<Entity> RenderText(
        string text,
        Vector3D<float> position,
        float scale = 1f,
        Vector4D<float>? color = null,
        bool flipY = true )
    {
        if ( !World.HasGlobal<Font>() || !World.HasGlobal<FontSystem>() )
            return new List<Entity>();

        Font font = World.GetGlobal<Font>();
        FontSystem fontSystem = World.GetGlobal<FontSystem>();
        MaterialSystem? materialSystem = World.GetSystem<MaterialSystem>();

        if ( materialSystem == null )
            return new List<Entity>();

        Vector4D<float> textColor = color ?? new Vector4D<float>( 1f, 1f, 1f, 1f );
        List<Entity> entities = new();

        float cursorX = position.X;
        float cursorY = position.Y;

        foreach ( char c in text )
        {
            if ( c == '\n' )
            {
                cursorX = position.X;
                cursorY -= font.LineHeight * scale;
                continue;
            }

            if ( c == ' ' )
            {
                ( Glyph spaceGlyph, _ ) = fontSystem.GetGlyph( ref font, ' ' );
                cursorX += spaceGlyph.Advance * scale;
                continue;
            }

            ( Glyph glyph, uint textureHandle ) = fontSystem.GetGlyph( ref font, c );

            Texture2D glyphTexture = new()
            {
                Texture = TextureObject.FromHandle(
                    WindowBase.Gl,
                    textureHandle,
                    ( int )glyph.Size.X,
                    ( int )glyph.Size.Y )
            };

            Material material = materialSystem.CreateTextured( glyphTexture, textColor );
            Sprite sprite = Sprite.Create( material, glyph.Size * scale );
            sprite.Pivot = new Vector2D<float>( 0, 0.5f );
            sprite.FlipY = flipY;

            Entity charEntity = World.Spawn();
            World.Add( charEntity, new Transform
            {
                Position = new Vector3D<float>( cursorX, cursorY, position.Z ),
                Rotation = Quaternion<float>.Identity,
                Scale = Vector3D<float>.One
            } );
            World.Add( charEntity, sprite );

            entities.Add( charEntity );
            cursorX += glyph.Advance * scale;
        }

        World.SetGlobal( font );
        return entities;
    }

    public (float width, float height) MeasureText( string text, float scale = 1f )
    {
        if ( !World.HasGlobal<Font>() || !World.HasGlobal<FontSystem>() )
            return ( 0, 0 );

        Font font = World.GetGlobal<Font>();
        FontSystem fontSystem = World.GetGlobal<FontSystem>();

        float width = 0;
        float height = font.LineHeight * scale;

        foreach ( char c in text )
        {
            if ( c == '\n' )
            {
                height += font.LineHeight * scale;
                continue;
            }

            ( Glyph glyph, _ ) = fontSystem.GetGlyph( ref font, c );
            width += glyph.Advance * scale;
        }

        return ( width, height );
    }

    public List<Entity> RenderTextCentered(
        string text,
        Vector3D<float> centerPosition,
        float scale = 1f,
        Vector4D<float>? color = null,
        bool flipY = true )
    {
        ( float width, float height ) = MeasureText( text, scale );
        Vector3D<float> position = new Vector3D<float>(
            centerPosition.X - width / 2f,
            centerPosition.Y,
            centerPosition.Z
        );

        return RenderText( text, position, scale, color, flipY );
    }
}