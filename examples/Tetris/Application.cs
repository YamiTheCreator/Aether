using Silk.NET.Maths;
using Aether.Core;
using Aether.Core.Enums;
using Graphics;
using Graphics.Components;
using Graphics.Systems;
using Tetris.Systems;

namespace Tetris;

public class Application() : ApplicationBase(
    title: "Tetris",
    width: 800,
    height: 660,
    createDefaultCamera: true)
{
    protected override void OnInitialize()
    {
        WindowBase.SetResizable(false);

        Renderer2D renderer = new();
        ShaderSystem shaderSystem = new(WindowBase.Gl);
        FontSystem fontSystem = new(WindowBase.Gl);
        TextureSystem textureSystem = new(WindowBase.Gl);

        Shader shader = shaderSystem.CreateShader();
        Font font = fontSystem.CreateFont(fontSize: 32f);

        World.SetGlobal(renderer);
        World.SetGlobal(shaderSystem);
        World.SetGlobal(fontSystem);
        World.SetGlobal(textureSystem);
        World.SetGlobal(shader);
        World.SetGlobal(font);

        World.AddSystem(new CameraSystem());
        World.AddSystem(new TetrisLogicSystem());
        World.AddSystem(new DebugInputSystem()); // Debug
        World.AddSystem(new TetrisInputSystem());
        World.AddSystem(new TetrisRenderSystem());

        foreach (Entity e in World.Filter<Camera>().With<Transform>())
        {
            ref Camera camera = ref World.Get<Camera>(e);

            camera.ProjectionType = ProjectionType.Orthographic;
            camera.IsStatic = true;
            camera.StaticPosition = new Vector3D<float>(0f, 0f, 0f);
            camera.OrthographicSize = 11f;
            camera.AspectRatio = (float)WindowBase.LogicalWidth / WindowBase.LogicalHeight;
            camera.NearPlane = -10f;
            camera.FarPlane = 10f;
            break;
        }
    }
}
