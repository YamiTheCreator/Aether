#version 410 core

in vec4 vColor;

out vec4 FragColor;

uniform vec4 uColor;

void main()
{
    FragColor = uColor * vColor;
}
