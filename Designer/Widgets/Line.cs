using System.Runtime.InteropServices;
using LuaNET.Lua51;
using static LuaNET.Lua51.Lua;

namespace WoWFrameTools.Widgets;

/// <summary>
/// https://warcraft.wiki.gg/wiki/UIOBJECT_Line
/// </summary>
public class Line : TextureBase
{
    // Line endpoints
    protected string? _startRelativePoint { get; set; }
    protected Region? _startRelativeTo { get; set; }
    protected float _startOffsetX { get; set; }
    protected float _startOffsetY { get; set; }

    protected string? _endRelativePoint { get; set; }
    protected Frame? _endRelativeTo { get; set; }
    protected float _endOffsetX { get; set; }
    protected float _endOffsetY { get; set; }

    protected float _thickness { get; set; } = 1.0f;

    public Line(string? name = null, string? drawLayer = null, string? templateName = null, int subLevel = 0, Frame? parent = null)
        : base("Line", name, drawLayer, templateName, subLevel, parent)
    {
    }

    /// <summary>
    /// Gets the start point data
    /// </summary>
    public (string? relativePoint, Region? relativeTo, float offsetX, float offsetY) GetStartPoint()
        => (_startRelativePoint, _startRelativeTo, _startOffsetX, _startOffsetY);

    /// <summary>
    /// Gets the end point data
    /// </summary>
    public (string? relativePoint, Frame? relativeTo, float offsetX, float offsetY) GetEndPoint()
        => (_endRelativePoint, _endRelativeTo, _endOffsetX, _endOffsetY);

    /// <summary>
    /// Gets the line thickness
    /// </summary>
    public float GetThickness() => _thickness;
    
    // Line:ClearAllPoints()
    // Line:GetEndPoint() : relativePoint, relativeTo, offsetX, offsetY
    // Line:GetHitRectThickness() : thickness
    // Line:GetStartPoint() : relativePoint, relativeTo, offsetX, offsetY
    // Line:GetThickness() : thickness
    
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_Line_SetEndPoint
    /// Line:SetEndPoint(relativePoint, relativeTo [, offsetX, offsetY])
    /// </summary>
    /// <param name="relativePoint"></param>
    /// <param name="relativeTo"></param>
    /// <param name="offsetX"></param>
    /// <param name="offsetY"></param>
    public void SetEndPoint(string? relativePoint, Frame? relativeTo, float offsetX, float offsetY)
    {
        _endRelativePoint = relativePoint;
        _endRelativeTo = relativeTo;
        _endOffsetX = offsetX;
        _endOffsetY = offsetY;
    }
    
    // Line:SetHitRectThickness(thickness)
    
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_Line_SetStartPoint
    /// Line:SetStartPoint(relativePoint, relativeTo [, offsetX, offsetY])
    /// </summary>
    /// <param name="relativePoint"></param>
    /// <param name="relativeTo"></param>
    /// <param name="offsetX"></param>
    /// <param name="offsetY"></param>
    public void SetStartPoint(string? relativePoint, Region? relativeTo, float offsetX, float offsetY)
    {
        _startRelativePoint = relativePoint;
        _startRelativeTo = relativeTo;
        _startOffsetX = offsetX;
        _startOffsetY = offsetY;
    }

    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_Line_SetThickness
    /// Line:SetThickness(thickness)
    /// </summary>
    /// <param name="thickness"></param>
    public void SetThickness(float thickness)
    {
        _thickness = thickness;
    }
}