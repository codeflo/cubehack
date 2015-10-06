// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

uniform sampler2D cubeTexture;

varying vec4 highlight;

void main() {
	gl_FragColor = gl_Color * texture2D(cubeTexture, gl_TexCoord[0].st) + highlight;
}
