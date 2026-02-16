using System.Numerics;
using Aether.Core;
using BouncingLetters.Components;
using BouncingLetters.Systems;
using Graphics;
using Graphics.Components;
using Graphics.Shaders;
using Graphics.Systems;
using Graphics.Textures;
using Graphics.Windowing;

namespace BouncingLetters;

public class Application() : ApplicationBase(
    title: "Bouncing Letters",
    width: 1200,
    height: 800,
    createDefaultCamera: true )
{
    protected override void OnInitialize()
    {
        Renderer2D renderer = new();
        Shader shader = new( MainWindow.Gl );
        Texture2D whiteTexture = new( MainWindow.Gl, 1, 1 );

        World.SetGlobal( renderer );
        World.SetGlobal( shader );
        World.SetGlobal( whiteTexture );

        World.AddSystem( new CameraSystem() );
        World.AddSystem( new GravitySystem() );
        World.AddSystem( new RenderSystem() );

        CreateLetter( 'С', new Vector3( -3, 0, 0 ), new Vector3( 0, 2, 0 ) );
        CreateLetter( 'К', new Vector3( 0, 2, 0 ), new Vector3( 0, 2, 0 ) );
        CreateLetter( 'А', new Vector3( 3, 0, 0 ), new Vector3( 0, 2, 0 ) );
    }

    private void CreateLetter( char character, Vector3 position, Vector3 initialVelocity )
    {
        Entity entity = World.Spawn();

        Transform transform = new( position )
        {
            Scale = new Vector3( 3f, 3f, 3f )
        };

        World.Add( entity, transform );
        World.Add( entity, new Letter( character ) );
        World.Add( entity, new Physics( bounciness: 1.0f )
        {
            Velocity = initialVelocity
        } );
    }
}