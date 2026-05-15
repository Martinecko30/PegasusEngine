#version 330 core
#define MAX_LIGHTS 10

out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
    vec4 Color;
} fs_in;

struct Light {
    vec3 position;
    vec3 color;
};

uniform sampler2D diffuseTexture;
uniform bool hasTexture;

uniform sampler2D specularTexture;

uniform Light lights[ MAX_LIGHTS ];

uniform sampler2D shadowMaps[ MAX_LIGHTS ];
uniform mat4 lightSpaceMatrices[ MAX_LIGHTS ];
uniform int activeLightCount;

uniform vec3 viewPos;
uniform bool gamma;

uniform float near_plane;
uniform float far_plane;

uniform samplerCube skybox;

float LinearizeDepth(float depth)
{
    float z = depth * 2.0 - 1.0; // Back to NDC 
    return (2.0 * near_plane * far_plane) / (far_plane + near_plane - z * (far_plane - near_plane));
    
    /*
    float depthValue = texture(depthMap, TexCoords).r;
    FragColor = vec4(vec3(LinearizeDepth(depthValue) / far_plane), 1.0); // perspective
    // FragColor = vec4(vec3(depthValue), 1.0); // orthographic
    */
}

vec3 BlinnPhong(vec3 normal, vec3 fragPos, vec3 lightPos, vec3 lightColor, float shadow) {
    // Ambient
    vec3 ambient = lightColor * 0.25;

    // Diffuse
    vec3 lightDir = normalize(lightPos - fragPos);
    float diff = max(dot(lightDir, normal), 0.0);
    vec3 diffuse = diff * lightColor;

    // Specular
    vec3 viewDir = normalize(viewPos - fragPos);
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    vec3 specular = spec * lightColor;

    // Simple attenuation
    float max_distance = 1.5;
    float distance = length(lightPos - fragPos);
    float attenuation = 1.0 / (gamma ? distance * distance : distance);

    // Apply attenuation
    diffuse *= attenuation;
    specular *= attenuation;

    // Combine ambient, diffuse, and specular with shadow calculation
    return (ambient + ((1.0 - shadow) * (diffuse + specular)));
}

float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal, vec3 lightDir, int lightIndex) {
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = (projCoords * 0.5) + 0.5;

    if(projCoords.z > 1.0)
        return 0.0;

    float currentDepth = projCoords.z;
    float bias = max( 0.05 * ( 1.0 - dot( normal, lightDir ) ), 0.005 );
    float shadow = 0.0;

    vec2 texelSize = 1.0 / textureSize(shadowMaps[ lightIndex ], 0);
    for(int x = -1; x <= 1; ++x)
    {
        for(int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(shadowMaps[ lightIndex ], projCoords.xy + vec2(x, y) * texelSize).r;
            shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;
        }
    }
    shadow /= 9.0;

    return shadow;
}

void main() {
    vec4 baseColor = fs_in.Color;
    
    if (hasTexture) {
        baseColor *= texture(diffuseTexture, fs_in.TexCoords); 
    }
    
    vec3 color = baseColor.rgb;
    vec3 normal = normalize(fs_in.Normal);

    vec3 lighting = vec3(0.0);
    for(int i = 0; i < activeLightCount; ++i) {
        Light light = lights[i];
        
        vec4 fragPosLightSpace = lightSpaceMatrices[ i ] * vec4( fs_in.FragPos, 1.0 );
        
        vec3 lightDir = normalize( light.position - fs_in.FragPos );
        float shadow = ShadowCalculation(fragPosLightSpace, normal, lightDir, i); // Calculate shadow for each light
        lighting += BlinnPhong(normal, fs_in.FragPos, light.position, light.color, shadow);
    }

    // Prevent over-exposure by clamping lighting
    lighting = clamp(lighting, 0.0, 1.0);

    // Apply the lighting result to the texture color
    color *= lighting;

    // Gamma correction
    if(gamma)
        color = pow(color, vec3((1.0 / 2.2)));

    FragColor = vec4(color, 1.0);
    //vec3 I = normalize(fs_in.FragPos - viewPos);
    //vec3 R = reflect(I, normalize(fs_in.Normal));
    //FragColor = vec4(texture(skybox, R).rgb, 1.0);
}