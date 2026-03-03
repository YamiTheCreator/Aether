#version 410 core

in vec2 vTexCoords;
in vec4 vColor;

out vec4 FragColor;

void main()
{
    // Преобразуем UV координаты [0,1] в пространство [-1, 1]
    vec2 p = vTexCoords * 2.0 - 1.0;
    
    // Рисуем флаг Японии: белый фон с красным кругом в центре
    float dist = length(p);  // Расстояние от центра - буквально формула Пифагора а это как раз уравнение окружности
    float circleRadius = 0.6;
    
    if (dist < circleRadius) {
        // Внутри круга - красный цвет
        FragColor = vec4(0.75, 0.0, 0.0, 1.0);
    } else {
        // Вне круга - белый фон
        FragColor = vec4(1.0, 1.0, 1.0, 1.0);
    }
}
