using Aether.Core;
using Silk.NET.Maths;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using MathShapes2D.Components;

namespace MathShapes2D.Systems;

/// <summary>
/// Star system - generates star quad for fragment shader
/// </summary>
public class StarSystem : SystemBase
{
    protected override void OnUpdate( float deltaTime )
    {
        MeshSystem meshSystem = World.GetGlobal<MeshSystem>();

        foreach ( Entity entity in World.Filter<Star>() )
        {
            if ( !World.Has<Transform>( entity ) )
                continue;

            ref Star star = ref World.Get<Star>( entity );

            if ( star.IsGenerated )
                continue;

            GenerateStar( entity, ref star, meshSystem );
            star.IsGenerated = true;
        }
    }

    private void GenerateStar( Entity entity, ref Star star, MeshSystem meshSystem )
    {
        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        Vector4D<float> white = new( 1f, 1f, 1f, 1f );

        // Quad для фрагментного шейдера
        vertices.Add( new Vertex( new Vector3D<float>( -1f, -1f, 0f ), new Vector2D<float>( 0, 0 ), white, 0,
            new Vector3D<float>( 0, 0, 1 ) ) );
        vertices.Add( new Vertex( new Vector3D<float>( 1f, -1f, 0f ), new Vector2D<float>( 1, 0 ), white, 0,
            new Vector3D<float>( 0, 0, 1 ) ) );
        vertices.Add( new Vertex( new Vector3D<float>( 1f, 1f, 0f ), new Vector2D<float>( 1, 1 ), white, 0,
            new Vector3D<float>( 0, 0, 1 ) ) );
        vertices.Add( new Vertex( new Vector3D<float>( -1f, 1f, 0f ), new Vector2D<float>( 0, 1 ), white, 0,
            new Vector3D<float>( 0, 0, 1 ) ) );

        indices.AddRange( [ 0, 1, 2, 0, 2, 3 ] );

        // Get material from entity (already added in Application.cs)
        Material material = World.Get<Material>( entity );

        Console.WriteLine( $"Star: Creating mesh with {vertices.Count} vertices, {indices.Count} indices" );
        Console.WriteLine( $"Material has shader: {material.Shader.HasValue}" );
        Console.WriteLine( $"Star outer radius: {star.OuterRadius}, inner radius: {star.InnerRadius}" );

        Mesh mesh = meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray(), material );
        World.Add( entity, mesh );
    }
}