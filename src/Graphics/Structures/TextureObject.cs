using Silk.NET.OpenGL;
using StbImageSharp;

namespace Graphics.Structures;

public class TextureObject : IDisposable
{
    private readonly GL _gl;
    public uint Handle { get; }
    public int Width { get; }
    public int Height { get; }
    public string? FilePath { get; }

    private TextureObject( GL gl, uint handle, int width, int height, string? filePath = null )
    {
        _gl = gl;
        Handle = handle;
        Width = width;
        Height = height;
        FilePath = filePath;
    }

    public static unsafe TextureObject FromFile(
        GL gl,
        string path,
        TextureWrapMode wrapS = TextureWrapMode.Repeat,
        TextureWrapMode wrapT = TextureWrapMode.Repeat,
        TextureMinFilter minFilter = TextureMinFilter.LinearMipmapLinear,
        TextureMagFilter magFilter = TextureMagFilter.Linear,
        bool generateMipmaps = true )
    {
        uint handle = gl.GenTexture();
        gl.BindTexture( TextureTarget.Texture2D, handle );

        int width, height;
        using ( FileStream stream = File.OpenRead( path ) )
        {
            ImageResult image = ImageResult.FromStream( stream, ColorComponents.RedGreenBlueAlpha );
            if ( image.Width == 0 || image.Height == 0 )
            {
                throw new Exception( $"Failed to load image from path: {path}" );
            }

            width = image.Width;
            height = image.Height;

            fixed ( byte* ptr = image.Data )
            {
                gl.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    InternalFormat.Rgba,
                    ( uint )image.Width,
                    ( uint )image.Height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    ptr
                );
            }
        }

        SetTextureParameters( gl, wrapS, wrapT, minFilter, magFilter );

        if ( generateMipmaps )
        {
            gl.GenerateMipmap( TextureTarget.Texture2D );
        }

        gl.BindTexture( TextureTarget.Texture2D, 0 );

        return new TextureObject( gl, handle, width, height, path );
    }

    public static unsafe TextureObject FromColor(
        GL gl,
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
        uint handle = gl.GenTexture();
        gl.BindTexture( TextureTarget.Texture2D, handle );

        Span<byte> data = stackalloc byte[ width * height * 4 ];
        for ( int i = 0; i < data.Length; i += 4 )
        {
            data[ i ] = r;
            data[ i + 1 ] = g;
            data[ i + 2 ] = b;
            data[ i + 3 ] = a;
        }

        fixed ( byte* ptr = data )
        {
            gl.TexImage2D(
                TextureTarget.Texture2D,
                0,
                InternalFormat.Rgba,
                ( uint )width,
                ( uint )height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                ptr
            );
        }

        SetTextureParameters( gl, wrapS, wrapT, minFilter, magFilter );

        if ( generateMipmaps )
        {
            gl.GenerateMipmap( TextureTarget.Texture2D );
        }

        gl.BindTexture( TextureTarget.Texture2D, 0 );

        return new TextureObject( gl, handle, width, height );
    }

    public void Bind( TextureUnit unit = TextureUnit.Texture0 )
    {
        _gl.ActiveTexture( unit );
        _gl.BindTexture( TextureTarget.Texture2D, Handle );
    }

    public void Unbind()
    {
        _gl.BindTexture( TextureTarget.Texture2D, 0 );
    }

    public void Dispose()
    {
        _gl.DeleteTexture( Handle );
    }

    private static void SetTextureParameters(
        GL gl,
        TextureWrapMode wrapS,
        TextureWrapMode wrapT,
        TextureMinFilter minFilter,
        TextureMagFilter magFilter )
    {
        gl.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, ( int )wrapS );
        gl.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapT, ( int )wrapT );
        gl.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, ( int )minFilter );
        gl.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, ( int )magFilter );
    }
}