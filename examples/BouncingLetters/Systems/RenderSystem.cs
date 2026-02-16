using Aether.Core;
using Aether.Core.Systems;
using BouncingLetters.Components;
using System.Numerics;
using Aether.Core.Structures;
using Graphics;
using Graphics.Components;
using Graphics.Textures;
using Graphics.Windowing;
using Silk.NET.OpenGL;
using Shader = Graphics.Shaders.Shader;

namespace BouncingLetters.Systems;

public class RenderSystem : SystemBase
{
    private Renderer2D _renderer = null!;
    private Shader _shader = null!;
    private Texture2D _whiteTexture2D = null!;
    private readonly Dictionary<char, (QuadVertex[] vertices, uint[] indices)> _letterGeometry = new();

    protected override void OnInit()
    {
        _whiteTexture2D = World.GetGlobal<Texture2D>();
        _renderer = World.GetGlobal<Renderer2D>();
        _shader = World.GetGlobal<Shader>();

        _letterGeometry[ 'С' ] = MeshBuilder.CreateLetterC();
        _letterGeometry[ 'К' ] = MeshBuilder.CreateLetterK();
        _letterGeometry[ 'А' ] = MeshBuilder.CreateLetterA();
    }

    protected override void OnRender()
    {
        GL gl = MainWindow.Gl;
        gl.ClearColor( 0.1f, 0.1f, 0.15f, 1.0f );
        gl.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

        Camera camera = default;

        foreach ( Entity e in World.Filter<Camera>().With<Transform>() )
        {
            World.Get<Transform>( e );
            camera = World.Get<Camera>( e );
            break;
        }

        Matrix4x4 viewProjection = camera.ViewProjectionMatrix;
        _renderer.Begin( viewProjection, _shader );

        foreach ( Entity e in World.Filter<Letter>().With<Transform>() )
        {
            ref Letter letter = ref World.Get<Letter>( e );
            ref Transform transform = ref World.Get<Transform>( e );

            if ( !_letterGeometry.TryGetValue( letter.Character,
                    out (QuadVertex[] vertices, uint[] indices) geometry ) )
                continue;

            QuadVertex[] transformedVertices = TransformVertices(
                geometry.vertices,
                transform.WorldMatrix,
                GetLetterColor( letter.Character, transform.Position.Y )
            );

            _renderer.SubmitVertices( transformedVertices, geometry.indices, _whiteTexture2D );
        }

        _renderer.End();
    }


    private static Vector4 GetLetterColor( char character, float yPosition )
    {
        float t = MathF.Max( 0, MathF.Min( 1, ( yPosition + 3f ) / 8f ) );

        Vector3 color = character switch
        {
            'С' => Vector3.Lerp(
                new Vector3( 0.8f, 0.2f, 0.2f ),
                new Vector3( 1.0f, 0.8f, 0.2f ),
                t
            ),
            'К' => Vector3.Lerp(
                new Vector3( 0.2f, 0.6f, 0.2f ),
                new Vector3( 0.2f, 0.8f, 1.0f ),
                t
            ),
            'А' => Vector3.Lerp(
                new Vector3( 0.4f, 0.2f, 0.8f ),
                new Vector3( 1.0f, 0.4f, 0.8f ),
                t
            ),
            _ => Vector3.One
        };

        return new Vector4( color, 1.0f );
    }

    private static QuadVertex[] TransformVertices( QuadVertex[] vertices, Matrix4x4 transform, Vector4 color )
    {
        QuadVertex[] result = new QuadVertex[ vertices.Length ];
        for ( int i = 0; i < vertices.Length; i++ )
        {
            Vector3 transformedPos = Vector3.Transform( vertices[ i ].Position, transform );
            result[ i ] = new QuadVertex( transformedPos, vertices[ i ].Uv, color );
        }

        return result;
    }
}