using Aether.Core;
using Aether.Core.Enums;
using Aether.Core.Extensions;
using Graphics.Components;
using Silk.NET.Maths;

namespace Graphics.Systems;

public class CameraSystem : SystemBase
{
    protected override void OnCreate() { }

    protected override void OnUpdate( float deltaTime )
    {
        foreach ( Entity entity in World.Filter<Camera, Transform>() )
        {
            ref Camera camera = ref World.Get<Camera>( entity );
            ref Transform transform = ref World.Get<Transform>( entity );

            UpdateCameraMatrices( ref camera, ref transform );
        }
    }

    protected override void OnRender()
    {
    }

    protected override void OnDestroy()
    {
    }

    // Методы создания камер
    public static Entity CreatePerspectiveCamera( World world, Vector3D<float> position, float yaw = -90f, float pitch = 0f,
        float fov = 45f, float aspectRatio = 16f / 9f, float near = 0.1f, float far = 100f )
    {
        Entity entity = world.Spawn();

        Camera camera = new()
        {
            ProjectionType = ProjectionType.Perspective,
            FieldOfView = fov,
            AspectRatio = aspectRatio,
            NearPlane = near,
            FarPlane = far
        };

        Transform transform = new( position );
        SetLookDirection( ref transform, yaw, pitch );

        world.Add( entity, camera );
        world.Add( entity, transform );

        return entity;
    }

    public static Entity CreateOrthographicCamera( World world, Vector3D<float> position,
        float size = 10f, float aspectRatio = 16f / 9f, float near = -10f, float far = 100f )
    {
        Entity entity = world.Spawn();

        Camera camera = new()
        {
            ProjectionType = ProjectionType.Orthographic,
            OrthographicSize = size,
            AspectRatio = aspectRatio,
            NearPlane = near,
            FarPlane = far
        };

        Transform transform = new( position )
        {
            Forward = new Vector3D<float>( 0, 0, -1 ),
            Right = new Vector3D<float>( 1, 0, 0 ),
            Up = new Vector3D<float>( 0, 1, 0 )
        };

        world.Add( entity, camera );
        world.Add( entity, transform );

        return entity;
    }

    public static Entity CreateOrbitCamera( World world, Vector3D<float> target, float distance,
        float yaw = 45f, float pitch = 20f, float fov = 60f, float aspectRatio = 16f / 9f,
        float near = 0.1f, float far = 100f )
    {
        Entity entity = world.Spawn();

        Camera camera = new()
        {
            ProjectionType = ProjectionType.Perspective,
            FieldOfView = fov,
            AspectRatio = aspectRatio,
            NearPlane = near,
            FarPlane = far
        };

        Transform transform = new( Vector3D<float>.Zero );
        SetOrbitPosition( ref transform, target, distance, yaw, pitch );

        world.Add( entity, camera );
        world.Add( entity, transform );

        return entity;
    }

    private void UpdateCameraMatrices( ref Camera camera, ref Transform transform )
    {
        Vector3D<float> target = transform.Position + transform.Forward;
        camera.ViewMatrix = Matrix4X4.CreateLookAt( transform.Position, target, transform.Up );

        if ( camera.ProjectionType == ProjectionType.Perspective )
        {
            camera.ProjectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(
                MathExtensions.DegToRad( camera.FieldOfView ),
                camera.AspectRatio,
                camera.NearPlane,
                camera.FarPlane
            );
        }
        else
        {
            float width = camera.OrthographicSize * camera.AspectRatio;
            float height = camera.OrthographicSize;
            camera.ProjectionMatrix = Matrix4X4.CreateOrthographic(
                width,
                height,
                camera.NearPlane,
                camera.FarPlane
            );
        }

        camera.ViewProjectionMatrix = camera.ViewMatrix * camera.ProjectionMatrix;
    }

    public static void SetLookDirection( ref Transform transform, float yaw, float pitch )
    {
        float yawRad = MathExtensions.DegToRad( yaw );
        float pitchRad = MathExtensions.DegToRad( pitch );

        Vector3D<float> forward;
        forward.X = MathF.Cos( yawRad ) * MathF.Cos( pitchRad );
        forward.Y = MathF.Sin( pitchRad );
        forward.Z = MathF.Sin( yawRad ) * MathF.Cos( pitchRad );

        transform.Forward = Vector3D.Normalize( forward );
        transform.Right = Vector3D.Normalize( Vector3D.Cross( transform.Forward, Vector3D<float>.UnitY ) );
        transform.Up = Vector3D.Normalize( Vector3D.Cross( transform.Right, transform.Forward ) );

        transform.Rotation = QuaternionFromLookDirection( transform.Forward, transform.Up );
    }

    public static void LookAt( ref Transform transform, Vector3D<float> target )
    {
        Vector3D<float> direction = Vector3D.Normalize( target - transform.Position );
        transform.Forward = direction;
        transform.Right = Vector3D.Normalize( Vector3D.Cross( direction, Vector3D<float>.UnitY ) );
        transform.Up = Vector3D.Normalize( Vector3D.Cross( transform.Right, direction ) );
        transform.Rotation = QuaternionFromLookDirection( transform.Forward, transform.Up );
    }

    public static void SetOrbitPosition( ref Transform transform, Vector3D<float> target, float distance, float yaw,
        float pitch )
    {
        float yawRad = MathExtensions.DegToRad( yaw );
        float pitchRad = MathExtensions.DegToRad( pitch );

        Vector3D<float> position;
        position.X = target.X + distance * MathF.Cos( pitchRad ) * MathF.Sin( yawRad );
        position.Y = target.Y + distance * MathF.Sin( pitchRad );
        position.Z = target.Z + distance * MathF.Cos( pitchRad ) * MathF.Cos( yawRad );

        transform.Position = position;
        LookAt( ref transform, target );
    }

    public static Vector3D<float> GetForwardFlat( float yaw )
    {
        float yawRad = MathExtensions.DegToRad( yaw );
        return Vector3D.Normalize( new Vector3D<float>(
            MathF.Cos( yawRad ),
            0,
            MathF.Sin( yawRad )
        ) );
    }

    public static Vector3D<float> GetRightFlat( float yaw )
    {
        float yawRad = MathExtensions.DegToRad( yaw + 90f );
        return Vector3D.Normalize( new Vector3D<float>(
            MathF.Cos( yawRad ),
            0,
            MathF.Sin( yawRad )
        ) );
    }

    private static Quaternion<float> QuaternionFromLookDirection( Vector3D<float> forward, Vector3D<float> up )
    {
        Vector3D<float> right = Vector3D.Normalize( Vector3D.Cross( up, forward ) );
        up = Vector3D.Cross( forward, right );

        Matrix4X4<float> rotationMatrix = new(
            right.X, right.Y, right.Z, 0,
            up.X, up.Y, up.Z, 0,
            -forward.X, -forward.Y, -forward.Z, 0,
            0, 0, 0, 1
        );

        return Quaternion<float>.CreateFromRotationMatrix( rotationMatrix );
    }
}