using OpenTK.Graphics.OpenGL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PegasusEngine.Project.Assets;

namespace PegasusEngine.Renderer;

/*
#version 450 core

// Binding 0: The Triangle Mega-Buffer
struct Triangle {
    vec4 v0;
    vec4 v1;
    vec4 v2;
};

layout(std430, binding = 0) readonly buffer MeshBuffer {
    Triangle Triangles[];
};

// Binding 1: The BVH Node Mega-Buffer
struct BVHNode {
    vec3 minBounds;
    uint leftChildOrFirstTri;
    vec3 maxBounds;
    uint triCount;
};

layout(std430, binding = 1) readonly buffer NodeBuffer {
    BVHNode Nodes[];
};

// Binding 2: The Index Mega-Buffer
layout(std430, binding = 2) readonly buffer IndexBuffer {
    uint Indices[];
};
*/

public class GraphicsResourceManager : IDisposable
{
    public int MeshSsboId { get; private set; }
    public int NodeSsboId { get; private set; }
    public int IndexSsboId { get; private set; }
    
    public void UploadMegaBuffers(AssetPool pool)
    {
       if (MeshSsboId == 0) MeshSsboId = GL.GenBuffer();
       if (NodeSsboId == 0) NodeSsboId = GL.GenBuffer();
       if (IndexSsboId == 0) IndexSsboId = GL.GenBuffer();
       UploadListToSsbo(pool.MeshBuffer, MeshSsboId, 0);
       UploadListToSsbo(pool.NodeBuffer, NodeSsboId, 1);
       UploadListToSsbo(pool.IndexBuffer, IndexSsboId, 2);
    }
    /// <summary>
    /// A generic helper that extracts the raw memory from a C# List and blasts it to OpenGL.
    /// </summary>
    private void UploadListToSsbo<T>(List<T> list, int bufferId, int bindingPoint) where T : unmanaged
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, bufferId);

        if (list.Count > 0)
        {
            Span<T> span = CollectionsMarshal.AsSpan(list);
            
            int sizeInBytes = span.Length * Unsafe.SizeOf<T>();

            GL.BufferData(
                BufferTarget.ShaderStorageBuffer, 
                sizeInBytes, 
                ref span.GetPinnableReference(), 
                BufferUsageHint.StaticDraw
            );
        }
        else
        {
            GL.BufferData(BufferTarget.ShaderStorageBuffer, 1, IntPtr.Zero, BufferUsageHint.StaticDraw);
        }

        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingPoint, bufferId);
        
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
    }

    public void Dispose()
    {
        if (MeshSsboId != 0) GL.DeleteBuffer(MeshSsboId);
        if (NodeSsboId != 0) GL.DeleteBuffer(NodeSsboId);
        if (IndexSsboId != 0) GL.DeleteBuffer(IndexSsboId);
    }
}