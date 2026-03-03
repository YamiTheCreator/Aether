#version 410 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoords;
layout(location = 2) in vec4 aColor;

out vec4 vColor;

uniform mat4 uModel;
uniform mat4 uViewProjection;

const float PI = 3.14159265359;

void main()
{
    float t = aPosition.x + PI;
    
    // Искривляем прямую линию в круг используя параметрическое уравнение
    float radius = 1.2;
    float x = radius * cos(t);
    float y = radius * sin(t);
    float z = aPosition.z;
    
    vec4 worldPos = uModel * vec4(x, y, z, 1.0);
    gl_Position = uViewProjection * worldPos;
    
    vColor = aColor;
}
