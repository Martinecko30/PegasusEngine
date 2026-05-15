using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using PegasusEngine.Objects.Components;
using PegasusEngine.Project;
using PegasusEngine.Project.Scenes;
using PegasusEngine.Renderer.Shaders;

namespace PegasusEditor.Renderer;

public class EditorRenderer : PegasusEngine.Renderer.Renderer
{
    private Shader gridShader;
    private Shader gizmoShader; // TODO: Add custom Gizmo Shader

    private int gridVao;
    
    private readonly ProjectManager projectManager;

    public EditorRenderer(ProjectManager projectManager) : base(projectManager)
    {}
    
    public override void Init()
    {
        base.Init();
        
        gridShader = new Shader(
            Path.Combine(EditorCfg.ResourcesPath, "Shaders/EditorGridShader.vert"),
            Path.Combine(EditorCfg.ResourcesPath, "Shaders/EditorGridShader.frag")
            );
        
        GL.Enable(EnableCap.Multisample);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);
        
        float[] gridVerts =
        {
            -1.0f, -1.0f,   1.0f, -1.0f,   -1.0f, 1.0f,
            -1.0f,  1.0f,   1.0f, -1.0f,    1.0f, 1.0f
        };

        gridVao = GL.GenVertexArray();
        GL.BindVertexArray(gridVao);
        
        int gridVbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, gridVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, gridVerts.Length * sizeof(float), gridVerts, BufferUsageHint.StaticDraw);
        
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.BindVertexArray(0);
    }

    protected override void OnPostMainRender(Scene scene, Camera camera)
    {
        if (camera == null)
            return;
        
        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix();
        
        GL.Disable(EnableCap.CullFace);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        GL.BindVertexArray(gridVao);
        RenderGrid(view, projection, camera.GameObject.Transform.Position);
        
        GL.Disable(EnableCap.Blend);
        
        GL.Clear(ClearBufferMask.DepthBufferBit);
        RenderGizmos(scene, view, projection);
        
        GL.BindVertexArray(0);
    }

    private void RenderGrid(Matrix4 view, Matrix4 projection, Vector3 cameraPosition)
    {
        gridShader.Use();
        gridShader.SetMatrix4("view", view);
        gridShader.SetMatrix4("projection", projection);
        gridShader.SetVector3("cameraPos", cameraPosition);
        
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    private void RenderGizmos(Scene scene, Matrix4 view, Matrix4 projection)
    {
        // TODO: Implement Gizmos
    }
    
    public void Dispose()
    {
        gridShader?.Dispose();
        gizmoShader?.Dispose();
        if (gridVao != 0) GL.DeleteVertexArray(gridVao);
        
        base.Dispose();
    }
}