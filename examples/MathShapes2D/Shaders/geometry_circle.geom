#version 410 core

layout(points) in;
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
    
    // Генерируем окружность из точки в world space
    for (int i = 0; i <= SEGMENTS; i++)
    {
        float angle = 2.0 * PI * float(i) / float(SEGMENTS);
        
        // Создаем точку на окружности в world space
        vec4 offset = vec4(uRadius * cos(angle), uRadius * sin(angle), 0.0, 0.0);
        vec4 worldPos = center + offset;
        
        // Применяем view-projection трансформацию
        gl_Position = uViewProjection * worldPos;
        gColor = vColor[0];
        EmitVertex();
    }
    
    EndPrimitive();
}
