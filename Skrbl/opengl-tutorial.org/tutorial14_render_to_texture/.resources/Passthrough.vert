#version 330 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 i_position;

// Output data ; will be interpolated for each fragment.
out vec2 UV;

void main()
{
	gl_Position =  vec4(i_position, 1);
	UV = (i_position.xy + vec2(1,1)) / 2.0;
}
