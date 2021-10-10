// Sphere
// s: radius
float sdSphere(float3 p, float s)
{
	return length(p) - s;
}

// Box
// b: size of box in x/y/z
float sdBox(float3 p, float3 b)
{
	float3 d = abs(p) - b;
	return min(max(d.x, max(d.y, d.z)), 0.0) +
		length(max(d, 0.0));
}

// Plane
float sdPlane(float3 p, float3 n)
{
	// n must be normalized
	return dot(p, n);
}

// Rounded Box
float sdRoundBox(float3 p, float3 b, float r)
{
	float3 q = abs(p) - b;
	return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0) - r;
}

// Capsule/Line
float sdCapsule(float3 p, float3 a, float3 b, float r)
{

	float3 pa = p - a, ba = b - a;
	float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
	return length(pa - ba * h) - r;
}

// Vertical Capsule
float sdVerticalCapsule(float3 p, float h, float r)
{
	p.y -= clamp(p.y, 0.0, h);
	return length(p) - r;
}

// Link
float sdLink(float3 p, float le, float r1, float r2)
{
	float3 q = float3(p.x, max(abs(p.y) - le, 0.0), p.z);
	return length(float2(length(q.xy) - r1, q.z)) - r2;
}

float sdMengerSpone(float3 _position)
{
	float f = sdBox(_position, float3(225.0, 225.0, 225.0));

	float s = 0.5;
	for (int m = 0; m < 3; m++)
	{
		float3 r = abs(1.0 - 3.0 * abs((_position * s) - 2.0 * floor((_position * s) / 2.0) - 1.0));

		s *= 3.0;

		float da = max(r.x, r.y);
		float db = max(r.y, r.z);
		float dc = max(r.z, r.x);
		float c = (min(da, min(db, dc)) - 1.0) / s;

		f = max(f, c);
	}

	return f;
}

// BOOLEAN OPERATORS //

// Union
float opU(float d1, float d2)
{
	return min(d1, d2);
}

// Subtraction
float opS(float d1, float d2)
{
	return max(-d1, d2);
}

// Intersection
float opI(float d1, float d2)
{
	return max(d1, d2);
}

// SMOOTH BOOLEAN OPERATORS

float4 opUS(float4 d1, float4 d2, float k)
{
	float h = clamp(0.5 + 0.5 * (d2.w - d1.w) / k, 0.0, 1.0);
	float3 color = lerp(d2.rgb, d1.rgb, h);
	float dist = lerp(d2.w, d1.w, h) - k * h * (1.0 - h);
	return float4(color, dist);
}

float opSS(float d1, float d2, float k)
{
	float h = clamp(0.5 - 0.5 * (d2 + d1) / k, 0.0, 1.0);
	return lerp(d2, -d1, h) + k * h * (1.0 - h);
}

float opIS(float d1, float d2, float k)
{
	float h = clamp(0.5 - 0.5 * (d2 - d1) / k, 0.0, 1.0);
	return lerp(d2, d1, h) + k * h * (1.0 - h);
}


// REPETITIONS 
// Finite space
// c : space between, l : amount of objects
float3 opRepLim(in float3 p, in float c, in float3 l)
{	
	float3 q = p - c * clamp(round(p / c), -l, l);
	return q;
}


float opRep(in float3 p, in float3 c)
{
	float3 q = fmod(p + 0.5 * c, c) - 0.5 * c;
	return q;
}

// Mod Position Axis (Infinite space)
float pMod1(inout float p, float size)
{
	//vec4( S( .5- abs( mod(l-.5, 2.) - 1.)
	// 
	float pfirst = float(p);

	float halfsize = size * 0.5f;
	float c =  floor((p + halfsize) / size);

	//p = fmod(p + 0.5 * c, c) - 0.5 * c;
	p = fmod(p + halfsize, size) - halfsize;
	p = fmod(-p + halfsize, size) - halfsize;

	//smoothstep(pfirst, p, halfsize);
	return c;
}

// TRANSFORMS
// Rotation
float2x2 GetRotMat(float angle)
{
	float s = sin(angle);
	float c = cos(angle);
	return float2x2(c, -s, s, c);
}

float3 Rotate(float3 p, float3 anglesPerAxis)
{ 
	p.yz = mul(GetRotMat(-anglesPerAxis.x), p.yz);
	p.xz = mul(GetRotMat(anglesPerAxis.y), p.xz);
	p.xy = mul(GetRotMat(-anglesPerAxis.z), p.xy);
	return p;
}

float opDisplace(float shape, in float3 p, float d)
{
	float d1 = shape;
	p /= 10;
	float d2 = lerp(0, 10 *(sin(d+d + p.x) * sin(d * 3.0f + p.y) * sin(d * 5.0f + p.z)), step(.001, d));
	return d1 + d2;
}

float opDisplacement(float shape, in float3 p, float d)
{
	float d1 = shape;
	float d2 = lerp(0, sin(d + p.x) * sin(d + p.y) * sin(d + p.z), step(.001, d));
	return d1 + d2;
}

float3 applyFog(in float3  rgb,      // original color of the pixel
	in float distance, // camera to point distance
	in float3  rayDir,   // camera to point vector
	in float3  sunDir)  // sun light direction
{
	float fogAmount = 1.0 - exp(-distance * .2);
	float sunAmount = max(dot(rayDir, sunDir), 0.0);
	float3  fogColor = lerp(float3(0.5, 0.6, 0.7), // bluish
		float3(1.0, 0.9, 0.7), // yellowish
		pow(sunAmount, 8.0));
	return lerp(rgb, fogColor, fogAmount);
}

float3 opTwist(in float3 p)
{
	const float k = 0.2; // or some other amount
	float c = cos(k * p.y);
	float s = sin(k * p.y);
	float2x2  m = float2x2(c, -s, s, c);
	float3  q = float3(mul(m, p.xz), p.y);
	return q;
}

float rep(float3 p, float mod)
{
	float halfsize = mod * 0.5f;
	float c = p;
	float t = floor((p * halfsize) / mod);
//	float t = 
	c = fmod(p + halfsize, mod) - halfsize;
//	c = fmod(c - halfsize, mod) - halfsize;
	c = fmod(-c + halfsize, mod) - halfsize;
	return c;
}

float rep2(float3 p, float mod)
{
	float halfsize = mod * 0.5f;
	float c = p;
	float t = abs(floor((p * halfsize) / mod));
	//	float t = 
	c = fmod(p + halfsize, mod) - halfsize;
	//	c = fmod(c - halfsize, mod) - halfsize;
	c = fmod(-c + halfsize, mod) - halfsize;
	return abs(c);
}