using System.Numerics;
using Aether.Core;
using Aether.Core.Structures;
using Aether.Core.Systems;
using Graphics;
using Graphics.Components;
using Graphics.Text;
using Graphics.Textures;
using Silk.NET.OpenGL;
using Shader = Graphics.Shaders.Shader;

namespace GameUtils.Systems;

/// <summary>
/// Base class for rendering grid-based games
/// </summary>
public abstract class GridRenderSystemBase : SystemBase
{
    protected Renderer2D Renderer = null!;
    protected Shader Shader = null!;
    protected Font Font = null!;
    protected Texture2D WhiteTexture = null!;

    protected override void OnInit()
    {
        Renderer = World.GetGlobal<Renderer2D>();
        Shader = World.GetGlobal<Shader>();
        Font = World.GetGlobal<Font>();
        WhiteTexture = World.GetGlobal<Texture2D>();
    }

    protected override void OnRender()
    {
        GL gl = Graphics.Windowing.MainWindow.Gl;
        gl.ClearColor( 0.1f, 0.1f, 0.15f, 1.0f );
        gl.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

        gl.Enable( EnableCap.Blend );
        gl.BlendFunc( BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha );
        gl.Disable( EnableCap.DepthTest );

        Matrix4x4 viewProjection = GetCameraViewProjection();

        Renderer.Begin( viewProjection, Shader );
        RenderGame();
        Renderer.End();
    }

    protected abstract void RenderGame();

    protected Matrix4x4 GetCameraViewProjection()
    {
        foreach ( Entity e in World.Filter<Camera>() )
        {
            Camera camera = World.Get<Camera>( e );
            return camera.ViewProjectionMatrix;
        }

        return Matrix4x4.Identity;
    }

    /// <summary>
    /// Renders a simple colored quad
    /// </summary>
    protected void RenderQuad( float x, float y, float width, float height, Vector4 color )
    {
        Vector3 pos = new( x, y, 0 );

        QuadVertex[] vertices =
        [
            new( pos, new Vector2( 0, 0 ), color ),
            new( pos + new Vector3( width, 0, 0 ), new Vector2( 1, 0 ), color ),
            new( pos + new Vector3( width, height, 0 ), new Vector2( 1, 1 ), color ),
            new( pos + new Vector3( 0, height, 0 ), new Vector2( 0, 1 ), color )
        ];

        Renderer.SubmitQuad( vertices, WhiteTexture );
    }

    /// <summary>
    /// Renders a textured quad
    /// </summary>
    protected void RenderTexturedQuad( float x, float y, float width, float height, Vector4 color,
        Texture2D texture )
    {
        Vector3 pos = new( x, y, 0 );

        QuadVertex[] vertices =
        [
            new( pos, new Vector2( 0, 0 ), color ),
            new( pos + new Vector3( width, 0, 0 ), new Vector2( 1, 0 ), color ),
            new( pos + new Vector3( width, height, 0 ), new Vector2( 1, 1 ), color ),
            new( pos + new Vector3( 0, height, 0 ), new Vector2( 0, 1 ), color )
        ];

        Renderer.SubmitQuad( vertices, texture );
    }

    /// <summary>
    /// Renders a textured quad with flipped Y coordinates (for upside-down textures)
    /// </summary>
    protected void RenderTexturedQuadFlipped( float x, float y, float width, float height, Vector4 color,
        Texture2D texture )
    {
        Vector3 pos = new( x, y, 0 );

        QuadVertex[] vertices =
        [
            new( pos, new Vector2( 0, 1 ), color ),
            new( pos + new Vector3( width, 0, 0 ), new Vector2( 1, 1 ), color ),
            new( pos + new Vector3( width, height, 0 ), new Vector2( 1, 0 ), color ),
            new( pos + new Vector3( 0, height, 0 ), new Vector2( 0, 0 ), color )
        ];

        Renderer.SubmitQuad( vertices, texture );
    }
}
