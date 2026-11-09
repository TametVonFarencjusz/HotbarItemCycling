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
    public class DrawUI : ModSystem
    {
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
            if (invSlot != -1 && Main.LocalPlayer.GetModPlayer<CyclingPlayer>().slotHighlight[invSlot])
            {
                // Getting style from MainConfig
                int outlineStyle = (int)GetInstance<MainConfig>().OutlineStylePicker;
                Texture2D outlineTexture = outlineTexturesA[outlineStyle].Value;

                // Checking if outline colors should use style B
                bool changeStyle = false;
                if (!GetInstance<MainConfig>().OutlineStyleDontChange && Main.LocalPlayer.selectedItem == invSlot && !Main.playerInventory)
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
                    outlineTexture = outlineTexturesB[outlineStyle].Value;
                }

                // Setting outlinePosition to center different size Outline textures. Normal size of ItemSlot is 52x52
                Vector2 outlinePosition = position + (new Vector2(52, 52) - outlineTexture.Size()) / 2;

                // Color updated by Alpha value
                Color color = Main.inventoryBack * GetInstance<MainConfig>().OutlineVisibility;

                // Drawing Outline
                spriteBatch.Draw(outlineTexture, outlinePosition, null, color, 0f, default(Vector2), Main.inventoryScale, SpriteEffects.None, 0f);


                // Draw slot under
                if (GetInstance<ItemPreviewConfig>().ItemPreviewStyle != ItemPreviewStyle.Disabled && !Main.playerInventory)
                {
                    if (Main.LocalPlayer.selectedItem == invSlot || GetInstance<ItemPreviewConfig>().ItemPreviewStyle == ItemPreviewStyle.All)
                    {
                        int count = 0;
                        Vector2 echoPosition = position;
                        for (int j = 1; j < 5; j++)
                        {
                            if (Main.LocalPlayer.GetModPlayer<CyclingPlayer>().slotHighlight[invSlot + j * 10])
                            {
                                echoPosition.Y += (52 + 4) * Main.inventoryScale;
                                SimpleDrawItemSlot(spriteBatch, inv, context, invSlot + j * 10, echoPosition, lightColor);

                                count += 1;
                                if (count >= GetInstance<ItemPreviewConfig>().ItemMaxCount)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void SimpleDrawItemSlot(SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor = default(Color))
        {
            Player player = Main.LocalPlayer;
            Item item = inv[slot];
            float inventoryScale = Main.inventoryScale;
            Color color = Color.White;
            if (lightColor != Color.Transparent)
            {
                color = lightColor;
            }

            Texture2D value = TextureAssets.InventoryBack.Value;
            Color color2 = Main.inventoryBack;

            byte b = 200;
            color2 = new Color(b, b, b, b);
            spriteBatch.Draw(value, position, null, color2, 0f, default(Vector2), inventoryScale, SpriteEffects.None, 0f);

            Vector2 vector = value.Size() * inventoryScale;
            if (item.type > ItemID.None && item.stack > 0)
            {
                // Drawing Item
                float scale = ItemSlot.DrawItemIcon(item, context, spriteBatch, position + vector / 2f, inventoryScale, 32f, color2);

                if (GetInstance<ItemPreviewConfig>().ShowItemStack)
                {
                    // Drawing Trap Sign
                    if (ItemID.Sets.TrapSigned[item.type])
                    {
                        spriteBatch.Draw(TextureAssets.Wire.Value, position + new Vector2(40f, 40f) * inventoryScale, new Rectangle(4, 58, 8, 8), color, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);
                    }

                    // Drawing Unsafe Indicator
                    if (ItemID.Sets.DrawUnsafeIndicator[item.type])
                    {
                        Vector2 vector2 = new Vector2(-4f, -4f) * inventoryScale;
                        Texture2D value7 = TextureAssets.Extra[ExtrasID.UnsafeIndicator].Value;
                        Rectangle rectangle2 = value7.Frame();
                        spriteBatch.Draw(value7, position + vector2 + new Vector2(40f, 40f) * inventoryScale, rectangle2, color, 0f, rectangle2.Size() / 2f, 1f, SpriteEffects.None, 0f);
                    }

                    // Drawing Rubble Maker Indicator
                    if (item.type == ItemID.RubblemakerSmall || item.type == ItemID.RubblemakerMedium || item.type == ItemID.RubblemakerLarge)
                    {
                        Vector2 vector3 = new Vector2(2f, -6f) * inventoryScale;
                        switch (item.type)
                        {
                            case ItemID.RubblemakerSmall:
                                {
                                    Texture2D value10 = TextureAssets.Extra[ExtrasID.RubbleMakerIndicator].Value;
                                    Rectangle rectangle5 = value10.Frame(3, 1, 2);
                                    spriteBatch.Draw(value10, position + vector3 + new Vector2(40f, 40f) * inventoryScale, rectangle5, color, 0f, rectangle5.Size() / 2f, 1f, SpriteEffects.None, 0f);
                                    break;
                                }
                            case ItemID.RubblemakerMedium:
                                {
                                    Texture2D value9 = TextureAssets.Extra[ExtrasID.RubbleMakerIndicator].Value;
                                    Rectangle rectangle4 = value9.Frame(3, 1, 1);
                                    spriteBatch.Draw(value9, position + vector3 + new Vector2(40f, 40f) * inventoryScale, rectangle4, color, 0f, rectangle4.Size() / 2f, 1f, SpriteEffects.None, 0f);
                                    break;
                                }
                            case ItemID.RubblemakerLarge:
                                {
                                    Texture2D value8 = TextureAssets.Extra[ExtrasID.RubbleMakerIndicator].Value;
                                    Rectangle rectangle3 = value8.Frame(3);
                                    spriteBatch.Draw(value8, position + vector3 + new Vector2(40f, 40f) * inventoryScale, rectangle3, color, 0f, rectangle3.Size() / 2f, 1f, SpriteEffects.None, 0f);
                                    break;
                                }
                        }
                    }

                    // Drawing stack number
                    if (item.stack > 1)
                    {
                        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, item.stack.ToString(), position + new Vector2(10f, 26f) * inventoryScale, color, 0f, Vector2.Zero, new Vector2(inventoryScale), -1f, inventoryScale);
                    }

                    // Drawing bonus number like Ammo Count, Bait Count or Wire Count
                    int num10 = -1;
                    if (item.DD2Summon)
                    {
                        for (int i = 0; i < 58; i++)
                        {
                            if (inv[i].type == ItemID.DD2EnergyCrystal)
                            {
                                num10 += inv[i].stack;
                            }
                        }

                        if (num10 >= 0)
                        {
                            num10++;
                        }
                    }

                    if (item.useAmmo > 0)
                    {
                        int useAmmo = item.useAmmo;
                        num10 = 0;
                        for (int j = 0; j < 58; j++)
                        {
                            if (inv[j].stack > 0 && ItemLoader.CanChooseAmmo(item, inv[j], player))
                            {
                                num10 += inv[j].stack;
                            }
                        }
                    }

                    if (item.fishingPole > 0)
                    {
                        num10 = 0;
                        for (int k = 0; k < 58; k++)
                        {
                            if (inv[k].bait > 0)
                            {
                                num10 += inv[k].stack;
                            }
                        }
                    }

                    if (item.tileWand > 0)
                    {
                        int tileWand = item.tileWand;
                        num10 = 0;
                        for (int l = 0; l < 58; l++)
                        {
                            if (inv[l].type == tileWand)
                            {
                                num10 += inv[l].stack;
                            }
                        }
                    }

                    if (item.type == ItemID.Wrench || item.type == ItemID.GreenWrench || item.type == ItemID.BlueWrench || item.type == ItemID.YellowWrench || item.type == ItemID.MulticolorWrench || item.type == ItemID.WireKite)
                    {
                        num10 = 0;
                        for (int m = 0; m < 58; m++)
                        {
                            if (inv[m].type == ItemID.Wire)
                            {
                                num10 += inv[m].stack;
                            }
                        }
                    }

                    if (num10 != -1)
                    {
                        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, num10.ToString(), position + new Vector2(8f, 30f) * inventoryScale, color, 0f, Vector2.Zero, new Vector2(inventoryScale * 0.8f), -1f, inventoryScale);
                    }
                }
            }
        }
    }
}
