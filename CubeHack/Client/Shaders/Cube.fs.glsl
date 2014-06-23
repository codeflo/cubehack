// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

uniform sampler2D cubeTexture;

varying vec4 highlight;

void main() {
	gl_FragColor = gl_Color * texture2D(cubeTexture, gl_TexCoord[0].st) + highlight;
}
