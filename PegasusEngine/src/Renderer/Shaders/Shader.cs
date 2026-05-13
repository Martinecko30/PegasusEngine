#region

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

#endregion

namespace PegasusEngine.Renderer.Shaders;

/// <summary>
/// Represents an OpenGL shader program created from vertex and fragment shader source files.
/// </summary>
public class Shader : IDisposable
{
    /// <summary>
    /// The OpenGL handle for the linked shader program.
    /// </summary>
    public int Handle;
    
    private readonly int vertexShaderHandle;
    private readonly int fragShaderHandle;

    private readonly Dictionary<string, int> uniformLocations;

    /// <summary>
    /// Creates, compiles, and links a shader program from vertex and fragment shader files.
    /// </summary>
    /// <param name="vertexShaderPath">The file path to the vertex shader source.</param>
    /// <param name="fragmentShaderPath">The file path to the fragment shader source.</param>
    /// <exception cref="Exception">
    /// Thrown when shader compilation or program linking fails.
    /// </exception>
    public Shader(string vertexShaderPath, string fragmentShaderPath)
    {
        string shaderSource = File.ReadAllText(vertexShaderPath);
        vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShaderHandle, shaderSource);
        CompileShader(vertexShaderHandle);

        shaderSource = File.ReadAllText(fragmentShaderPath);
        fragShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragShaderHandle, shaderSource);
        CompileShader(fragShaderHandle);

        Handle = GL.CreateProgram();
        
        GL.AttachShader(Handle, vertexShaderHandle);
        GL.AttachShader(Handle, fragShaderHandle);
        
        LinkProgram();

        uniformLocations = new();
        
        GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

        for (int i = 0; i < numberOfUniforms; i++)
        {
            string key = GL.GetActiveUniform(Handle, i, out _, out _);
            int location = GL.GetUniformLocation(Handle, key);
            uniformLocations.Add(key, location);
        }
        
        
        
        GL.DetachShader(Handle, vertexShaderHandle);
        GL.DeleteShader(vertexShaderHandle);
        GL.DetachShader(Handle, fragShaderHandle);
        GL.DeleteShader(fragShaderHandle);
    }

    /// <summary>
    /// Compiles an OpenGL shader and throws an exception if compilation fails.
    /// </summary>
    /// <param name="shader">The OpenGL shader handle to compile.</param>
    /// <exception cref="Exception">Thrown when shader compilation fails.</exception>
    private void CompileShader(int shader)
    {
        GL.CompileShader(shader);
        
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            var infoLog = GL.GetShaderInfoLog(shader);
            throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
        }
    }

    /// <summary>
    /// Links the shader program and throws an exception if linking fails.
    /// </summary>
    /// <exception cref="Exception">Thrown when program linking fails.</exception>
    private void LinkProgram()
    {
        GL.LinkProgram(Handle);
        
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(Handle);
            throw new Exception($"Error occurred whilst linking Program {Handle}.\n\n{infoLog}");
        }
    }
    
    /// <summary>
    /// Makes this shader program the active OpenGL program.
    /// </summary>
    public void Use()
    {
        GL.UseProgram(Handle);
    }
    
    /// <summary>
    /// Gets the location of an attribute in the shader program.
    /// </summary>
    /// <param name="attribName">The name of the shader attribute.</param>
    /// <returns>The location of the attribute, or -1 if it is not found.</returns>
    [Obsolete("It's unsafe to use this function to get attributes\nAssign attributes location in VBO")]
    public int GetAttribLocation(string attribName)
    {
        return GL.GetAttribLocation(Handle, attribName);
    }

    /// <summary>
    /// Gets the location of a uniform in the shader program.
    /// </summary>
    /// <param name="uniformName">The name of the shader uniform.</param>
    /// <returns>The location of the uniform, or -1 if it is not found.</returns>
    public int GetUniformLocation(string uniformName)
    {
        return GL.GetUniformLocation(Handle, uniformName);
    }
    
    /// <summary>
    /// Sets an integer uniform value.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <param name="data">The integer value to set.</param>
    public void SetInt(string name, int data)
    {
        GL.UseProgram(Handle);
        GL.Uniform1(UniformLocationsLookUp(name), data);
    }
    
    /// <summary>
    /// Sets an unsigned integer uniform value.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <param name="data">The unsigned integer value to set.</param>
    public void SetUInt(string name, uint data)
    {
        GL.UseProgram(Handle);
        GL.Uniform1(UniformLocationsLookUp(name), data);
    }
    
    /// <summary>
    /// Sets a floating-point uniform value.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <param name="data">The floating-point value to set.</param>
    public void SetFloat(string name, float data)
    {
        GL.UseProgram(Handle);
        GL.Uniform1(UniformLocationsLookUp(name), data);
    }

    /// <summary>
    /// Sets a 4x4 matrix uniform value.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <param name="data">The matrix value to set.</param>
    public void SetMatrix4(string name, Matrix4 data)
    {
        GL.UseProgram(Handle);
        GL.UniformMatrix4(UniformLocationsLookUp(name), true, ref data);
    }

    /// <summary>
    /// Sets a three-component vector uniform value.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <param name="data">The vector value to set.</param>
    public void SetVector3(string name, Vector3 data)
    {
        GL.UseProgram(Handle);
        GL.Uniform3(UniformLocationsLookUp(name), data);
    }

    /// <summary>
    /// Sets a 3x3 matrix uniform value.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <param name="data">The matrix value to set.</param>
    public void SetMatrix3(string name, Matrix3 data)
    {
        GL.UseProgram(Handle);
        GL.UniformMatrix3(UniformLocationsLookUp(name), true, ref data);
    }

    /// <summary>
    /// Sets a boolean uniform value.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <param name="data">The boolean value to set.</param>
    public void SetBool(string name, bool data)
    {
        SetInt(name, data ? 1 : 0);
    }
    
    
    /// <summary>
    /// Gets a cached uniform location, querying OpenGL and caching the result if needed.
    /// </summary>
    /// <param name="name">The name of the uniform.</param>
    /// <returns>The OpenGL location of the uniform.</returns>
    private int UniformLocationsLookUp(string name)
    {
        if (uniformLocations.TryGetValue(name, out int value))
        {
            return value;
        }

        value = GetUniformLocation(name);
        return (uniformLocations[name] = value);
    }
    
    
    
    private bool disposedValue;
    
    /// <summary>
    /// Releases the unmanaged OpenGL shader program resource.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> when called from <see cref="Dispose()"/>;
    /// otherwise, <see langword="false"/> when called from the finalizer.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            GL.DeleteProgram(Handle);

            disposedValue = true;
        }
    }

    /// <summary>
    /// Finalizes the shader and warns if the shader program was not disposed.
    /// </summary>
    ~Shader()
    {
        if (!disposedValue)
        {
            Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
        }
    }

    /// <summary>
    /// Releases the OpenGL shader program and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}