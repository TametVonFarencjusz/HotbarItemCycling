using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

// ATTENTION: Below this point is custom config UI element.
// Be aware that mods using custom config elements will break with the next few tModLoader updates until their design is finalized.
// You will need to be very active in updating your mod if you use these as they can break in any update.

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
        ArrowsInCorner,
        GreenArrows,
        YellowBlueArrows
    }

    class OutlineStyleElement : ConfigElement
    {
        string[] valueStrings;
        Texture2D backgroundTexture;

        public override void OnBind()
        {
            base.OnBind();
            backgroundTexture = Main.Assets.Request<Texture2D>("Images/Inventory_Back", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            valueStrings = Enum.GetNames(MemberInfo.Type);
            TextDisplayFunction = () => Label + ": " + GetStringValue();
        }

        void SetValue(OutlineStyle value) => SetObject(value);

        OutlineStyle GetValue() => (OutlineStyle)GetObject();

        string GetStringValue()
        {
            return valueStrings[(int)GetValue()];
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);
            SetValue(GetValue().NextEnum());
        }

        public override void RightClick(UIMouseEvent evt)
        {
            base.RightClick(evt);
            SetValue(GetValue().PreviousEnum());
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            CalculatedStyle dimensions = GetDimensions();


            //var circleSourceRectangle = new Rectangle(0, 0, (texture.Width - 2) / 2, texture.Height);
            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)(dimensions.X + dimensions.Width - 25), (int)(dimensions.Y + 4), 22, 22), Color.LightGreen);

            float scale = 0.5f;
            Vector2 pos = new Vector2(dimensions.X + dimensions.Width - 34, dimensions.Y + 2);
            spriteBatch.Draw(backgroundTexture, pos, null, Color.White, 0f, default(Vector2), scale, SpriteEffects.None, 0f);

            OutlineStyle outline = GetValue();


            Texture2D texture = CyclingPlayer.outlineTextures[(int)outline].Value;
            if (GetInstance<CyclingConfig>().OutlineStyleColorSwap)
            {
                texture = CyclingPlayer.outline2Textures[(int)outline].Value;
            }

            pos += (new Vector2(52, 52) - texture.Size()) / 2 * scale;
            spriteBatch.Draw(texture, pos, null, Color.White, 0f, default(Vector2), scale, SpriteEffects.None, 0f);
        }
    }
}
