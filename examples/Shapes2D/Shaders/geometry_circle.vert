#version 410 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoords;
layout(location = 2) in vec4 aColor;

out vec4 vColor;

uniform mat4 uModel;

void main()
{
    gl_Position = uModel * vec4(aPosition, 1.0);
    vColor = aColor;
}
