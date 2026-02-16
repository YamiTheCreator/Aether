using Silk.NET.OpenGL;
using StbImageSharp;

namespace Graphics.Textures;

public class Texture2D : IDisposable
{
    private readonly uint _handle;
    public int Width { get; }
    public int Height { get; }
    private readonly GL _gl;

    public unsafe Texture2D(
        GL gl,
        string path,
        TextureWrapMode wrapS = TextureWrapMode.Repeat,
        TextureWrapMode wrapT = TextureWrapMode.Repeat,
        TextureMinFilter minFilter = TextureMinFilter.LinearMipmapLinear,
        TextureMagFilter magFilter = TextureMagFilter.Linear,
        bool generateMipmaps = true )
    {
        _gl = gl;
        _handle = _gl.GenTexture();
        Bind();
        using ( FileStream stream = File.OpenRead( path ) )
        {
            ImageResult image = ImageResult.FromStream( stream, ColorComponents.RedGreenBlueAlpha );
            if ( image.Width == 0 || image.Height == 0 )
            {
                throw new Exception( "Failed to load image from path: " + path );
            }

            fixed ( byte* ptr = image.Data )
            {
                _gl.TexImage2D(
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

        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            ( int )wrapS
        );
        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            ( int )wrapT
        );
        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            ( int )minFilter
        );
        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            ( int )magFilter
        );
        if ( generateMipmaps )
        {
            _gl.GenerateMipmap( TextureTarget.Texture2D );
        }

        _gl.BindTexture( TextureTarget.Texture2D, 0 ); // Unbind for safety
        CheckGlError( "Texture2D creation from file" );
    }

    public unsafe Texture2D(
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
        _gl = gl;
        _handle = _gl.GenTexture();
        Bind();
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
            _gl.TexImage2D(
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

        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            ( int )wrapS
        );
        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            ( int )wrapT
        );
        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            ( int )minFilter
        );
        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            ( int )magFilter
        );
        if ( generateMipmaps )
        {
            _gl.GenerateMipmap( TextureTarget.Texture2D );
        }

        _gl.BindTexture( TextureTarget.Texture2D, 0 );
        CheckGlError( "Texture2D creation from color" );
    }

    public Texture2D( GL gl, uint handle, int width, int height )
    {
        _gl = gl;
        _handle = handle;
        Width = width;
        Height = height;
    }

    public void Bind( TextureUnit unit = TextureUnit.Texture0 )
    {
        _gl.ActiveTexture( unit );
        _gl.BindTexture( TextureTarget.Texture2D, _handle );
    }

    public void Dispose()
    {
        _gl.DeleteTexture( _handle );
    }

    private void CheckGlError( string operation )
    {
        GLEnum error;
        while ( ( error = _gl.GetError() ) != GLEnum.NoError )
        {
            Console.WriteLine( $"GL Error during {operation}: {error}" );
        }
    }
}