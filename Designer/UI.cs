using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using WoWFrameTools.Widgets;

namespace WoWFrameTools;

public class UI
{
    private Designer _designer;
    private GL _gl;
    private ImGuiController? _controller;
    private readonly List<Menu> _menus;
    private readonly MainMenu? _mainMenu;
    private string _layoutFilePath = "imgui_layout.ini";
    private ScriptObject? _selectedFrame;
    private bool _showDemoWindow = false;

    // Strata ordering (lowest to highest)
    private static readonly Dictionary<string, int> StrataOrder = new()
    {
        { "WORLD", 0 },
        { "BACKGROUND", 1 },
        { "LOW", 2 },
        { "MEDIUM", 3 },
        { "HIGH", 4 },
        { "DIALOG", 5 },
        { "FULLSCREEN", 6 },
        { "FULLSCREEN_DIALOG", 7 },
        { "TOOLTIP", 8 }
    };

    // Draw layer ordering within a frame (lowest to highest)
    private static readonly Dictionary<string, int> DrawLayerOrder = new()
    {
        { "BACKGROUND", 0 },
        { "BORDER", 1 },
        { "ARTWORK", 2 },
        { "OVERLAY", 3 },
        { "HIGHLIGHT", 4 }
    };

    public UI(Designer designer)
    {
        _designer = designer;
        _menus = [];
        _mainMenu = new MainMenu(this);
    }

    public void Load(GL gl, IWindow window, IInputContext inputContext)
    {
        _gl = gl;
        _controller = new ImGuiController(gl, window, inputContext);
        
        // Enable docking
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        
        SetImguiStyle();
        
        _mainMenu?.Load();
        
        LoadLayout();
    }
    
    private void SaveLayout()
    {
        // Save the current ImGui layout to a file
        ImGui.SaveIniSettingsToDisk(_layoutFilePath);
    }
    
    private void LoadLayout()
    {
        // Load the ImGui layout from a file
        ImGui.LoadIniSettingsFromDisk(_layoutFilePath);
    }

    public void AddMenu(Menu menu)
    {
        _menus.Add(menu);
    }
    
    public void Update(float deltaTime)
    {
        // Feed the input events to our ImGui controller, which passes them through to ImGui.
        _controller?.Update(deltaTime);
    }
    
    public void Render()
    {
        // Render UI components
        if (!_designer.addon.isLoaded)
        {
            // Show loading screen
            ImGui.SetWindowSize(new Vector2(_designer.window.Size.X, _designer.window.Size.Y));
            ImGui.Begin("Loading", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
            ImGui.Text("Loading...");
            ImGui.ProgressBar(_designer.addon.loadPercentage, new System.Numerics.Vector2(300, 0), "");
            ImGui.End();
        }
        
        // Docking
        CreateDockingSpaceAndMainMenu();
        
        // Rest of the UI
        if (_designer.addon.isLoaded)
        {
            if (_showDemoWindow)
                ImGui.ShowDemoWindow();
            
            RenderAddon();
            RenderHierarchy();
        }

        _controller?.Render();
    }

    private void RenderHierarchy()
    {
        ImGui.Begin("Hierarchy");
            
        // Temporarily adjust style to reduce vertical padding
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4.0f, 10.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4.0f, 0.0f));
            
        RenderFrameItem(API.UIObjects.UIParent);
            
        ImGui.PopStyleVar();

        ImGui.End();
    }
    
    private void RenderFrameItem(ScriptObject? frameScriptObject)
    {
        if (frameScriptObject == null)
            return;

        var frameName = frameScriptObject.GetName();
        if (string.IsNullOrEmpty(frameName))
            frameName = frameScriptObject.UserdataPtr.ToString();

        var frameType = frameScriptObject.GetType().ToString().Substring(22);

        int numChildren = 0;
        if (frameScriptObject is Widgets.Frame frame0)
        {
            numChildren = frame0.GetNumChildren();
        }

        var flags = ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.OpenOnArrow;// | ImGuiTreeNodeFlags.Framed;

        if (_selectedFrame == frameScriptObject)
        {
            flags |= ImGuiTreeNodeFlags.Selected;
        }

        if (numChildren == 0)
        {
            flags |= ImGuiTreeNodeFlags.Leaf;
        }
        
        // Create a unique ID for the node to avoid conflicts
        ImGui.PushID(frameScriptObject.GetHashCode());

        // Check if the tree node is expanded
        var opened = ImGui.TreeNodeEx(frameName, flags);

        // Handle clicks on the tree node, even if it's not expanded
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            _selectedFrame = frameScriptObject; // Set the selected frame
        }

        // Render children if the node is expanded
        if (opened)
        {
            if (frameScriptObject is Widgets.Frame frame)
            {
                foreach (var child in frame.GetChildren())
                {
                    RenderFrameItem(child);
                }
            }

            ImGui.TreePop();
        }

        ImGui.PopID();
    }
    
    private void RenderAddon()
    {
        ImGui.Begin("Addon");

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Get the current window's draw list
        var drawList = ImGui.GetWindowDrawList();

        // Get the content region (excluding title bar)
        var windowPos = ImGui.GetWindowPos();
        var cursorPos = ImGui.GetCursorScreenPos();
        var contentSize = ImGui.GetContentRegionAvail();

        // UIParent represents the full addon viewport
        if (API.UIObjects.UIParent != null)
        {
            // Set UIParent's size to match the content area
            API.UIObjects.UIParent._width = contentSize.X;
            API.UIObjects.UIParent._height = contentSize.Y;
            API.UIObjects.UIParent._computedX = cursorPos.X;
            API.UIObjects.UIParent._computedY = cursorPos.Y;
            API.UIObjects.UIParent._computedWidth = contentSize.X;
            API.UIObjects.UIParent._computedHeight = contentSize.Y;
            API.UIObjects.UIParent._layoutDirty = false;
        }

        // Collect all frames and sort by strata/level
        var sortedFrames = CollectAndSortFrames(API.UIObjects.UIParent);

        // Compute layout for all frames
        foreach (var frame in sortedFrames)
        {
            ComputeFrameLayout(frame, cursorPos);
        }

        // Draw frames in sorted order
        foreach (var frame in sortedFrames)
        {
            DrawFrame(frame, drawList);
        }

        ImGui.End();
    }

    /// <summary>
    /// Collects all frames recursively and sorts them by strata and level
    /// </summary>
    private List<Frame> CollectAndSortFrames(Frame? root)
    {
        var frames = new List<Frame>();
        CollectFramesRecursive(root, frames);

        // Sort by strata, then by level
        frames.Sort((a, b) =>
        {
            int strataA = GetStrataOrder(a._strata);
            int strataB = GetStrataOrder(b._strata);
            if (strataA != strataB) return strataA.CompareTo(strataB);
            return a._frameLevel.CompareTo(b._frameLevel);
        });

        return frames;
    }

    private void CollectFramesRecursive(Frame? frame, List<Frame> frames)
    {
        if (frame == null) return;

        frames.Add(frame);

        foreach (var child in frame.GetChildren())
        {
            if (child is Frame childFrame)
            {
                CollectFramesRecursive(childFrame, frames);
            }
        }
    }

    private int GetStrataOrder(string? strata)
    {
        if (strata == null) return StrataOrder["MEDIUM"];
        return StrataOrder.TryGetValue(strata.ToUpper(), out var order) ? order : StrataOrder["MEDIUM"];
    }

    private int GetDrawLayerOrder(string? layer)
    {
        if (layer == null) return DrawLayerOrder["ARTWORK"];
        return DrawLayerOrder.TryGetValue(layer.ToUpper(), out var order) ? order : DrawLayerOrder["ARTWORK"];
    }

    /// <summary>
    /// Computes the absolute position and size of a frame based on its anchors
    /// </summary>
    private void ComputeFrameLayout(Frame frame, Vector2 containerOffset)
    {
        if (!frame._layoutDirty && frame._computedWidth > 0) return;

        // Get parent's computed rect
        float parentX, parentY, parentWidth, parentHeight;
        if (frame._parent is ScriptRegion parentRegion)
        {
            parentX = parentRegion._computedX;
            parentY = parentRegion._computedY;
            parentWidth = parentRegion._computedWidth;
            parentHeight = parentRegion._computedHeight;
        }
        else
        {
            // No parent, use container
            parentX = containerOffset.X;
            parentY = containerOffset.Y;
            parentWidth = API.UIObjects.UIParent?._computedWidth ?? 800;
            parentHeight = API.UIObjects.UIParent?._computedHeight ?? 600;
        }

        // If no anchor points, position at parent's top-left
        if (frame._points.Count == 0)
        {
            frame._computedX = parentX;
            frame._computedY = parentY;
            frame._computedWidth = frame._width > 0 ? frame._width : parentWidth;
            frame._computedHeight = frame._height > 0 ? frame._height : parentHeight;
            frame._layoutDirty = false;
            return;
        }

        // Calculate position from anchors
        float? left = null, right = null, top = null, bottom = null;
        float? centerX = null, centerY = null;

        foreach (var kvp in frame._points)
        {
            var anchorPoint = kvp.Key.ToUpper();
            var point = kvp.Value;

            // Get the relativeTo frame (or parent if null)
            float refX, refY, refWidth, refHeight;
            if (point.relativeTo != null)
            {
                refX = point.relativeTo._computedX;
                refY = point.relativeTo._computedY;
                refWidth = point.relativeTo._computedWidth;
                refHeight = point.relativeTo._computedHeight;
            }
            else
            {
                refX = parentX;
                refY = parentY;
                refWidth = parentWidth;
                refHeight = parentHeight;
            }

            // Get the position of the relative point on the reference frame
            var relPoint = (point.relativePoint ?? anchorPoint).ToUpper();
            var (refPointX, refPointY) = GetAnchorPosition(refX, refY, refWidth, refHeight, relPoint);

            // Apply offset
            float targetX = refPointX + point.offsetX;
            float targetY = refPointY + point.offsetY;

            // Store the constraint based on anchor point type
            switch (anchorPoint)
            {
                case "TOPLEFT":
                    left = targetX;
                    top = targetY;
                    break;
                case "TOP":
                    centerX = targetX;
                    top = targetY;
                    break;
                case "TOPRIGHT":
                    right = targetX;
                    top = targetY;
                    break;
                case "LEFT":
                    left = targetX;
                    centerY = targetY;
                    break;
                case "CENTER":
                    centerX = targetX;
                    centerY = targetY;
                    break;
                case "RIGHT":
                    right = targetX;
                    centerY = targetY;
                    break;
                case "BOTTOMLEFT":
                    left = targetX;
                    bottom = targetY;
                    break;
                case "BOTTOM":
                    centerX = targetX;
                    bottom = targetY;
                    break;
                case "BOTTOMRIGHT":
                    right = targetX;
                    bottom = targetY;
                    break;
            }
        }

        // Resolve the final position and size
        float frameWidth = frame._width;
        float frameHeight = frame._height;

        // If we have both left and right, compute width from them
        if (left.HasValue && right.HasValue)
        {
            frameWidth = right.Value - left.Value;
        }
        // If we have both top and bottom, compute height from them
        if (top.HasValue && bottom.HasValue)
        {
            frameHeight = bottom.Value - top.Value;
        }

        // Use explicit size if anchors don't define it
        if (frameWidth <= 0) frameWidth = frame._width > 0 ? frame._width : 100;
        if (frameHeight <= 0) frameHeight = frame._height > 0 ? frame._height : 100;

        // Compute final X position
        float finalX;
        if (left.HasValue)
        {
            finalX = left.Value;
        }
        else if (right.HasValue)
        {
            finalX = right.Value - frameWidth;
        }
        else if (centerX.HasValue)
        {
            finalX = centerX.Value - frameWidth / 2;
        }
        else
        {
            finalX = parentX;
        }

        // Compute final Y position
        float finalY;
        if (top.HasValue)
        {
            finalY = top.Value;
        }
        else if (bottom.HasValue)
        {
            finalY = bottom.Value - frameHeight;
        }
        else if (centerY.HasValue)
        {
            finalY = centerY.Value - frameHeight / 2;
        }
        else
        {
            finalY = parentY;
        }

        frame._computedX = finalX;
        frame._computedY = finalY;
        frame._computedWidth = frameWidth;
        frame._computedHeight = frameHeight;
        frame._layoutDirty = false;
    }

    /// <summary>
    /// Gets the absolute position of an anchor point on a frame
    /// </summary>
    private (float x, float y) GetAnchorPosition(float frameX, float frameY, float frameWidth, float frameHeight, string anchorPoint)
    {
        return anchorPoint switch
        {
            "TOPLEFT" => (frameX, frameY),
            "TOP" => (frameX + frameWidth / 2, frameY),
            "TOPRIGHT" => (frameX + frameWidth, frameY),
            "LEFT" => (frameX, frameY + frameHeight / 2),
            "CENTER" => (frameX + frameWidth / 2, frameY + frameHeight / 2),
            "RIGHT" => (frameX + frameWidth, frameY + frameHeight / 2),
            "BOTTOMLEFT" => (frameX, frameY + frameHeight),
            "BOTTOM" => (frameX + frameWidth / 2, frameY + frameHeight),
            "BOTTOMRIGHT" => (frameX + frameWidth, frameY + frameHeight),
            _ => (frameX, frameY)
        };
    }

    /// <summary>
    /// Draws a frame and its regions
    /// </summary>
    private void DrawFrame(Frame frame, ImDrawListPtr drawList)
    {
        // Skip invisible frames
        if (!frame.IsVisible()) return;

        float effectiveAlpha = frame.GetEffectiveAlpha();
        if (effectiveAlpha <= 0) return;

        var rectStart = new Vector2(frame._computedX, frame._computedY);
        var rectEnd = new Vector2(frame._computedX + frame._computedWidth, frame._computedY + frame._computedHeight);

        // Draw backdrop if set
        var backdrop = frame.GetBackdrop();
        if (backdrop != null)
        {
            // Draw backdrop background color
            var (bgR, bgG, bgB, bgA) = frame.GetBackdropColor();
            var bgColor = ImGui.GetColorU32(new Vector4(bgR, bgG, bgB, bgA * effectiveAlpha));

            // Apply insets
            var insetStart = new Vector2(
                rectStart.X + backdrop.insetLeft,
                rectStart.Y + backdrop.insetTop);
            var insetEnd = new Vector2(
                rectEnd.X - backdrop.insetRight,
                rectEnd.Y - backdrop.insetBottom);

            drawList.AddRectFilled(insetStart, insetEnd, bgColor);

            // Draw backdrop border if edgeFile is set
            if (!string.IsNullOrEmpty(backdrop.edgeFile) && backdrop.edgeSize > 0)
            {
                var (borderR, borderG, borderB, borderA) = frame.GetBackdropBorderColor();
                var borderColor = ImGui.GetColorU32(new Vector4(borderR, borderG, borderB, borderA * effectiveAlpha));
                drawList.AddRect(rectStart, rectEnd, borderColor, 0f, ImDrawFlags.None, backdrop.edgeSize > 0 ? backdrop.edgeSize : 1);
            }
        }

        // Draw outline for selected frame
        if (_selectedFrame == frame)
        {
            var selectedColor = ImGui.GetColorU32(new Vector4(1f, 1f, 0f, 0.8f * effectiveAlpha));
            drawList.AddRect(rectStart, rectEnd, selectedColor, 0f, ImDrawFlags.None, 2f);
        }

        // Collect and sort regions by draw layer
        var regions = new List<(Region region, int layerOrder, int subLevel)>();

        foreach (var texture in frame._textures)
        {
            if (texture.IsVisible())
            {
                regions.Add((texture, GetDrawLayerOrder(texture.GetDrawLayer()), texture.GetSubLevel()));
            }
        }

        foreach (var fontString in frame._fontStrings)
        {
            if (fontString.IsVisible())
            {
                regions.Add((fontString, GetDrawLayerOrder(fontString.GetDrawLayer()), fontString.GetSubLevel()));
            }
        }

        foreach (var line in frame._lines)
        {
            if (line.IsVisible())
            {
                regions.Add((line, GetDrawLayerOrder(line.GetDrawLayer()), line.GetSubLevel()));
            }
        }

        // Sort by layer order, then sublevel
        regions.Sort((a, b) =>
        {
            if (a.layerOrder != b.layerOrder) return a.layerOrder.CompareTo(b.layerOrder);
            return a.subLevel.CompareTo(b.subLevel);
        });

        // Draw each region
        foreach (var (region, _, _) in regions)
        {
            // Compute region layout relative to parent frame
            ComputeRegionLayout(region, frame);

            if (region is Widgets.Texture texture)
            {
                DrawTexture(texture, drawList, effectiveAlpha);
            }
            else if (region is FontString fontString)
            {
                DrawFontString(fontString, drawList, effectiveAlpha);
            }
            else if (region is Line line)
            {
                DrawLine(line, drawList, effectiveAlpha);
            }
        }
    }

    /// <summary>
    /// Computes layout for a region (texture, fontstring, line) within its parent frame
    /// </summary>
    private void ComputeRegionLayout(Region region, Frame parentFrame)
    {
        if (!region._layoutDirty && region._computedWidth > 0) return;

        float parentX = parentFrame._computedX;
        float parentY = parentFrame._computedY;
        float parentWidth = parentFrame._computedWidth;
        float parentHeight = parentFrame._computedHeight;

        // If no anchor points, fill parent
        if (region._points.Count == 0)
        {
            region._computedX = parentX;
            region._computedY = parentY;
            region._computedWidth = region._width > 0 ? region._width : parentWidth;
            region._computedHeight = region._height > 0 ? region._height : parentHeight;
            region._layoutDirty = false;
            return;
        }

        // Same logic as frame layout
        float? left = null, right = null, top = null, bottom = null;
        float? centerX = null, centerY = null;

        foreach (var kvp in region._points)
        {
            var anchorPoint = kvp.Key.ToUpper();
            var point = kvp.Value;

            float refX, refY, refWidth, refHeight;
            if (point.relativeTo != null)
            {
                refX = point.relativeTo._computedX;
                refY = point.relativeTo._computedY;
                refWidth = point.relativeTo._computedWidth;
                refHeight = point.relativeTo._computedHeight;
            }
            else
            {
                refX = parentX;
                refY = parentY;
                refWidth = parentWidth;
                refHeight = parentHeight;
            }

            var relPoint = (point.relativePoint ?? anchorPoint).ToUpper();
            var (refPointX, refPointY) = GetAnchorPosition(refX, refY, refWidth, refHeight, relPoint);

            float targetX = refPointX + point.offsetX;
            float targetY = refPointY + point.offsetY;

            switch (anchorPoint)
            {
                case "TOPLEFT": left = targetX; top = targetY; break;
                case "TOP": centerX = targetX; top = targetY; break;
                case "TOPRIGHT": right = targetX; top = targetY; break;
                case "LEFT": left = targetX; centerY = targetY; break;
                case "CENTER": centerX = targetX; centerY = targetY; break;
                case "RIGHT": right = targetX; centerY = targetY; break;
                case "BOTTOMLEFT": left = targetX; bottom = targetY; break;
                case "BOTTOM": centerX = targetX; bottom = targetY; break;
                case "BOTTOMRIGHT": right = targetX; bottom = targetY; break;
            }
        }

        float regionWidth = region._width;
        float regionHeight = region._height;

        if (left.HasValue && right.HasValue) regionWidth = right.Value - left.Value;
        if (top.HasValue && bottom.HasValue) regionHeight = bottom.Value - top.Value;

        if (regionWidth <= 0) regionWidth = region._width > 0 ? region._width : parentWidth;
        if (regionHeight <= 0) regionHeight = region._height > 0 ? region._height : parentHeight;

        float finalX = left ?? (right.HasValue ? right.Value - regionWidth : (centerX.HasValue ? centerX.Value - regionWidth / 2 : parentX));
        float finalY = top ?? (bottom.HasValue ? bottom.Value - regionHeight : (centerY.HasValue ? centerY.Value - regionHeight / 2 : parentY));

        region._computedX = finalX;
        region._computedY = finalY;
        region._computedWidth = regionWidth;
        region._computedHeight = regionHeight;
        region._layoutDirty = false;
    }

    /// <summary>
    /// Draws a texture region
    /// </summary>
    private void DrawTexture(Widgets.Texture texture, ImDrawListPtr drawList, float parentAlpha)
    {
        float alpha = texture.GetEffectiveAlpha() * parentAlpha;
        if (alpha <= 0) return;

        var rectStart = new Vector2(texture._computedX, texture._computedY);
        var rectEnd = new Vector2(texture._computedX + texture._computedWidth, texture._computedY + texture._computedHeight);

        if (texture.IsColorTexture())
        {
            // Solid color texture
            var (r, g, b, a) = texture.GetColorTextureValues();
            var color = ImGui.GetColorU32(new Vector4(r, g, b, a * alpha));
            drawList.AddRectFilled(rectStart, rectEnd, color);
        }
        else
        {
            // TODO: Image texture rendering
            // For now, draw a placeholder with vertex color
            var vertexColor = texture.GetVertexColor();
            var color = ImGui.GetColorU32(new Vector4(vertexColor[0], vertexColor[1], vertexColor[2], vertexColor[3] * alpha));
            drawList.AddRectFilled(rectStart, rectEnd, color);
        }
    }

    /// <summary>
    /// Draws a font string region
    /// </summary>
    private void DrawFontString(FontString fontString, ImDrawListPtr drawList, float parentAlpha)
    {
        var text = fontString.GetText();
        if (string.IsNullOrEmpty(text)) return;

        float alpha = fontString.GetEffectiveAlpha() * parentAlpha;
        if (alpha <= 0) return;

        var (r, g, b, a) = fontString.GetTextColor();
        var color = ImGui.GetColorU32(new Vector4(r, g, b, a * alpha));

        // Calculate text position based on justification
        var (_, fontSize, _) = fontString.GetFont();
        var justifyH = fontString.GetJustifyH();
        var justifyV = fontString.GetJustifyV();

        float textX = fontString._computedX;
        float textY = fontString._computedY;

        // Horizontal justification
        var textSize = ImGui.CalcTextSize(text);
        switch (justifyH.ToUpper())
        {
            case "CENTER":
                textX = fontString._computedX + (fontString._computedWidth - textSize.X) / 2;
                break;
            case "RIGHT":
                textX = fontString._computedX + fontString._computedWidth - textSize.X;
                break;
            // LEFT is default
        }

        // Vertical justification
        switch (justifyV.ToUpper())
        {
            case "MIDDLE":
                textY = fontString._computedY + (fontString._computedHeight - textSize.Y) / 2;
                break;
            case "BOTTOM":
                textY = fontString._computedY + fontString._computedHeight - textSize.Y;
                break;
            // TOP is default
        }

        drawList.AddText(new Vector2(textX, textY), color, text);
    }

    /// <summary>
    /// Draws a line region
    /// </summary>
    private void DrawLine(Line line, ImDrawListPtr drawList, float parentAlpha)
    {
        float alpha = line.GetEffectiveAlpha() * parentAlpha;
        if (alpha <= 0) return;

        var vertexColor = line.GetVertexColor();
        var color = ImGui.GetColorU32(new Vector4(vertexColor[0], vertexColor[1], vertexColor[2], vertexColor[3] * alpha));
        var thickness = line.GetThickness();

        // Get start and end point data
        var (startRelPoint, startRelTo, startOffX, startOffY) = line.GetStartPoint();
        var (endRelPoint, endRelTo, endOffX, endOffY) = line.GetEndPoint();

        // Resolve start position
        Vector2 startPos;
        if (startRelPoint != null && startRelTo != null)
        {
            // Get the anchor position on the relativeTo frame
            var (anchorX, anchorY) = GetAnchorPosition(
                startRelTo._computedX, startRelTo._computedY,
                startRelTo._computedWidth, startRelTo._computedHeight,
                startRelPoint.ToUpper());
            startPos = new Vector2(anchorX + startOffX, anchorY + startOffY);
        }
        else if (startRelPoint != null && line._parent is ScriptRegion parentRegion)
        {
            // Use parent frame as default relativeTo
            var (anchorX, anchorY) = GetAnchorPosition(
                parentRegion._computedX, parentRegion._computedY,
                parentRegion._computedWidth, parentRegion._computedHeight,
                startRelPoint.ToUpper());
            startPos = new Vector2(anchorX + startOffX, anchorY + startOffY);
        }
        else
        {
            // Fallback to computed position
            startPos = new Vector2(line._computedX, line._computedY);
        }

        // Resolve end position
        Vector2 endPos;
        if (endRelPoint != null && endRelTo != null)
        {
            // Get the anchor position on the relativeTo frame
            var (anchorX, anchorY) = GetAnchorPosition(
                endRelTo._computedX, endRelTo._computedY,
                endRelTo._computedWidth, endRelTo._computedHeight,
                endRelPoint.ToUpper());
            endPos = new Vector2(anchorX + endOffX, anchorY + endOffY);
        }
        else if (endRelPoint != null && line._parent is ScriptRegion parentRegion)
        {
            // Use parent frame as default relativeTo
            var (anchorX, anchorY) = GetAnchorPosition(
                parentRegion._computedX, parentRegion._computedY,
                parentRegion._computedWidth, parentRegion._computedHeight,
                endRelPoint.ToUpper());
            endPos = new Vector2(anchorX + endOffX, anchorY + endOffY);
        }
        else
        {
            // Fallback to bottom-right of computed region
            endPos = new Vector2(line._computedX + line._computedWidth, line._computedY + line._computedHeight);
        }

        drawList.AddLine(startPos, endPos, color, thickness);
    }
    
    private void CreateDockingSpaceAndMainMenu()
    {
        // Check if Docking is enabled
        var io = ImGui.GetIO();
        if ((io.ConfigFlags & ImGuiConfigFlags.DockingEnable) != 0)
        {
            var viewport = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(viewport.Pos);
            ImGui.SetNextWindowSize(viewport.Size);
            ImGui.SetNextWindowViewport(viewport.ID);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);

            ImGui.Begin("DockSpace", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |
                                     ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
                                     ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoBringToFrontOnFocus |
                                     ImGuiWindowFlags.NoNavFocus);

            foreach (var menu in _menus)
            {
                menu.Render();
            }
            
            var dockspaceId = ImGui.GetID("MainDockSpace");
            ImGui.DockSpace(dockspaceId, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

            ImGui.End();
            ImGui.PopStyleVar(2);
        }
    }
    
    private void SetImguiStyle()
    {
        // Access the ImGui style
        ImGui.StyleColorsClassic();
        var style = ImGui.GetStyle();
        
        // Change the spacing between items
        style.ItemSpacing = new System.Numerics.Vector2(1.0f, 5.0f);
        style.ItemInnerSpacing = new System.Numerics.Vector2(1.0f, 1.0f);
        style.FramePadding = new System.Numerics.Vector2(10.0f, 6.0f);
        style.ScrollbarSize = 20.0f;
        style.ScrollbarRounding = 2.0f;
        style.WindowRounding = 3.0f;
        style.TabRounding = 2.0f;
        style.GrabRounding = 2.0f;
        style.FrameRounding = 2.0f;
        
        var colors = style.Colors;

        // Base Colors (Brighter Background)
        colors[(int)ImGuiCol.WindowBg] = new System.Numerics.Vector4(0.18f, 0.18f, 0.18f, 1.00f);
        colors[(int)ImGuiCol.ChildBg] = new System.Numerics.Vector4(0.22f, 0.22f, 0.22f, 1.00f);
        colors[(int)ImGuiCol.FrameBg] = new System.Numerics.Vector4(0.28f, 0.28f, 0.28f, 1.00f);
        colors[(int)ImGuiCol.FrameBgHovered] = new System.Numerics.Vector4(0.35f, 0.30f, 0.30f, 1.00f);
        colors[(int)ImGuiCol.FrameBgActive] = new System.Numerics.Vector4(0.40f, 0.35f, 0.35f, 1.00f);
        colors[(int)ImGuiCol.TitleBg] = new System.Numerics.Vector4(0.15f, 0.15f, 0.15f, 1.00f);
        colors[(int)ImGuiCol.TitleBgActive] = new System.Numerics.Vector4(0.20f, 0.18f, 0.18f, 1.00f);
        colors[(int)ImGuiCol.TitleBgCollapsed] = new System.Numerics.Vector4(0.12f, 0.12f, 0.12f, 1.00f);
        colors[(int)ImGuiCol.MenuBarBg] = new System.Numerics.Vector4(0.25f, 0.25f, 0.25f, 1.00f);

        // Accent Colors (Brighter Buttons)
        colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0.60f, 0.20f, 0.20f, 1.00f); // Brighter Red
        colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(0.75f, 0.25f, 0.25f, 1.00f); // Even Brighter on Hover
        colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0.85f, 0.30f, 0.30f, 1.00f); // Brightest when Active

        colors[(int)ImGuiCol.Header] = new System.Numerics.Vector4(0.50f, 0.15f, 0.15f, 1.00f);
        colors[(int)ImGuiCol.HeaderHovered] = new System.Numerics.Vector4(0.65f, 0.20f, 0.20f, 1.00f);
        colors[(int)ImGuiCol.HeaderActive] = new System.Numerics.Vector4(0.75f, 0.25f, 0.25f, 1.00f);

        colors[(int)ImGuiCol.Tab] = new System.Numerics.Vector4(0.40f, 0.20f, 0.20f, 1.00f);
        colors[(int)ImGuiCol.TabHovered] = new System.Numerics.Vector4(0.60f, 0.25f, 0.25f, 1.00f);
        colors[(int)ImGuiCol.TabActive] = new System.Numerics.Vector4(0.75f, 0.30f, 0.30f, 1.00f);
        colors[(int)ImGuiCol.TabUnfocused] = new System.Numerics.Vector4(0.35f, 0.18f, 0.18f, 1.00f);
        colors[(int)ImGuiCol.TabUnfocusedActive] = new System.Numerics.Vector4(0.50f, 0.22f, 0.22f, 1.00f);

        colors[(int)ImGuiCol.CheckMark] = new System.Numerics.Vector4(0.90f, 0.30f, 0.30f, 1.00f);
        colors[(int)ImGuiCol.SliderGrab] = new System.Numerics.Vector4(0.80f, 0.25f, 0.25f, 1.00f);
        colors[(int)ImGuiCol.SliderGrabActive] = new System.Numerics.Vector4(0.90f, 0.30f, 0.30f, 1.00f);

        colors[(int)ImGuiCol.Separator] = new System.Numerics.Vector4(0.35f, 0.18f, 0.18f, 1.00f);
        colors[(int)ImGuiCol.SeparatorHovered] = new System.Numerics.Vector4(0.50f, 0.22f, 0.22f, 1.00f);
        colors[(int)ImGuiCol.SeparatorActive] = new System.Numerics.Vector4(0.65f, 0.28f, 0.28f, 1.00f);

        // Scrollbar
        colors[(int)ImGuiCol.ScrollbarBg] = new System.Numerics.Vector4(0.20f, 0.20f, 0.20f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarGrab] = new System.Numerics.Vector4(0.45f, 0.20f, 0.20f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = new System.Numerics.Vector4(0.60f, 0.25f, 0.25f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarGrabActive] = new System.Numerics.Vector4(0.75f, 0.30f, 0.30f, 1.00f);
    }
    
    public void Dispose()
    {
        SaveLayout();
        _controller?.Dispose();
    }
}
