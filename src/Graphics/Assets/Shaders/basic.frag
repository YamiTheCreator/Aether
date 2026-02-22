#version 410 core

in vec3 WorldPos;
in vec3 Normal;
in vec2 TexCoords;
in vec4 vColor;

out vec4 FragColor;

// Basic material
uniform vec4 uColor;
uniform sampler2D uTexture;
uniform int uHasTexture;

void main() {
    vec4 color = uColor * vColor;
    
    if (uHasTexture == 1) {
        vec4 texColor = texture(uTexture, TexCoords);
        color *= texColor;
    }
    
    FragColor = color;
}
