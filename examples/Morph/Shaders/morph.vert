#version 410 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec4 aColor;

out vec4 vColor;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

uniform float uMorphFactor;
uniform float uTorusRadiusMajor;
uniform float uTorusRadiusMinor;
uniform float uSphereRadius;

const float PI = 3.14159265359;
const float TWO_PI = 6.28318530718;

// Преобразуем UV координаты в позицию на сфере
// uv.x = theta (0 до 2π), uv.y = phi (0 до π)
vec3 uvToSphere(vec2 uv, float radius) {
    float theta = uv.x * TWO_PI;  // Угол вокруг оси Y
    float phi = uv.y * PI;        // Угол от полюса к полюсу
    
    // Сферические координаты в декартовы
    float x = radius * sin(phi) * cos(theta);
    float y = radius * sin(phi) * sin(theta);
    float z = radius * cos(phi);
    
    return vec3(x, y, z);
}

// Преобразуем UV координаты в позицию на торе
// uv.x = theta (угол малого круга), uv.y = phi (угол большого круга)
vec3 uvToTorus(vec2 uv, float majorRadius, float minorRadius) {
    float theta = uv.x * TWO_PI;  // Угол вокруг малого круга
    float phi = uv.y * TWO_PI;    // Угол вокруг большого круга
    
    // Параметрическое уравнение тора
    float x = (majorRadius + minorRadius * cos(theta)) * cos(phi);
    float y = (majorRadius + minorRadius * cos(theta)) * sin(phi);
    float z = minorRadius * sin(theta);
    
    return vec3(x, y, z);
}

void main()
{
    vec2 uv = aTexCoord;
    
    // Вычисляем позиции на сфере и торе
    vec3 spherePos = uvToSphere(uv, uSphereRadius);
    vec3 torusPos = uvToTorus(uv, uTorusRadiusMajor, uTorusRadiusMinor);
    
    // Линейная интерполяция между сферой и тором
    vec3 morphedPos = mix(spherePos, torusPos, uMorphFactor);
    
    vec4 worldPos = uModel * vec4(morphedPos, 1.0);
    gl_Position = uProjection * uView * worldPos;
    
    vColor = aColor;
}
