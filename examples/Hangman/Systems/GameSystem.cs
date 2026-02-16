using Aether.Core.Systems;

namespace Hangman.Systems;

/// <summary>
/// Main game logic for Hangman.
/// </summary>
public class GameSystem : SystemBase
{
    protected override void OnInit()
    {
        // Game state is set as global in Application
    }

    protected override void OnUpdate( float deltaTime )
    {
        // Game logic is handled by input system
    }
}
