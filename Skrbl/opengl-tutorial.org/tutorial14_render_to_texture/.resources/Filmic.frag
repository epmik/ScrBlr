//
// Created by inigo quilez - iq/2013
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.// Filmic Shader
// https://www.shadertoy.com/view/4dfGzn
// 

#version 330 core

in vec2 t_uv;

uniform float u_time;
uniform sampler2D u_source;

out vec4 o_color;

void main() 
{
    vec4 c = texture( u_source, t_uv);
    o_color = c * c;

    //vec2 q = t_uv;
    //vec2 uv = 0.5 + (q-0.5)*(0.9 + 0.1*sin(0.2*u_time));

    //vec3 oricol = texture( u_source, vec2(q.x,1.0-q.y) ).xyz;
    //vec3 col;

    //col.r = texture(u_source,vec2(uv.x+0.003,-uv.y)).x;
    //col.g = texture(u_source,vec2(uv.x+0.000,-uv.y)).y;
    //col.b = texture(u_source,vec2(uv.x-0.003,-uv.y)).z;

    //col = clamp(col*0.5+0.5*col*col*1.2,0.0,1.0);

    //col *= 0.5 + 0.5*16.0*uv.x*uv.y*(1.0-uv.x)*(1.0-uv.y);

    //col *= vec3(0.95,1.05,0.95);

    //col *= 0.9+0.1*sin(10.0*u_time+uv.y*1000.0);

    //col *= 0.99+0.01*sin(110.0*u_time);

    //float comp = smoothstep( 0.2, 0.7, sin(u_time) );
    //col = mix( col, oricol, clamp(-2.0+2.0*q.x+3.0*comp,0.0,1.0) );

    //o_color = col;
    //o_color = texture( u_source, t_uv).xyz;
}
