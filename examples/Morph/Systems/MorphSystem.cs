using Silk.NET.OpenGL;
using Silk.NET.Maths;
using Silk.NET.Input;
using Aether.Core;
using Graphics.Components;
using Graphics.Structures;
using Graphics.Systems;
using SphereToTor.Components;

namespace SphereToTor.Systems;

public class MorphSystem( GL gl ) : SystemBase
{
    private InputSystem? _inputSystem;
    private Input? _input;

    protected override void OnCreate()
    {
        _inputSystem = World.GetGlobal<InputSystem>();
        _input = World.GetGlobal<Input>();
    }

    protected override void OnUpdate( float deltaTime )
    {
        if ( _inputSystem is null || _input is null )
            return;

        foreach ( Entity entity in World.Filter<Morph>() )
        {
            ref Morph morph = ref World.Get<Morph>( entity );

            HandleInput( ref morph );
            UpdateMorphTime( ref morph, deltaTime );
        }
    }

    private void HandleInput( ref Morph morph )
    {
        if ( _inputSystem!.IsKeyPressed( _input!, Key.R ) )
        {
            morph.Time = 0f;
            morph.IsPlaying = true;
            morph.IsForward = !morph.IsForward;
        }
    }

    private void UpdateMorphTime( ref Morph morph, float deltaTime )
    {
        if ( !morph.IsPlaying )
            return;

        morph.Time += deltaTime;

        if ( morph.Time >= morph.Duration )
        {
            morph.Time = morph.Duration;
            morph.IsPlaying = false;
        }
    }

    protected override void OnRender()
    {
        gl.ClearColor( 0.1f, 0.1f, 0.15f, 1.0f );
        gl.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

        gl.Enable( EnableCap.DepthTest );
        gl.DepthFunc( DepthFunction.Less );
        gl.Enable( EnableCap.LineSmooth );
        gl.LineWidth( 2.0f );

        Camera camera = default;
        bool cameraFound = false;

        foreach ( Entity entity in World.Filter<Camera>() )
        {
            camera = World.Get<Camera>( entity );
            cameraFound = true;
            break;
        }

        if ( !cameraFound )
            return;

        foreach ( Entity entity in World.Filter<Morph, Transform, Mesh>() )
        {
            Morph morph = World.Get<Morph>( entity );
            Transform transform = World.Get<Transform>( entity );
            Mesh mesh = World.Get<Mesh>( entity );

            if ( mesh.Material is not { Shader: not null } )
                continue;

            ShaderProgram shader = mesh.Material.Value.Shader.Value.Program;
            shader.Use();

            float t = morph.Time / morph.Duration;
            t = SmoothStep( t );
            float morphFactor = morph.IsForward ? t : 1.0f - t;

            shader.TrySetUniform( "uMorphFactor", morphFactor );
            shader.TrySetUniform( "uTorusRadiusMajor", morph.TorusRadiusMajor );
            shader.TrySetUniform( "uTorusRadiusMinor", morph.TorusRadiusMinor );
            shader.TrySetUniform( "uSphereRadius", morph.SphereRadius );

            Matrix4X4<float> model = TransformSystem.CreateModelMatrix(
                transform.Position,
                transform.Rotation,
                transform.Scale );

            shader.TrySetUniform( "uModel", model );
            shader.TrySetUniform( "uView", camera.ViewMatrix );
            shader.TrySetUniform( "uProjection", camera.ProjectionMatrix );

            Vector4D<float> color = new(
                mesh.Material.Value.DiffuseColor.X,
                mesh.Material.Value.DiffuseColor.Y,
                mesh.Material.Value.DiffuseColor.Z,
                mesh.Material.Value.Alpha );
            shader.TrySetUniform( "uColor", color );

            mesh.Vao.Bind();
            unsafe
            {
                gl.DrawElements( mesh.Topology, ( uint )mesh.IndexCount, DrawElementsType.UnsignedInt, null );
            }

            mesh.Vao.Unbind();
        }

        gl.UseProgram( 0 );
    }

    protected override void OnDestroy()
    {
    }

    private float SmoothStep( float t )
    {
        return t * t * ( 3f - 2f * t );
    }
}