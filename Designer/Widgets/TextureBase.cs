using System.Runtime.InteropServices;
using LuaNET.Lua51;
using static LuaNET.Lua51.Lua;

namespace WoWFrameTools.Widgets;

public class TextureBase : Region
{
    // Texture data
    protected string? _texturePath { get; set; }
    protected int _textureFileID { get; set; }
    protected bool _isColorTexture { get; set; }
    protected float _colorR { get; set; }
    protected float _colorG { get; set; }
    protected float _colorB { get; set; }
    protected float _colorA { get; set; } = 1.0f;

    // Texture coordinates (UV mapping)
    protected float _texCoordLeft { get; set; } = 0f;
    protected float _texCoordRight { get; set; } = 1f;
    protected float _texCoordTop { get; set; } = 0f;
    protected float _texCoordBottom { get; set; } = 1f;

    // Rotation
    protected float _rotation { get; set; }
    protected string? _rotationPoint { get; set; }

    // Blend mode
    protected string _blendMode { get; set; } = "BLEND";

    protected TextureBase(string objectType, string? name, string? drawLayer, string? templateName, int subLevel, Frame? parent)
        : base(objectType, name, drawLayer, parent, subLevel)
    {
    }

    /// <summary>
    /// Gets the texture path if set
    /// </summary>
    public string? GetTexturePath() => _texturePath;

    /// <summary>
    /// Returns true if this is a solid color texture
    /// </summary>
    public bool IsColorTexture() => _isColorTexture;

    /// <summary>
    /// Gets the solid color values (for color textures)
    /// </summary>
    public (float r, float g, float b, float a) GetColorTextureValues() => (_colorR, _colorG, _colorB, _colorA);

    /// <summary>
    /// Gets the texture coordinates
    /// </summary>
    public (float left, float right, float top, float bottom) GetTexCoords() => (_texCoordLeft, _texCoordRight, _texCoordTop, _texCoordBottom);

    /// <summary>
    /// Gets the blend mode
    /// </summary>
    public string GetBlendMode() => _blendMode;
    // TextureBase:ClearTextureSlice()
    // TextureBase:GetAtlas() : atlas - Returns the atlas for the texture.
    // TextureBase:GetBlendMode() : blendMode - Returns the blend mode of the texture.
    // TextureBase:GetDesaturation() : desaturation - Returns the desaturation level of the texture.
    // TextureBase:GetHorizTile() : tiling - Returns true if the texture is tiling horizontally.
    // TextureBase:GetRotation() : radians, normalizedRotationPoint - Returns the rotation of the texture.
    // TextureBase:GetTexCoord() : ULx, ULy, LLx, LLy, URx, URy, LRx, LRy - Returns the texture space coordinates of the texture.
    // TextureBase:GetTexelSnappingBias() : bias - Returns the texel snapping bias for the texture.
    // TextureBase:GetTexture() : textureFile - Returns the FileID for the texture.
    // TextureBase:GetTextureFileID() : textureFile - Returns the FileID for the texture.
    // TextureBase:GetTextureFilePath() : textureFile - Returns the FileID for the texture.
    // TextureBase:GetTextureSliceMargins() : left, top, right, bottom
    // TextureBase:GetTextureSliceMode() : sliceMode
    // TextureBase:GetVertTile() : tiling - Returns true if the texture is tiling vertically.
    // TextureBase:GetVertexOffset(vertexIndex) : offsetX, offsetY - Returns a vertex offset for the texture.
    // TextureBase:IsBlockingLoadRequested() : blocking
    // TextureBase:IsDesaturated() : desaturated - Returns true if the texture is desaturated.
    // TextureBase:IsSnappingToPixelGrid() : snap - Returns true if the texture is snapping to the pixel grid.
    // TextureBase:SetAtlas(atlas [, useAtlasSize [, filterMode [, resetTexCoords]]]) - Sets the texture to an atlas.
    // TextureBase:SetBlendMode(blendMode) - Sets the blend mode of the texture.
    // TextureBase:SetBlockingLoadsRequested([blocking])
    
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_TextureBase_SetColorTexture
    /// TextureBase:SetColorTexture(colorR, colorG, colorB [, a]) - Sets the texture to a solid color.
    /// </summary>
    /// <param name="colorR"></param>
    /// <param name="colorG"></param>
    /// <param name="colorB"></param>
    /// <param name="colorA"></param>
    public void SetColorTexture(float colorR, float colorG, float colorB, float colorA = 1.0f)
    {
        _isColorTexture = true;
        _colorR = colorR;
        _colorG = colorG;
        _colorB = colorB;
        _colorA = colorA;
        _texturePath = null;
        _textureFileID = 0;
    }

    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_TextureBase_SetBlendMode
    /// TextureBase:SetBlendMode(blendMode) - Sets the blend mode of the texture.
    /// </summary>
    public void SetBlendMode(string blendMode)
    {
        _blendMode = blendMode;
    }
    
    // TextureBase:SetDesaturated([desaturated]) - Sets the texture to be desaturated.
    // TextureBase:SetDesaturation(desaturation) - Sets the desaturation level of the texture.
    // TextureBase:SetGradient(orientation, minColor, maxColor) - Sets a gradient color shading for the texture.
    // TextureBase:SetHorizTile([tiling]) - Sets whether the texture should tile horizontally.
    // TextureBase:SetMask(file) - Applies a mask to the texture.
    
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_TextureBase_SetRotation
    /// TextureBase:SetRotation(radians [, normalizedRotationPoint]) - Applies a rotation to the texture.
    /// </summary>
    /// <param name="radians"></param>
    /// <param name="normalizedRotationPoint"></param>
    public void SetRotation(float radians, string? normalizedRotationPoint)
    {
        _rotation = radians;
        _rotationPoint = normalizedRotationPoint;
    }
    
    // TextureBase:SetSnapToPixelGrid([snap]) - Sets the texture to snap to the pixel grid.
    
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_TextureBase_SetTexCoord
    /// TextureBase:SetTexCoord(left, right, top, bottom) - Sets the coordinates for cropping or transforming the texture.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="top"></param>
    /// <param name="bottom"></param>
    public void SetTexCoord(float left, float right, float top, float bottom)
    {
        _texCoordLeft = left;
        _texCoordRight = right;
        _texCoordTop = top;
        _texCoordBottom = bottom;
    }
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_TextureBase_SetTexCoord
    /// TextureBase:SetTexCoord(ULx, ULy, LLx, LLy, URx, URy, LRx, LRy)
    /// </summary>
    /// <param name="uLx"></param>
    /// <param name="uLy"></param>
    /// <param name="lLx"></param>
    /// <param name="lLy"></param>
    /// <param name="uRx"></param>
    /// <param name="uRy"></param>
    /// <param name="lRx"></param>
    /// <param name="lRy"></param>
    public void SetTexCoord(float uLx, float uLy, float lLx, float lLy, float uRx, float uRy, float lRx, float lRy)
    {
        // 8-parameter version for arbitrary quad mapping
        // For simple cases, convert to left/right/top/bottom
        _texCoordLeft = Math.Min(uLx, lLx);
        _texCoordRight = Math.Max(uRx, lRx);
        _texCoordTop = Math.Min(uLy, uRy);
        _texCoordBottom = Math.Max(lLy, lRy);
    }
    
    // TextureBase:SetTexelSnappingBias(bias) - Returns the texel snapping bias for the texture.
    
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_TextureBase_SetTexture
    /// TextureBase:SetTexture([textureAsset [, wrapModeHorizontal [, wrapModeVertical [, filterMode]]]]) - Sets the texture to an image.
    /// </summary>
    /// <param name="fileID"></param>
    /// <param name="wrapModeHorizontal"></param>
    /// <param name="wrapModeVertical"></param>
    /// <param name="filterMode"></param>
    public void SetTexture(int fileID, string? wrapModeHorizontal, string? wrapModeVertical, string? filterMode)
    {
        _textureFileID = fileID;
        _texturePath = null;
        _isColorTexture = false;
    }

    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_TextureBase_SetTexture
    /// TextureBase:SetTexture([textureAsset [, wrapModeHorizontal [, wrapModeVertical [, filterMode]]]]) - Sets the texture to an image.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="wrapModeHorizontal"></param>
    /// <param name="wrapModeVertical"></param>
    /// <param name="filterMode"></param>
    public void SetTexture(string? filePath, string? wrapModeHorizontal, string? wrapModeVertical, string? filterMode)
    {
        _texturePath = filePath;
        _textureFileID = 0;
        _isColorTexture = false;
    }
    
    // TextureBase:SetTextureSliceMargins(left, top, right, bottom)
    // TextureBase:SetTextureSliceMode(sliceMode)
    // TextureBase:SetVertTile([tiling]) - Sets whether the texture should tile vertically.
    
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_TextureBase_SetVertexOffset
    /// TextureBase:SetVertexOffset(vertexIndex, offsetX, offsetY) - Sets a vertex offset for the texture.
    /// </summary>
    /// <param name="vertexIndex"></param>
    /// <param name="offsetX"></param>
    /// <param name="offsetY"></param>
    public void SetVertexOffset(int vertexIndex, float offsetX, float offsetY)
    {
    }
}