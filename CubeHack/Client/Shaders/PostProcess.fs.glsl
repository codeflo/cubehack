// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

uniform sampler2D depth_buffer;

const float edgeWidth = 0.4;

float lookUp(float x, float y) {
	return texture2D(depth_buffer, gl_TexCoord[0].st + vec2(x / 1280.0, y / 720)).x; 
}

float isEdge(float x, float y) {
	float d = lookUp(x, y);
	float d1 = lookUp(x + edgeWidth, y + edgeWidth);
	float d2 = lookUp(x - edgeWidth, y + edgeWidth);
	float d3 = lookUp(x - edgeWidth, y - edgeWidth);
	float d4 = lookUp(x + edgeWidth, y - edgeWidth);
	float ddiff = max(abs(d - d1), max(abs(d - d2), max(abs(d - d3), abs(d - d4))));
	float v = smoothstep(0, 0.002, ddiff);
	return v;
}

void main() {
	float v = isEdge(0, 0);
	gl_FragColor = vec4(0, 0, 0, v);
}
