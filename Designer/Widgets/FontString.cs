using System.Runtime.InteropServices;
using LuaNET.Lua51;
using static LuaNET.Lua51.Lua;

namespace WoWFrameTools.Widgets;

/// <summary>
/// https://warcraft.wiki.gg/wiki/UIOBJECT_FontString
/// </summary>
public class FontString : Region, IFontInstance
{
    // Text content
    protected string? _text { get; set; }

    // Font properties
    protected string? _fontPath { get; set; }
    protected float _fontSize { get; set; } = 12f;
    protected string? _fontFlags { get; set; }

    // Text color
    protected float _textColorR { get; set; } = 1.0f;
    protected float _textColorG { get; set; } = 1.0f;
    protected float _textColorB { get; set; } = 1.0f;
    protected float _textColorA { get; set; } = 1.0f;

    // Shadow
    protected float _shadowColorR { get; set; }
    protected float _shadowColorG { get; set; }
    protected float _shadowColorB { get; set; }
    protected float _shadowColorA { get; set; }
    protected float _shadowOffsetX { get; set; }
    protected float _shadowOffsetY { get; set; }

    // Justification
    protected string _justifyH { get; set; } = "CENTER";
    protected string _justifyV { get; set; } = "MIDDLE";

    // Max lines
    protected int _maxLines { get; set; }

    public FontString(string? name = null, string? drawLayer = null, string? templateName = null, Frame? parent = null)
        : base("FontString", name, drawLayer, parent)
    {
    }

    /// <summary>
    /// Gets the text content
    /// </summary>
    public string? GetText() => _text;

    /// <summary>
    /// Gets the text color
    /// </summary>
    public (float r, float g, float b, float a) GetTextColor() => (_textColorR, _textColorG, _textColorB, _textColorA);

    /// <summary>
    /// Gets the horizontal justification
    /// </summary>
    public string GetJustifyH() => _justifyH;

    /// <summary>
    /// Gets the vertical justification
    /// </summary>
    public string GetJustifyV() => _justifyV;

    /// <summary>
    /// Gets font properties
    /// </summary>
    public (string? path, float size, string? flags) GetFont() => (_fontPath, _fontSize, _fontFlags);
    
    // FontString:CalculateScreenAreaFromCharacterSpan(leftIndex, rightIndex) : areas
    // FontString:CanNonSpaceWrap() : wrap
    // FontString:CanWordWrap() : wrap
    // FontString:FindCharacterIndexAtCoordinate(x, y) : characterIndex, inside
    // FontString:GetFieldSize() : fieldSize
    // FontString:GetLineHeight() : lineHeight
    // FontString:GetMaxLines() : maxLines
    // FontString:GetNumLines() : numLines
    // FontString:GetRotation() : radians
    // FontString:GetStringHeight() : height
    
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_FontString_GetStringWidth
    /// FontString:GetStringWidth() : width
    /// </summary>
    /// <returns></returns>
    public float GetStringWidth()
    {
        return 1.0f;
    }
    
    // FontString:GetText() : text
    // FontString:GetTextScale() : textScale
    // FontString:GetUnboundedStringWidth() : width
    // FontString:GetWrappedWidth() : width
    // FontString:IsTruncated() : isTruncated
    // FontString:SetAlphaGradient(start, length) : isWithinText
    // FontString:SetFixedColor(fixedColor)
    // FontString:SetFormattedText(text)
    // FontString:SetMaxLines(maxLines)
    // FontString:SetNonSpaceWrap(wrap)
    // FontString:SetRotation(radians)
    
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_FontString_SetText
    /// FontString:SetText([text])
    /// </summary>
    /// <param name="text"></param>
    public void SetText(string? text)
    {
        _text = text;
    }
    
    // FontString:SetTextHeight(height)
    // FontString:SetTextScale(textScale)
    // FontString:SetTextToFit([text])
    // FontString:SetWordWrap(wrap)
    
    // IFontInstance //
    // FontInstance:GetFont() : fontFile, height, flags - Returns the font path, height, and flags.
    // FontInstance:GetFontObject() : font - Returns the "parent" font object.
    // FontInstance:GetIndentedWordWrap() : wordWrap - Returns the indentation when text wraps beyond the first line.
    // FontInstance:GetJustifyH() : justifyH - Returns the horizontal text justification.
    // FontInstance:GetJustifyV() : justifyV - Returns the vertical text justification.
    // FontInstance:GetShadowColor() : colorR, colorG, colorB, colorA - Sets the text shadow color.
    // FontInstance:GetShadowOffset() : offsetX, offsetY - Returns the text shadow offset.
    // FontInstance:GetSpacing() : spacing - Returns the line spacing.
    // FontInstance:GetTextColor() : colorR, colorG, colorB, colorA - Returns the default text color.

    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_FontInstance_SetFont
    /// FontInstance:SetFont(fontFile, height, flags) - Sets the basic font properties.
    /// </summary>
    /// <param name="fontFile"></param>
    /// <param name="height"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public bool SetFont(string? fontFile, float height, string? flags)
    {
        _fontPath = fontFile;
        _fontSize = height;
        _fontFlags = flags;
        return true;
    }
    
    // FontInstance:SetFontObject(font) - Sets the "parent" font object from which this object inherits properties.
    // FontInstance:SetIndentedWordWrap(wordWrap) - Sets the indentation when text wraps beyond the first line.

    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_FontInstance_SetJustifyH
    /// FontInstance:SetJustifyH(justifyH) - Sets the horizontal text justification
    /// </summary>
    /// <param name="justify"></param>
    public void SetJustifyH(string justify)
    {
        _justifyH = justify;
    }

    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_FontInstance_SetJustifyV
    /// FontInstance:SetJustifyV(justifyV) - Sets the vertical text justification.
    /// </summary>
    /// <param name="justify"></param>
    public void SetJustifyV(string justify)
    {
        _justifyV = justify;
    }

    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_FontInstance_SetShadowColor
    /// FontInstance:SetShadowColor(colorR, colorG, colorB [, a]) - Sets the text shadow color.
    /// </summary>
    public void SetShadowColor(float colorR, float colorG, float colorB, float colorA = 1.0f)
    {
        _shadowColorR = colorR;
        _shadowColorG = colorG;
        _shadowColorB = colorB;
        _shadowColorA = colorA;
    }

    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_FontInstance_SetShadowOffset
    /// FontInstance:SetShadowOffset(offsetX, offsetY) - Sets the text shadow offset.
    /// </summary>
    public void SetShadowOffset(float offsetX, float offsetY)
    {
        _shadowOffsetX = offsetX;
        _shadowOffsetY = offsetY;
    }

    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_FontString_SetMaxLines
    /// FontString:SetMaxLines(maxLines) - Sets the maximum number of lines.
    /// </summary>
    public void SetMaxLines(int maxLines)
    {
        _maxLines = maxLines;
    }
    
    // FontInstance:SetShadowColor(colorR, colorG, colorB [, a]) - Returns the color of text shadow.
    // FontInstance:SetShadowOffset(offsetX, offsetY) - Sets the text shadow offset.
    // FontInstance:SetSpacing(spacing) - Sets the spacing between lines of text in the object.
    
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/API_FontInstance_SetTextColor
    /// FontInstance:SetTextColor(colorR, colorG, colorB [, a]) - Sets the default text color.
    /// </summary>
    /// <param name="colorR"></param>
    /// <param name="colorG"></param>
    /// <param name="colorB"></param>
    /// <param name="colorA"></param>
    public void SetTextColor(float colorR, float colorG, float colorB, float colorA = 1.0f)
    {
        _textColorR = colorR;
        _textColorG = colorG;
        _textColorB = colorB;
        _textColorA = colorA;
    }
    
    public override string ToString()
    {
        return $"FontString: {GetName() ?? "nil"} - {_drawLayer ?? "nil"}";
    }
}