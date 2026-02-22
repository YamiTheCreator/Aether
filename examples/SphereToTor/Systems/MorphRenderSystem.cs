using Silk.NET.OpenGL;
using Silk.NET.Maths;
using Aether.Core;
using Graphics;
using Graphics.Components;
using Graphics.Structures;
using Graphics.Systems;
using SphereToTor.Components;

namespace SphereToTor.Systems;

/// <summary>
/// Custom render system for morphing shader
/// </summary>
public class MorphRenderSystem( GL gl ) : SystemBase
{
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

        foreach ( Entity entity in World.Filter<MorphComponent>().With<Mesh>() )
        {
            if ( !World.Has<Transform>( entity ) )
                continue;

            MorphComponent morph = World.Get<MorphComponent>( entity );
            Transform transform = World.Get<Transform>( entity );
            Mesh mesh = World.Get<Mesh>( entity );

            if ( mesh.Material is not { Shader: not null } )
                continue;

            ShaderProgram shader = mesh.Material.Value.Shader.Value.Program;
            shader.Use();

            // Calculate morph factor
            float t = morph.Time / morph.Duration;
            t = SmoothStep( t );
            float morphFactor = morph.IsForward ? t : ( 1.0f - t );

            // Set uniforms
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

    private float SmoothStep( float t )
    {
        return t * t * ( 3f - 2f * t );
    }
}
