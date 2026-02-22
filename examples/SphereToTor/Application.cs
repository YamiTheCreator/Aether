using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Aether.Core;
using Graphics;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using SphereToTor.Components;
using SphereToTor.Systems;
using Shader = Graphics.Components.Shader;

namespace SphereToTor;

public class Application() : ApplicationBase(
    title: "Sphere to Torus Morphing",
    width: 1280,
    height: 720,
    createDefaultCamera: false )
{
    private const float _morphDuration = 2f;

    protected override void OnInitialize()
    {
        ShaderSystem shaderSystem = new( WindowBase.Gl );
        TextureSystem textureSystem = new( WindowBase.Gl );
        MeshSystem meshSystem = new( WindowBase.Gl );
        MaterialSystem materialSystem = new();
        InputSystem inputSystem = new();

        Texture2D whiteTexture = textureSystem.CreateTextureFromColor( 1, 1 );

        Shader morphShader = shaderSystem.CreateShader(
            "examples/SphereToTor/Shaders/morph.vert",
            "examples/SphereToTor/Shaders/morph.frag" );

        Input input = inputSystem.CreateInput( WindowBase.Input );

        World.SetGlobal( shaderSystem );
        World.SetGlobal( textureSystem );
        World.SetGlobal( meshSystem );
        World.SetGlobal( materialSystem );
        World.SetGlobal( inputSystem );
        World.SetGlobal( morphShader );
        World.SetGlobal( input );
        World.SetGlobal( whiteTexture );

        World.AddSystem( shaderSystem );
        World.AddSystem( textureSystem );
        World.AddSystem( materialSystem );
        World.AddSystem( new CameraSystem() );
        World.AddSystem( new OrbitCameraSystem() );
        World.AddSystem( new MorphUpdateSystem() );
        World.AddSystem( new MorphRenderSystem( WindowBase.Gl ) );

        Entity cameraEntity = World.Spawn();
        World.Add( cameraEntity, Camera.CreateOrbit(
            target: Vector3D<float>.Zero,
            distance: 4f,
            yaw: 45f,
            pitch: 20f,
            fov: 60f,
            aspectRatio: ( float )WindowBase.LogicalWidth / WindowBase.LogicalHeight
        ) );

        World.Add( cameraEntity, new Transform
        {
            Position = Vector3D<float>.Zero,
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );

        CreateMorphingSurface( meshSystem, morphShader );
    }

    private void CreateMorphingSurface( MeshSystem meshSystem, Shader morphShader )
    {
        Entity entity = World.Spawn();

        World.Add( entity, new Transform
        {
            Position = Vector3D<float>.Zero,
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );

        // Создаем сетку на XOY
        int segments = 64;
        int rings = 32;
        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        Vector4D<float> white = new( 1, 1, 1, 1 );
        Vector3D<float> normal = new( 0, 0, 1 );

        // Используем UV координаты текстур чтобы показать параметрические координаты
        for ( int ring = 0; ring <= rings; ring++ )
        {
            float v = ( float )ring / rings;

            for ( int segment = 0; segment <= segments; segment++ )
            {
                float u = ( float )segment / segments;

                float x = ( u - 0.5f ) * 2f;
                float y = ( v - 0.5f ) * 2f;

                Vector3D<float> pos = new( x, y, 0f );
                Vector2D<float> uv = new( u, v );

                vertices.Add( new Vertex( pos, uv, white, 0, normal ) );
            }
        }

        // Индексы для вайрфрейма
        for ( int ring = 0; ring <= rings; ring++ )
        {
            for ( int segment = 0; segment < segments; segment++ )
            {
                uint current = ( uint )( ring * ( segments + 1 ) + segment );
                uint next = current + 1;

                indices.Add( current );
                indices.Add( next );
            }
        }

        for ( int segment = 0; segment <= segments; segment++ )
        {
            for ( int ring = 0; ring < rings; ring++ )
            {
                uint current = ( uint )( ring * ( segments + 1 ) + segment );
                uint next = ( uint )( ( ring + 1 ) * ( segments + 1 ) + segment );

                indices.Add( current );
                indices.Add( next );
            }
        }

        Material material = new()
        {
            Shader = morphShader,
            DiffuseColor = new Vector3D<float>( 0.2f, 0.8f, 1.0f ), // Синий
            Alpha = 1f,
            SetCustomUniforms = shader =>
            {
                // Initial setup - will be updated by MorphRenderSystem
                shader.TrySetUniform( "uMorphFactor", 0f );
                shader.TrySetUniform( "uTorusRadiusMajor", 1.0f );
                shader.TrySetUniform( "uTorusRadiusMinor", 0.4f );
                shader.TrySetUniform( "uSphereRadius", 1.0f );
            }
        };

        Mesh mesh = meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray(), material );
        mesh.Topology = PrimitiveType.Lines;

        World.Add( entity, mesh );
        World.Add( entity, new MorphComponent
        {
            Time = 0f,
            Duration = _morphDuration,
            IsPlaying = false,
            IsForward = true,
            TorusRadiusMajor = 1.0f,
            TorusRadiusMinor = 0.4f,
            SphereRadius = 1.0f
        } );
    }
}