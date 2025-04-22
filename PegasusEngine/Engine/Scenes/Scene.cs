using OpenTK.Mathematics;
using PegasusEngine.Engine.Core;
using PegasusEngine.Engine.Objects;
using PegasusEngine.Engine.Utils;

namespace PegasusEngine.Engine.Scenes;

public class Scene
{
    private readonly List<GameObject> gameObjects = new List<GameObject>();
    private readonly List<Light> lights = new List<Light>();
    private readonly Skybox? skybox;
    
    private Camera camera;

    public Scene(Camera camera, Skybox skybox = null)
    {
        this.skybox = skybox;
        
        lights.Add(new DirectLight(
            new Vector3(-2f, 10f, -1f),
            new Vector3(1.0f, 0.95f, 0.8f)
            ));
        
        this.camera = camera;
    }

    public void Render()
    {
        foreach (var gameObject in gameObjects)
        {
            var shader = gameObject.Shader;
            
            shader.Use();
            
            // Setup lights
            shader.SetVector3("viewPos", camera.Position);
        
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());
            // shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix); // TODO: Lightning
        
            shader.SetFloat("near_plane", camera.NearPlane);
            shader.SetFloat("far_plane", camera.FarPlane);
            
            var viewModel = Matrix4.Identity;
            viewModel *= Matrix4.CreateTranslation(gameObject.Transform.Position);
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
        {
            gameObject.Update();
        }
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