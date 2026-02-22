using Aether.Core;
using Graphics.Components;
using MemoryTrainer.Components;
using Silk.NET.Maths;

namespace MemoryTrainer.Systems;

public class CardFlipSystem : SystemBase
{
    protected override void OnUpdate(float deltaTime)
    {
        foreach (Entity entity in World.Filter<Card>().With<Transform>())
        {
            ref Card card = ref World.Get<Card>(entity);
            ref Transform transform = ref World.Get<Transform>(entity);

            if (card.IsFlipping)
            {
                // Обновляем прогресс анимации
                card.FlipProgress += card.FlipSpeed * deltaTime;

                if (card.FlipProgress >= 1f)
                {
                    // Анимация завершена
                    card.FlipProgress = 1f;
                    card.IsFlipping = false;
                    card.IsRevealed = card.FlipToFront;
                }

                // Вычисляем угол поворота (0 - 180 градусов)
                float flipAngle = card.FlipProgress * MathF.PI;
                
                // Базовый поворот
                Quaternion<float> baseRotation = Quaternion<float>.CreateFromAxisAngle(
                    new Vector3D<float>(1f, 0f, 0f), 
                    90f * MathF.PI / 180f
                );
                
                // Поворот для переворота вокруг оси X 
                Quaternion<float> flipRotation = Quaternion<float>.CreateFromAxisAngle(
                    new Vector3D<float>(1f, 0f, 0f),
                    card.FlipToFront ? flipAngle : -flipAngle
                );
                
                // Комбинируем повороты
                transform.Rotation = flipRotation * baseRotation;
            }
        }
    }
}
