#version 410 core

in vec3 WorldPos;
in vec3 Normal;
in vec2 TexCoords;
in vec4 vColor;

out vec4 FragColor;

uniform sampler2D uTexture;
uniform sampler2D uTexture2;
uniform float uTime;
uniform vec2 uResolution;
uniform vec2 uRippleCenter;

const float PI = 3.14159265359;
const float WAVE_SPEED = 0.7;
const float WAVE_FREQUENCY = 12.0;
const float WAVE_AMPLITUDE = 0.05;
const float BRIGHTNESS_AMPLITUDE = 0.4;

void main()
{
    vec2 uv = TexCoords;
    // Основа ряби - радиальное расстояние от текущего пикселя до центра
    float dist = distance(uv, uRippleCenter);
    
    // Вычисление волны - фронт волны, фаза колебания + смещение
    float waveFront = uTime * WAVE_SPEED;
    float phase = (dist - waveFront) * WAVE_FREQUENCY;
    float wave = sin(phase) * WAVE_AMPLITUDE;
    
    float waveZone = smoothstep(waveFront + 0.3, waveFront, dist) * 
                     smoothstep(waveFront - 0.3, waveFront, dist);
    
    vec2 direction = normalize(uv - uRippleCenter);
    vec2 distortedUV = uv + direction * wave * waveZone;
    distortedUV = clamp(distortedUV, 0.0, 1.0);
    
    vec4 color1 = texture(uTexture, distortedUV);
    vec4 color2 = texture(uTexture2, distortedUV);
    
    float transition = smoothstep(waveFront - 0.1, waveFront + 0.1, dist);
    vec4 finalColor = mix(color1, color2, 1.0 - transition);
    
    float brightness = 1.0 + sin(phase) * BRIGHTNESS_AMPLITUDE * waveZone;
    finalColor.rgb *= brightness;
    
    FragColor = finalColor;
}
