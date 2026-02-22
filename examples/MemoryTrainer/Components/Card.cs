using Aether.Core;

namespace MemoryTrainer.Components;

public struct Card : Component
{
    public int CardId;
    public int PairId;
    public int TextureIndex;

    public bool IsRevealed;
    public bool IsMatched;
    public bool IsFlipping;

    public float FlipProgress;
    public float FlipSpeed;
    public bool FlipToFront;
}