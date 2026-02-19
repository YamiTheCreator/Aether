#version 330 core

layout (location = 0) in vec3 aPosition;  // Позиция вершины
layout (location = 1) in vec2 aTexCoord;  // UV для текстуры
layout (location = 2) in vec4 aColor;     // Цвет вершины

uniform mat4 uViewProjection;  // Объединенная матрица view * projection

out vec2 vTexCoord;  // UV для fragment shader
out vec4 vColor;     // Цвет для fragment shader

void main() {
    gl_Position = uViewProjection * vec4(aPosition, 1.0);
    vTexCoord = aTexCoord;
    vColor = aColor;
}
