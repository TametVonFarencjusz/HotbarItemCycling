using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace HotbarItemCycling
{
    public class ModGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        // For now, 0 means item is marked for Cycling and 255 that it's not
        public byte hotbarSlot = 255;

        public override void LoadData(Item item, TagCompound tag)
        {
            hotbarSlot = tag.Get<byte>("hotbarSlot"); // Load hotbarSlot tag
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            tag["hotbarSlot"] = hotbarSlot; // Save hotbarSlot tag
        }
    }
}
