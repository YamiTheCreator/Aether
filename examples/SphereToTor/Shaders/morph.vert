#version 410 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec4 aColor;
layout(location = 3) in float aTextureId;
layout(location = 4) in vec3 aNormal;

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

// Развертка для сферы - сферические координаты
vec3 uvToSphere(vec2 uv, float radius) {
    float theta = uv.x * TWO_PI;
    float phi = uv.y * PI;
    
    float x = radius * sin(phi) * cos(theta);
    float y = radius * sin(phi) * sin(theta);
    float z = radius * cos(phi);
    
    return vec3(x, y, z);
}

// Развертка тора
vec3 uvToTorus(vec2 uv, float majorRadius, float minorRadius) {
    float theta = uv.x * TWO_PI;
    float phi = uv.y * TWO_PI;
    
    float x = (majorRadius + minorRadius * cos(theta)) * cos(phi);
    float y = (majorRadius + minorRadius * cos(theta)) * sin(phi);
    float z = minorRadius * sin(theta);
    
    return vec3(x, y, z);
}

void main()
{
    vec2 uv = aTexCoord;
    
    vec3 spherePos = uvToSphere(uv, uSphereRadius);
    vec3 torusPos = uvToTorus(uv, uTorusRadiusMajor, uTorusRadiusMinor);
    vec3 morphedPos = mix(spherePos, torusPos, uMorphFactor);
    
    vec4 worldPos = uModel * vec4(morphedPos, 1.0);
    gl_Position = uProjection * uView * worldPos;
    
    vColor = aColor;
}
