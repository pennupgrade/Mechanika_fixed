#define PI 3.14159265
#define TAU 6.283185
#define IPI .3183099
#define eps float2(0.00001, 0.)
#define V float2(1., 0.)
//#define e 2.718282﻿

float saturate(float f)
{
	return clamp(f, 0., 1.);
}

float fract(float v)
{
	//saturate
	return fmod(v, 1.);
}

float3 fract(float3 v)
{
	return fmod(v, 1.);
}

float hash11(float p)
{
	p = fract(p * .1031);
	p *= p + 33.33;
	p *= p + p;
	return fract(p);
}

float smoothhash11(float p)
{
	return (hash11(p + .01) + hash11(p - .01)) * .5;
}

float hash21(float2 p)
{
	float3 p3 = fract(float3(p.xyx) * .1031);
	p3 += dot(p3, p3.yzx + 33.33);
	return fract((p3.x + p3.y) * p3.z);
}

//MATH

float hlength(float2 v, float k)
{
	return pow(pow(v.x, k) + pow(v.y, k), 1. / k);
}

float stepped(float v, float s) //v ~ [0, inf)
{
	return v - fmod(v, s);
}

float2 stepped(float2 v, float2 s) //v ~ [0, inf)
{
	return v - fmod(v, s);
}

float amod(float v, float m)
{
	float s = fmod(abs(v), m);
	return s + step(v, 0) * (m - 2. * s);
}

float2 amod(float2 v, float2 m)
{
	float2 modded = fmod(abs(v), m);
	return modded + (m - 2. * modded) * step(v, 0.);
}

float snap(float v, float grid)
{
	return v - amod(v, grid);
}
float2 snap(float2 v, float2 grid)
{
	return v - amod(v, grid);
}

float2 fold(float2 p, float2 normal) //negative to positive only
{
	return p - 2. * min(0., dot(p, normal)) * normal;
}

float crossLength(float2 v1, float2 v2)
{
	return length(cross(float3(v1.x,v1.y, 0.), float3(v2.x, v2.y, 0.)));
}

float2 perp(float2 v)
{
	//return cross(float3(v.x, v.y, 0.), float3(0., 0., -1.)).xy;
	return mul(float2x2(0., -1, 1, 0.), v);
}

float2 rot(float2 v, float o)
{
	//return cos(o) * v + sin(o) * perp(v);
	return mul(float2x2(cos(o), -sin(o), sin(o), cos(o)), v);
}

float3 rot(float3 a, float3 v, float o)
{

	float along = dot(v, a);
	float3 proj = v - along * a;

	return proj * cos(o) + cross(a, proj) * sin(o) + along * a;

}

float linearstep(float start, float end, float v)
{
	return clamp((v - start) / (end - start), 0., 1.);
}

float min(float2 v)
{
	return min(v.x, v.y);
}

float max(float2 v)
{
	return max(v.x, v.y);
}

float add(float2 v)
{
	return v.x + v.y;
}

float sub(float2 v)
{
	return v.x - v.y;
}

float mul(float2 v)
{
	return v.x * v.y;
}

float div(float2 v)
{
	return v.x / v.y;
}

float fsin(float t) { return sin(t)*.5+.5;}

float2 stepv(float2 v, float2 edge)
{
	return float2(step(v.x, edge.x), step(v.y, edge.y));
}

float btwn(float a, float b, float x)
{
	return step(a, x) * step(x, b);
}

float atanP(float2 v) //atanPositive -> [0, 2PI)
{
	return fmod(atan2(v.y, v.x) + PI * 2., PI * 2.);
}

float2 toCartesian(float2 polar)
{
	return polar.x * float2(cos(polar.y), sin(polar.y));
}

float2 toCartesian(float r, float theta)
{
	return toCartesian(float2(r, theta));
}

float2 toPolar(float2 cartesian)
{
	return float2(length(cartesian), atanP(cartesian));
}

float2 toPolar(float x, float y)
{
	return toPolar(float2(x, y));
}

float sb(float dist)
{
	return step(dist, 0.);
}

//Intersections

float raySphereGeometric(float3 rd, float4 sphere)
{

	float3 toSphere = sphere.xyz;
	float3 R = rd * dot(toSphere, rd);

	float b = sqrt(dot(toSphere, toSphere) - dot(R, R));
	float inDist = sqrt(sphere.a * sphere.a - b * b);

	float hit = step(b, sphere.a);

	return hit * (length(R) - inDist) - (1. - hit);

}

float raySphere(float3 rd, float4 sphere)
{

	//float a = 1.0;
	float b = -dot(rd, sphere.xyz) * 2.;
	float c = dot(sphere.xyz, sphere.xyz) - sphere.a * sphere.a;

	float discriminant = b * b - 4. * c;
	float left = -b;

	if (discriminant < 0) return -1.;

	return (left - sqrt(discriminant)) * .5;

}

//SDFs
float sdCircle(float2 p, float radius)
{
	return length(p) - radius;
}

float sdHyperCircle(float2 p, float r, float k)
{
	return pow(pow(p.x, k) + pow(p.y, k), 1./k) - r;
}

float sdLine(float2 p, float4 points, float thickness)
{

	float2 lp = p - (points.xy + points.zw) * .5;
	float2 dir = normalize(points.zw - points.xy);
	lp = float2(dot(lp, dir), length(cross(float3(lp.x, lp.y, 0.), float3(dir.x, dir.y, 0.))));
	lp = abs(lp);
	lp.x = max(0., lp.x - length(points.zw - points.xy) * .5);
	return length(lp) - thickness;

}

float sdLine(float2 p, float2 start, float2 end, float thickness)
{
	return sdLine(p, float4(start.x, start.y, end.x, end.y), thickness);
}

float sdLine(float2 p, float2 dir, float thickness)
{
	return abs(dot(p, perp(dir))) - thickness;
}

float sdPartition(float2 p, float2 lineDir) //2D plane
{
	return dot(p, perp(lineDir));
}

float sdBox(float2 p, float2 fo, float2 dim)
{
	p = abs(float2(dot(p, fo), dot(p, perp(fo)))) - dim*.5;
	float sd = sqrt(p.y * p.y * step(0., p.y) + p.x * p.x * step(0., p.x));
	return sd + step(sd, 0.) * max(p);
}

float sdBox(float2 p, float2 dim)
{
	p = abs(p) - dim * .5;
	float sd = sqrt(p.y * p.y * step(0., p.y) + p.x * p.x * step(0., p.x));
	return sd + step(sd, 0.) * max(p);
}

float sdRightTri(float2 p, float2 corner, float2 dim) //x ~ [0, inf)
{

	p = p - corner;

	float2 top = float2(0., dim.y);
	float2 right = float2(dim.x, 0.);

	return max(sdPartition(p - top, normalize(right - top)), sdPartition(p, -normalize(right)));

}

float sdTri(float2 p, float2 p1, float2 p2, float2 p3) //split into two right tris?
{
	//return max(max(sdPartition(p, -normalize(p2 - p1)), sdPartition(p, -normalize(p3 - p1))), sdPartition(p, normalize(p3 - p2)));

	float2 p4 = p2 + normalize(-perp(p2 - p1)) * crossLength(p2 - p1, p3 - p1) / length(p3-p2);
	p -= p4;
	return min(
		sdRightTri(float2(dot(p, normalize(p1 - p4)), dot(p, normalize(p2 - p4))), float2(0.,0.), float2(length(p1 - p4), 1./length(p2 - p4))),
		sdRightTri(float2(dot(p, normalize(p3 - p4)), dot(p, normalize(p2 - p4))), float2(0.,0.), float2(length(p3 - p4), 1./length(p2 - p4)))
		);
}

float sdPolarLine(float2 radii, float thetaThickness, float2 p) //atan2(p) ~ [-thetaThickness*.5, thetaThickness*.5]
{
	float r = length(p);

	float2 space = float2(sin(thetaThickness * .5), cos(thetaThickness * .5));

	float2 start = radii.x * space * float2(-1., 1.);
	float2 end = radii.y * space;

	return sdPartition(p - start, normalize(end - start));

	float m = (end.y - start.y) / (end.x - start.x);

	float x = (start.x * m - start.y) / (m - p.y / p.x);
	float y = x * p.y / p.x;

	return r - length(float2(x, y)); //intersection on the other side of the polar ray
}

float sdSpikeBall(float2 r, float partitions, float2 p, float variation, float seed, float time) //radii [lowRadius, highRadius]
{
	float randSeed = hash11(seed);
	p = rot(p, -(time * (randSeed * 4. - 2.) + randSeed * 100.) * 2.);
	float2 polar = float2(length(p), atan2(p.y, p.x) + 2. * PI);
	float ltheta = fmod(polar.y, 2. * PI / partitions) - PI / partitions;

	float cid = polar.y - ltheta;
	float sid = cid - PI / partitions;
	float eid = cid + PI / partitions;

	float sRa = r.x + (r.y - r.x) * step(0., fmod(sid * .5 * partitions / PI, 1.99999) - .5);
	float eRa = r.x + (r.y - r.x) * step(0., fmod(eid * .5 * partitions / PI, 1.99999) - .5);

	float2 rand = float2(hash11(sid), hash11(eid));
	float2 variationOut = variation * (float2(sin(time * 18. + 600. * (rand.x + seed)), sin(time * 18. + 600. * (rand.y + seed))) * .5 + .5);

	float2 radii = float2(sRa, eRa) + variationOut;// float2(variation * hash11(sid), variation * hash11(eid));
	return sdPolarLine(radii, 2. * PI / partitions, polar.x * float2(sin(ltheta), cos(ltheta)));
}

float sdAngledLines(float2 p, float startTheta, float thetaWidth)
{
	float2 polar = toPolar(p);
	float rTheta = polar.y - startTheta;

	rTheta = PI-abs(rTheta-PI);
	rTheta = abs(rTheta);
	rTheta -= thetaWidth*.5;

	return rTheta*polar.x;
}

//Higher level stuff (insert here)
float noisySin(float t, float seed)
{
	return .6 * sin(t - 10.) + .25 * sin(t * .5 + hash11(seed) * 100.) + .15 * sin(t * .25 + hash11(seed * 2.) * 300.);
}

float randTrig(float t, float seed, float2 speedAndRand) //speedAndRand = [C, variationOffset]
{
	float rand = hash11(seed * .02);
	return sin((speedAndRand.x + speedAndRand.y * (rand * 2. - 1.)) * 0. * t + 600. * rand);
}

float randLinear(float t, float seed, float2 speedAndRand)
{
	float rand = hash11(seed) * 2. - 1.;
	float sign = step(.5, hash11(seed * 2.)) * 2. - 1.;
	return sign * (speedAndRand.x + rand * speedAndRand.y) * t;
}

float2 pixelate(float2 uv, float multiplier)
{
	return snap(uv, multiplier) + eps.xxx; //eps for de-alignment cuz length(0) is so bad
}
float2 pixelate(float2 uv)
{
	return pixelate(uv, .04);
}