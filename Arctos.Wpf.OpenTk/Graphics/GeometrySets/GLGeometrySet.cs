using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Arctos.Wpf.OpenTk.Util;
using OpenTK.Compute.OpenCL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Arctos.Wpf.OpenTk.Graphics
{
    public class GLGeometrySet<T>
        where T : IGLGeometry
    {
        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Fields

        protected int VertexCount;
        public PrimitiveType PrimitiveType;
        public List<T> Geometries = new();

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties


        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Constructor

        public GLGeometrySet()
        {
        }

        #endregion Constructor

        public void Add(T geometry)
        {
            Geometries.Add(geometry);
        }

        public virtual void Render(int program, double width, double height)
        {
            var position = GL.GetAttribLocation(program, "iPosition");
            var color = GL.GetAttribLocation(program, "iColor");
            var viewport = GL.GetUniformLocation(program, "uViewport");



            GLVertex[] vertexData = GetVertexData();

            var vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * Unsafe.SizeOf<GLVertex>(), vertexData, BufferUsageHint.StaticDraw);

            var vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);


            GL.EnableVertexAttribArray(position);
            GL.VertexAttribPointer(position, 3, VertexAttribPointerType.Float, false, Unsafe.SizeOf<GLVertex>(), 0);

            GL.EnableVertexAttribArray(color);
            GL.VertexAttribPointer(color, 4, VertexAttribPointerType.Float, false, Unsafe.SizeOf<GLVertex>(), Unsafe.SizeOf<Vector3>());

            GL.Uniform2(viewport, new Vector2((float)width, (float)height));


            Draw();
        }

        protected virtual void Draw()
        {
            GL.DrawArrays(PrimitiveType, 0, Geometries.Count * VertexCount);
        }

        private GLVertex[] GetVertexData()
        {
            var vertexData = new List<GLVertex>();

            foreach (var geometry in Geometries)
            {
                vertexData.AddRange(geometry.GetVertexData());
            }

            return vertexData.ToArray();
        }
    }
}
