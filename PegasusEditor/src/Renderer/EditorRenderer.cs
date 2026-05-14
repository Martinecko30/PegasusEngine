using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using PegasusEngine.Common;
using PegasusEngine.Debug;
using PegasusEngine.Objects;
using PegasusEngine.Objects.Components;
using PegasusEngine.Objects.Components.Meshes;
using PegasusEngine.Project;
using PegasusEngine.Project.Assets;
using PegasusEngine.Project.Scenes;
using PegasusEngine.Renderer;
using PegasusEngine.Renderer.Shaders;
using PegasusEngine.Renderer.Textures;

namespace PegasusEditor.Renderer;

public class EditorRenderer : IRenderer
{
    private int framebufferId;
    private int colorTextureId;
    private int depthRenderbufferId;
    private int renderWidth = 1280;
    private int renderHeight = 720;
    
    private Texture2D? finalFrameTexture;

    private Shader defaultShader;
    private Shader gridShader;
    private Shader gizmoShader; // TODO: Add custom Gizmo Shader

    private int sceneVao;
    private int gridVao;
    
    private readonly ProjectManager projectManager;
    private readonly GraphicsResourceManager resourceManager;
    private uint lastMeshBufferVersion = 0;

    public EditorRenderer(ProjectManager projectManager)
    {
        this.projectManager = projectManager;
        this.resourceManager = new GraphicsResourceManager();
    }
    
    public void Init()
    {
        defaultShader = new Shader(
            Path.Combine(EditorCfg.ResourcesPath, "Shaders/DefaultShader.vert"),
            Path.Combine(EditorCfg.ResourcesPath, "Shaders/DefaultShader.frag")
            );
        gridShader = new Shader(
            Path.Combine(EditorCfg.ResourcesPath, "Shaders/EditorGridShader.vert"),
            Path.Combine(EditorCfg.ResourcesPath, "Shaders/EditorGridShader.frag")
            );
        
        GL.Enable(EnableCap.Multisample);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);
        
        sceneVao = GL.GenVertexArray();
        
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
        
        RebuildRenderTarget(renderWidth, renderHeight);
    }

    public Texture2D Render(Scene scene, Camera camera)
    {
        UpdateResources();
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);
        GL.Viewport(0, 0, renderWidth, renderHeight);
        
        GL.Disable(EnableCap.ScissorTest);
        
        Clear(new Color4(0.15f, 0.15f, 0.15f, 1.0f));
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        Matrix4 view = camera.GetViewMatrix();
        Matrix4 projection = camera.GetProjectionMatrix();
        
        GL.BindVertexArray(sceneVao);
        // GL.Enable(EnableCap.CullFace);
        RenderScene(scene, camera, view, projection);
        
        GL.Disable(EnableCap.CullFace);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        GL.BindVertexArray(gridVao);
        RenderGrid(view, projection, camera.GameObject.Transform.Position);
        
        GL.Disable(EnableCap.Blend);
        
        GL.Clear(ClearBufferMask.DepthBufferBit);
        RenderGizmos(scene, view, projection);
        
        GL.BindVertexArray(0);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        return finalFrameTexture;
    }

    private void RenderScene(Scene scene, Camera camera, Matrix4 view, Matrix4 projection)
    {
        defaultShader.Use();
        defaultShader.SetMatrix4("view", view);
        defaultShader.SetMatrix4("projection", projection);
        
        // TODO: Add lighting
        defaultShader.SetVector3("viewPos", camera.GameObject.Transform.Position);
        defaultShader.SetVector3("lights[0].position", new Vector3(10.0f, 20.0f, 10.0f)); // Light high in the sky
        defaultShader.SetVector3("lights[0].color", new Vector3(1.0f, 1.0f, 1.0f));       // Pure white light
        defaultShader.SetInt("gamma", 0); // 0 = false
        // ==================
        
        var renderables = scene.Entities.Values
            .Where(e => e.HasComponent<MeshFilter>() &&
                                  e.HasComponent<MeshRenderer>() &&
                                  e.HasComponent<Transform>());
        foreach (var entity in renderables)
        {
            var filter = entity.GetComponent<MeshFilter>();
            if (filter!.MeshGuid == GUID.INVALID)
                continue;
            
            var metadata = projectManager.AssetManager.AssetPool.FindMetadata<MeshMetadata>(filter.MeshGuid);

            if (metadata == null)
                continue;

            var transform = entity.Transform;
            Matrix4 modelMatrix = Matrix4.CreateScale(transform.Scale) *
                                  Matrix4.CreateFromQuaternion(transform.Rotation) *
                                  Matrix4.CreateTranslation(transform.Position);
            
            defaultShader.SetMatrix4("model", modelMatrix);

            Matrix4 modelInvTrans = modelMatrix;
            modelInvTrans.Invert();
            modelInvTrans.Transpose();
            defaultShader.SetMatrix4("modelInverseTransposed", modelInvTrans);
            
            defaultShader.SetUInt("u_FirstTriIdx", metadata.FirstTriIdx);

            int vertexCount = (int)metadata.TriCount * 3;
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
        }  
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

    public void Resize(int width, int height)
    {
        if (width == renderWidth && height == renderHeight) return;
        if (width <= 0 || height <= 0) return;

        renderWidth = width;
        renderHeight = height;

        RebuildRenderTarget(width, height);
    }
    
    private void RebuildRenderTarget(int width, int height)
    {
        if (framebufferId != 0)
        {
            GL.DeleteFramebuffer(framebufferId);
            GL.DeleteTexture(colorTextureId);
            GL.DeleteRenderbuffer(depthRenderbufferId);
        }

        framebufferId = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);

        // Color Texture
        colorTextureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, colorTextureId);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTextureId, 0);

        // Depth/Stencil Buffer
        depthRenderbufferId = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRenderbufferId);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, depthRenderbufferId);

        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
        {
            throw new Exception("Core Renderer: Framebuffer is not complete!");
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        // Wrap it in the Engine's Texture2D class
        finalFrameTexture = new Texture2D(colorTextureId, "RenderTarget");
    }

    public void UpdateResources()
    {
        if (projectManager.AssetManager == null)
            return;
        
        // Check if the AssetManager have loaded new geometry since the last frame
        uint currentVersion = projectManager.AssetManager.AssetPool.GetUpdateVersion(AssetPool.AssetType.MeshBuffer);
        
        if (currentVersion > lastMeshBufferVersion)
        {
            resourceManager.UploadMegaBuffers(projectManager.AssetManager.AssetPool);
            lastMeshBufferVersion = currentVersion;
            Log.EngineInfo("Renderer: Mega-Buffers uploaded to GPU (Version {0})", currentVersion);
        }
    }

    public void Clear(Color4 color)
    {
        GL.ClearColor(color.R, color.G, color.B, color.A);
    }
    
    public void Dispose()
    {
        defaultShader?.Dispose();
        gridShader?.Dispose();
        gizmoShader?.Dispose();
        if (sceneVao != 0) GL.DeleteVertexArray(sceneVao);
        if (gridVao != 0) GL.DeleteVertexArray(gridVao);
        if (framebufferId != 0) GL.DeleteFramebuffer(framebufferId);
        resourceManager?.Dispose();
    }
}