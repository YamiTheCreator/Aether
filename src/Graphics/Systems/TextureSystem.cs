using Silk.NET.OpenGL;
using Silk.NET.Maths;
using Aether.Core;
using Graphics.Structures;
using Texture2DComponent = Graphics.Components.Texture2D;

namespace Graphics.Systems;

public class TextureSystem( GL gl ) : SystemBase
{
    public Texture2DComponent CreateTextureFromFile(
        string path,
        TextureWrapMode wrapS = TextureWrapMode.Repeat,
        TextureWrapMode wrapT = TextureWrapMode.Repeat,
        TextureMinFilter minFilter = TextureMinFilter.LinearMipmapLinear,
        TextureMagFilter magFilter = TextureMagFilter.Linear,
        bool generateMipmaps = true )
    {
        TextureObject texture = TextureObject.FromFile(
            gl, path, wrapS, wrapT, minFilter, magFilter, generateMipmaps );

        return new Texture2DComponent
        {
            Texture = texture
        };
    }

    public Texture2DComponent CreateTextureFromColor(
        int width,
        int height,
        byte r = 255,
        byte g = 255,
        byte b = 255,
        byte a = 255,
        TextureWrapMode wrapS = TextureWrapMode.ClampToEdge,
        TextureWrapMode wrapT = TextureWrapMode.ClampToEdge,
        TextureMinFilter minFilter = TextureMinFilter.Linear,
        TextureMagFilter magFilter = TextureMagFilter.Linear,
        bool generateMipmaps = false )
    {
        TextureObject texture = TextureObject.FromColor(
            gl, width, height, r, g, b, a, wrapS, wrapT, minFilter, magFilter, generateMipmaps );

        return new Texture2DComponent
        {
            Texture = texture
        };
    }

    public void BindTexture( Texture2DComponent texture, TextureUnit unit = TextureUnit.Texture0 )
    {
        texture.Texture.Bind( unit );
    }

    public void DeleteTexture( Texture2DComponent texture )
    {
        texture.Texture.Dispose();
    }

    public Texture2DComponent CreateTextureFromPixels( byte[] pixels, int width, int height,
        TextureWrapMode wrapS = TextureWrapMode.Repeat,
        TextureWrapMode wrapT = TextureWrapMode.Repeat,
        TextureMinFilter minFilter = TextureMinFilter.LinearMipmapLinear,
        TextureMagFilter magFilter = TextureMagFilter.Linear,
        bool generateMipmaps = true )
    {
        uint handle = gl.GenTexture();
        gl.BindTexture( TextureTarget.Texture2D, handle );

        unsafe
        {
            fixed ( byte* ptr = pixels )
            {
                gl.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    InternalFormat.Rgba8,
                    ( uint )width,
                    ( uint )height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    ptr
                );
            }
        }

        gl.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, ( int )wrapS );
        gl.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapT, ( int )wrapT );
        gl.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, ( int )minFilter );
        gl.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, ( int )magFilter );

        if ( generateMipmaps )
        {
            gl.GenerateMipmap( TextureTarget.Texture2D );
        }

        gl.BindTexture( TextureTarget.Texture2D, 0 );

        TextureObject textureObj = TextureObject.FromHandle( gl, handle, width, height );

        return new Texture2DComponent
        {
            Texture = textureObj
        };
    }

    public Texture2DComponent CreateTextureFromMemory( byte[] data,
        TextureWrapMode wrapS = TextureWrapMode.Repeat,
        TextureWrapMode wrapT = TextureWrapMode.Repeat,
        TextureMinFilter minFilter = TextureMinFilter.LinearMipmapLinear,
        TextureMagFilter magFilter = TextureMagFilter.Linear,
        bool generateMipmaps = true )
    {
        TextureObject texture = TextureObject.FromMemory(
            gl, data, wrapS, wrapT, minFilter, magFilter, generateMipmaps );

        return new Texture2DComponent
        {
            Texture = texture
        };
    }
}