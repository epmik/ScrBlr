
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using static OpenTK.Graphics.OpenGL.GL;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace OpenGlTutorialOrg
{
    public class Controls
    {
        //glm::mat4 ViewMatrix;
        public Matrix4 ViewMatrix = Matrix4.Identity;
        //glm::mat4 ProjectionMatrix;
        public Matrix4 ProjectionMatrix = Matrix4.Identity;

        //glm::mat4 getViewMatrix()
        //{
        //    return ViewMatrix;
        //}
        //glm::mat4 getProjectionMatrix()
        //{
        //    return ProjectionMatrix;
        //}


        // Initial position : on +Z
        //glm::vec3 position = glm::vec3(0, 0, 5);
        private Vector3 position = new Vector3(0, 0, 5);
        // Initial horizontal angle : toward -Z
        float horizontalAngle = 3.14f;
        // Initial vertical angle : none
        float verticalAngle = 0.0f;
        // Initial Field of View
        float initialFoV = 45.0f;

        float speed = 3.0f; // 3 units / second
        float mouseSpeed = 0.005f;

        public void Update(GameWindow window, float deltaTime)
        {

            // glfwGetTime is called only once, the first time this function is called
            //static double lastTime = glfwGetTime();

            // Compute time difference between current and last frame
            //double currentTime = glfwGetTime();
            //float deltaTime = float(currentTime - lastTime);

            // Get mouse position
            //double xpos, ypos;
            //glfwGetCursorPos(window, &xpos, &ypos);
            var xpos = window.MouseState.X;
            var ypos = window.MouseState.Y;

            // Reset mouse position for next frame
            //glfwSetCursorPos(window, 1024 / 2, 768 / 2);
            window.MousePosition = new Vector2(1024 / 2, 768 / 2);

            // Compute new orientation
            //horizontalAngle += mouseSpeed * float(1024 / 2 - xpos);
            //verticalAngle += mouseSpeed * float(768 / 2 - ypos);
            horizontalAngle += mouseSpeed * (float)(1024 / 2 - xpos);
            verticalAngle += mouseSpeed * (float)(768 / 2 - ypos);

            // Direction : Spherical coordinates to Cartesian coordinates conversion
            //glm::vec3 direction(
            //    cos(verticalAngle)* sin(horizontalAngle),
            //    sin(verticalAngle),
            //    cos(verticalAngle)* cos(horizontalAngle)
        
            //);
            var direction = new Vector3(
                MathF.Cos(verticalAngle) * MathF.Sin(horizontalAngle),
                MathF.Sin(verticalAngle),
                MathF.Cos(verticalAngle) * MathF.Cos(horizontalAngle)        
            );

            // Right vector
            //glm::vec3 right = glm::vec3(
            //    sin(horizontalAngle - 3.14f / 2.0f),
            //    0,
            //    cos(horizontalAngle - 3.14f / 2.0f)
            //);
            var right = new Vector3(
                MathF.Sin(horizontalAngle - 3.14f / 2.0f),
                0,
                MathF.Cos(horizontalAngle - 3.14f / 2.0f)
            );

            // Up vector
            //glm::vec3 up = glm::cross(right, direction);
            var up = Vector3.Cross(right, direction);

            // Move forward
            //if (glfwGetKey(window, GLFW_KEY_UP) == GLFW_PRESS)
            //{
            //    position += direction * deltaTime * speed;
            //}
            if (window.KeyboardState.IsKeyDown(Keys.Up))
            {
                position += direction * deltaTime * speed;
            }
            // Move backward
            //if (glfwGetKey(window, GLFW_KEY_DOWN) == GLFW_PRESS)
            //{
            //    position -= direction * deltaTime * speed;
            //}
            if (window.KeyboardState.IsKeyDown(Keys.Down))
            {
                position -= direction * deltaTime * speed;
            }
            // Strafe right
            //if (glfwGetKey(window, GLFW_KEY_RIGHT) == GLFW_PRESS)
            //{
            //    position += right * deltaTime * speed;
            //}
            if (window.KeyboardState.IsKeyDown(Keys.Right))
            {
                position += right * deltaTime * speed;
            }
            // Strafe left
            //if (glfwGetKey(window, GLFW_KEY_LEFT) == GLFW_PRESS)
            //{
            //    position -= right * deltaTime * speed;
            //}
            if (window.KeyboardState.IsKeyDown(Keys.Left))
            {
                position -= right * deltaTime * speed;
            }
            if (window.KeyboardState.IsKeyDown(Keys.PageUp))
            {
                position += up * deltaTime * speed;
            }
            if (window.KeyboardState.IsKeyDown(Keys.PageDown))
            {
                position -= up * deltaTime * speed;
            }


            float FoV = initialFoV;// - 5 * glfwGetMouseWheel(); // Now GLFW 3 requires setting up a callback for this. It's a bit too complicated for this beginner's tutorial, so it's disabled instead.

            // Projection matrix : 45� Field of View, 4:3 ratio, display range : 0.1 unit <-> 100 units
            //ProjectionMatrix = glm::perspective(glm::radians(FoV), 4.0f / 3.0f, 0.1f, 100.0f);
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FoV), 4.0f / 3.0f, 0.1f, 100.0f);

            // Camera matrix
            //ViewMatrix = glm::lookAt(
            //    position,           // Camera is here
            //    position + direction, // and looks here : at the same position, plus "direction"
            //    up                  // Head is up (set to 0,-1,0 to look upside-down)
            //);
            ViewMatrix = Matrix4.LookAt(
                position,   // Camera is here
                position + direction,         // and looks here : at the same position, plus "direction"
                up         // Head is up (set to 0,-1,0 to look upside-down)
            );

            // For the next frame, the "last time" will be "now"
            //lastTime = currentTime;
        }
    }
}   