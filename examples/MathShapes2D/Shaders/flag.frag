#version 410 core

in vec2 vTexCoords;
in vec4 vColor;

out vec4 FragColor;

void main()
{
    // Координаты в пространстве [-1, 1]
    vec2 p = vTexCoords * 2.0 - 1.0;
    
    // Флаг Японии: белый фон с красным кругом в центре
    float dist = length(p);
    float circleRadius = 0.6;
    
    if (dist < circleRadius) {
        // Красный круг
        FragColor = vec4(0.75, 0.0, 0.0, 1.0);
    } else {
        // Белый фон
        FragColor = vec4(1.0, 1.0, 1.0, 1.0);
    }
}
