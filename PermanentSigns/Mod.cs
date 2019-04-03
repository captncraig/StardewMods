using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermanentSigns
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Helper.Events.Input.ButtonPressed += ButtonPressed;
        }

        private void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!e.Button.IsActionButton()) return;
            var t = e.Cursor.GrabTile;
            var sign = (Game1.currentLocation.getObjectAtTile((int)t.X,(int) t.Y) as Sign);
            if (sign == null) return;
            if (sign.displayItem.Value != null){
                this.Helper.Input.Suppress(e.Button);
            }
        }
    }
}
