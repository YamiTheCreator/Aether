using Aether.Core;

namespace MemoryTrainer.Components;

public struct Card : Component
{
    public int CardId; // Уникальный ID карты
    public int PairId; // ID пары (две карты с одинаковым PairId - это пара)
    public int TextureIndex; // Индекс текстуры лицевой стороны

    public bool IsRevealed; // Открыта ли карта
    public bool IsMatched; // Найдена ли пара
    public bool IsFlipping; // В процессе переворота

    public float FlipProgress; // Прогресс анимации переворота (0.0 - 1.0)
    public float FlipSpeed; // Скорость переворота
    public bool FlipToFront; // Направление переворота (true = к лицевой стороне)
}