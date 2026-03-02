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

    // Вычисляем радиальное расстояние от текущего пикселя до центра ряби
    float dist = distance(uv, uRippleCenter);

    // Фронт волны — радиус, на котором находится гребень волны в текущий момент
    float waveFront = uTime * WAVE_SPEED;

    // Фаза колебания: 
    // dist - waveFront — насколько точка опережает или отстаёт от фронта волны
    // умножение на WAVE_FREQUENCY задаёт "плотность" колебаний
    float phase = (dist - waveFront) * WAVE_FREQUENCY;
    
    // WAVE_AMPLITUDE контролирует силу смещения UV-координат
    float wave = sin(phase) * WAVE_AMPLITUDE;

    // smoothstep возвращает плавное значение от 0 до 1
    // Первый smoothstep: 1 внутри волны (dist < waveFront + 0.3), 0 снаружи
    // Второй smoothstep: 1 внутри волны (dist > waveFront - 0.3), 0 внутри центра
    // 
    // Перемножение даёт 1 в зоне [waveFront - 0.3, waveFront + 0.3], 
    // плавно спадает к 0 за пределами
    float waveZone = smoothstep(waveFront + 0.3, waveFront, dist) *
    smoothstep(waveFront - 0.3, waveFront, dist);
    
    // Вектор направления от центра ряби к текущему пикселю
    vec2 direction = normalize(uv - uRippleCenter);
    
    // direction * wave — вектор смещения
    // Умножение на waveZone "включает" эффект только в зоне волны
    vec2 distortedUV = uv + direction * wave * waveZone;
    
    // clamp() обрезает значения в диапазон от 0 до 1
    distortedUV = clamp(distortedUV, 0.0, 1.0);
    
    vec4 color1 = texture(uTexture, distortedUV);
    
    vec4 color2 = texture(uTexture2, distortedUV);
    
    float transition = smoothstep(waveFront - 0.1, waveFront + 0.1, dist);
    
    vec4 finalColor = mix(color1, color2, 1.0 - transition);
    
    FragColor = finalColor;
}
