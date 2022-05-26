using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

namespace ray_tracing
{
    internal class Window : GameWindow
    {
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _elementBufferObject;

        private int _basicProgramId;
        private int _basicVertexShader;
        private int _basicFragmentShader;

        private readonly Vector3[] _vertices = new Vector3[] {
            new Vector3(1f, 1f, 0f),
            new Vector3( 1f, -1f, 0f),
            new Vector3( -1f, -1f, 0f),
            new Vector3(-1f, 1f, 0f)
        };

        private readonly uint[] _indices ={
            0, 1, 3, 
            1, 2, 3 
        };

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings) {}

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.GenBuffers(1, out _vertexBufferObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(_vertices.Length * Vector3.SizeInBytes), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            InitShaders();
            GL.UseProgram(_basicProgramId);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(_basicProgramId);

            GL.BindVertexArray(_vertexArrayObject);

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
            GL.DeleteBuffer(_elementBufferObject);

            GL.DeleteProgram(_basicVertexShader);
            GL.DeleteProgram(_basicFragmentShader);

            base.OnUnload();
        }

        private void LoadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (System.IO.StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        private void InitShaders()
        {
            _basicProgramId = GL.CreateProgram();

            LoadShader("..\\..\\..\\Shaders\\shader.vert", ShaderType.VertexShader, _basicProgramId,
                out _basicVertexShader);
            LoadShader("..\\..\\..\\Shaders\\shader.frag", ShaderType.FragmentShader, _basicProgramId,
                out _basicFragmentShader);

            GL.LinkProgram(_basicProgramId);

            int status = 0;
            GL.GetProgram(_basicProgramId, GetProgramParameterName.LinkStatus, out status);
            var programInfoLog = GL.GetProgramInfoLog(_basicProgramId);
            Console.WriteLine(programInfoLog);
        }
    }
}
