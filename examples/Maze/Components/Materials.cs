using Aether.Core;
using Graphics.Components;

namespace Maze.Components;

public struct Materials : Component
{
    public Material BrickMaterial;
    public Material StoneMaterial;
    public Material TileMaterial;
    public Material SandstoneMaterial;
    public Material GrassMaterial;
    public Texture2D SkyTexture;
}
