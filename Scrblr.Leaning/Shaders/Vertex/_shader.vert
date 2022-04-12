#version 330 core

// the position variable has attribute position 0
layout(location = 0) in vec3 aPosition;  

// This is where the color values we assigned in the main program goes to
layout(location = 1) in vec3 aColor;

out vec3 ioColor; // output a color to the fragment shader

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;

	// We use the ioColor variable to pass on the color information to the frag shader
	ioColor = aColor;
}