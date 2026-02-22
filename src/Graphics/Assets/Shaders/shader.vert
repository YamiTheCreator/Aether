#version 410 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec4 aColor;
layout (location = 4) in vec3 aNormal;

// Camera uniforms (no UBO)
uniform mat4 uViewProjection;
uniform mat4 uModel;

// Outputs
out vec3 WorldPos;
out vec3 Normal;
out vec2 TexCoords;
out vec4 vColor;

void main() {
    WorldPos = vec3(uModel * vec4(aPosition, 1.0));
    
    // Transform and normalize normal
    Normal = normalize(mat3(uModel) * aNormal);
    
    TexCoords = aTexCoord;
    vColor = aColor;
    
    gl_Position = uViewProjection * vec4(WorldPos, 1.0);
}
