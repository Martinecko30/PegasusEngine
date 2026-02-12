// using ImGuiNET;
// using OpenTK.Graphics.OpenGL;
// using OpenTK.Mathematics;
// using OpenTK.Windowing.Common;
// using OpenTK.Windowing.Desktop;
// using PegasusEngine.Editor.Tabs;
// using PegasusEngine.Modules.Camera;
// using PegasusEngine.Modules.Rendering.Shaders;
// using PegasusEngine.Modules.Scripting;
// using PegasusEngine.PegasusEditor;
// using PegasusEngine.Runtime.Objects;
// using PegasusEngine.Runtime.Scenes;
//
// namespace PegasusEngine.Editor.Rendering;
//
// public class EditorRenderer
// {
//     private Camera camera;
//     
//     private ImGuiController controller;
//     private GameWindow gameWindow;
//     private readonly List<TabPanel> tabPanels = new List<TabPanel>();
//     public static Scene CurrentScene { private set; get; }
//     
//     public static bool DEBUG { private set; get; }
//     
//     private Vector2i clientSize = Vector2i.Zero;
//     private Vector2i windowSize = Vector2i.Zero;
//
//     public void Start(Vector2i clientSize, Vector2i windowSize)
//     {
//         this.clientSize = clientSize;
//         this.windowSize = windowSize;
//         
//         // ImGui setup
//         controller = new ImGuiController(clientSize.X / 2, clientSize.Y / 2);
//         
//         List<string> skyboxFaces = new List<string>
//         {
//             "Resources\\Skybox\\right.jpg",
//             "Resources\\Skybox\\left.jpg",
//             "Resources\\Skybox\\top.jpg",
//             "Resources\\Skybox\\bottom.jpg",
//             "Resources\\Skybox\\front.jpg",
//             "Resources\\Skybox\\back.jpg"
//         };
//         
//         CurrentScene = new Scene(
//             new Camera(new Vector3(0, 1, 2), clientSize.X / (float)clientSize.Y),
//             new Skybox(
//                 skyboxFaces, 
//                 new Shader(
//                     "Resources\\Shaders\\SkyboxShader.vert", 
//                     "Resources\\Shaders\\SkyboxShader.frag"
//                 )
//             ));
//         
//         var defaultShader = new Shader(
//             "Resources\\Shaders\\DefaultShader.vert",
//             "Resources\\Shaders\\DefaultShader.frag"
//         );
//         
//         
//         // TODO: Move scene manipulation to serialized scene
//         var tree = new GameObject(
//             "Tree",
//             "Resources\\Models\\tree_forest.obj",
//             Vector3.Zero, 
//             Vector3.One,
//             defaultShader
//         );
//         
//         tree.Behaviours.Add(new TestBehaviour());
//         tree.Behaviours.Add(new TestBehaviour());
//         
//         CurrentScene.AddObject(tree);
//     }
//
//     public void Render(GameWindow gameWindow, FrameEventArgs args)
//     {
//         controller.Update(gameWindow, (float)args.Time);
//
//         clientSize = gameWindow.ClientSize;
//
//         GL.ClearColor(new Color4(0, 32, 48, 255));
//         GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
//
//         RenderEditor();
//         
//         RenderScene(CurrentScene);
//         ResizeToNormal();
//         
//         controller.Render();
//         
//         ImGuiController.CheckGLError("End of frame");
//
//         gameWindow.SwapBuffers();
//     }
//     
//     private void RenderEditor()
//     {
//         ImGui.DockSpaceOverViewport();
//         
//         // Render
//         foreach (TabPanel tabPanel in tabPanels)
//             tabPanel.Render();
//     }
//
//     private void RenderScene(Scene scene)
//     {        
//         GL.Enable(EnableCap.DepthTest);
//         // GL.DepthFunc(DepthFunction.Less);
//
//
//         // GL.BindFramebuffer(FramebufferTarget.Framebuffer, Viewport.FBO);
//         GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
//             
//         // GL.Viewport(0, 0, Viewport.WindowSize.X, Viewport.WindowSize.Y);
//
//
//         scene.Render();
//         
//
//         GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
//     }
//     
//     public void ResizeToNormal()
//     {
//         GL.Viewport(0, 0, clientSize.X, clientSize.Y);
//         controller.WindowResized(clientSize.X, clientSize.Y);
//     }
//
//     public void Resize(Vector2i size)
//     {
//         GL.Viewport(0, 0, size.X, size.Y);
//         controller.WindowResized(size.X, size.Y);
//     }
//
//     public ImGuiController GetImGuiController()
//     {
//         return controller;
//     }
// }