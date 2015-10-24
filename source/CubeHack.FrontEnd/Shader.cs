// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using OpenTK.Graphics.OpenGL;
using System;
using System.Text;

namespace CubeHack.FrontEnd
{
    internal class Shader
    {
        private int _id;

        public Shader(int id)
        {
            _id = id;
        }

        public int Id
        {
            get
            {
                return _id;
            }
        }

        public static Shader Load(string name)
        {
            int vertexShaderId = LoadProgram(name + ".vs.glsl", ShaderType.VertexShader);
            int fragmentShaderId = LoadProgram(name + ".fs.glsl", ShaderType.FragmentShader);

            int id = GL.CreateProgram();
            GL.AttachShader(id, vertexShaderId);
            GL.AttachShader(id, fragmentShaderId);
            GL.LinkProgram(id);

            int status;
            GL.GetProgram(id, GetProgramParameterName.LinkStatus, out status);
            if (status == 0)
            {
                throw new Exception("Error linking shader: " + GL.GetProgramInfoLog(id));
            }

            return new Shader(id);
        }

        private static int LoadProgram(string path, ShaderType type)
        {
            var source = LoadResource(path);
            var id = GL.CreateShader(type);
            GL.ShaderSource(id, source);

            GL.CompileShader(id);

            int status;
            GL.GetShader(id, ShaderParameter.CompileStatus, out status);
            if (status == 0)
            {
                throw new Exception("Error compiling shader: " + GL.GetShaderInfoLog(id));
            }

            return id;
        }

        private static string LoadResource(string path)
        {
            using (var stream = typeof(Shader).Assembly.GetManifestResourceStream(path))
            {
                using (var reader = new System.IO.StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
