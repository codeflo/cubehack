// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

uniform sampler2D depth_buffer;

const float edgeWidth = 10;

float lookUp(float x, float y) {
	return texture2D(depth_buffer, gl_TexCoord[0].st + vec2(x / 1280.0, y / 720)).x; 
}

const float near = 0.05f;
const float far = 1000.0f;

float getZ(float d) {
	float zn = 2 * d - 1;
	return 2.0 * near * far / (far + near - zn * (far - near));
}

float isEdge(float x, float y) {
	float d = lookUp(x, y);
	float z = getZ(d);

	float e = edgeWidth / z;

	float d1 = lookUp(x + e, y + e);
	float d2 = lookUp(x - e, y + e);
	float d3 = lookUp(x - e, y - e);
	float d4 = lookUp(x + e, y - e);
	float d5 = lookUp(x + e, y);
	float d6 = lookUp(x - e, y);
	float d7 = lookUp(x, y + e);
	float d8 = lookUp(x, y - e);

	float dmax = max(max(max(d1, d2), max(d3, d4)), max(max(d5, d6), max(d7, d8)));

	float zmax = getZ(dmax);

	float v = smoothstep(0, 0.075,  (zmax - z) / z - 0.025);
	return v;
}

void main() {
	float v = isEdge(0, 0);
	gl_FragColor = vec4(0, 0, 0, v * 0.75);
}
