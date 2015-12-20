// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

#version 430

layout(location = 8) uniform sampler2D CubeTexture;

in vec2 FragmentTextureCoordinate;
in vec4 FragmentColor;
in vec4 FragmentHighlight;

void main() {
	gl_FragColor = vec4(FragmentColor.xyz, 1.0) * texture2D(CubeTexture, FragmentTextureCoordinate.st) + vec4(FragmentColor.w, FragmentColor.w, FragmentColor.w, 0.0);
}
