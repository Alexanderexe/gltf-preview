﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;

using SDPixelFormat = System.Drawing.Imaging.PixelFormat;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace prvw
{
    class Window : GameWindow
    {
        // Because we're adding a texture, we modify the vertex array to include texture coordinates.
        // Texture coordinates range from 0.0 to 1.0, with (0.0, 0.0) representing the bottom left, and (1.0, 1.0) representing the top right.
        // The new layout is three floats to create a vertex, then two floats to create the coordinates.


        private readonly float[] _vertices;

        private readonly uint[] _indices;

        private int _elementBufferObject;

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        // For documentation on this, check Texture.cs.
        private Texture _texture;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            
            var reader = new GLTFReader.Renderer();
            reader.Render("Resources/BoxTextured/BoxTextured.gltf");
            _vertices = reader.Vertices ;
            _indices = reader.Indices;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // The shaders have been modified to include the texture coordinates, check them out after finishing the OnLoad function.
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            // Because there's now 5 floats between the start of the first vertex and the start of the second,
            // we modify the stride from 3 * sizeof(float) to 5 * sizeof(float).
            // This will now pass the new vertex array to the buffer.
            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            // Next, we also setup texture coordinates. It works in much the same way.
            // We add an offset of 3, since the texture coordinates comes after the position data.
            // We also change the amount of data to 2 because there's only 2 floats for texture coordinates.
            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            _texture = Texture.LoadFromFile("Resources/BoxTextured/CesiumLogoFlat.png");
            _texture.Use(TextureUnit.Texture0);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindVertexArray(_vertexArrayObject);

            _texture.Use(TextureUnit.Texture0);
            _shader.Use();

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            using (var bmp = new Bitmap(800, 600, SDPixelFormat.Format32bppArgb))
            {
                var mem = bmp.LockBits(new Rectangle(0, 0,800, 600), ImageLockMode.WriteOnly, SDPixelFormat.Format32bppArgb);
                GL.PixelStore(PixelStoreParameter.PackRowLength, mem.Stride / 4);
                GL.ReadPixels(0, 0, 800, 600, (OpenTK.Graphics.OpenGL4.PixelFormat)PixelFormat.Bgra, PixelType.UnsignedByte, mem.Scan0);
                bmp.UnlockBits(mem);
                bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
                bmp.Save(@"test.png", ImageFormat.Png);
            }
            Close();
            SwapBuffers();
        }
    }

}
