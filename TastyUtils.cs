﻿using UnityEngine;

public class TastyUtils
{
    public static bool IgnoreKeyPresses(bool extra = false)
    {
        if (!extra)
            return ZNetScene.instance == null || Player.m_localPlayer == null || Minimap.IsOpen() || Console.IsVisible() || TextInput.IsVisible() || ZNet.instance.InPasswordDialog() || Chat.instance?.HasFocus() == true;
        return ZNetScene.instance == null || Player.m_localPlayer == null || Minimap.IsOpen() || Console.IsVisible() || TextInput.IsVisible() || ZNet.instance?.InPasswordDialog() == true || Chat.instance?.HasFocus() == true || StoreGui.IsVisible() || InventoryGui.IsVisible() || Menu.IsVisible() || TextViewer.instance?.IsVisible() == true;
    }

}
