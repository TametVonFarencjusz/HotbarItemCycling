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
        public override bool PreDraw(SpriteBatch spriteBatch, int type, int buffIndex, ref BuffDrawParams drawParams)
        {
            if (GetInstance<CyclingConfig>().ShowNextItemPicker > 0 && GetInstance<CyclingConfig>().ShowNextItemBuffPosition > 0 && !Main.playerInventory)
            {
                int buffPositionOffsetY = (int)((54 + 4 + 10) * Main.inventoryScale * 1.4f);
                int moveBuffs = 0;

                int selectedCount = 0;
                if (Main.LocalPlayer.selectedItem < 10)
                {
                    for (int j = 1; j < 5; j++)
                    {
                        if (Main.LocalPlayer.GetModPlayer<CyclingPlayer>().slotHighlight[Main.LocalPlayer.selectedItem + j * 10])
                        {
                            selectedCount += 1;
                        }
                    }
                }
                selectedCount = Math.Min(selectedCount, GetInstance<CyclingConfig>().ShowNextItemMax);

                int maxCount = 0;
                for (int i = 0; i < 10; i++)
                {
                    int columnMax = 0;
                    for (int j = 1; j < 5; j++)
                    {
                        if (Main.LocalPlayer.GetModPlayer<CyclingPlayer>().slotHighlight[i + j * 10])
                        {
                            columnMax += 1;
                        }
                    }
                    maxCount = Math.Max(maxCount, columnMax);
                }
                maxCount = Math.Min(maxCount, GetInstance<CyclingConfig>().ShowNextItemMax);


                if (GetInstance<CyclingConfig>().ShowNextItemBuffPosition == ShowNextItemMoveBuff.Dynamic
                    && GetInstance<CyclingConfig>().ShowNextItemPicker == ShowNextItem.Single)
                {
                    moveBuffs += buffPositionOffsetY * selectedCount;
                }
                else if (GetInstance<CyclingConfig>().ShowNextItemBuffPosition == ShowNextItemMoveBuff.Static
                    || GetInstance<CyclingConfig>().ShowNextItemPicker == ShowNextItem.All)
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
