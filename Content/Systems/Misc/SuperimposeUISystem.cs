using BossForgiveness.Content.NPCs.Mechanics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;

namespace BossForgiveness.Content.Systems.Misc;

internal class SuperimposeUISystem : ModSystem
{
    private UserInterface imposeUI;

    public override void Load()
    {
        if (!Main.dedServ)
        {
            imposeUI = new UserInterface();
            imposeUI.SetState(null);
        }
    }

    public override void UpdateUI(GameTime gameTime) => imposeUI.Update(gameTime);

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));

        if (resourceBarIndex != -1)
        {
            layers.Insert(resourceBarIndex - 2, new LegacyGameInterfaceLayer(
                "BossForgiveness: Special TE Drawing",
                delegate
                {
                    foreach (var item in TileEntity.ByPosition)
                    {
                        if (item.Value is QueenBeePacificationNPC.QueenBeeDreamTE dreamTE)
                        {
                            var pos = item.Key.ToWorldCoordinates() - Main.screenPosition;
                            dreamTE.DrawDream(pos);
                        }
                    }

                    return true;
                },
                InterfaceScaleType.Game)
            );
        }
    }
}
