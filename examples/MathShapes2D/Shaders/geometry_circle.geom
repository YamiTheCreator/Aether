#version 410 core

// Входная топология - точки
layout(points) in;
// Выходная топология - линии 
layout(line_strip, max_vertices = 65) out;

in vec4 vColor[];
out vec4 gColor;

uniform float uRadius;
uniform mat4 uViewProjection;

const float PI = 3.14159265359;
const int SEGMENTS = 64;

void main()
{
    vec4 center = gl_in[0].gl_Position;
    
    // Генерируем окружность из одной точки
    for (int i = 0; i <= SEGMENTS; i++)
    {
        // Вычисляем угол для текущей вершины
        float angle = 2.0 * PI * float(i) / float(SEGMENTS);
        
        // Создаём точку на окружности в world space
        vec4 offset = vec4(uRadius * cos(angle), uRadius * sin(angle), 0.0, 0.0);
        vec4 worldPos = center + offset;
        
        gl_Position = uViewProjection * worldPos;
        gColor = vColor[0];
        EmitVertex();
    }
    
    EndPrimitive();
}
