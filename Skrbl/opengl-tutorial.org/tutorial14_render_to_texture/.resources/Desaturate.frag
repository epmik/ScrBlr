
//
// Original shader from ShaderToy by user sepehr
// https://www.shadertoy.com/view/lsdXDH
//

#version 330 core

in vec2 t_uv;

out vec4 o_color;

uniform sampler2D u_source;

// Generic algorithm to desaturate images used in most game engines
vec4 generic_desaturate(vec3 color, float factor)
{
	vec3 lum = vec3(0.299, 0.587, 0.114);
	vec3 gray = vec3(dot(lum, color));
	return vec4(mix(color, gray, factor), 1.0);
}

// Algorithm employed by photoshop to desaturate the input
vec4 photoshop_desaturate(vec3 color)
{
    float bw = (min(color.r, min(color.g, color.b)) + max(color.r, max(color.g, color.b))) * 0.5;
    return vec4(bw, bw, bw, 1.0);
}

void main()
{
 	// o_color = generic_desaturate(texture(u_source, t_uv).rgb, 1.0).xyz;
    o_color = photoshop_desaturate(texture(u_source, t_uv).rgb);
}