using Aether.Core;

namespace MemoryTrainer.Components;

public class GameState
{
    public int GridRows = 4;
    public int GridCols = 4;

    public Entity? FirstRevealedCard = null;
    public Entity? SecondRevealedCard = null;
    public Entity? ClickedCard = null;

    public float DelayTimer = 0f;
    public bool IsWaitingForFlipBack = false;
    public bool RestartRequested = false;
}
