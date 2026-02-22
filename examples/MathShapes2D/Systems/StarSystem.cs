using Aether.Core;
using Silk.NET.Maths;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using MathShapes2D.Components;

namespace MathShapes2D.Systems;

public class StarSystem : SystemBase
{
    private MeshSystem? _meshSystem;

    protected override void OnCreate()
    {
        _meshSystem = World.GetGlobal<MeshSystem>();
    }

    protected override void OnUpdate( float deltaTime ) { }

    protected override void OnRender()
    {
        if ( _meshSystem == null ) return;

        foreach ( Entity entity in World.Filter<Star>() )
        {
            if ( !World.Has<Transform>( entity ) )
                continue;

            ref Star star = ref World.Get<Star>( entity );

            if ( star.IsGenerated )
                continue;

            GenerateStar( entity, ref star, _meshSystem );
            star.IsGenerated = true;
        }
    }

    protected override void OnDestroy() { }

    private void GenerateStar( Entity entity, ref Star star, MeshSystem meshSystem )
    {
        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        Vector4D<float> white = new( 1f, 1f, 1f, 1f );

        vertices.Add( new Vertex( new Vector3D<float>( -1f, -1f, 0f ), new Vector2D<float>( 0, 0 ), white, 0,
            new Vector3D<float>( 0, 0, 1 ) ) );
        vertices.Add( new Vertex( new Vector3D<float>( 1f, -1f, 0f ), new Vector2D<float>( 1, 0 ), white, 0,
            new Vector3D<float>( 0, 0, 1 ) ) );
        vertices.Add( new Vertex( new Vector3D<float>( 1f, 1f, 0f ), new Vector2D<float>( 1, 1 ), white, 0,
            new Vector3D<float>( 0, 0, 1 ) ) );
        vertices.Add( new Vertex( new Vector3D<float>( -1f, 1f, 0f ), new Vector2D<float>( 0, 1 ), white, 0,
            new Vector3D<float>( 0, 0, 1 ) ) );

        indices.AddRange( [ 0, 1, 2, 0, 2, 3 ] );

        Material material = World.Get<Material>( entity );

        Console.WriteLine( $"Star: Creating mesh with {vertices.Count} vertices, {indices.Count} indices" );
        Console.WriteLine( $"Material has shader: {material.Shader.HasValue}" );
        Console.WriteLine( $"Star outer radius: {star.OuterRadius}, inner radius: {star.InnerRadius}" );

        Mesh mesh = meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray(), material );
        World.Add( entity, mesh );
    }
}