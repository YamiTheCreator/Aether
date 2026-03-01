using Aether.Core;
using Graphics.Components;

namespace Maze.Components;

/// <summary>
/// Container for maze PBR materials: 4 wall materials + floor + sky
/// </summary>
public struct MazeMaterials : Component
{
    public Material BrickMaterial;      // Sector 1 walls
    public Material StoneMaterial;      // Sector 2 walls
    public Material TileMaterial;       // Sector 3 walls
    public Material SandstoneMaterial;  // Sector 4 walls
    public Material GrassMaterial;      // Floor
    public Texture2D SkyTexture;        // Sky texture
}
