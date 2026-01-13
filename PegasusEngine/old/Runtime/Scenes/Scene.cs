#region

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using PegasusEngine.Modules.Camera;
using PegasusEngine.Modules.Lighting;
using PegasusEngine.Modules.Rendering.Shaders;
using PegasusEngine.Runtime.Objects;
using PegasusEngine.Utils;

#endregion

namespace PegasusEngine.Runtime.Scenes;

public class Scene
{
    private readonly List<GameObject> gameObjects = new List<GameObject>();
    private readonly List<DirectLight> directLights = new List<DirectLight>();
    private readonly Skybox? skybox;
    
    private Camera camera;
    private CameraController cameraController;
    
    
    // Lighting
    private const int SHADOW_WIDTH = 4096;
    private const int SHADOW_HEIGHT = 4096;
    private readonly List<(int, int)> depthMaps = new();
    private Shader depthShader; // TODO: Fix?
    private Matrix4 lightSpaceMatrix;

    public Scene(Camera camera, Skybox skybox = null)
    {
        this.skybox = skybox;
        
        depthShader = new Shader(
            "Resources\\Shaders\\DepthShader.vert",
            "Resources\\Shaders\\DepthShader.frag"
        );
        
        directLights.Add(new DirectLight(
            new Vector3(-2f, 10f, -1f),
            new Vector3(1.0f, 0.95f, 0.8f)
            ));
        
        this.camera = camera;
        cameraController = new CameraController(camera);

        for (int i = 0; i < directLights.Count; i++)
        {
            (int, int) depthMap = directLights[i].CreateShadowMap(new Vector2i(SHADOW_WIDTH, SHADOW_HEIGHT));
            depthMaps.Add(depthMap);
        }
    }

    public void Render(Shader? overShader = null)
    {
        // foreach (var (map, mapFBO) in depthMaps)
        //     RenderShadowMap(map, mapFBO);
        
        RenderScene(overShader);
    }

    private void RenderScene(Shader? overShader = null)
    {
        foreach (var gameObject in gameObjects)
        {
            var shader = overShader == null ? gameObject.Shader : overShader;
            
            shader.Use();
            
            // Setup lights
            shader.SetVector3("viewPos", camera.Position);
        
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());
            shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix); // TODO: Lightning
        
            shader.SetFloat("near_plane", camera.NearPlane);
            shader.SetFloat("far_plane", camera.FarPlane);
            
            var viewModel = Matrix4.Identity;
            viewModel *= Matrix4.CreateTranslation(gameObject.Transform.Position);
            viewModel *= Matrix4.CreateFromQuaternion(gameObject.Transform.Rotation);
            viewModel *= Matrix4.CreateScale(gameObject.Transform.Scale);
            gameObject.Shader.SetMatrix4("model", viewModel);
            gameObject.Shader.SetMatrix4(
                "modelInverseTransposed",
                BaseUtils.TransposeAndInverseMatrix(viewModel)
            );
            gameObject.Draw();
        }
        
        var viewMatrix = new Matrix4(new Matrix3(camera.GetViewMatrix()));
        skybox.Render(viewMatrix, camera.GetProjectionMatrix());
    }

    public void Update()
    {
        foreach (var gameObject in gameObjects)
            gameObject.Update();
    }

    public void UpdateEditor()
    {
        cameraController.UpdatePosition();
        cameraController.UpdateView();
    }
    
    private void RenderShadowMap(int depthMap, int depthMapFBO)
    {
        GL.Enable(EnableCap.FramebufferSrgb);
        
        GL.CullFace(CullFaceMode.Front);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        float nearPlane = 1.0f, farPlane = 100f;
        Matrix4 lightProjection = Matrix4.CreateOrthographic(40f, 40f, nearPlane, farPlane);
        Matrix4 lightView = Matrix4.LookAt(directLights[0].Position, Vector3.Zero, Vector3.UnitY);
        lightSpaceMatrix = lightView * lightProjection;
        
        depthShader.Use();
        depthShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
        
        
        GL.Viewport(0, 0, SHADOW_WIDTH, SHADOW_HEIGHT); // Shadow map TODO: IMPORTANT
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
        
        var shadowMapLocation = gameObjects[0].Shader.GetUniformLocation("shadowMap");
        gameObjects[0].Shader.Use();
        GL.Uniform1(shadowMapLocation, 2); // TODO: Rework so it isn't hardcoded 2 (0 = diffuse, 1 = specular)
        
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.ActiveTexture(TextureUnit.Texture2);
        GL.BindTexture(TextureTarget.Texture2D, depthMap);
        
        RenderScene(depthShader);
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.CullFace(CullFaceMode.Back);
        
        GL.Disable(EnableCap.FramebufferSrgb);
    }
    
    public void SerializeScene(string sceneSource)
    {
        
    }

    public void AddObject(GameObject gameObject)
    {
        gameObjects.Add(gameObject);
    }
    
    public List<GameObject> GetObjects()
    {
        return new List<GameObject>(gameObjects);
    }
}