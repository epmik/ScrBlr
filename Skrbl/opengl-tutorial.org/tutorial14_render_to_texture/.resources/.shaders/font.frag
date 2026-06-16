
// --------------------------------

#version 330 core

// --------------------------------

in vec4 t_rgba;
in vec2 t_uv;

uniform sampler2D u_source;

out vec4 o_color;

// --------------------------------
			
void main()
{
	o_color = texture(u_source, t_uv);
}

// --------------------------------
