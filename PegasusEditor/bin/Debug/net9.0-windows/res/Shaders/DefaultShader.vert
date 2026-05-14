#version 430 core

struct Triangle {
    vec4 v0;
    vec4 v1;
    vec4 v2;
};

layout(std430, binding = 0) buffer MeshBuffer {
    Triangle triangles[];
};

out VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
    vec4 FragPosLightSpace;
} vs_out;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 lightSpaceMatrix;

uniform mat4 modelInverseTransposed;

uniform uint u_FirstTriIdx;

void main()
{
    uint triIdx = u_FirstTriIdx + ( gl_VertexID / 3 );
    
    uint vertOffset = gl_VertexID % 3;
    
    vec4 rawPos;
    if ( vertOffset == 0 )
        rawPos = triangles[ triIdx ].v0;
    else if ( vertOffset == 1 )
        rawPos = triangles[ triIdx ].v1;
    else
        rawPos = triangles[ triIdx ].v2;
    
    vec3 aPos = rawPos.xyz;
    
    vec3 aNormal = vec3( 0.0, 1.0, 0.0 );
    vec2 aTexCoords = vec2( 0.0 );
    
    vs_out.FragPos = vec3(vec4(aPos, 1.0) * model);
    vs_out.Normal = mat3(modelInverseTransposed) * aNormal;
    vs_out.TexCoords = aTexCoords;
    vs_out.FragPosLightSpace = vec4(aPos, 1.0) * model * lightSpaceMatrix;

    gl_Position = vec4(aPos, 1.0) * model * view * projection;
}