#region

using Assimp;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;
using TextureWrapMode = OpenTK.Graphics.OpenGL.TextureWrapMode;

#endregion

namespace PegasusEngine.Renderer.Textures;

/// <summary>
/// Represents a two-dimensional OpenGL texture used by the renderer.
/// </summary>
/// <remarks>
/// A <see cref="Texture2D"/> can wrap an existing OpenGL texture handle, load image data from disk,
/// or create a single-pixel texture from a color value.
/// </remarks>
public class Texture2D
{
    /// <summary>
    /// The OpenGL texture object identifier.
    /// </summary>
    public int textureID;

    /// <summary>
    /// The semantic texture type, such as diffuse, specular, normal, or framebuffer attachment.
    /// </summary>
    public string type;
    
    /// <summary>
    /// The source path used to create the texture, or a descriptive value for generated textures.
    /// </summary>
    public string path;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Texture2D"/> class from an existing OpenGL texture handle.
    /// </summary>
    /// <param name="textureId">The OpenGL texture object identifier to wrap.</param>
    /// <param name="type">The semantic texture type.</param>
    /// <remarks>
    /// This constructor is typically used for textures created elsewhere, such as framebuffer attachments.
    /// </remarks>
    public Texture2D(int textureId, string type)
    {
        this.textureID = textureId;
        this.type = type;
        this.path = "FBO_Attachment";
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Texture2D"/> class by loading image data from disk.
    /// </summary>
    /// <param name="filePath">The path to the image file to load.</param>
    /// <param name="type">The semantic texture type.</param>
    /// <remarks>
    /// The image is loaded as RGBA data, uploaded as an sRGB alpha texture, configured with linear filtering
    /// and repeat wrapping, and mipmaps are generated automatically.
    /// </remarks>
    /// <exception cref="FileNotFoundException">
    /// Thrown when <paramref name="filePath"/> does not exist.
    /// </exception>
    /// <exception cref="IOException">
    /// Thrown when the image file cannot be opened or read.
    /// </exception>
    public Texture2D(string filePath, string type)
    {
        this.type = type;
        path = filePath;
        textureID = GL.GenTexture();
        
        //GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, textureID);
        
        StbImage.stbi_set_flip_vertically_on_load(1);
        
        using (Stream stream = File.OpenRead(filePath))
        {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            
            GL.TexImage2D(
                TextureTarget.Texture2D, 
                0, 
                PixelInternalFormat.SrgbAlpha, 
                image.Width, 
                image.Height, 
                0, 
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                image.Data
            );
        }
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Texture2D"/> class as a generated 1x1 color texture.
    /// </summary>
    /// <param name="type">The semantic texture type.</param>
    /// <param name="color">The color value used for the single texel.</param>
    /// <remarks>
    /// The color is uploaded as floating-point RGBA data, configured with linear filtering and repeat wrapping,
    /// and mipmaps are generated automatically.
    /// </remarks>
    public Texture2D(string type, Color4D color)
    {
        this.type = type;
        path = "";
        textureID = GL.GenTexture();
        
        GL.BindTexture(TextureTarget.Texture2D, textureID);

        GL.TexImage2D(
            TextureTarget.Texture2D, 
            0, 
            PixelInternalFormat.Rgba, 
            1, 
            1, 
            0, 
            PixelFormat.Rgba,
            PixelType.Float,
            ref color
        );
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }

    /// <summary>
    /// Activates the specified texture unit and binds this texture to the 2D texture target.
    /// </summary>
    /// <param name="unit">The texture unit to activate before binding the texture.</param>
    public void Use(TextureUnit unit = TextureUnit.Texture0)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, textureID);
    }

    /// <summary>
    /// Returns a string that describes this texture.
    /// </summary>
    /// <returns>A string containing the texture path, semantic type, and OpenGL texture identifier.</returns>
    public override string ToString()
    {
        return $"Texture2D: Path: {path}, Type: {type}, ID: {textureID}";
    }
}