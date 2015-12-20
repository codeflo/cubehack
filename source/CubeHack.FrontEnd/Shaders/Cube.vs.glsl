// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

#version 430

layout(location = 0) uniform mat4 ProjectionMatrix;
layout(location = 4) uniform mat4 ModelViewMatrix;

layout(location = 12) uniform vec3 UniformOffset;

layout(location = 0) in vec3 VertexPosition;
layout(location = 1) in vec2 VertexTextureCoordinate;
layout(location = 2) in vec4 VertexColor;

out vec2 FragmentTextureCoordinate;
out vec4 FragmentColor;

void main() {
	gl_Position = ProjectionMatrix * ModelViewMatrix * vec4(VertexPosition + UniformOffset, 1.0);
	FragmentTextureCoordinate = VertexTextureCoordinate;
	FragmentColor = VertexColor;
}
