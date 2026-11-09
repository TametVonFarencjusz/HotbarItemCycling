using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
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
            slotHighlight = new bool[50];
            for (int i = 0; i < 10; i++)
            {
                // If item in hotbar is not favorited, then we don't need to check items under it
                if (!Player.inventory[i].favorited)
                {
                    continue;
                }

                // Check if item under 
                for (int j = 1; j < 5; j++)
                {
                    int nextSlot = i + j * 10;
                    // If item is favorited, then we set slotHighlight value of it's hotbarSlot to value of row of the item
                    if (Player.inventory[nextSlot].favorited)
                    {
                        slotHighlight[i] = true;
                        slotHighlight[nextSlot] = true;
                    }
                    // If we skip unconnected items then we don't need to check other items
                    else if (GetInstance<MainConfig>().SkipUnconnectedItems)
                    {
                        break;
                    }
                }
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
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
            // If item in hotbar is not favorited then it won't cycle
            if (!Player.inventory[hotbarSlot].favorited)
            {
                return;
            }

            // Collecting what inventory slots should cycle
            int[] slotsToSwap = new int[5] { hotbarSlot, -1, -1, -1, -1 };
            int i = 1;

            // Check what items should cycle
            for (int nextSlot = hotbarSlot + 10; nextSlot < 50; nextSlot += 10)
            {
                // If item is favorited, then we set slotsToSwap next -1 to inventory slot of it
                if (Player.inventory[nextSlot].favorited)
                {
                    slotsToSwap[i] = nextSlot;
                    i += 1;
                }
                // If we skip unconnected items then we don't need to check other items
                else if (GetInstance<MainConfig>().SkipUnconnectedItems)
                {
                    break;
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
