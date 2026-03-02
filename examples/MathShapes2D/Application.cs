using Silk.NET.Maths;
using Aether.Core;
using Aether.Core.Enums;
using Graphics;
using Graphics.Components;
using Graphics.Systems;
using MathShapes2D.Components;
using MathShapes2D.Systems;

namespace MathShapes2D;

public class Application() : ApplicationBase(
    title: "Math Shapes 2D",
    width: 1280,
    height: 720 )
{
    private Shader? _circleShader;
    private Shader? _flagShader;
    private Shader? _geometryCircleShader;

    protected override void OnInitialize()
    {
        ShaderSystem shaderSystem = new( WindowBase.Gl );
        TextureSystem textureSystem = new( WindowBase.Gl );
        MaterialSystem materialSystem = new();
        InputSystem inputSystem = new();
        MeshSystem meshSystem = new( WindowBase.Gl );

        Texture2D whiteTexture = textureSystem.CreateTextureFromColor( 1, 1 );

        Input input = inputSystem.CreateInput( WindowBase.Input );

        World.SetGlobal( shaderSystem );
        World.SetGlobal( textureSystem );
        World.SetGlobal( inputSystem );
        World.SetGlobal( meshSystem );
        World.SetGlobal( materialSystem );
        World.SetGlobal( whiteTexture );
        World.SetGlobal( input );

        _circleShader = shaderSystem.CreateShader(
            "examples/MathShapes2D/Shaders/circle.vert",
            "examples/MathShapes2D/Shaders/circle.frag"
        );

        _flagShader = shaderSystem.CreateShader(
            "examples/MathShapes2D/Shaders/flag.vert",
            "examples/MathShapes2D/Shaders/flag.frag"
        );

        _geometryCircleShader = shaderSystem.CreateShader(
            "examples/MathShapes2D/Shaders/geometry_circle.vert",
            "examples/MathShapes2D/Shaders/geometry_circle.frag",
            "examples/MathShapes2D/Shaders/geometry_circle.geom"
        );

        World.AddSystem( shaderSystem );
        World.AddSystem( inputSystem );
        World.AddSystem( new CameraSystem() );
        World.AddSystem( materialSystem );

        World.AddSystem( new CameraMovementSystem() );
        World.AddSystem( new CircleSystem() );
        World.AddSystem( new StarSystem() );
        World.AddSystem( new GeometryCircleSystem() );

        World.AddSystem( meshSystem );
        
        CameraSystem.CreateOrthographicCamera(
            World,
            position: new Vector3D<float>( 0f, 0f, 5f ),
            size: 5f,
            aspectRatio: ( float )WindowBase.LogicalWidth / WindowBase.LogicalHeight
        );

        CreateCircleScene();
        CreateStarScene();
        CreateGeometryCircleScene();
    }

    private void CreateCircleScene()
    {
        Entity entity = World.Spawn();

        World.Add( entity, new Transform
        {
            Position = new Vector3D<float>( 0f, 0f, 0f ),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );

        Circle circle = new()
        {
            Segments = 200,
            IsGenerated = false
        };
        World.Add( entity, circle );

        World.Add( entity, new Material
        {
            Shader = _circleShader,
            DiffuseColor = new Vector3D<float>( 1f, 1f, 1f ),
            AmbientColor = new Vector3D<float>( 1f, 1f, 1f ),
            SpecularColor = Vector3D<float>.Zero,
            Shininess = 1f,
            Alpha = 1f
        } );
    }

    private void CreateStarScene()
    {
        Entity entity = World.Spawn();

        World.Add( entity, new Transform
        {
            Position = new Vector3D<float>( 8f, 0f, 0f ),
            Rotation = Quaternion<float>.Identity,
            Scale = new Vector3D<float>( 3f, 3f, 1f )
        } );

        Star flag = new()
        {
            OuterRadius = 0.8f,
            InnerRadius = 0.35f,
            IsGenerated = false
        };
        World.Add( entity, flag );

        World.Add( entity, new Material
        {
            Shader = _flagShader,
            DiffuseColor = new Vector3D<float>( 1f, 1f, 1f ),
            AmbientColor = new Vector3D<float>( 1f, 1f, 1f ),
            SpecularColor = Vector3D<float>.Zero,
            Shininess = 1f,
            Alpha = 1f
        } );
    }

    private void CreateGeometryCircleScene()
    {
        Entity entity = World.Spawn();

        World.Add( entity, new Transform
        {
            Position = new Vector3D<float>( 16f, 0f, 0f ),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );

        GeometryCircle circle = new()
        {
            IsGenerated = false
        };
        World.Add( entity, circle );

        World.Add( entity, new Material
        {
            Shader = _geometryCircleShader,
            DiffuseColor = new Vector3D<float>( 1f, 1f, 0f ),
            AmbientColor = new Vector3D<float>( 1f, 1f, 0f ),
            SpecularColor = Vector3D<float>.Zero,
            Shininess = 1f,
            Alpha = 1f,
            SetCustomUniforms = shader =>
            {
                shader.TrySetUniform( "uRadius", 1.5f );
            }
        } );
    }
}