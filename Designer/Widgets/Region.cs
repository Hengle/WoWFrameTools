using System.Runtime.InteropServices;
using LuaNET.Lua51;
using static LuaNET.Lua51.Lua;

namespace WoWFrameTools.Widgets;

public class Region : ScriptRegion
{
    protected string _drawLayer { get; set; }
    protected int _subLevel { get; set; }
    protected float _vertexColorR { get; set; } = 1.0f;
    protected float _vertexColorG { get; set; } = 1.0f;
    protected float _vertexColorB { get; set; } = 1.0f;
    protected float _vertexColorA { get; set; } = 1.0f;

    protected Region(string objectType, string? name, string? drawLayer, Region? parent, int subLevel = 0) : base(objectType, name, parent)
    {
        _drawLayer = drawLayer ?? "ARTWORK";
        _subLevel = subLevel;
    }
    
    // Region:GetAlpha() : alpha - Returns the region's opacity.
    // Region:GetDrawLayer() : layer, sublayer - Returns the layer in which the region is drawn.

    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_Region_GetScale
    /// Region:GetEffectiveScale() : effectiveScale - Returns the scale of the region after propagating from its parents.
    /// </summary>
    /// <returns></returns>
    public new float GetEffectiveScale()
    {
        return base.GetEffectiveScale();
    }
    
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_Region_GetScale
    /// Region:GetScale() : scale - Returns the scale of the region.
    /// </summary>
    /// <returns></returns>
    public new float GetScale()
    {
        return _scale;
    }
    
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_Region_GetVertexColor
    /// Region:GetVertexColor() : colorR, colorG, colorB, colorA - Returns the vertex color shading of the region.
    /// </summary>
    /// <returns></returns>
    public float[] GetVertexColor()
    {
        return [_vertexColorR, _vertexColorG, _vertexColorB, _vertexColorA];
    }

    /// <summary>
    /// Gets the draw layer for this region
    /// </summary>
    public string GetDrawLayer()
    {
        return _drawLayer;
    }

    /// <summary>
    /// Gets the sublevel within the draw layer
    /// </summary>
    public int GetSubLevel()
    {
        return _subLevel;
    }
    
    // Region:IsIgnoringParentAlpha() : isIgnoring - Returns true if the region is ignoring parent alpha.
    // Region:IsIgnoringParentScale() : isIgnoring - Returns true if the region is ignoring parent scale.
    // Region:IsObjectLoaded() : isLoaded - Returns true if the region is fully loaded.
    
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_Region_SetAlpha
    /// Region:SetAlpha(alpha) - Sets the opacity of the region.
    /// </summary>
    /// <param name="alpha"></param>
    public void SetAlpha(float alpha)
    {
        _alpha = alpha;
    }

    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_Region_GetAlpha
    /// Region:GetAlpha() : alpha - Returns the region's opacity.
    /// </summary>
    public float GetAlpha()
    {
        return _alpha;
    }
    
    // Region:SetDrawLayer(layer [, sublevel]) - Sets the layer in which the region is drawn.
    // Region:SetIgnoreParentAlpha(ignore) - Sets whether the region should ignore its parent's alpha.
    // Region:SetIgnoreParentScale(ignore) - Sets whether the region should ignore its parent's scale.
    
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_Region_SetScale
    /// Region:SetScale(scale) - Sets the size scaling of the region.
    /// </summary>
    /// <param name="scale"></param>
    public void SetScale(float scale)
    {
        _scale = scale;
        InvalidateLayout();
    }
    
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_Region_SetVertexColor
    /// Region:SetVertexColor(colorR, colorG, colorB [, a]) - Sets the vertex shading color of the region.
    /// </summary>
    /// <param name="colorR"></param>
    /// <param name="colorG"></param>
    /// <param name="colorB"></param>
    /// <param name="colorA"></param>
    public void SetVertexColor(float colorR, float colorG, float colorB, float? colorA)
    {
        _vertexColorR = colorR;
        _vertexColorG = colorG;
        _vertexColorB = colorB;
        _vertexColorA = colorA ?? 1.0f;
    }

    /// <summary>
    /// Sets the draw layer for this region
    /// </summary>
    public void SetDrawLayer(string layer, int? subLevel = null)
    {
        _drawLayer = layer;
        if (subLevel.HasValue)
        {
            _subLevel = subLevel.Value;
        }
    }
}