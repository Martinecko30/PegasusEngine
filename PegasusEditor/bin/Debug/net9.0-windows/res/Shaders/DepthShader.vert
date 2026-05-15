#version 430

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

layout( std430, binding = 0 ) buffer MeshBuffer {
    Triangle triangles[];
};

uniform mat4 model;
uniform mat4 lightSpaceMatrix;
uniform uint u_FirstTriIdx;

void main() {
    uint triIdx = u_FirstTriIdx + ( gl_VertexID / 3 );
    uint vertOffset = gl_VertexID % 3;

    Vertex rawVertex;
    if ( vertOffset == 0 )
        rawVertex = triangles[ triIdx ].v0;
    else if ( vertOffset == 1 )
        rawVertex = triangles[ triIdx ].v1;
    else
        rawVertex = triangles[ triIdx ].v2;

    gl_Position = vec4( rawVertex.Position.xyz, 1.0 ) * model * lightSpaceMatrix;
}