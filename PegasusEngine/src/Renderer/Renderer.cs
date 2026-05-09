using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using PegasusEngine.Common;
using PegasusEngine.Debug;
using PegasusEngine.Objects.Components;
using PegasusEngine.Objects.Components.Meshes;
using PegasusEngine.old.Modules.Rendering.Shaders;
using PegasusEngine.Project.Assets;
using PegasusEngine.Project.Scenes;
using PegasusEngine.Renderer.Textures;

namespace PegasusEngine.Renderer;

public class Renderer : IRenderer
{
    // Internal Engine Render Target
    private int framebufferId;
    private int colorTextureId;
    private int depthRenderbufferId;
    private int renderWidth = 1280;
    private int renderHeight = 720;
    
    // The final composed image of the game frame
    private Texture2D? _finalFrameTexture;

    // Shaders & Shadows
    private Shader defaultShader;
    private Shader depthShader;
    
    private const int SHADOW_WIDTH = 4096;
    private const int SHADOW_HEIGHT = 4096;
    private int shadowMapTexture;
    private int shadowMapFbo;
    private Matrix4 lightSpaceMatrix;
    
    private int dummyVao;
    
    private AssetManager assetManager;
    private GraphicsResourceManager resourceManager;
    private uint lastMeshBufferVersion = 0;

    public Renderer(AssetManager assetManager)
    {
        this.assetManager = assetManager;
    }
    
    public void Init()
    {
        this.resourceManager = new GraphicsResourceManager();
        
        this.defaultShader = new Shader(
            "res/Shaders/DefaultShader.vert",
            "res/Shaders/DefaultShader.frag"
            );
        
        GL.Enable(EnableCap.Multisample);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Lequal);
        // GL.Enable(EnableCap.FramebufferSrgb);
        GL.ClearColor(0.01f, 0.01f, 0.01f, 1.0f);
        
        dummyVao = GL.GenVertexArray();
        
        RebuildRenderTarget(renderWidth, renderHeight);
        GenerateShadowMap();
    }

    public Texture2D Render(Scene scene)
    {
        // Shadow pass
        RenderShadows();
        
        // Setup Render Target
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);
        GL.Viewport(0, 0, renderWidth, renderHeight);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        // Main render pass

        Camera? activeCamera = null;
        foreach (var entity in scene.Entities.Values)
        {
            if (entity.HasComponent<Camera>())
            {
                activeCamera = entity.GetComponent<Camera>();
                break; // Found one!
            }
        }

        if (activeCamera != null && defaultShader != null)
        {
            defaultShader.Use();
            
            activeCamera.AspectRatio = (float)renderWidth / renderHeight;
            
            defaultShader.SetMatrix4("view", activeCamera.GetViewMatrix());
            defaultShader.SetMatrix4("projection", activeCamera.GetProjectionMatrix());
            
            GL.BindVertexArray(dummyVao);
            
            RenderScene(scene, activeCamera);
            
            GL.BindVertexArray(0);
        }
        
        // Unbind FBO
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        
        return _finalFrameTexture!;
    }

    public void RenderScene(Scene scene, Camera mainCamera)
    {
        Matrix4 viewMatrix = mainCamera.GetViewMatrix();
        Matrix4 projectionMatrix = mainCamera.GetProjectionMatrix();

        var renderables = scene.Entities.Values
            .Where(e => e.HasComponent<MeshFilter>() && e.HasComponent<MeshRenderer>());
        foreach (var entity in renderables)
        {
            var filter = entity.GetComponent<MeshFilter>();
            if (filter!.MeshGuid == GUID.INVALID)
                continue;
            
            var metadata = assetManager.AssetPool.FindMetadata<MeshMetadata>(filter.MeshGuid);

            if (metadata == null)
                continue;

            var transform = entity.Transform;
            Matrix4 modelMatrix = Matrix4.CreateScale(transform.Scale) *
                                  Matrix4.CreateFromQuaternion(transform.Rotation) *
                                  Matrix4.CreateTranslation(transform.Position);
                                  
            defaultShader.SetMatrix4("model", modelMatrix);
            defaultShader.SetUInt("u_FirstTriIdx", metadata.FirstTriIdx);

            int vertexCount = (int)metadata.TriCount * 3;
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
        }  
    }
    
    // TODO: Implement shadows
    private void RenderShadows()
    {
        /*
        GL.CullFace(CullFaceMode.Front);

        // ... calculate lightSpaceMatrix ...

        _depthShader.Use();
        _depthShader.SetMatrix4("lightSpaceMatrix", _lightSpaceMatrix);

        GL.Viewport(0, 0, SHADOW_WIDTH, SHADOW_HEIGHT);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _shadowMapFbo);
        GL.Clear(ClearBufferMask.DepthBufferBit);

        // ... draw objects with depth shader ...

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.CullFace(CullFaceMode.Back);
        */
    }


    public void Clear(Color4 color)
    {
        GL.ClearColor(color.R, color.G, color.B, color.A);
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
        _finalFrameTexture = new Texture2D(colorTextureId, "RenderTarget");
    }

    private void GenerateShadowMap()
    {
        shadowMapFbo = GL.GenFramebuffer();
        shadowMapTexture = GL.GenTexture();
        
        GL.BindTexture(TextureTarget.Texture2D, shadowMapTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, SHADOW_WIDTH, SHADOW_HEIGHT, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
        
        float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowMapFbo);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, shadowMapTexture, 0);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }
    
    public void UpdateResources()
    {
        if (assetManager == null)
            return;
        
        // Check if the AssetManager have loaded new geometry since the last frame
        uint currentVersion = assetManager.AssetPool.GetUpdateVersion(AssetPool.AssetType.MeshBuffer);
        
        if (currentVersion > lastMeshBufferVersion)
        {
            resourceManager.UploadMegaBuffers(assetManager.AssetPool);
            lastMeshBufferVersion = currentVersion;
            Log.EngineInfo("Renderer: Mega-Buffers uploaded to GPU (Version {0})", currentVersion);
        }
    }
    
    public void Dispose()
    {
        defaultShader?.Dispose();
        depthShader?.Dispose();
        
        if (dummyVao != 0) GL.DeleteVertexArray(dummyVao);
        if (framebufferId != 0) GL.DeleteFramebuffer(framebufferId);
        if (shadowMapFbo != 0) GL.DeleteFramebuffer(shadowMapFbo);
        
        resourceManager?.Dispose();
    }
}