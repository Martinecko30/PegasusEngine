#version 330 core
layout ( location = 0 ) in vec2 aPos;

uniform mat4 view;
uniform mat4 projection;
uniform vec3 cameraPos;

out vec3 WorldPos;

void main()
{
    vec3 pos = vec3( aPos.x * 10000.0, 0.0, aPos.y * 10000.0 );
    
    pos.x += cameraPos.x;
    pos.z += cameraPos.z;
    
    WorldPos = pos;    
    gl_Position = vec4( pos, 1.0 ) * view * projection;
}