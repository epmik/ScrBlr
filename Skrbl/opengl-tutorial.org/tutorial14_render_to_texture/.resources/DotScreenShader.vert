
layout(location = 0) in vec3 position;
layout(location = 1) in vec2 uv;

out vec2 vUv;

uniform mat4 projectionMatrix;
uniform mat4 modelViewMatrix;

void main() 
{

	vUv = uv;
	gl_Position = projectionMatrix * modelViewMatrix * vec4( position, 1.0 );

}