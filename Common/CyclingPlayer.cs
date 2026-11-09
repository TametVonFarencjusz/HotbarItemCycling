using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using static Terraria.ModLoader.ModContent;

namespace HotbarItemCycling
{
    public class CyclingPlayer : ModPlayer
    {
        // Using ProcessTriggers we check if Hotbar Keys are pressed, but we can't check if there ware just pressed
        // buttonCooldown is used to prevent holding Hotbar Key to trigger cycle
        public int buttonCooldown = 0;
        // This array is set every frame during ModPlayer.PreUpdate() so any other ItemSlot can know if it should draw outline 
        public bool[] slotHighlight = new bool[50];

        public override void PreUpdate()
        {
            // If buttonCooldown is greater than 0, it should be reduced
            if (buttonCooldown > 0)
            {
                buttonCooldown -= 1;
            }

            // Update slotHighlight
            UpdateSlotHighlight();
        }

        private void UpdateSlotHighlight()
        {
            slotHighlight = new bool[50];

            // 
            if (GetInstance<MainConfig>().CyclingStyle == HotbarItemCyclingStyle.Disabled)
            {
                return;
            }

            for (int i = 0; i < 10; i++)
            {
                // Check if item under 
                for (int j = 0; j < 5; j++)
                {
                    int nextSlot = i + j * 10;
                    // If item is favorited/marked, then we set slotHighlight value of it's hotbarSlot to value of row of the item
                    if (CheckItem(Player.inventory[nextSlot]))
                    {
                        slotHighlight[nextSlot] = true;
                    }
                    // If we skip unconnected items then we don't need to check other items
                    else if (GetInstance<MainConfig>().CyclingStyle == HotbarItemCyclingStyle.FavoriteOnlyTop)
                    {
                        break;
                    }
                }
            }
        }

        // Check if item should be considered for drawing outline and cycling
        private bool CheckItem(Item item)
        {
            if (item.IsAir)
            {
                return false;
            }

            if (GetInstance<MainConfig>().CyclingStyle == HotbarItemCyclingStyle.FavoriteOnlyTop || GetInstance<MainConfig>().CyclingStyle == HotbarItemCyclingStyle.FavoriteFullColumn)
            {
                return item.favorited;
            }

            if (GetInstance<MainConfig>().CyclingStyle == HotbarItemCyclingStyle.Marked)
            {
                return item.GetGlobalItem<ModGlobalItem>().hotbarSlot == 0;
            }

            return false;
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (triggersSet.MouseMiddle && GetInstance<MainConfig>().CyclingStyle == HotbarItemCyclingStyle.Marked)
            {
                if (!DrawUI.hoverItem.IsAir && buttonCooldown == 0)
                {
                    //byte newSLot = (byte)(Main.LocalPlayer.selectedItem < 10 ? Main.LocalPlayer.selectedItem : 255);
                    byte newSLot = 0;

                    if (DrawUI.hoverItem.GetGlobalItem<ModGlobalItem>().hotbarSlot != newSLot)
                    {
                        DrawUI.hoverItem.GetGlobalItem<ModGlobalItem>().hotbarSlot = newSLot;
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    }
                    else
                    {
                        DrawUI.hoverItem.GetGlobalItem<ModGlobalItem>().hotbarSlot = 255;
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    }
                }
                buttonCooldown = 2;
            }
            DrawUI.hoverItem = new Item();


            // Prevent Cycling when player is using item (We don't want some kind of Attribute Swapping from Minecraft)
            if (Main.LocalPlayer.ItemAnimationActive && GetInstance<MainConfig>().NoCyclingWhileUsingItem)
            {
                return;
            }

            // Array of all triggersSet Hotbar variables
            bool[] hotbars = new bool[10] { triggersSet.Hotbar1, triggersSet.Hotbar2, triggersSet.Hotbar3, triggersSet.Hotbar4, triggersSet.Hotbar5, triggersSet.Hotbar6, triggersSet.Hotbar7, triggersSet.Hotbar8, triggersSet.Hotbar9, triggersSet.Hotbar10 };


            // Checking if any Hotbar Key is pressed
            for (int i = 0; i < hotbars.Length; i++)
            {
                if (hotbars[i])
                {
                    // Preventing item cycling if Hotbar Key was pressed and not just pressed
                    if (buttonCooldown == 0)
                    {
                        // Fortunetly Player.selectedItem is updated after ProcessTriggers so we can check if item is already selected in hotbar
                        if (Player.selectedItem == i)
                        {
                            TryToCycle(i);
                        }
                    }
                    // Setting buttonCooldown to value 2 to prevent held Hotbar Key from triggering item cycle
                    buttonCooldown = 2;
                }
            }
        }

        private void TryToCycle(int hotbarSlot)
        {
            // If item in hotbar is not favorited/marked then it won't cycle
            if (!slotHighlight[hotbarSlot])
            {
                return;
            }

            // Collecting what inventory slots should cycle
            int[] slotsToSwap = new int[5] { hotbarSlot, -1, -1, -1, -1 };
            int i = 1;

            // Check what items should cycle
            for (int nextSlot = hotbarSlot + 10; nextSlot < 50; nextSlot += 10)
            {
                // If item is favorited/marked, then we set slotsToSwap next -1 to inventory slot of it
                if (slotHighlight[nextSlot])
                {
                    slotsToSwap[i] = nextSlot;
                    i += 1;
                }
            }

            // Swaping items from top to bottom
            for (int j = 0; j < i - 1; j++)
            {
                (Player.inventory[slotsToSwap[j + 1]], Player.inventory[slotsToSwap[j]]) = (Player.inventory[slotsToSwap[j]], Player.inventory[slotsToSwap[j + 1]]);
            }
        }
    }
}
