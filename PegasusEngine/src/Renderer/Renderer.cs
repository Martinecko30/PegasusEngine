using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using PegasusEngine.Common;
using PegasusEngine.Debug;
using PegasusEngine.Objects;
using PegasusEngine.Objects.Components;
using PegasusEngine.Objects.Components.Lights;
using PegasusEngine.Objects.Components.Meshes;
using PegasusEngine.Project;
using PegasusEngine.Project.Assets;
using PegasusEngine.Project.Scenes;
using PegasusEngine.Renderer.Shaders;
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

    private const int MAX_SHADOWS = 10;
    
    private readonly Vector2i SHADOW_DIMENSIONS = new Vector2i(4096, 4096);
    // private const int SHADOW_WIDTH = 4096;
    // private const int SHADOW_HEIGHT = 4096;

    private Matrix4[] lightSpaceMatrices = new Matrix4[MAX_SHADOWS];
    private int activeLightCount = 0;
    
    private int[] shadowMapTextures = new int[MAX_SHADOWS];
    private int[] shadowMapFbos = new int[MAX_SHADOWS];
    
    private Matrix4 lightSpaceMatrix;
    
    private int dummyVao;
    
    private ProjectManager projectManager;
    private GraphicsResourceManager resourceManager;
    private uint lastMeshBufferVersion = 0;
    
    
    private readonly Dictionary<GUID, Texture2D> gpuTextureCache = new();
    
    

    public Renderer(ProjectManager projectManager)
    {
        this.projectManager = projectManager;
    }
    
    public virtual void Init()
    {
        this.resourceManager = new GraphicsResourceManager();
        
        this.defaultShader = new Shader(
            "res/Shaders/DefaultShader.vert",
            "res/Shaders/DefaultShader.frag"
            );
        
        this.depthShader = new Shader(
            "res/Shaders/DepthShader.vert",
            "res/Shaders/DepthShader.frag"
        );
        
        GL.Enable(EnableCap.Multisample);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Lequal);
        // GL.Enable(EnableCap.FramebufferSrgb);
        GL.ClearColor(0.01f, 0.01f, 0.01f, 1.0f);
        
        dummyVao = GL.GenVertexArray();
        
        RebuildRenderTarget(renderWidth, renderHeight);
        for (int i = 0; i < MAX_SHADOWS; i++)
            GenerateShadowMap(i);
    }

    
    public virtual Texture2D Render(Scene scene, Camera camera)
    {
        // Shadow pass
        RenderLights(scene);
        
        // Setup Render Target
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);
        GL.Viewport(0, 0, renderWidth, renderHeight);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        // Main render pass
        if (camera != null && defaultShader != null)
        {
            defaultShader.Use();
            
            camera.AspectRatio = (float)renderWidth / renderHeight;
            defaultShader.SetMatrix4("view", camera.GetViewMatrix());
            defaultShader.SetMatrix4("projection", camera.GetProjectionMatrix());
            defaultShader.SetVector3("viewPos", camera.GameObject.Transform.Position);
            
            UploadLightsToShader(scene);
            for (int i = 0; i < activeLightCount; i++)
            {
                defaultShader.SetMatrix4($"lightSpaceMatrices[{i}]", lightSpaceMatrices[i]);
                
                GL.ActiveTexture(TextureUnit.Texture1 + i);
                GL.BindTexture(TextureTarget.Texture2D, shadowMapTextures[i]);
                defaultShader.SetInt($"shadowMaps[{i}]", 1 + i);
            }
            
            GL.BindVertexArray(dummyVao);
            DrawMeshes(scene, defaultShader);
            GL.BindVertexArray(0);
        }
        
        OnPostMainRender(scene, camera);
        
        // Unbind FBO
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        return _finalFrameTexture!;
    }
    
    protected virtual void OnPostMainRender(Scene scene, Camera camera) {}
    
    [Obsolete("This is not used anymore, and will be removed in the future")]
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
            
            var metadata = projectManager.AssetManager.AssetPool.FindMetadata<MeshMetadata>(filter.MeshGuid);

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
    
    
    private void RenderLights(Scene scene)
    {
        var lightEntities = scene.Entities.Values
            .Where(e => e.HasComponent<Light>() && e.HasComponent<Transform>())
            .ToList();
        
        activeLightCount = Math.Clamp(lightEntities.Count, 0, MAX_SHADOWS);
        for (int i = 0; i < activeLightCount; ++i )
        {
            var light = lightEntities[i].GetComponent<Light>();
            var transform = lightEntities[i].Transform;
            
            Vector3 lightForward = transform.Rotation * new Vector3(0, 0, -1);
            Vector3 lightPos = transform.Position;
            Vector3 lightTarget = lightPos + lightForward;
            
            Matrix4 lightView = Matrix4.LookAt(lightPos, lightTarget, new Vector3(0, 1, 0));
            Matrix4 lightProjection;
            
            // TODO: Implement different lights
            lightProjection = Matrix4.CreateOrthographicOffCenter(-50.0f, 50.0f, -50.0f, 50.0f, 1.0f, 100.0f);
            
            // This is for spotlight
            // lightProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(light.SpotAngle), 1.0f, 1.0f, 100.0f);
            
            
            lightSpaceMatrices[i] = lightView * lightProjection;
            
            depthShader.Use();
            depthShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrices[i]);
            
            GL.Viewport(0, 0, SHADOW_DIMENSIONS.X, SHADOW_DIMENSIONS.Y);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowMapFbos[i]);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            
            GL.CullFace(TriangleFace.Front);
            
            GL.BindVertexArray(dummyVao);
            DrawMeshes(scene, depthShader);
            GL.BindVertexArray(0);
            
            GL.CullFace(TriangleFace.Back);
        }
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private void UploadLightsToShader(Scene scene)
    {
        var lightEntities = scene.Entities.Values
            .Where(e => e.HasComponent<Light>() && e.HasComponent<Transform>()).ToList();
        
        int lightCount = Math.Clamp(lightEntities.Count, 0, MAX_SHADOWS);
        for (int i = 0; i < lightCount; i++)
        {
            var light = lightEntities[i].GetComponent<Light>();
            var transform = lightEntities[i].Transform;
            
            defaultShader.SetVector3($"lights[{i}].position", transform.Position);
            
            Vector3 lightColor = new Vector3(1.0f, 1.0f, 1.0f) * light.ShadowStrength;
            defaultShader.SetVector3($"lights[{i}].color", lightColor);
        }
        
        defaultShader.SetInt("activeLightCount", Math.Max(0, lightCount));
    }

    private void DrawMeshes(Scene scene, Shader activeShader)
    {
        var renderables = scene.Entities.Values
            .Where(e => e.HasComponent<MeshFilter>() && e.HasComponent<MeshRenderer>());

        foreach (var entity in renderables)
        {
            var filter = entity.GetComponent<MeshFilter>();
            var renderer = entity.GetComponent<MeshRenderer>();
            
            if (filter!.MeshGuid == GUID.INVALID)
                continue;
            
            var metadata = projectManager.AssetManager.AssetPool.FindMetadata<MeshMetadata>(filter.MeshGuid);
            if (metadata == null)
                continue;

            if (renderer.DiffuseTexture != GUID.INVALID)
            {
                if (!gpuTextureCache.TryGetValue(renderer.DiffuseTexture, out Texture2D? text))
                {
                    var texMeta = projectManager.AssetManager.AssetPool.FindMetadata<TextureMetadata>(renderer.DiffuseTexture);
                    if (texMeta != null)
                    {
                        int byteLength = (int)(texMeta.Width * texMeta.Height * texMeta.Channels);

                        byte[] pixelData = projectManager.AssetManager.AssetPool.TextureBuffer
                            .Skip((int)texMeta.TexStartIdx)
                            .Take(byteLength)
                            .ToArray();
                        
                        text = new Texture2D(pixelData, (int)texMeta.Width, (int)texMeta.Height, (int)texMeta.Channels, "texture_diffuse");
                        gpuTextureCache[renderer.DiffuseTexture] = text;
                    }
                }

                if (text != null && activeShader == defaultShader)
                {
                    text.Use(TextureUnit.Texture0);
                    activeShader.SetInt("diffuseTexture", 0);
                    activeShader.SetInt("hasTexture", 1); // Tell GLSL we HAVE a texture
                }else
                {
                    // No texture GUID? Tell GLSL to skip sampling!
                    if (activeShader == defaultShader)
                        activeShader.SetInt("hasTexture", 0); 
                }
            }
            
            var transform = entity.Transform;
            Matrix4 modelMatrix = Matrix4.CreateScale(transform.Scale) *
                                  Matrix4.CreateFromQuaternion(transform.Rotation) *
                                  Matrix4.CreateTranslation(transform.Position);
                                  
            activeShader.SetMatrix4("model", modelMatrix);
            activeShader.SetUInt("u_FirstTriIdx", metadata.FirstTriIdx);
            
            if (activeShader == defaultShader)
            {
                Matrix4 modelInvTrans = modelMatrix;
                modelInvTrans.Invert();
                modelInvTrans.Transpose();
                activeShader.SetMatrix4("modelInverseTransposed", modelInvTrans);
            }

            int vertexCount = (int)metadata.TriCount * 3;
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
        }
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

    private void GenerateShadowMap(int index)
    {
        int shadowMapFbo = GL.GenFramebuffer();
        int shadowMapTexture = GL.GenTexture();
        
        GL.BindTexture(TextureTarget.Texture2D, shadowMapTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, SHADOW_DIMENSIONS.X, SHADOW_DIMENSIONS.Y, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
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
        
        shadowMapFbos[index] = shadowMapFbo;
        shadowMapTextures[index] = shadowMapTexture;
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
    
    public virtual void Dispose()
    {
        defaultShader?.Dispose();
        depthShader?.Dispose();
        
        if (dummyVao != 0) GL.DeleteVertexArray(dummyVao);
        if (framebufferId != 0) GL.DeleteFramebuffer(framebufferId);

        for (int i = 0; i < MAX_SHADOWS; i++)
        {
            if (shadowMapTextures[i] != 0) GL.DeleteTexture(shadowMapTextures[i]);
            if (shadowMapFbos[i] != 0) GL.DeleteFramebuffer(shadowMapFbos[i]);
        }
        
        resourceManager?.Dispose();
    }
}