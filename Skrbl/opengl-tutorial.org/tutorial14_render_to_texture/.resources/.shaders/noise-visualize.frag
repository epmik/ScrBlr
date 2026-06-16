
#version 330 core

in vec2 t_uv;
in vec3 t_position;

uniform float u_bias;
uniform int u_noiseType;
uniform int u_octaves;
uniform float u_amplitude;
uniform float u_frequency;
uniform float u_lacunarity;
uniform float u_persistence;

uniform vec4 u_inputScale;
uniform vec4 u_inputOffset;

out vec4 o_color;

// --------------------------------
// https://stackoverflow.com/a/4275343

float randomOneLiner(vec2 co)
{
	return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

// --------------------------------
// https://stackoverflow.com/a/17479300

// A single iteration of Bob Jenkins' One-At-A-Time hashing algorithm.
uint hash( uint x ) {
	x += ( x << 10u );
	x ^= ( x >>  6u );
	x += ( x <<  3u );
	x ^= ( x >> 11u );
	x += ( x << 15u );
	return x;
}

// Compound versions of the hashing algorithm I whipped together.
uint hash( uvec2 v ) { return hash( v.x ^ hash(v.y)                         ); }
uint hash( uvec3 v ) { return hash( v.x ^ hash(v.y) ^ hash(v.z)             ); }
uint hash( uvec4 v ) { return hash( v.x ^ hash(v.y) ^ hash(v.z) ^ hash(v.w) ); }

// Construct a float with half-open range [0:1] using low 23 bits.
// All zeroes yields 0.0, all ones yields the next smallest representable value below 1.0.
float floatConstruct( uint m ) {
	const uint ieeeMantissa = 0x007FFFFFu; // binary32 mantissa bitmask
	const uint ieeeOne      = 0x3F800000u; // 1.0 in IEEE binary32

	m &= ieeeMantissa;                     // Keep only mantissa bits (fractional part)
	m |= ieeeOne;                          // Add fractional part to 1.0

	float  f = uintBitsToFloat( m );       // Range [1:2]
	return f - 1.0;                        // Range [0:1]
}

// https://stackoverflow.com/a/17479300
// Pseudo-random value in half-open range [0:1].
float randomIntHash( float x ) { return floatConstruct(hash(floatBitsToUint(x))); }
float randomIntHash( vec2  v ) { return floatConstruct(hash(floatBitsToUint(v))); }
float randomIntHash( vec3  v ) { return floatConstruct(hash(floatBitsToUint(v))); }
float randomIntHash( vec4  v ) { return floatConstruct(hash(floatBitsToUint(v))); }

float randomIntHash( float x, float min, float max ) { return min + ((max - min) * randomIntHash(x)); }
float randomIntHash( vec2 v, float min, float max ) { return min + ((max - min) * randomIntHash(v)); }
float randomIntHash( vec3 v, float min, float max ) { return min + ((max - min) * randomIntHash(v)); }
float randomIntHash( vec4 v, float min, float max ) { return min + ((max - min) * randomIntHash(v)); }

// --------------------------------

// taken from https://youtu.be/lctXaT9pxA0?t=458

float biasValue(float x, float bias)
{
	float k = pow(1.0 - bias, 3.0);
			
	return (x * k) / (x * k - x + 1.0);
}

// --------------------------------
// https://stackoverflow.com/a/28095165

float PHI = 1.61803398874989484820459;  // Φ = Golden Ratio   

float gold_noise(in vec2 xy, in float seed)
{
	return fract(tan(distance(xy*PHI, xy)*seed)*xy.x);
}

// --------------------------------
// https://stackoverflow.com/a/57390568

highp float randomImprovedOneLiner(vec2 co)
{
	highp float a = 12.9898;
	highp float b = 78.233;
	highp float c = 43758.5453;
	highp float dt= dot(co.xy ,vec2(a,b));
	highp float sn= mod(dt,3.14);
		
	return fract(sin(sn) * c);
}

// --------------------------------
// https://stackoverflow.com/a/54874852

int b(int N, int B) { return N>>B & 1; }
int T[] = int[](0x15,0x38,0x32,0x2c,0x0d,0x13,0x07,0x2a);
int A[] = int[](0,0,0);

int b(int i, int j, int k, int B) { return T[b(i,B)<<2 | b(j,B)<<1 | b(k,B)]; }

int shuffle(int i, int j, int k) {
	return b(i,j,k,0) + b(j,k,i,1) + b(k,i,j,2) + b(i,j,k,3) +
		b(j,k,i,4) + b(k,i,j,5) + b(i,j,k,6) + b(j,k,i,7) ;
}

float K(int a, vec3 uvw, vec3 ijk)
{
	float s = float(A[0]+A[1]+A[2])/6.0;
	float x = uvw.x - float(A[0]) + s,
		y = uvw.y - float(A[1]) + s,
		z = uvw.z - float(A[2]) + s,
		t = 0.6 - x * x - y * y - z * z;
	int h = shuffle(int(ijk.x) + A[0], int(ijk.y) + A[1], int(ijk.z) + A[2]);
	A[a]++;
	if (t < 0.0)
		return 0.0;
	int b5 = h>>5 & 1, b4 = h>>4 & 1, b3 = h>>3 & 1, b2= h>>2 & 1, b = h & 3;
	float p = b==1?x:b==2?y:z, q = b==1?y:b==2?z:x, r = b==1?z:b==2?x:y;
	p = (b5==b3 ? -p : p); q = (b5==b4 ? -q : q); r = (b5!=(b4^b3) ? -r : r);
	t *= t;
	return 8.0 * t * t * (p + (b==0 ? q+r : b2==0 ? q : r));
}

float AndrewMeservyKenPerlinNoise(float x, float y, float z)
{
	float s = (x + y + z) / 3.0;  
	vec3 ijk = vec3(int(floor(x+s)), int(floor(y+s)), int(floor(z+s)));
	s = float(ijk.x + ijk.y + ijk.z) / 6.0;
	vec3 uvw = vec3(x - float(ijk.x) + s, y - float(ijk.y) + s, z - float(ijk.z) + s);
	A[0] = A[1] = A[2] = 0;
	int hi = uvw.x >= uvw.z ? uvw.x >= uvw.y ? 0 : 1 : uvw.y >= uvw.z ? 1 : 2;
	int lo = uvw.x <  uvw.z ? uvw.x <  uvw.y ? 0 : 1 : uvw.y <  uvw.z ? 1 : 2;
	return K(hi, uvw, ijk) + K(3 - hi - lo, uvw, ijk) + K(lo, uvw, ijk) + K(0, uvw, ijk);
}		

float AndrewMeservyKenPerlinNoise(vec3 xyz)
{
	float s = (xyz.x + xyz.y + xyz.z) / 3.0;  
			
	vec3 ijk = vec3(int(floor(xyz.x+s)), int(floor(xyz.y+s)), int(floor(xyz.z+s)));
			
	s = float(ijk.x + ijk.y + ijk.z) / 6.0;
			
	vec3 uvw = vec3(xyz.x - float(ijk.x) + s, xyz.y - float(ijk.y) + s, xyz.z - float(ijk.z) + s);
			
	A[0] = A[1] = A[2] = 0;
			
	int hi = uvw.x >= uvw.z ? uvw.x >= uvw.y ? 0 : 1 : uvw.y >= uvw.z ? 1 : 2;
	int lo = uvw.x <  uvw.z ? uvw.x <  uvw.y ? 0 : 1 : uvw.y <  uvw.z ? 1 : 2;
			
	return K(hi, uvw, ijk) + K(3 - hi - lo, uvw, ijk) + K(lo, uvw, ijk) + K(0, uvw, ijk);
}		

// --------------------------------

//
// Description : Array and textureless GLSL 2D simplex noise function.
//      Author : Ian McEwan, Ashima Arts.
//  Maintainer : stegu
//     Lastmod : 20110822 (ijm)
//     License : Copyright (C) 2011 Ashima Arts. All rights reserved.
//               Distributed under the MIT License. See LICENSE file.
//               https://github.com/ashima/webgl-noise
//               https://github.com/stegu/webgl-noise
// 

float mod289(float x) 
{
  	return x - floor(x * (1.0 / 289.0)) * 289.0; 
}

vec2 mod289(vec2 x) 
{
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec3 mod289(vec3 x) 
{
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 mod289(vec4 x)
{
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}

float permute(float x) 
{
	return mod289(((x*34.0)+10.0)*x);
}
	   
vec3 permute(vec3 x) 
{
	return mod289(((x*34.0)+10.0)*x);
}
		
vec4 permute(vec4 x)
{
	return mod289(((x*34.0)+10.0)*x);
}

float taylorInvSqrt(float r)
{
	return 1.79284291400159 - 0.85373472095314 * r;
}
		
vec4 taylorInvSqrt(vec4 r)
{
return 1.79284291400159 - 0.85373472095314 * r;
}

		
vec2 fade(vec2 t) {
return t*t*t*(t*(t*6.0-15.0)+10.0);
}

float snoise(vec2 v)
{
const vec4 C = vec4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
					0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
					-0.577350269189626,  // -1.0 + 2.0 * C.x
					0.024390243902439); // 1.0 / 41.0
// First corner
vec2 i  = floor(v + dot(v, C.yy) );
vec2 x0 = v -   i + dot(i, C.xx);

// Other corners
vec2 i1;
//i1.x = step( x0.y, x0.x ); // x0.x > x0.y ? 1.0 : 0.0
//i1.y = 1.0 - i1.x;
i1 = (x0.x > x0.y) ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
// x0 = x0 - 0.0 + 0.0 * C.xx ;
// x1 = x0 - i1 + 1.0 * C.xx ;
// x2 = x0 - 1.0 + 2.0 * C.xx ;
vec4 x12 = x0.xyxy + C.xxzz;
x12.xy -= i1;

// Permutations
i = mod289(i); // Avoid truncation effects in permutation
vec3 p = permute( permute( i.y + vec3(0.0, i1.y, 1.0 ))
		+ i.x + vec3(0.0, i1.x, 1.0 ));

vec3 m = max(0.5 - vec3(dot(x0,x0), dot(x12.xy,x12.xy), dot(x12.zw,x12.zw)), 0.0);
m = m*m ;
m = m*m ;

// Gradients: 41 points uniformly over a line, mapped onto a diamond.
// The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)

vec3 x = 2.0 * fract(p * C.www) - 1.0;
vec3 h = abs(x) - 0.5;
vec3 ox = floor(x + 0.5);
vec3 a0 = x - ox;

// Normalise gradients implicitly by scaling m
// Approximation of: m *= inversesqrt( a0*a0 + h*h );
m *= 1.79284291400159 - 0.85373472095314 * ( a0*a0 + h*h );

// Compute final noise value at P
vec3 g;
g.x  = a0.x  * x0.x  + h.x  * x0.y;
g.yz = a0.yz * x12.xz + h.yz * x12.yw;
return 130.0 * dot(m, g);
}		

float snoise(vec3 v)
{ 
const vec2  C = vec2(1.0/6.0, 1.0/3.0) ;
const vec4  D = vec4(0.0, 0.5, 1.0, 2.0);

// First corner
vec3 i  = floor(v + dot(v, C.yyy) );
vec3 x0 =   v - i + dot(i, C.xxx) ;

// Other corners
vec3 g = step(x0.yzx, x0.xyz);
vec3 l = 1.0 - g;
vec3 i1 = min( g.xyz, l.zxy );
vec3 i2 = max( g.xyz, l.zxy );

//   x0 = x0 - 0.0 + 0.0 * C.xxx;
//   x1 = x0 - i1  + 1.0 * C.xxx;
//   x2 = x0 - i2  + 2.0 * C.xxx;
//   x3 = x0 - 1.0 + 3.0 * C.xxx;
vec3 x1 = x0 - i1 + C.xxx;
vec3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
vec3 x3 = x0 - D.yyy;      // -1.0+3.0*C.x = -0.5 = -D.y

// Permutations
i = mod289(i); 
vec4 p = permute( permute( permute( 
			i.z + vec4(0.0, i1.z, i2.z, 1.0 ))
		+ i.y + vec4(0.0, i1.y, i2.y, 1.0 )) 
		+ i.x + vec4(0.0, i1.x, i2.x, 1.0 ));

// Gradients: 7x7 points over a square, mapped onto an octahedron.
// The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
float n_ = 0.142857142857; // 1.0/7.0
vec3  ns = n_ * D.wyz - D.xzx;

vec4 j = p - 49.0 * floor(p * ns.z * ns.z);  //  mod(p,7*7)

vec4 x_ = floor(j * ns.z);
vec4 y_ = floor(j - 7.0 * x_ );    // mod(j,N)

vec4 x = x_ *ns.x + ns.yyyy;
vec4 y = y_ *ns.x + ns.yyyy;
vec4 h = 1.0 - abs(x) - abs(y);

vec4 b0 = vec4( x.xy, y.xy );
vec4 b1 = vec4( x.zw, y.zw );

//vec4 s0 = vec4(lessThan(b0,0.0))*2.0 - 1.0;
//vec4 s1 = vec4(lessThan(b1,0.0))*2.0 - 1.0;
vec4 s0 = floor(b0)*2.0 + 1.0;
vec4 s1 = floor(b1)*2.0 + 1.0;
vec4 sh = -step(h, vec4(0.0));

vec4 a0 = b0.xzyw + s0.xzyw*sh.xxyy ;
vec4 a1 = b1.xzyw + s1.xzyw*sh.zzww ;

vec3 p0 = vec3(a0.xy,h.x);
vec3 p1 = vec3(a0.zw,h.y);
vec3 p2 = vec3(a1.xy,h.z);
vec3 p3 = vec3(a1.zw,h.w);

//Normalise gradients
vec4 norm = taylorInvSqrt(vec4(dot(p0,p0), dot(p1,p1), dot(p2, p2), dot(p3,p3)));
p0 *= norm.x;
p1 *= norm.y;
p2 *= norm.z;
p3 *= norm.w;

// Mix final noise value
vec4 m = max(0.5 - vec4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
m = m * m;
return 105.0 * dot( m*m, vec4( dot(p0,x0), dot(p1,x1), 
								dot(p2,x2), dot(p3,x3) ) );
}


vec4 grad4(float j, vec4 ip)
{
	const vec4 ones = vec4(1.0, 1.0, 1.0, -1.0);
	vec4 p,s;
		
	p.xyz = floor( fract (vec3(j) * ip.xyz) * 7.0) * ip.z - 1.0;
	p.w = 1.5 - dot(abs(p.xyz), ones.xyz);
	s = vec4(lessThan(p, vec4(0.0)));
	p.xyz = p.xyz + (s.xyz*2.0 - 1.0) * s.www; 
		
	return p;
}		
	

// (sqrt(5) - 1)/4 = F4, used once below
#define F4 0.309016994374947451
		
float snoise(vec4 v)
{
	const vec4  C = vec4( 0.138196601125011,  // (5 - sqrt(5))/20  G4
							0.276393202250021,  // 2 * G4
							0.414589803375032,  // 3 * G4
						-0.447213595499958); // -1 + 4 * G4

	// First corner
	vec4 i  = floor(v + dot(v, vec4(F4)) );
	vec4 x0 = v -   i + dot(i, C.xxxx);

	// Other corners

	// Rank sorting originally contributed by Bill Licea-Kane, AMD (formerly ATI)
	vec4 i0;
	vec3 isX = step( x0.yzw, x0.xxx );
	vec3 isYZ = step( x0.zww, x0.yyz );
	//  i0.x = dot( isX, vec3( 1.0 ) );
	i0.x = isX.x + isX.y + isX.z;
	i0.yzw = 1.0 - isX;
	//  i0.y += dot( isYZ.xy, vec2( 1.0 ) );
	i0.y += isYZ.x + isYZ.y;
	i0.zw += 1.0 - isYZ.xy;
	i0.z += isYZ.z;
	i0.w += 1.0 - isYZ.z;

	// i0 now contains the unique values 0,1,2,3 in each channel
	vec4 i3 = clamp( i0, 0.0, 1.0 );
	vec4 i2 = clamp( i0-1.0, 0.0, 1.0 );
	vec4 i1 = clamp( i0-2.0, 0.0, 1.0 );

	//  x0 = x0 - 0.0 + 0.0 * C.xxxx
	//  x1 = x0 - i1  + 1.0 * C.xxxx
	//  x2 = x0 - i2  + 2.0 * C.xxxx
	//  x3 = x0 - i3  + 3.0 * C.xxxx
	//  x4 = x0 - 1.0 + 4.0 * C.xxxx
	vec4 x1 = x0 - i1 + C.xxxx;
	vec4 x2 = x0 - i2 + C.yyyy;
	vec4 x3 = x0 - i3 + C.zzzz;
	vec4 x4 = x0 + C.wwww;

	// Permutations
	i = mod289(i); 
	float j0 = permute( permute( permute( permute(i.w) + i.z) + i.y) + i.x);
	vec4 j1 = permute( permute( permute( permute (
				i.w + vec4(i1.w, i2.w, i3.w, 1.0 ))
			+ i.z + vec4(i1.z, i2.z, i3.z, 1.0 ))
			+ i.y + vec4(i1.y, i2.y, i3.y, 1.0 ))
			+ i.x + vec4(i1.x, i2.x, i3.x, 1.0 ));

	// Gradients: 7x7x6 points over a cube, mapped onto a 4-cross polytope
	// 7*7*6 = 294, which is close to the ring size 17*17 = 289.
	vec4 ip = vec4(1.0/294.0, 1.0/49.0, 1.0/7.0, 0.0) ;

	vec4 p0 = grad4(j0,   ip);
	vec4 p1 = grad4(j1.x, ip);
	vec4 p2 = grad4(j1.y, ip);
	vec4 p3 = grad4(j1.z, ip);
	vec4 p4 = grad4(j1.w, ip);

	// Normalise gradients
	vec4 norm = taylorInvSqrt(vec4(dot(p0,p0), dot(p1,p1), dot(p2, p2), dot(p3,p3)));
	p0 *= norm.x;
	p1 *= norm.y;
	p2 *= norm.z;
	p3 *= norm.w;
	p4 *= taylorInvSqrt(dot(p4,p4));

	// Mix contributions from the five corners
	vec3 m0 = max(0.6 - vec3(dot(x0,x0), dot(x1,x1), dot(x2,x2)), 0.0);
	vec2 m1 = max(0.6 - vec2(dot(x3,x3), dot(x4,x4)            ), 0.0);
	m0 = m0 * m0;
	m1 = m1 * m1;
	return 49.0 * ( dot(m0*m0, vec3( dot( p0, x0 ), dot( p1, x1 ), dot( p2, x2 )))
				+ dot(m1*m1, vec2( dot( p3, x3 ), dot( p4, x4 ) ) ) ) ;
}

// --------------------------------

//
// GLSL textureless classic 2D noise "cnoise",
// with an RSL-style periodic variant "pnoise".
// Author:  Stefan Gustavson (stefan.gustavson@liu.se)
// Version: 2024-11-07
//
// Many thanks to Ian McEwan of Ashima Arts for the
// ideas for permutation and gradient selection.
//
// Copyright (c) 2011 Stefan Gustavson. All rights reserved.
// Distributed under the MIT license. See LICENSE file.
// https://github.com/stegu/webgl-noise
//

// Classic Perlin noise
float cnoise(vec2 P)
{
vec4 Pi = floor(P.xyxy) + vec4(0.0, 0.0, 1.0, 1.0);
vec4 Pf = fract(P.xyxy) - vec4(0.0, 0.0, 1.0, 1.0);
Pi = mod289(Pi); // To avoid truncation effects in permutation
vec4 ix = Pi.xzxz;
vec4 iy = Pi.yyww;
vec4 fx = Pf.xzxz;
vec4 fy = Pf.yyww;

vec4 i = permute(permute(ix) + iy);

vec4 gx = fract(i * (1.0 / 41.0)) * 2.0 - 1.0 ;
vec4 gy = abs(gx) - 0.5 ;
vec4 tx = floor(gx + 0.5);
gx = gx - tx;

vec2 g00 = vec2(gx.x,gy.x);
vec2 g10 = vec2(gx.y,gy.y);
vec2 g01 = vec2(gx.z,gy.z);
vec2 g11 = vec2(gx.w,gy.w);

vec4 norm = taylorInvSqrt(vec4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));

float n00 = norm.x * dot(g00, vec2(fx.x, fy.x));
float n10 = norm.y * dot(g10, vec2(fx.y, fy.y));
float n01 = norm.z * dot(g01, vec2(fx.z, fy.z));
float n11 = norm.w * dot(g11, vec2(fx.w, fy.w));

vec2 fade_xy = fade(Pf.xy);
vec2 n_x = mix(vec2(n00, n01), vec2(n10, n11), fade_xy.x);
float n_xy = mix(n_x.x, n_x.y, fade_xy.y);
return 2.3 * n_xy;
}

// Classic Perlin noise, periodic variant
float pnoise(vec2 P, vec2 rep)
{
vec4 Pi = floor(P.xyxy) + vec4(0.0, 0.0, 1.0, 1.0);
vec4 Pf = fract(P.xyxy) - vec4(0.0, 0.0, 1.0, 1.0);
Pi = mod(Pi, rep.xyxy); // To create noise with explicit period
Pi = mod289(Pi);        // To avoid truncation effects in permutation
vec4 ix = Pi.xzxz;
vec4 iy = Pi.yyww;
vec4 fx = Pf.xzxz;
vec4 fy = Pf.yyww;

vec4 i = permute(permute(ix) + iy);

vec4 gx = fract(i * (1.0 / 41.0)) * 2.0 - 1.0 ;
vec4 gy = abs(gx) - 0.5 ;
vec4 tx = floor(gx + 0.5);
gx = gx - tx;

vec2 g00 = vec2(gx.x,gy.x);
vec2 g10 = vec2(gx.y,gy.y);
vec2 g01 = vec2(gx.z,gy.z);
vec2 g11 = vec2(gx.w,gy.w);

vec4 norm = taylorInvSqrt(vec4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));

float n00 = norm.x * dot(g00, vec2(fx.x, fy.x));
float n10 = norm.y * dot(g10, vec2(fx.y, fy.y));
float n01 = norm.z * dot(g01, vec2(fx.z, fy.z));
float n11 = norm.w * dot(g11, vec2(fx.w, fy.w));

vec2 fade_xy = fade(Pf.xy);
vec2 n_x = mix(vec2(n00, n01), vec2(n10, n11), fade_xy.x);
float n_xy = mix(n_x.x, n_x.y, fade_xy.y);
return 2.3 * n_xy;
}		

// --------------------------------

// #version 120

// Cellular noise ("Worley noise") in 2D in GLSL.
// Copyright (c) Stefan Gustavson 2011-04-19. All rights reserved.
// This code is released under the conditions of the MIT license.
// See LICENSE file for details.
// https://github.com/stegu/webgl-noise

// Modulo 7 without a division
vec3 mod7(vec3 x) {
return x - floor(x * (1.0 / 7.0)) * 7.0;
}

// Cellular noise, returning F1 and F2 in a vec2.
// Standard 3x3 search window for good F1 and F2 values
vec2 cellular(vec2 P) {
#define K 0.142857142857 // 1/7
#define Ko 0.428571428571 // 3/7
#define jitter 1.0 // Less gives more regular pattern
	vec2 Pi = mod289(floor(P));
	vec2 Pf = fract(P);
	vec3 oi = vec3(-1.0, 0.0, 1.0);
	vec3 of = vec3(-0.5, 0.5, 1.5);
	vec3 px = permute(Pi.x + oi);
	vec3 p = permute(px.x + Pi.y + oi); // p11, p12, p13
	vec3 ox = fract(p*K) - Ko;
	vec3 oy = mod7(floor(p*K))*K - Ko;
	vec3 dx = Pf.x + 0.5 + jitter*ox;
	vec3 dy = Pf.y - of + jitter*oy;
	vec3 d1 = dx * dx + dy * dy; // d11, d12 and d13, squared
	p = permute(px.y + Pi.y + oi); // p21, p22, p23
	ox = fract(p*K) - Ko;
	oy = mod7(floor(p*K))*K - Ko;
	dx = Pf.x - 0.5 + jitter*ox;
	dy = Pf.y - of + jitter*oy;
	vec3 d2 = dx * dx + dy * dy; // d21, d22 and d23, squared
	p = permute(px.z + Pi.y + oi); // p31, p32, p33
	ox = fract(p*K) - Ko;
	oy = mod7(floor(p*K))*K - Ko;
	dx = Pf.x - 1.5 + jitter*ox;
	dy = Pf.y - of + jitter*oy;
	vec3 d3 = dx * dx + dy * dy; // d31, d32 and d33, squared
	// Sort out the two smallest distances (F1, F2)
	vec3 d1a = min(d1, d2);
	d2 = max(d1, d2); // Swap to keep candidates for F2
	d2 = min(d2, d3); // neither F1 nor F2 are now in d3
	d1 = min(d1a, d2); // F1 is now in d1
	d2 = max(d1a, d2); // Swap to keep candidates for F2
	d1.xy = (d1.x < d1.y) ? d1.xy : d1.yx; // Swap if smaller
	d1.xz = (d1.x < d1.z) ? d1.xz : d1.zx; // F1 is in d1.x
	d1.yz = min(d1.yz, d2.yz); // F2 is now not in d2.yz
	d1.y = min(d1.y, d1.z); // nor in  d1.z
	d1.y = min(d1.y, d2.x); // F2 is in d1.y, we're done.
	return sqrt(d1.xy);
}		


// --------------------------------
// https://github.com/patriciogonzalezvivo/lygia/blob/main/generative/random.glsl 

/*
contributors: ["Patricio Gonzalez Vivo", "David Hoskins", "Inigo Quilez"]
description: Pass a value and get some random normalize value between 0 and 1
use: float random[2|3](<float|vec2|vec3> value)
options:
	- RANDOM_HIGHER_RANGE: for working with a range over 0 and 1
	- RANDOM_SINLESS: Use sin-less random, which tolerates bigger values before producing pattern. From https://www.shadertoy.com/view/4djSRW
	- RANDOM_SCALE: by default this scale if for number with a big range. For producing good random between 0 and 1 use bigger range
examples:
	- /shaders/generative_random.frag
license:
	- MIT License (MIT) Copyright 2014, David Hoskins
*/

#ifndef RANDOM_SCALE
#ifdef RANDOM_HIGHER_RANGE
#define RANDOM_SCALE vec4(.1031, .1030, .0973, .1099)
#else
#define RANDOM_SCALE vec4(443.897, 441.423, .0973, .1099)
#endif
#endif

#ifndef FNC_RANDOM
#define FNC_RANDOM
float random(in float x) {
#ifdef RANDOM_SINLESS
	x = fract(x * RANDOM_SCALE.x);
	x *= x + 33.33;
	x *= x + x;
	return fract(x);
#else
	return fract(sin(x) * 43758.5453);
#endif
}

float random(in vec2 st) {
#ifdef RANDOM_SINLESS
	vec3 p3  = fract(vec3(st.xyx) * RANDOM_SCALE.xyz);
	p3 += dot(p3, p3.yzx + 33.33);
	return fract((p3.x + p3.y) * p3.z);
#else
	return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453);
#endif
}

float random(in vec3 pos) {
#ifdef RANDOM_SINLESS
	pos  = fract(pos * RANDOM_SCALE.xyz);
	pos += dot(pos, pos.zyx + 31.32);
	return fract((pos.x + pos.y) * pos.z);
#else
	return fract(sin(dot(pos.xyz, vec3(70.9898, 78.233, 32.4355))) * 43758.5453123);
#endif
}

float random(in vec4 pos) {
#ifdef RANDOM_SINLESS
	pos = fract(pos * RANDOM_SCALE);
	pos += dot(pos, pos.wzxy + 33.33);
	return fract((pos.x + pos.y) * (pos.z + pos.w));
#else
	float dot_product = dot(pos, vec4(12.9898,78.233,45.164,94.673));
	return fract(sin(dot_product) * 43758.5453);
#endif
}

vec2 random2(float p) {
	vec3 p3 = fract(vec3(p) * RANDOM_SCALE.xyz);
	p3 += dot(p3, p3.yzx + 19.19);
	return fract((p3.xx + p3.yz) * p3.zy);
}

vec2 random2(vec2 p) {
	vec3 p3 = fract(p.xyx * RANDOM_SCALE.xyz);
	p3 += dot(p3, p3.yzx + 19.19);
	return fract((p3.xx + p3.yz) * p3.zy);
}

vec2 random2(vec3 p3) {
	p3 = fract(p3 * RANDOM_SCALE.xyz);
	p3 += dot(p3, p3.yzx + 19.19);
	return fract((p3.xx + p3.yz) * p3.zy);
}

vec3 random3(float p) {
	vec3 p3 = fract(vec3(p) * RANDOM_SCALE.xyz);
	p3 += dot(p3, p3.yzx + 19.19);
	return fract((p3.xxy + p3.yzz) * p3.zyx); 
}

vec3 random3(vec2 p) {
	vec3 p3 = fract(vec3(p.xyx) * RANDOM_SCALE.xyz);
	p3 += dot(p3, p3.yxz + 19.19);
	return fract((p3.xxy + p3.yzz) * p3.zyx);
}

vec3 random3(vec3 p) {
	p = fract(p * RANDOM_SCALE.xyz);
	p += dot(p, p.yxz + 19.19);
	return fract((p.xxy + p.yzz) * p.zyx);
}

vec4 random4(float p) {
	vec4 p4 = fract(p * RANDOM_SCALE);
	p4 += dot(p4, p4.wzxy + 19.19);
	return fract((p4.xxyz + p4.yzzw) * p4.zywx);   
}

vec4 random4(vec2 p) {
	vec4 p4 = fract(p.xyxy * RANDOM_SCALE);
	p4 += dot(p4, p4.wzxy + 19.19);
	return fract((p4.xxyz + p4.yzzw) * p4.zywx);
}

vec4 random4(vec3 p) {
	vec4 p4 = fract(p.xyzx * RANDOM_SCALE);
	p4 += dot(p4, p4.wzxy + 19.19);
	return fract((p4.xxyz + p4.yzzw) * p4.zywx);
}

vec4 random4(vec4 p4) {
	p4 = fract(p4  * RANDOM_SCALE);
	p4 += dot(p4, p4.wzxy + 19.19);
	return fract((p4.xxyz + p4.yzzw) * p4.zywx);
}
#endif

// --------------------------------

/*
contributors: Patricio Gonzalez Vivo
description: Signed Random
use: srandomX(<vec2|vec3> x)
license:
	- Copyright (c) 2021 Patricio Gonzalez Vivo under Prosperity License - https://prosperitylicense.com/versions/3.0.0
	- Copyright (c) 2021 Patricio Gonzalez Vivo under Patron License - https://lygia.xyz/license
*/

#ifndef FNC_SRANDOM
#define FNC_SRANDOM

float srandom(in float x) {
return -1. + 2. * fract(sin(x) * 43758.5453);
}

float srandom(in vec2 st) {
return -1. + 2. * fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453);
}

float srandom(in vec3 pos) {
return -1. + 2. * fract(sin(dot(pos.xyz, vec3(70.9898, 78.233, 32.4355))) * 43758.5453123);
}

float srandom(in vec4 pos) {
	float dot_product = dot(pos, vec4(12.9898,78.233,45.164,94.673));
	return -1. + 2. * fract(sin(dot_product) * 43758.5453);
}

vec2 srandom2(in vec2 st) {
	const vec2 k = vec2(.3183099, .3678794);
	st = st * k + k.yx;
	return -1. + 2. * fract(16. * k * fract(st.x * st.y * (st.x + st.y)));
}

vec3 srandom3(in vec3 p) {
	p = vec3( dot(p, vec3(127.1, 311.7, 74.7)),
			dot(p, vec3(269.5, 183.3, 246.1)),
			dot(p, vec3(113.5, 271.9, 124.6)));
	return -1. + 2. * fract(sin(p) * 43758.5453123);
}

vec2 srandom2(in vec2 p, const in float tileLength) {
	p = mod(p, vec2(tileLength));
	return srandom2(p);
}

vec3 srandom3(in vec3 p, const in float tileLength) {
	p = mod(p, vec3(tileLength));
	return srandom3(p);
}

#endif	

// --------------------------------

/*
contributors: Inigo Quiles
description: cubic polynomial https://iquilezles.org/articles/smoothsteps/
use: <float|vec2|vec3|vec4> cubic(<float|vec2|vec3|vec4> value[, <float> in, <float> out]);
examples:
	- https://raw.githubusercontent.com/patriciogonzalezvivo/lygia_examples/main/math_functions.frag
*/

#ifndef FNC_CUBIC
#define FNC_CUBIC 
float cubic(const in float v) { return v*v*(3.0-2.0*v); }
vec2  cubic(const in vec2 v)  { return v*v*(3.0-2.0*v); }
vec3  cubic(const in vec3 v)  { return v*v*(3.0-2.0*v); }
vec4  cubic(const in vec4 v)  { return v*v*(3.0-2.0*v); }

float cubic(const in float v, in float slope0, in float slope1) {
	float a = slope0 + slope1 - 2.;
	float b = -2. * slope0 - slope1 + 3.;
	float c = slope0;
	float v2 = v * v;
	float v3 = v * v2;
	return a * v3 + b * v2 + c * v;
}

vec2 cubic(const in vec2 v, in float slope0, in float slope1) {
	float a = slope0 + slope1 - 2.;
	float b = -2. * slope0 - slope1 + 3.;
	float c = slope0;
	vec2 v2 = v * v;
	vec2 v3 = v * v2;
	return a * v3 + b * v2 + c * v;
}

vec3 cubic(const in vec3 v, in float slope0, in float slope1) {
	float a = slope0 + slope1 - 2.;
	float b = -2. * slope0 - slope1 + 3.;
	float c = slope0;
	vec3 v2 = v * v;
	vec3 v3 = v * v2;
	return a * v3 + b * v2 + c * v;
}

vec4 cubic(const in vec4 v, in float slope0, in float slope1) {
	float a = slope0 + slope1 - 2.;
	float b = -2. * slope0 - slope1 + 3.;
	float c = slope0;
	vec4 v2 = v * v;
	vec4 v3 = v * v2;
	return a * v3 + b * v2 + c * v;
}
#endif

// --------------------------------

/*
contributors: Inigo Quiles
description: quintic polynomial https://iquilezles.org/articles/smoothsteps/
use: <float|vec2|vec3|vec4> quintic(<float|vec2|vec3|vec4> value);
examples:
	- https://raw.githubusercontent.com/patriciogonzalezvivo/lygia_examples/main/math_functions.frag
*/

#ifndef FNC_QUINTIC
#define FNC_QUINTIC 

float quintic(const in float v) { return v*v*v*(v*(v*6.0-15.0)+10.0); }
vec2  quintic(const in vec2 v)  { return v*v*v*(v*(v*6.0-15.0)+10.0); }
vec3  quintic(const in vec3 v)  { return v*v*v*(v*(v*6.0-15.0)+10.0); }
vec4  quintic(const in vec4 v)  { return v*v*v*(v*(v*6.0-15.0)+10.0); }

#endif

// --------------------------------

/*
contributors: Patricio Gonzalez Vivo
description: some useful math constants
license:
	- Copyright (c) 2021 Patricio Gonzalez Vivo under Prosperity License - https://prosperitylicense.com/versions/3.0.0
	- Copyright (c) 2021 Patricio Gonzalez Vivo under Patron License - https://lygia.xyz/license
*/
#ifndef EIGHTH_PI
#define EIGHTH_PI 0.39269908169
#endif
#ifndef QTR_PI
#define QTR_PI 0.78539816339
#endif
#ifndef HALF_PI
#define HALF_PI 1.5707963267948966192313216916398
#endif
#ifndef PI
#define PI 3.1415926535897932384626433832795
#endif
#ifndef TWO_PI
#define TWO_PI 6.2831853071795864769252867665590
#endif
#ifndef TAU
#define TAU 6.2831853071795864769252867665590
#endif
#ifndef INV_PI
#define INV_PI 0.31830988618379067153776752674503
#endif
#ifndef INV_SQRT_TAU
#define INV_SQRT_TAU 0.39894228040143267793994605993439  // 1.0/SQRT_TAU
#endif
#ifndef SQRT_HALF_PI
#define SQRT_HALF_PI 1.25331413732
#endif
#ifndef PHI
#define PHI 1.618033988749894848204586834
#endif
#ifndef EPSILON
#define EPSILON 0.0000001
#endif
#ifndef GOLDEN_RATIO
#define GOLDEN_RATIO 1.6180339887
#endif
#ifndef GOLDEN_RATIO_CONJUGATE 
#define GOLDEN_RATIO_CONJUGATE 0.61803398875
#endif
#ifndef GOLDEN_ANGLE // (3.-sqrt(5.0))*PI radians
#define GOLDEN_ANGLE 2.39996323
#endif
#ifndef DEG2RAD
#define DEG2RAD (PI / 180.0)
#endif
#ifndef RAD2DEG
#define RAD2DEG (180.0 / PI)
#endif

// --------------------------------

/*
contributors: Patricio Gonzalez Vivo
description: Gradient Noise
use: gnoise(<float> x)
license:
	- Copyright (c) 2021 Patricio Gonzalez Vivo under Prosperity License - https://prosperitylicense.com/versions/3.0.0
	- Copyright (c) 2021 Patricio Gonzalez Vivo under Patron License - https://lygia.xyz/license
*/

#ifndef FNC_GNOISE
#define FNC_GNOISE

float gnoise(float x) {
	float i = floor(x);  // integer
	float f = fract(x);  // fraction
	return mix(random(i), random(i + 1.0), smoothstep(0.,1.,f)); 
}

float gnoise(vec2 st) {
	vec2 i = floor(st);
	vec2 f = fract(st);
	float a = random(i);
	float b = random(i + vec2(1.0, 0.0));
	float c = random(i + vec2(0.0, 1.0));
	float d = random(i + vec2(1.0, 1.0));
	vec2 u = cubic(f);
	return mix( a, b, u.x) +
				(c - a)* u.y * (1.0 - u.x) +
				(d - b) * u.x * u.y;
}

float gnoise(vec3 p) {
	vec3 i = floor(p);
	vec3 f = fract(p);
	vec3 u = quintic(f);
	return -1.0 + 2.0 * mix( mix( mix( random(i + vec3(0.0,0.0,0.0)), 
										random(i + vec3(1.0,0.0,0.0)), u.x),
								mix( random(i + vec3(0.0,1.0,0.0)), 
										random(i + vec3(1.0,1.0,0.0)), u.x), u.y),
							mix( mix( random(i + vec3(0.0,0.0,1.0)), 
										random(i + vec3(1.0,0.0,1.0)), u.x),
								mix( random(i + vec3(0.0,1.0,1.0)), 
										random(i + vec3(1.0,1.0,1.0)), u.x), u.y), u.z );
}

float gnoise(vec3 p, float tileLength) {
	vec3 i = floor(p);
	vec3 f = fract(p);
					
	vec3 u = quintic(f);
				
	return mix( mix( mix( dot( srandom3(i + vec3(0.0,0.0,0.0), tileLength), f - vec3(0.0,0.0,0.0)), 
							dot( srandom3(i + vec3(1.0,0.0,0.0), tileLength), f - vec3(1.0,0.0,0.0)), u.x),
					mix( dot( srandom3(i + vec3(0.0,1.0,0.0), tileLength), f - vec3(0.0,1.0,0.0)), 
							dot( srandom3(i + vec3(1.0,1.0,0.0), tileLength), f - vec3(1.0,1.0,0.0)), u.x), u.y),
				mix( mix( dot( srandom3(i + vec3(0.0,0.0,1.0), tileLength), f - vec3(0.0,0.0,1.0)), 
							dot( srandom3(i + vec3(1.0,0.0,1.0), tileLength), f - vec3(1.0,0.0,1.0)), u.x),
					mix( dot( srandom3(i + vec3(0.0,1.0,1.0), tileLength), f - vec3(0.0,1.0,1.0)), 
							dot( srandom3(i + vec3(1.0,1.0,1.0), tileLength), f - vec3(1.0,1.0,1.0)), u.x), u.y), u.z );
}

vec3 gnoise3(vec3 x) {
	return vec3(gnoise(x+vec3(123.456, 0.567, 0.37)),
				gnoise(x+vec3(0.11, 47.43, 19.17)),
				gnoise(x) );
}

#endif

// --------------------------------

/*
contributors: Patricio Gonzalez Vivo
description: Fractal Brownian Motion
use: fbm(<vec2> pos)
options:
	FBM_NOISE_FNC(UV): noise function to use Default 'snoise(UV)' (simplex noise)
examples:
	- /shaders/generative_fbm.frag
license:
	- Copyright (c) 2021 Patricio Gonzalez Vivo under Prosperity License - https://prosperitylicense.com/versions/3.0.0
	- Copyright (c) 2021 Patricio Gonzalez Vivo under Patron License - https://lygia.xyz/license
*/
		
#ifndef FBM_NOISE_FNC
#define FBM_NOISE_FNC(UV) snoise(UV)
#endif

#ifndef FBM_NOISE2_FNC
#define FBM_NOISE2_FNC(UV) FBM_NOISE_FNC(UV)
#endif

#ifndef FBM_NOISE3_FNC
#define FBM_NOISE3_FNC(UV) FBM_NOISE_FNC(UV)
#endif

#ifndef FBM_NOISE_TILABLE_FNC
#define FBM_NOISE_TILABLE_FNC(UV, TILE) gnoise(UV, TILE)
#endif

#ifndef FBM_NOISE3_TILABLE_FNC
#define FBM_NOISE3_TILABLE_FNC(UV, TILE) FBM_NOISE_TILABLE_FNC(UV, TILE)
#endif

#ifndef FBM_NOISE_TYPE
#define FBM_NOISE_TYPE float
#endif


#ifndef FNC_FBM
#define FNC_FBM
FBM_NOISE_TYPE fbm(in vec2 st) {
	// Initial values
	FBM_NOISE_TYPE value = FBM_NOISE_TYPE(0.0);
	float a = u_amplitude;

	// Loop of octaves
	for (int i = 0; i < u_octaves; i++) {
		value += a * FBM_NOISE2_FNC(st);
		st *= u_lacunarity;
		a *= u_persistence;
	}
	return value;
}

FBM_NOISE_TYPE fbm(in vec3 pos) {
	// Initial values
	FBM_NOISE_TYPE value = FBM_NOISE_TYPE(0.0);
	float a = u_amplitude;

	// Loop of octaves
	for (int i = 0; i < u_octaves; i++) 
	{
		value += a * FBM_NOISE3_FNC(pos);
		pos *= u_lacunarity;
		a *= u_persistence;
	}
	return value;
}

FBM_NOISE_TYPE fbm(vec3 p, float tileLength) {

	float a = u_amplitude;
	FBM_NOISE_TYPE total = FBM_NOISE_TYPE(0.0);
	float normalization = 0.0;

	for (int i = 0; i < u_octaves; ++i) 
	{
		float noiseValue = FBM_NOISE3_TILABLE_FNC(p, tileLength * u_lacunarity * 0.5) * 0.5 + 0.5;
		total += noiseValue * a;
		normalization += u_amplitude;
		a *= u_persistence;
		p = p * u_lacunarity;
	}

	return total / normalization;
}
#endif

// --------------------------------
		
// summed simplex noise

float summedSimplexNoise(vec2 v)
{
	float value = 0.0;
	float a = u_amplitude;
	float f = u_frequency;

	for (int c = 0; c < u_octaves; c++)
	{
		value += snoise(v * f) * a;

		f *= u_lacunarity;
		a *= u_persistence;
	}

	return value;
}

float summedSimplexNoise(vec3 v)
{
	float value = 0.0;
	float a = u_amplitude;
	float f = u_frequency;

	for (int c = 0; c < u_octaves; c++)
	{
		value += snoise(v * f) * a;

		f *= u_lacunarity;
		a *= u_persistence;
	}

	return value;
}

// --------------------------------
		


// --------------------------------

void main()
{
		
	float grey = 0.0;
			
	switch (u_noiseType) 
	{
		case 1:
			grey = (1.0 + AndrewMeservyKenPerlinNoise(t_position.xyz * u_inputScale.xyz)) * 0.5;
			break;
		case 2:
			grey = (1.0 + snoise(t_position.xy * u_inputScale.xy)) * 0.5;
			break;
		case 3:
			grey = (1.0 + gnoise(t_position.xy * u_inputScale.xy)) * 0.5;
			break;
		case 4:
			grey = (1.0 + fbm(t_position.xy * u_inputScale.xy)) * 0.5;
			break;
		case 5:
			grey = (1.0 + summedSimplexNoise(t_position.xy * u_inputScale.xy)) * 0.5;
			break;
		default:
			grey = (1.0 + cnoise(t_position.xy * u_inputScale.xy)) * 0.5;
			break;
	}			

//	grey = biasValue(grey, u_bias);
	
	o_color = vec4(grey, grey, grey, 1.0);
}
