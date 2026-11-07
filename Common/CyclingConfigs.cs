using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace HotbarItemCycling
{
    public class CyclingConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        // Add option to cycle through all favorited item in a column (false) or only items connected in chain to hotbar (true) 
        [Header("InGame")]
        [DefaultValue(true)]
        public bool SkipUnconnectedItems;


        // Prevent Cycling when player is using item (true), if false some kind of bugs might occur something like Attribute Swapping from Minecraft
        [DefaultValue(true)]
        public bool NoCyclingWhileUsingItem;


        // Prevent Cycling when player is using item (true), if false some kind of bugs might occur something like Attribute Swapping from Minecraft
        [Header("Outlines")]
        public OutlineStyle OutlineStylePicker;

        [DefaultValue(false)]
        public bool OutlineStyleDontChange;

        [DefaultValue(false)]
        public bool OutlineStyleColorSwap;
    }
}
