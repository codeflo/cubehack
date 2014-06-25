// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

void main() {
	gl_TexCoord[0] = gl_MultiTexCoord0;
	gl_Position = gl_Vertex;
}
