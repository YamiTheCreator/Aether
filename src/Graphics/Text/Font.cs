using System.Numerics;
using Graphics.Textures;
using Silk.NET.OpenGL;
using StbTrueTypeSharp;
using CharGlyph = Aether.Core.Structures.Glyph;

namespace Graphics.Text;

public class Font : IDisposable
{
    private readonly GL _gl;
    private readonly StbTrueType.stbtt_fontinfo _fontInfo;
    private readonly float _scale;
    private readonly Dictionary<char, (CharGlyph glyph, Texture2D texture)> _glyphCache = new();

    public float LineHeight { get; }

    public unsafe Font( GL gl, float fontSize = 48f, string fontPath = "src/Graphics/Text/Fonts/Strogo.ttf" )
    {
        _gl = gl;

        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string solutionRoot = Path.GetFullPath( Path.Combine( basePath, "../../../../../" ) );
        string fullFontPath = Path.Combine( solutionRoot, fontPath );

        if ( !File.Exists( fullFontPath ) )
            throw new FileNotFoundException( $"Font not found: {fullFontPath}" );

        byte[] fontData = File.ReadAllBytes( fullFontPath );
        _fontInfo = StbTrueType.CreateFont( fontData, 0 );
        _scale = StbTrueType.stbtt_ScaleForPixelHeight( _fontInfo, fontSize );

        int ascent = 0, descent = 0, lineGap = 0;
        StbTrueType.stbtt_GetFontVMetrics( _fontInfo, &ascent, &descent, &lineGap );
        LineHeight = ( ascent - descent ) * _scale;

        _gl.PixelStore( PixelStoreParameter.UnpackAlignment, 1 );
    }

    public unsafe (CharGlyph glyph, Texture2D texture) GetGlyph( char c )
    {
        if ( _glyphCache.TryGetValue( c, out (CharGlyph glyph, Texture2D texture) cached ) )
            return cached;

        int advanceWidth = 0, leftSideBearing = 0;
        StbTrueType.stbtt_GetCodepointHMetrics( _fontInfo, c, &advanceWidth, &leftSideBearing );

        int x0 = 0, y0 = 0, x1 = 0, y1 = 0;
        StbTrueType.stbtt_GetCodepointBitmapBox( _fontInfo, c, _scale, _scale, &x0, &y0, &x1, &y1 );

        int width = x1 - x0;
        int height = y1 - y0;

        CharGlyph glyph = new()
        {
            Size = new Vector2( width, height ),
            Bearing = new Vector2( x0, -y1 ),
            Advance = advanceWidth * _scale,
            UvMin = Vector2.Zero,
            UvMax = Vector2.One
        };

        Texture2D texture = CreateGlyphTexture( c, width, height );

        _glyphCache[ c ] = ( glyph, texture );
        return ( glyph, texture );
    }

    private unsafe Texture2D CreateGlyphTexture( char c, int width, int height )
    {
        if ( width <= 0 || height <= 0 )
        {
            ReadOnlySpan<byte> emptyData = "\0\0\0\0"u8;
            uint handle = _gl.GenTexture();
            _gl.BindTexture( TextureTarget.Texture2D, handle );

            fixed ( byte* ptr = emptyData )
            {
                _gl.TexImage2D( TextureTarget.Texture2D, 0, InternalFormat.Rgba, 1, 1, 0,
                    PixelFormat.Rgba, PixelType.UnsignedByte, ptr );
            }

            SetTextureParameters();
            _gl.BindTexture( TextureTarget.Texture2D, 0 );
            return new Texture2D( _gl, handle, 1, 1 );
        }

        int bitmapSize = width * height;
        int rgbaSize = bitmapSize * 4;
        
        // Use stackalloc for small glyphs (< 256KB), otherwise allocate on heap
        Span<byte> bitmap = bitmapSize <= 65536 
            ? stackalloc byte[ bitmapSize ] 
            : new byte[ bitmapSize ];
        
        Span<byte> rgbaData = rgbaSize <= 65536 
            ? stackalloc byte[ rgbaSize ] 
            : new byte[ rgbaSize ];

        fixed ( byte* bitmapPtr = bitmap )
        {
            StbTrueType.stbtt_MakeCodepointBitmap( _fontInfo, bitmapPtr, width, height, width, _scale, _scale, c );
        }

        for ( int y = 0; y < height; y++ )
        {
            for ( int x = 0; x < width; x++ )
            {
                int srcIndex = y * width + x;
                int dstIndex = ( height - 1 - y ) * width + x; // Flip Y
                byte alpha = bitmap[ srcIndex ];
                rgbaData[ dstIndex * 4 + 0 ] = 255; // R
                rgbaData[ dstIndex * 4 + 1 ] = 255; // G
                rgbaData[ dstIndex * 4 + 2 ] = 255; // B
                rgbaData[ dstIndex * 4 + 3 ] = alpha; // A
            }
        }

        uint textureHandle = _gl.GenTexture();
        _gl.BindTexture( TextureTarget.Texture2D, textureHandle );

        fixed ( byte* ptr = rgbaData )
        {
            _gl.TexImage2D( TextureTarget.Texture2D, 0, InternalFormat.Rgba,
                ( uint )width, ( uint )height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr );
        }

        SetTextureParameters();
        _gl.BindTexture( TextureTarget.Texture2D, 0 );

        return new Texture2D( _gl, textureHandle, width, height );
    }

    private void SetTextureParameters()
    {
        _gl.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
            ( int )TextureWrapMode.ClampToEdge );
        _gl.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
            ( int )TextureWrapMode.ClampToEdge );
        _gl.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            ( int )TextureMinFilter.Linear );
        _gl.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            ( int )TextureMagFilter.Linear );
    }

    public (float width, float height) MeasureText( string text )
    {
        float width = 0;
        float height = LineHeight;

        foreach ( char c in text )
        {
            if ( c == '\n' )
            {
                height += LineHeight;
                continue;
            }

            ( CharGlyph glyph, _ ) = GetGlyph( c );
            width += glyph.Advance;
        }

        return ( width, height );
    }

    public void Dispose()
    {
        foreach ( ( CharGlyph _, Texture2D texture ) in _glyphCache.Values )
        {
            texture.Dispose();
        }

        _glyphCache.Clear();
    }
}