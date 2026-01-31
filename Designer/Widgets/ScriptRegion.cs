using System.Runtime.InteropServices;
using LuaNET.Lua51;
using static LuaNET.Lua51.Lua;

namespace WoWFrameTools.Widgets
{
    /// <summary>
    /// https://warcraft.wiki.gg/wiki/UIOBJECT_ScriptRegion
    /// </summary>
    public class ScriptRegion : ScriptObject, IScriptRegionResizing
    {
        public Dictionary<string, Point> _points { get; set; }
        public float _width { get; set; }
        public float _height { get; set; }
        public float _alpha { get; set; }
        public float _scale { get; set; }
        private bool _shown;
        private bool _mouseEnabled;

        // Computed absolute position (top-left corner in screen space)
        public float _computedX { get; set; }
        public float _computedY { get; set; }
        public float _computedWidth { get; set; }
        public float _computedHeight { get; set; }
        public bool _layoutDirty { get; set; } = true;

        protected ScriptRegion(string objectType, string? name, ScriptRegion? parent) : base(objectType, name, parent)
        {
            _points = [];
            _alpha = 1.0f;
            _scale = 1.0f;
            _shown = true;
        }

        /// <summary>
        /// Marks this region and all children as needing layout recalculation
        /// </summary>
        public void InvalidateLayout()
        {
            _layoutDirty = true;
            foreach (var child in _children)
            {
                if (child is ScriptRegion region)
                {
                    region.InvalidateLayout();
                }
            }
        }

        /// <summary>
        /// Gets the effective alpha considering parent chain
        /// </summary>
        public float GetEffectiveAlpha()
        {
            float parentAlpha = (_parent as ScriptRegion)?.GetEffectiveAlpha() ?? 1.0f;
            return _alpha * parentAlpha;
        }

        /// <summary>
        /// Gets the effective scale considering parent chain
        /// </summary>
        public float GetEffectiveScale()
        {
            float parentScale = (_parent as ScriptRegion)?.GetEffectiveScale() ?? 1.0f;
            return _scale * parentScale;
        }

        /// <summary>
        /// Returns true if this region is shown (its own visibility state)
        /// </summary>
        public bool IsShown()
        {
            return _shown;
        }
        // ScriptRegion:CanChangeProtectedState() : canChange - Returns true if protected properties of the region can be changed by non-secure scripts.
        // ScriptRegion:CollapsesLayout() : collapsesLayout
        
        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegion_EnableMouse
        /// ScriptRegion:EnableMouse([enable]) - Sets whether the region should receive mouse input.
        /// </summary>
        /// <param name="enable"></param>
        public void EnableMouse(bool enable)
        {
            _mouseEnabled = enable;
        }
        
        // ScriptRegion:EnableMouseMotion([enable]) - Sets whether the region should receive mouse hover events.
        // ScriptRegion:EnableMouseWheel([enable]) - Sets whether the region should receive mouse wheel input.
        // ScriptRegion:GetBottom() : bottom #restrictedframe - Returns the offset to the bottom edge of the region.
        // ScriptRegion:GetCenter() : x, y #restrictedframe - Returns the offset to the center of the region.
        
        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegion_GetSize
        /// ScriptRegion:GetHeight([ignoreRect]) : height - Returns the height of the region.
        /// </summary>
        /// <returns></returns>
        public float GetHeight()
        {
            return _height;
        }
        
        // ScriptRegion:GetLeft() : left #restrictedframe - Returns the offset to the left edge of the region.
        // ScriptRegion:GetRect() : left, bottom, width, height #restrictedframe - Returns the coords and size of the region.
        // ScriptRegion:GetRight() : right #restrictedframe - Returns the offset to the right edge of the region.
        // ScriptRegion:GetScaledRect() : left, bottom, width, height - Returns the scaled coords and size of the region.
        // ScriptRegion:GetSize([ignoreRect]) : width, height - Returns the width and height of the region.
        // ScriptRegion:GetSourceLocation() : location - Returns the script name and line number where the region was created.
        // ScriptRegion:GetTop() : top #restrictedframe - Returns the offset to the top edge of the region.
        
        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegion_GetSize
        /// ScriptRegion:GetWidth([ignoreRect]) : width - Returns the width of the region.
        /// </summary>
        /// <returns></returns>
        public float GetWidth()
        {
            return _width;
        }

        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegion_GetLeft
        /// ScriptRegion:GetLeft() : left - Returns the offset to the left edge of the region.
        /// </summary>
        /// <returns>The left edge x coordinate</returns>
        public float GetLeft()
        {
            return _computedX;
        }

        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegion_GetRight
        /// ScriptRegion:GetRight() : right - Returns the offset to the right edge of the region.
        /// </summary>
        /// <returns>The right edge x coordinate</returns>
        public float GetRight()
        {
            return _computedX + _computedWidth;
        }

        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegion_GetTop
        /// ScriptRegion:GetTop() : top - Returns the offset to the top edge of the region.
        /// </summary>
        /// <returns>The top edge y coordinate</returns>
        public float GetTop()
        {
            return _computedY;
        }

        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegion_GetBottom
        /// ScriptRegion:GetBottom() : bottom - Returns the offset to the bottom edge of the region.
        /// </summary>
        /// <returns>The bottom edge y coordinate</returns>
        public float GetBottom()
        {
            return _computedY + _computedHeight;
        }

        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegion_GetCenter
        /// ScriptRegion:GetCenter() : x, y - Returns the offset to the center of the region.
        /// </summary>
        /// <returns>The center x and y coordinates</returns>
        public (float x, float y) GetCenter()
        {
            return (_computedX + _computedWidth / 2, _computedY + _computedHeight / 2);
        }

        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegion_GetRect
        /// ScriptRegion:GetRect() : left, bottom, width, height - Returns the coords and size of the region.
        /// </summary>
        /// <returns>The left, bottom, width, and height values</returns>
        public (float left, float bottom, float width, float height) GetRect()
        {
            return (_computedX, _computedY + _computedHeight, _computedWidth, _computedHeight);
        }

        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegion_GetNumPoints
        /// ScriptRegion:GetNumPoints() : numPoints - Returns the number of anchor points for the region.
        /// </summary>
        /// <returns>The number of anchor points</returns>
        public int GetNumPoints()
        {
            return _points.Count;
        }

        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegion_GetPoint
        /// ScriptRegion:GetPoint(pointNum) : point, relativeTo, relativePoint, offsetX, offsetY - Returns an anchor point for the region.
        /// </summary>
        /// <param name="pointNum">1-based index of the point</param>
        /// <returns>The anchor point info, or null if not found</returns>
        public (string point, ScriptRegion? relativeTo, string? relativePoint, float offsetX, float offsetY)? GetPoint(int pointNum)
        {
            if (pointNum < 1 || pointNum > _points.Count) return null;

            var keys = _points.Keys.ToList();
            var key = keys[pointNum - 1];
            var point = _points[key];

            return (point.point, point.relativeTo, point.relativePoint, point.offsetX, point.offsetY);
        }

        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegion_Hide
        /// ScriptRegion:Hide() #secureframe - Hides the region.
        /// </summary>
        [Attributes.SecureFrame]
        public void Hide()
        {
            if (_shown)
            {
                _shown = false;

                // Fire OnHide event
                OnHide();

                // TODO : Pause OnUpdate script
            }
        }
        
        // ScriptRegion:IsCollapsed() : isCollapsed
        // ScriptRegion:SetCollapsesLayout(collapsesLayout)
        // ScriptRegion:IsAnchoringRestricted() : isRestricted - Returns true if the region has cross-region anchoring restrictions applied.
        // ScriptRegion:IsDragging() : isDragging - Returns true if the region is being dragged.
        // ScriptRegion:IsMouseClickEnabled() : enabled - Returns true if the region can receive mouse clicks.
        // ScriptRegion:IsMouseEnabled() : enabled - Returns true if the region can receive mouse input.
        // ScriptRegion:IsMouseMotionEnabled() : enabled - Returns true if the region can receive mouse hover events.
        // ScriptRegion:IsMouseMotionFocus() : isMouseMotionFocus - Returns true if the mouse cursor is hovering over the region.
        // ScriptRegion:IsMouseOver([offsetTop [, offsetBottom [, offsetLeft [, offsetRight]]]]) : isMouseOver - Returns true if the mouse cursor is hovering over the region.
        // ScriptRegion:IsMouseWheelEnabled() : enabled - Returns true if the region can receive mouse wheel input.
        // ScriptRegion:IsProtected() : isProtected, isProtectedExplicitly - Returns whether the region is currently protected.
        // ScriptRegion:IsRectValid() : isValid - Returns true if the region can be positioned on the screen.
        // ScriptRegion:IsShown() : isShown - Returns true if the region should be shown; it depends on the parents if it's visible.
        
        // ScriptRegion:IsVisible() : isVisible - Returns true if the region and its parents are shown.
        public bool IsVisible()
        {
            if (!_shown) return false;
            if (_parent is ScriptRegion parentRegion)
            {
                return parentRegion.IsVisible();
            }
            return true;
        }
        
        // ScriptRegion:SetMouseClickEnabled([enabled]) - Sets whether the region should receive mouse clicks.
        // ScriptRegion:SetMouseMotionEnabled([enabled]) - Sets whether the region should receive mouse hover events.
        
        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegion_SetParent
        /// ScriptRegion:SetParent([parent]) - Sets the parent of the region.
        /// </summary>
        public void SetParent(ScriptRegion? parent)
        {
            if (_parent != null)
            {
                if (_parent is ScriptObject frame)
                {
                    frame._children.Remove(this as Frame);
                }
            }
            
            _parent = parent;
            
            if (parent != null)
            {
                if (parent is ScriptObject frame)
                {
                    frame._children.Add(this as Frame);
                }
            }
        }
        
        // ScriptRegion:SetPassThroughButtons([button1, ...]) #nocombat - Allows the region to propagate mouse clicks to underlying regions or the world frame.
        // ScriptRegion:SetPropagateMouseClicks(propagate)
        // ScriptRegion:SetPropagateMouseMotion(propagate)
        // ScriptRegion:SetShown([show]) #secureframe - Shows or hides the region.
        
        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegion_Show
        /// ScriptRegion:Show() #secureframe - Shows the region.
        /// </summary>
        [Attributes.SecureFrame]
        public void Show()
        {
            if (!_shown)
            {
                _shown = true;

                // Fire OnShow script
                OnShow();
                // TODO : Resume OnUpdate script
            }
        }
        
        // IScriptRegionResizing //
        // ScriptRegionResizing:AdjustPointsOffset(x, y) #secureframe - Adjusts the x and y offset of the region.
        
        // ScriptRegionResizing:ClearAllPoints() - Removes all anchor points from the region.
        
        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegionResizing_ClearAllPoints
        /// </summary>
        public void ClearAllPoints()
        {
            _points.Clear();
            InvalidateLayout();
        }
        
        // ScriptRegionResizing:ClearPoint(point) - Removes an anchor point from the region by name.
        // ScriptRegionResizing:ClearPointsOffset() #secureframe - Resets the x and y offset on the region to zero.
        // ScriptRegionResizing:GetNumPoints() : numPoints - Returns the number of anchor points for the region.
        // ScriptRegionResizing:GetPoint([anchorIndex [, resolveCollapsed]]) : point, relativeTo, relativePoint, offsetX, offsetY #restrictedframe - Returns an anchor point for the region.
        // ScriptRegionResizing:GetPointByName(point [, resolveCollapsed]) : point, relativeTo, relativePoint, offsetX, offsetY - Returns an anchor point by name for the region.
        
        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegionResizing_SetAllPoints
        /// ScriptRegionResizing:SetAllPoints(relativeTo [, doResize]) - Positions the region the same as another region.
        /// </summary>
        /// <param name="relativeTo"></param>
        /// <param name="doResize"></param>
        public void SetAllPoints(ScriptRegion? relativeTo = null, bool doResize = true)
        {
            ClearAllPoints();

            // Default to parent if no relativeTo specified
            var target = relativeTo ?? _parent as ScriptRegion;
            if (target == null) return;

            // Anchor top-left and bottom-right to fill the target frame
            SetPoint("TOPLEFT", target as Frame, "TOPLEFT", 0, 0);
            SetPoint("BOTTOMRIGHT", target as Frame, "BOTTOMRIGHT", 0, 0);

            if (doResize)
            {
                // When resizing, match the target's explicit size if it has one
                if (target._width > 0) _width = target._width;
                if (target._height > 0) _height = target._height;
            }

            InvalidateLayout();
        }
        
        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegionResizing_SetSize
        /// ScriptRegionResizing:SetHeight(height) - Sets the height of the region.
        /// </summary>
        /// <param name="height"></param>
        public void SetHeight(float height)
        {
            _height = height;
            InvalidateLayout();
        }
        
        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegionResizing_SetPoint
        /// ScriptRegionResizing:SetPoint(point [, relativeTo [, relativePoint]] [, offsetX, offsetY]) #anchorfamily - Sets an anchor point for the region.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="relativeTo"></param>
        /// <param name="relativePoint"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public void SetPoint(string point, Frame? relativeTo = null, string? relativePoint = null, float offsetX = 0, float offsetY = 0)
        {
            _points.Remove(point);
            _points.Add(point, new Point(point, relativeTo, relativePoint, offsetX, offsetY));
            InvalidateLayout();
        }
        
        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegionResizing_SetSize
        /// ScriptRegionResizing:SetSize(x, y) - Sets the width and height of the region.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetSize(float width, float height)
        {
            _width = width;
            _height = height;
            InvalidateLayout();
        }
        
        /// <summary>
        /// https://warcraft.wiki.gg/wiki/API_ScriptRegionResizing_SetSize
        /// ScriptRegionResizing:SetWidth(width) - Sets the width of the region.
        /// </summary>
        /// <param name="width"></param>
        public void SetWidth(float width)
        {
            _width = width;
            InvalidateLayout();
        }
    }
}