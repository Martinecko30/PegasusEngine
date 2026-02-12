// #region
//
// using OpenTK.Mathematics;
// using OpenTK.Windowing.GraphicsLibraryFramework;
// using PegasusEngine.Core.InputSystem;
// using PegasusEngine.Core.TimeSystem;
// using PegasusEngine.Editor.Tabs;
//
// #endregion
//
// namespace PegasusEngine.Modules.Camera;
//
// public class CameraController
// {
//     private readonly Camera camera;
//
//     private const float cameraSpeed = 1.5f;
//     private const float sensitivity = 0.2f;
//     
//     
//     private bool _firstMove = true;
//     private Vector2 _lastPos;
//     
//     public CameraController(Camera camera)
//     {
//         this.camera = camera;
//     }
//
//     public void UpdatePosition()
//     {
//         // if (!Viewport.IS_VIEWPORT_FOCUSED)
//         //     return;
//         
//         var speed = cameraSpeed;
//         var input = Input.KeyboardState;
//         
//         if (input.IsKeyDown(Keys.LeftControl))
//         {
//             speed *= 2f;
//         }
//         
//         if (input.IsKeyDown(Keys.W))
//         {
//             camera.Position += camera.Front * speed * (float)Time.DeltaTime; // Forward
//         }
//         if (input.IsKeyDown(Keys.S))
//         {
//             camera.Position -= camera.Front * speed * (float)Time.DeltaTime; // Backwards
//         }
//         if (input.IsKeyDown(Keys.A))
//         {
//             camera.Position -= camera.Right * speed * (float)Time.DeltaTime; // Left
//         }
//         if (input.IsKeyDown(Keys.D))
//         {
//             camera.Position += camera.Right * speed * (float)Time.DeltaTime; // Right
//         }
//         
//         if (input.IsKeyDown(Keys.Space))
//         {
//             camera.Position += Vector3.UnitY * cameraSpeed * (float)Time.DeltaTime; // Up
//         }
//         if (input.IsKeyDown(Keys.LeftShift))
//         {
//             camera.Position -= Vector3.UnitY * cameraSpeed * (float)Time.DeltaTime; // Down
//         }
//     }
//
//     public void UpdateView()
//     {
//         var mouse = Input.MouseState;
//
//         if (_firstMove)
//         {
//             _lastPos = mouse.Position;
//             _firstMove = false;
//         }
//         else
//         {
//             var deltaX = mouse.X - _lastPos.X;
//             var deltaY = mouse.Y - _lastPos.Y;
//             _lastPos = new Vector2(mouse.X, mouse.Y);
//
//             // if (Viewport.IS_VIEWPORT_FOCUSED && mouse.IsButtonDown(MouseButton.Right))
//             // {
//             //     camera.Yaw += deltaX * sensitivity;
//             //     camera.Pitch -= deltaY * sensitivity;
//             // }
//         }
//     }
// }