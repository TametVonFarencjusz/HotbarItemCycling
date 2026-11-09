using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using Newtonsoft.Json;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace HotbarItemCycling
{
    // This custom config UI element shows a completely custom config element that handles setting and getting the values in addition to custom drawing.
    [JsonConverter(typeof(StringEnumConverter))]
    [CustomModConfigItem(typeof(OutlineStyleElement))]
    public enum OutlineStyle
    {
        White,
        YellowBlue,
        Golden,
        ArrowsInTheCorner,
        GreenArrows,
        YellowBlueArrows
    }

    public enum ShowNextItem
    {
        Disabled,
        Single,
        All,
    }

    public enum ShowNextItemMoveBuff
    {
        Disabled,
        Dynamic,
        Static,
    }


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

        // Sets visibility of Outline. 1f - non-transparent, 0f - invisible
        [DefaultValue(1f)]
        [Range(0f, 1f)]
        [Increment(.025f)]
        public float OutlineVisibility;

        // Prevent Outline colors from changing between color A and color B.
        [DefaultValue(false)]
        public bool OutlineStyleDontChange;

        // Swap outline color A with color B.
        [DefaultValue(false)]
        public bool OutlineStyleColorSwap;

        [Header("ShowNextItemBeta")]

        // Next Item Preview Style
        [DefaultValue(ShowNextItem.Disabled)]
        public ShowNextItem ShowNextItemPicker;

        // Limit how many items should be shown below items in hotbar.
        [Range(1, 4)]
        public int ShowNextItemMax;

        // Set how buffs should be moved be moved when Next Item Preview is shown.
        [DefaultValue(ShowNextItemMoveBuff.Dynamic)]
        public ShowNextItemMoveBuff ShowNextItemBuffPosition;

        // If it number next to Item should be shown. Numbers like stack count, ammo count, wire count, etc.
        [DefaultValue(true)]
        public bool ShowNextItemStack;
    }
}
