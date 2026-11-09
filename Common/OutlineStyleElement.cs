using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
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
    class OutlineStyleElement : ConfigElement
    {
        string[] valueStrings;
        Texture2D backgroundTextureA;
        Texture2D backgroundTextureB;
        Texture2D itemTexture;

        public override void OnBind()
        {
            base.OnBind();
            backgroundTextureA = Main.Assets.Request<Texture2D>("Images/Inventory_Back", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            backgroundTextureB = Main.Assets.Request<Texture2D>("Images/Inventory_Back14", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            itemTexture = Main.Assets.Request<Texture2D>("Images/Item_3507", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            valueStrings = Enum.GetNames(MemberInfo.Type);
            TextDisplayFunction = () => Label + ": " + Language.GetText("Mods.HotbarItemCycling.Configs.OutlineStyle." + GetStringValue() + ".Label");

            Height.Pixels *= 2f;
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
            // Drawing base ConfigElement
            base.Draw(spriteBatch);

            // Position of ItemSlot in mod config
            CalculatedStyle dimensions = GetDimensions();
            Vector2 itemSlotPosition = new Vector2(dimensions.X + dimensions.Width - 70, dimensions.Y + 2);
            Vector2 itemSlotCenter = itemSlotPosition + backgroundTextureA.Size() / 2;

            // Check if mouse is over ItemSlot to set it to alt style
            bool mouseOverX = Main.mouseX >= itemSlotPosition.X && Main.mouseX <= itemSlotPosition.X + backgroundTextureA.Width;
            bool mouseOverY = Main.mouseY >= itemSlotPosition.Y && Main.mouseY <= itemSlotPosition.Y + backgroundTextureA.Height;
            bool altStyle = mouseOverX && mouseOverY;

            // Drawing ItemSlot
            Texture2D itemSlotTexture = altStyle ? backgroundTextureB : backgroundTextureA;
            spriteBatch.Draw(itemSlotTexture, itemSlotPosition, null, Color.White, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);


            // Draw Copper Shortsword
            spriteBatch.Draw(itemTexture, itemSlotCenter, null, Color.White, 0f, itemTexture.Size() / 2, 1f, SpriteEffects.None, 0f);


            // Draw Outline
            OutlineStyle outline = GetValue();
            Texture2D outlineTexture = CyclingPlayer.outlineTexturesA[(int)outline].Value;

            // Checking if outline colors should use style B
            bool changeStyle = false;
            if (altStyle && !GetInstance<MainConfig>().OutlineStyleDontChange)
            {
                changeStyle = true;
            }

            // Checking if outline colors should be swapped
            if (GetInstance<MainConfig>().OutlineStyleColorSwap)
            {
                changeStyle = !changeStyle;
            }

            // If changeStyle is true it means that outline should be changed to style 
            if (changeStyle)
            {
                outlineTexture = CyclingPlayer.outlineTexturesB[(int)outline].Value;
            }

            // Color updated by Alpha value
            Color color = Color.White * GetInstance<MainConfig>().OutlineVisibility;

            // Drawing outline
            spriteBatch.Draw(outlineTexture, itemSlotCenter, null, color, 0f, outlineTexture.Size() / 2, 1f, SpriteEffects.None, 0f);
        }
    }
}
