using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuySprinklers
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Display.MenuChanged += MenuChanged;
        }



        private void MenuChanged(object sender, MenuChangedEventArgs e)
        {

            var shop = e.NewMenu as StardewValley.Menus.ShopMenu;
            if (shop == null || shop.portraitPerson?.Name != "Clint") return;

            IReflectedField<Dictionary<Item, int[]>> inventoryInformation = this.Helper.Reflection.GetField<Dictionary<Item, int[]>>(shop, "itemPriceAndStock");
            Dictionary<Item, int[]> itemPriceAndStock = inventoryInformation.GetValue();
            IReflectedField<List<Item>> forSaleInformation = this.Helper.Reflection.GetField<List<Item>>(shop, "forSale");
            List<Item> forSale = forSaleInformation.GetValue();

            var year1 = Game1.year == 1;
            var items = new Dictionary<int, int>
            {
                // Bars he sells ore for. Materials + 100g/hr smelting cost
                { 334, year1 ? 575 : 1050 }, // copper bar
                { 335, year1 ? 1100 : 1700 }, // iron Bar
                { 336, year1 ? 2650 : 4500 }, // gold bar
                { 337, year1 ? 10000 : 17000  }, // iridium bar (using approximate profit ratios from above calcs: 10x and 17x sell price of 1000)
                { 338, year1 ?  675 : 1150}, // Refined Quartz. 15x / 30x sell price of 25 + 1 coal + 150g smelting
                // sprinklers are bar cost + fee
                { 599, year1 ?  1875 : 3050 }, // basic sprinkler. Fee 200/300
                { 621, year1 ?  4825 : 7950 }, // quality. Fee 400 / 600
                { 645, year1 ?  15450 : 26100 }, // Iridium. Fee 800 / 1600
            };

            foreach (var kvp in items)
            {
                int itemId = kvp.Key;
                int price = kvp.Value;
                Item objectToAdd = new StardewValley.Object(Vector2.Zero, itemId, int.MaxValue);
                itemPriceAndStock.Add(objectToAdd, new int[2] { price, int.MaxValue });
                forSale.Add(objectToAdd);
            }

            inventoryInformation.SetValue(itemPriceAndStock);
            forSaleInformation.SetValue(forSale);
        }
    }
}
