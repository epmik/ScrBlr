


// --------------------------------

#version 330 core

// --------------------------------

in vec2 t_uv;

uniform sampler2D u_source;
uniform vec2 u_resolution;
uniform float u_radius;

out vec4 o_color;

// --------------------------------
			
void main()
{
	if(u_radius < 1.0)
	{
		o_color = texture2D(u_source, t_uv);
	
		return;
	}

	float x, y, rr = u_radius * u_radius, d, w, w0;
	vec2 p = t_uv;
	vec4 col = vec4(0.0, 0.0, 0.0, 0.0);
	w0 = 0.5135 / pow(u_radius, 0.96);
	for (d = 1.0 / u_resolution.x, x = -u_radius, p.x += x * d; x <= u_radius; x++, p.x += d)
	{
		w = w0 * exp((-x * x) / (2.0 * rr));
		col += texture2D(u_source, p) * w;
	}
	o_color = col;
}

// --------------------------------
