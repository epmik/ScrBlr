#version 330 core

layout(location = 0) in vec3 i_position;
layout(location = 1) in vec2 i_uv;

out vec2 t_uv;

void main()
{
	gl_Position =  vec4(i_position, 1);
	t_uv = i_uv;
}
