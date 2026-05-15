#version 430 core

struct Vertex {
    vec4 Position;
    vec4 Normal;
    vec4 TexCoord;
    vec4 Color;
};

struct Triangle {
    Vertex v0;
    Vertex v1;
    Vertex v2;
};

layout(std430, binding = 0) buffer MeshBuffer {
    Triangle triangles[];
};

out VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
    vec4 Color;
} vs_out;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform mat4 modelInverseTransposed;
uniform uint u_FirstTriIdx;

void main()
{
    uint triIdx = u_FirstTriIdx + ( gl_VertexID / 3 );
    uint vertOffset = gl_VertexID % 3;
    
    Vertex rawVertex;
    if ( vertOffset == 0 )
        rawVertex = triangles[ triIdx ].v0;
    else if ( vertOffset == 1 )
        rawVertex = triangles[ triIdx ].v1;
    else
        rawVertex = triangles[ triIdx ].v2;
    
    vec3 aPos = rawVertex.Position.xyz;    
    vec3 aNormal = rawVertex.Normal.xyz;
    vec2 aTexCoords = rawVertex.TexCoord.xy;
    vec4 aColor = rawVertex.Color;
    
    vs_out.FragPos = vec3( vec4( aPos, 1.0 ) * model );
    vs_out.Normal = mat3( modelInverseTransposed ) * aNormal;
    vs_out.TexCoords = aTexCoords;
    vs_out.Color = aColor;

    gl_Position = vec4(aPos, 1.0) * model * view * projection;
}