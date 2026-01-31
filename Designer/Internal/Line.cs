using System.Runtime.InteropServices;
using LuaNET.Lua51;
using static LuaNET.Lua51.Lua;

namespace WoWFrameTools.Internal;

public static class Line
{
    public static int internal_SetEndPoint(lua_State L)
    {
        var line = GetThis(L, 1);

        var relativePoint = luaL_checkstring(L, 2);
        Widgets.Frame? relativeTo = null;
        float offsetX = 0;
        float offsetY = 0;

        int argIndex = 3;
        // Check if arg 3 is a frame reference or a number
        if (lua_isnumber(L, argIndex) == 0 && lua_isnil(L, argIndex) == 0)
        {
            // It's a frame reference - use LuaHelpers to get the frame
            relativeTo = LuaHelpers.GetFrame(L, argIndex);
            argIndex++;
        }

        // Get offsets if present
        if (lua_isnumber(L, argIndex) != 0)
        {
            offsetX = (float)lua_tonumber(L, argIndex);
            offsetY = (float)lua_tonumber(L, argIndex + 1);
        }

        line?.SetEndPoint(relativePoint, relativeTo, offsetX, offsetY);

        lua_pushboolean(L, 1);
        return 1;
    }
    
    public static int internal_SetStartPoint(lua_State L)
    {
        var line = GetThis(L, 1);

        var relativePoint = luaL_checkstring(L, 2);
        Widgets.Region? relativeTo = null;
        float offsetX = 0;
        float offsetY = 0;

        int argIndex = 3;
        // Check if arg 3 is a frame reference or a number
        if (lua_isnumber(L, argIndex) == 0 && lua_isnil(L, argIndex) == 0)
        {
            // It's a frame/region reference - use LuaHelpers to get the frame
            relativeTo = LuaHelpers.GetFrame(L, argIndex) as Widgets.Region;
            argIndex++;
        }

        // Get offsets if present
        if (lua_isnumber(L, argIndex) != 0)
        {
            offsetX = (float)lua_tonumber(L, argIndex);
            offsetY = (float)lua_tonumber(L, argIndex + 1);
        }

        line?.SetStartPoint(relativePoint, relativeTo, offsetX, offsetY);

        lua_pushboolean(L, 1);
        return 1;
    }

    public static int internal_SetThickness(lua_State L)
    {
        var line = GetThis(L, 1);
        if (line == null) return 0;

        var thickness = (float)luaL_checknumber(L, 2);
        line.SetThickness(thickness);

        return 0;
    }
    
    public static string GetMetatableName() => "LineMetaTable";

    public static Widgets.Line? GetThis(lua_State L, int index)
    {
        // 1) Check the correct metatable
        // var metaName = GetMetatableName();
        // luaL_getmetatable(L, metaName);
        // lua_getmetatable(L, index);
        // bool same = (lua_rawequal(L, -1, -2) != 0);
        // lua_pop(L, 2);
        //
        // if (!same)
        //     return null;

        // If it's a table, retrieve the __frame key
        if (lua_istable(L, index) != 0)
        {
            lua_pushstring(L, "__frame");
            lua_gettable(L, index); // Pushes table["__frame"]
            index = -1; // Update index to point to __frame value
        }

        IntPtr userdataPtr = (IntPtr)lua_touserdata(L, index);
        if (userdataPtr == IntPtr.Zero)
            return null;

        IntPtr handlePtr = Marshal.ReadIntPtr(userdataPtr);
        if (handlePtr == IntPtr.Zero)
            return null;

        var handle = GCHandle.FromIntPtr(handlePtr);
        if (!handle.IsAllocated)
            return null;

        return handle.Target as Widgets.Line;
    }
    
    public static void RegisterMetaTable(lua_State L)
    {
        // 1) call base to register the "LineMetaTable"
        TextureBase.RegisterMetaTable(L);

        // 2) Now define "LineMetaTable"
        var metaName = GetMetatableName();
        luaL_newmetatable(L, metaName);

        // 3) __index = LineMetaTable
        lua_pushvalue(L, -1);
        lua_setfield(L, -2, "__index");

        // 4) Link to the base class's metatable ("TextureBaseMetaTable")
        var baseMetaName = TextureBase.GetMetatableName();
        luaL_getmetatable(L, baseMetaName);
        lua_setmetatable(L, -2); // Sets LineMetaTable's metatable to TextureBaseMetaTable
        
        // 5) Bind Frame-specific methods
        LuaHelpers.RegisterMethod(L, "SetThickness", internal_SetThickness);
        LuaHelpers.RegisterMethod(L, "SetEndPoint", internal_SetEndPoint);
        LuaHelpers.RegisterMethod(L, "SetStartPoint", internal_SetStartPoint);
        
        // 6) pop
        lua_pop(L, 1);
    }
}