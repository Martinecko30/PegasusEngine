#version 330 core

in vec3 WorldPos;
out vec4 FragColor;

uniform vec3 cameraPos;

const float lineThickness = 0.02;

void main()
{    
    vec2 gridPos = fract( WorldPos.xz );
    
    float isLineX = step( gridPos.x, lineThickness ) + step( 1.0 - lineThickness, gridPos.x );
    float isLineZ = step( gridPos.y, lineThickness ) + step( 1.0 - lineThickness, gridPos.y );
    float isLine = max( isLineX, isLineZ );
    
    vec4 color = vec4( 0.4, 0.4, 0.4, isLine * 0.3 );
    
    if (abs(WorldPos.x) < lineThickness * 1.5) color = vec4(0.2, 0.2, 1.0, 1.0); // Thick Blue Z-Axis
    if (abs(WorldPos.z) < lineThickness * 1.5) color = vec4(1.0, 0.2, 0.2, 1.0); // Thick Red X-Axis
    
    float dist = distance( WorldPos, cameraPos );
    float fade = 1.0 - clamp( dist / 50.0, 0.0, 1.0 );
    
    color.a *= fade;
    
    if ( color.a == 0.0 ) discard;
    
    FragColor = color;
}