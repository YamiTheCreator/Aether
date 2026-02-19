using Silk.NET.OpenGL;
using Silk.NET.Maths;
using Graphics.Structures;
using Texture2DComponent = Graphics.Components.Texture2D;

namespace Graphics.Systems;

public class TextureSystem(GL gl)
{
    public Texture2DComponent CreateTextureFromFile(
        string path,
        TextureWrapMode wrapS = TextureWrapMode.Repeat,
        TextureWrapMode wrapT = TextureWrapMode.Repeat,
        TextureMinFilter minFilter = TextureMinFilter.LinearMipmapLinear,
        TextureMagFilter magFilter = TextureMagFilter.Linear,
        bool generateMipmaps = true)
    {
        TextureObject texture = TextureObject.FromFile(
            gl, path, wrapS, wrapT, minFilter, magFilter, generateMipmaps);

        return new Texture2DComponent
        {
            Texture = texture
        };
    }

    public Texture2DComponent CreateTextureFromColor(
        int width,
        int height,
        byte r = 255,
        byte g = 255,
        byte b = 255,
        byte a = 255,
        TextureWrapMode wrapS = TextureWrapMode.ClampToEdge,
        TextureWrapMode wrapT = TextureWrapMode.ClampToEdge,
        TextureMinFilter minFilter = TextureMinFilter.Linear,
        TextureMagFilter magFilter = TextureMagFilter.Linear,
        bool generateMipmaps = false)
    {
        TextureObject texture = TextureObject.FromColor(
            gl, width, height, r, g, b, a, wrapS, wrapT, minFilter, magFilter, generateMipmaps);

        return new Texture2DComponent
        {
            Texture = texture
        };
    }

    public void BindTexture(Texture2DComponent texture, TextureUnit unit = TextureUnit.Texture0)
    {
        texture.Texture?.Bind(unit);
    }

    public void DeleteTexture(Texture2DComponent texture)
    {
        texture.Texture?.Dispose();
    }

    /// <summary>
    /// Generates vertices for nine-slice rendering
    /// </summary>
    public Vertex[] GenerateNineSliceVertices(
        Texture2DComponent texture,
        Vector2D<float> position,
        Vector2D<float> size,
        int borderPixels,
        Vector4D<float> color,
        float texIndex = 0)
    {
        if (texture.Texture == null)
            throw new ArgumentNullException(nameof(texture), "Texture is null");

        int textureSize = Math.Max(texture.Width, texture.Height);
        Slice[] slices = CalculateNineSlices(position, size, borderPixels, textureSize);

        // 9 slices * 4 vertices per slice = 36 vertices
        Vertex[] vertices = new Vertex[36];
        int vertexIndex = 0;

        foreach (Slice slice in slices)
        {
            // Bottom-left
            vertices[vertexIndex++] = new Vertex(
                new Vector3D<float>(slice.PositionMin.X, slice.PositionMin.Y, 0),
                new Vector2D<float>(slice.UvMin.X, slice.UvMin.Y),
                color,
                texIndex
            );

            // Bottom-right
            vertices[vertexIndex++] = new Vertex(
                new Vector3D<float>(slice.PositionMax.X, slice.PositionMin.Y, 0),
                new Vector2D<float>(slice.UvMax.X, slice.UvMin.Y),
                color,
                texIndex
            );

            // Top-right
            vertices[vertexIndex++] = new Vertex(
                new Vector3D<float>(slice.PositionMax.X, slice.PositionMax.Y, 0),
                new Vector2D<float>(slice.UvMax.X, slice.UvMax.Y),
                color,
                texIndex
            );

            // Top-left
            vertices[vertexIndex++] = new Vertex(
                new Vector3D<float>(slice.PositionMin.X, slice.PositionMax.Y, 0),
                new Vector2D<float>(slice.UvMin.X, slice.UvMax.Y),
                color,
                texIndex
            );
        }

        return vertices;
    }

    private static Slice[] CalculateNineSlices(Vector2D<float> position, Vector2D<float> size, int borderPixels, int textureSize)
    {
        float borderUv = (float)borderPixels / textureSize;

        // World space border size
        float worldBorder = (float)borderPixels / textureSize * size.X;

        // Clamp border if size is too small
        if (worldBorder * 2 > size.X) worldBorder = size.X / 2;
        if (worldBorder * 2 > size.Y) worldBorder = size.Y / 2;

        // Position coordinates
        float x0 = position.X;
        float x1 = position.X + worldBorder;
        float x2 = position.X + size.X - worldBorder;
        float x3 = position.X + size.X;

        float y0 = position.Y;
        float y1 = position.Y + worldBorder;
        float y2 = position.Y + size.Y - worldBorder;
        float y3 = position.Y + size.Y;

        // UV coordinates
        const float u0 = 0f;
        float u2 = 1f - borderUv;
        const float u3 = 1f;

        const float v0 = 0f;
        float v2 = 1f - borderUv;
        const float v3 = 1f;

        // Return 9 quads in order: top-left, top-center, top-right,
        // middle-left, center, middle-right,
        // bottom-left, bottom-center, bottom-right
        return
        [
            // Top row
            new Slice(new Vector2D<float>(x0, y2), new Vector2D<float>(x1, y3), new Vector2D<float>(u0, v2),
                new Vector2D<float>(borderUv, v3)),
            new Slice(new Vector2D<float>(x1, y2), new Vector2D<float>(x2, y3), new Vector2D<float>(borderUv, v2),
                new Vector2D<float>(u2, v3)),
            new Slice(new Vector2D<float>(x2, y2), new Vector2D<float>(x3, y3), new Vector2D<float>(u2, v2), new Vector2D<float>(u3, v3)),

            // Middle row
            new Slice(new Vector2D<float>(x0, y1), new Vector2D<float>(x1, y2), new Vector2D<float>(u0, borderUv),
                new Vector2D<float>(borderUv, v2)),
            new Slice(new Vector2D<float>(x1, y1), new Vector2D<float>(x2, y2), new Vector2D<float>(borderUv, borderUv),
                new Vector2D<float>(u2, v2)),
            new Slice(new Vector2D<float>(x2, y1), new Vector2D<float>(x3, y2), new Vector2D<float>(u2, borderUv),
                new Vector2D<float>(u3, v2)),

            // Bottom row
            new Slice(new Vector2D<float>(x0, y0), new Vector2D<float>(x1, y1), new Vector2D<float>(u0, v0),
                new Vector2D<float>(borderUv, borderUv)),
            new Slice(new Vector2D<float>(x1, y0), new Vector2D<float>(x2, y1), new Vector2D<float>(borderUv, v0),
                new Vector2D<float>(u2, borderUv)),
            new Slice(new Vector2D<float>(x2, y0), new Vector2D<float>(x3, y1), new Vector2D<float>(u2, v0),
                new Vector2D<float>(u3, borderUv))
        ];
    }
}