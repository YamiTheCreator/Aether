#version 410 core

out vec4 FragColor;

in vec3 WorldPos;
in vec3 Normal;
in vec2 TexCoords;
in vec4 vColor;

// Текстуры материала
uniform sampler2D uTexture;
uniform sampler2D uNormalMap;
uniform sampler2D uMetallicMap;
uniform sampler2D uRoughnessMap;
uniform sampler2D uAOMap;

uniform int uHasTexture;
uniform int uHasNormalMap;
uniform int uHasMetallicMap;
uniform int uHasRoughnessMap;
uniform int uHasAOMap;

// Свойства материала
uniform float uMetallic;
uniform float uRoughness;
uniform vec3 uMaterialDiffuse;
uniform float uAlpha;

// Свечение
uniform vec3 uEmissionColor;
uniform float uEmissionIntensity;

// Источники света
uniform vec4 uPointLightPositions[4];
uniform vec4 uPointLightColors[4];
uniform int uNumLights;

// Камера
uniform vec3 uCameraPosition;

const float PI = 3.14159265359;

// Распределение микро-граней (GGX) — определяет размер и форму блика
float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH * NdotH;

    float nom = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}

// Геометрическое затенение (Schlick-GGX) — учитывает само-затенение неровностей
float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = roughness + 1.0;
    float k = (r * r) / 8.0;

    float nom = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}

// Полная геометрия (Smith) — затенение для света и камеры
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

// Эффект Френеля (Schlick) — блики усиливаются под острым углом
vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

void main()
{
    // Базовый цвет материала
    vec3 albedo = uMaterialDiffuse * vColor.rgb;
    if (uHasTexture == 1) {
        albedo = texture(uTexture, TexCoords).rgb;
    }

    // Параметры материала из текстур или констант
    float metallic = uMetallic;
    if (uHasMetallicMap == 1) {
        metallic = texture(uMetallicMap, TexCoords).r;
    }

    float roughness = uRoughness;
    if (uHasRoughnessMap == 1) {
        roughness = texture(uRoughnessMap, TexCoords).r;
    }

    float ao = 1.0;
    if (uHasAOMap == 1) {
        ao = texture(uAOMap, TexCoords).r;
    }

    // Нормаль поверхности
    vec3 N = normalize(Normal);

    // Применяем normal map если есть
    if (uHasNormalMap == 1) {
        vec3 tangentNormal = texture(uNormalMap, TexCoords).xyz * 2.0 - 1.0;

        // Строим TBN-матрицу для перехода в мировое пространство
        vec3 Q1  = dFdx(WorldPos);
        vec3 Q2  = dFdy(WorldPos);
        vec2 st1 = dFdx(TexCoords);
        vec2 st2 = dFdy(TexCoords);

        vec3 T  = normalize(Q1 * st2.t - Q2 * st1.t);
        vec3 B  = -normalize(cross(N, T));
        mat3 TBN = mat3(T, B, N);

        N = normalize(TBN * tangentNormal);
    }

    // Вектор к камере
    vec3 V = normalize(uCameraPosition - WorldPos);

    // Базовое отражение F0: 0.04 для диэлектриков, цвет металла для металлов
    vec3 F0 = vec3(0.04);
    F0 = mix(F0, albedo, metallic);

    // Суммарный свет от всех источников
    vec3 Lo = vec3(0.0);
    for(int i = 0; i < uNumLights && i < 4; ++i)
    {
        vec3 lightPosition = uPointLightPositions[i].xyz;
        vec3 lightColor = uPointLightColors[i].rgb;
        float lightIntensity = uPointLightColors[i].a;

        // Направление к свету и полувектор для отражения
        vec3 L = normalize(lightPosition - WorldPos);
        vec3 H = normalize(V + L);

        // Затухание света с расстоянием
        float distance = length(lightPosition - WorldPos);
        float attenuation = 1.0 / (distance * distance);
        vec3 radiance = lightColor * lightIntensity * attenuation;

        // Cook-Torrance BRDF: три компонента
        float NDF = DistributionGGX(N, H, roughness);   // Распределение граней
        float G   = GeometrySmith(N, V, L, roughness);   // Геометрическое затенение
        vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0); // Френель

        // Зеркальная составляющая
        vec3 numerator = NDF * G * F;
        float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
        vec3 specular = numerator / denominator;

        // Баланс энергии: зеркальный + рассеянный свет
        vec3 kS = F;
        vec3 kD = vec3(1.0) - kS;
        kD *= 1.0 - metallic;  // Металлы не имеют рассеянного света

        float NdotL = max(dot(N, L), 0.0);
        Lo += (kD * albedo / PI + specular) * radiance * NdotL;
    }

    // Фоновое освещение
    vec3 ambient = vec3(0.5) * albedo * ao;
    vec3 color = ambient + Lo;

    // Добавляем свечение
    if (uEmissionIntensity > 0.0) {
        vec3 emission = uEmissionColor * uEmissionIntensity;
        if (uHasTexture == 1) {
            emission *= texture(uTexture, TexCoords).rgb;
        }
        color += emission;
    }

    // Тон маппинг - сжимаем HDR в диапазон от 0 до 1
    color = color / (color + vec3(1.0));
    // Гамма коррекция - корректируем под монитор
    color = pow(color, vec3(1.0/2.2));

    FragColor = vec4(color, uAlpha);
}