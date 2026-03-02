using Aether.Core;

namespace Asteroids.Components;

public struct GameState : Component
{
    public int Lives;
    public int Score;
    public int Wave;
    public bool IsGameOver;
    public float RespawnTimer;
}
