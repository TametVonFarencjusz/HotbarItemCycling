using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace HotbarItemCycling
{
    public class CyclingPlayer : ModPlayer
    {
        // Using ProcessTriggers we check if Hotbar Keys are pressed, but we can't check if there ware just pressed
        // buttonCooldown is used to prevent holding Hotbar Key to trigger cycle
        public int buttonCooldown = 0;
        // This array is set every frame during ModPlayer.PreUpdate() so any other ItemSlot can know if it should draw outline 
        public int[] slotHighlight = new int[10];
        // Texture of the Outline
        public static Asset<Texture2D>[] outlineTexturesA;
        // Texture of the Outline while item is selected
        public static Asset<Texture2D>[] outlineTexturesB;

        public override void Load()
        {
            // Loading Outline Texture
            string[] outlineNames = Enum.GetNames(typeof(OutlineStyle));
            int size = outlineNames.Length;
            string outlinePath = "HotbarItemCycling/Assets/Outlines/";

            outlineTexturesA = new Asset<Texture2D>[size];
            outlineTexturesB = new Asset<Texture2D>[size];

            // Loading all textures based on names from Enum (OutlineStyle)
            for (int i = 0; i < size; i++)
            {
                outlineTexturesA[i] = Request<Texture2D>(outlinePath + outlineNames[i] + "_A");
                outlineTexturesB[i] = Request<Texture2D>(outlinePath + outlineNames[i] + "_B");
            }

            // Detouring ItemSlot draw function to Draw Outline after it was drawn
            Terraria.UI.On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += PostDrawSlot;
        }

        public override void Unload()
        {
            // Unloading Outline Texture
            outlineTexturesA = null;
            outlineTexturesB = null;
        }

        private static void PostDrawSlot(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor = default(Color))
        {
            // Orginal draw function
            orig(spriteBatch, inv, context, slot, position, lightColor);

            // 1st check if we should draw outline
            if (context != ItemSlot.Context.InventoryItem && context != ItemSlot.Context.HotbarItem)
            {
                return;
            }

            if (inv[slot].IsAir || !inv[slot].favorited)
            {
                return;
            }

            // Finding item from ItemSlot in players inventory
            int invSlot = -1;
            for (int i = 0; i < 50; i++)
            {
                if (Main.LocalPlayer.inventory[i] == inv[slot])
                {
                    invSlot = i;
                    break;
                }
            }

            // Checking if item should be highlighted
            if (invSlot != -1 && Main.LocalPlayer.GetModPlayer<CyclingPlayer>().slotHighlight[invSlot % 10] >= invSlot / 10)
            {
                float inventoryScale = Main.inventoryScale;

                int outlineStyle = (int)GetInstance<CyclingConfig>().OutlineStylePicker;

                Texture2D texture = outlineTexturesA[outlineStyle].Value;

                bool changeStyle = false;
                if (Main.LocalPlayer.selectedItem == invSlot && !GetInstance<CyclingConfig>().OutlineStyleDontChange && !Main.playerInventory)
                {
                    changeStyle = true;
                }

                if (GetInstance<CyclingConfig>().OutlineStyleColorSwap)
                {
                    changeStyle = !changeStyle;
                }

                if (changeStyle)
                {
                    texture = outlineTexturesB[outlineStyle].Value;
                }

                Color color2 = Main.inventoryBack;
                Vector2 pos = position + (new Vector2(52, 52) - texture.Size()) / 2;
                spriteBatch.Draw(texture, pos, null, color2, 0f, default(Vector2), inventoryScale, SpriteEffects.None, 0f);
            }
        }

        public override void PreUpdate()
        {
            // If buttonCooldown is greater than 0, it should be reduced
            if (buttonCooldown > 0)
            {
                buttonCooldown -= 1;
            }

            // Update slotHighlight
            for (int i = 0; i < 10; i++)
            {
                slotHighlight[i] = -1;

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
                        slotHighlight[i] = j;
                    }
                    // If we skip unconnected items then we don't need to check other items
                    else if (GetInstance<CyclingConfig>().SkipUnconnectedItems)
                    {
                        break;
                    }
                }
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // Prevent Cycling when player is using item (We don't want some kind of Attribute Swapping from Minecraft)
            if (Main.LocalPlayer.ItemAnimationActive && GetInstance<CyclingConfig>().NoCyclingWhileUsingItem)
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
                else if (GetInstance<CyclingConfig>().SkipUnconnectedItems)
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
