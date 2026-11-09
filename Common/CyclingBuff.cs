using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace HotbarItemCycling
{
    public class BuffBarSystem : GlobalBuff
    {
        private static CyclingPlayer modPlayer => Main.LocalPlayer.GetModPlayer<CyclingPlayer>();

        public override bool PreDraw(SpriteBatch spriteBatch, int type, int buffIndex, ref BuffDrawParams drawParams)
        {
            if (GetInstance<ItemPreviewConfig>().ItemPreviewStyle > 0 && GetInstance<ItemPreviewConfig>().BuffPosition > 0 && !Main.playerInventory)
            {
                int buffPositionOffsetY = (int)((54 + 4 + 10) * Main.inventoryScale * 1.4f);
                int moveBuffs = 0;

                int selectedCount = 0;
                if (Main.LocalPlayer.selectedItem < 10)
                {
                    if (modPlayer.slotHighlight[Main.LocalPlayer.selectedItem])
                    {
                        for (int j = 1; j < 5; j++)
                        {
                            if (modPlayer.slotHighlight[Main.LocalPlayer.selectedItem + j * 10])
                            {
                                selectedCount += 1;
                            }
                        }
                    }
                }
                selectedCount = Math.Min(selectedCount, GetInstance<ItemPreviewConfig>().ItemMaxCount);

                int maxCount = 0;
                for (int i = 0; i < 10; i++)
                {
                    if (modPlayer.slotHighlight[i])
                    {
                        int columnMax = 0;
                        for (int j = 1; j < 5; j++)
                        {
                            if (modPlayer.slotHighlight[i + j * 10])
                            {
                                columnMax += 1;
                            }
                        }
                        maxCount = Math.Max(maxCount, columnMax);
                    }
                }
                maxCount = Math.Min(maxCount, GetInstance<ItemPreviewConfig>().ItemMaxCount);


                if (GetInstance<ItemPreviewConfig>().BuffPosition == ItemPreviewBuffPosition.Dynamic
                    && GetInstance<ItemPreviewConfig>().ItemPreviewStyle == ItemPreviewStyle.Single)
                {
                    moveBuffs += buffPositionOffsetY * selectedCount;
                }
                else if (GetInstance<ItemPreviewConfig>().BuffPosition == ItemPreviewBuffPosition.Static
                    || GetInstance<ItemPreviewConfig>().ItemPreviewStyle == ItemPreviewStyle.All)
                {
                    moveBuffs += buffPositionOffsetY * maxCount;
                }

                drawParams.Position.Y += moveBuffs;
                drawParams.MouseRectangle.Y += moveBuffs;
                drawParams.TextPosition.Y += moveBuffs;
            }
            return base.PreDraw(spriteBatch, type, buffIndex, ref drawParams);
        }
    }
}
