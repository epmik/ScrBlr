
// 
// color multiply shader
// like the muliply blend mode in photoshop
// 

#version 330 core

in vec2 t_uv;

uniform sampler2D u_source;

out vec4 o_color;

void main() 
{
    vec4 c = texture( u_source, t_uv);
    o_color = c * c;
}
