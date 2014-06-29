// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

#include "stdafx.h"

using namespace System;

// Force use of the GPU on NVIDIA Optimus laptops.
extern "C" {
	_declspec(dllexport) uint32_t NvOptimusEnablement = 0x00000001;
}

int __stdcall WinMain(
	void *hInstance,
	void *hPrevInstance,
	const char *lpCmdLine,
	int nCmdShow)
{
	Console::WriteLine(L"Hello World");
	CubeHack::Client::Program::Main();
	return 0;
}
