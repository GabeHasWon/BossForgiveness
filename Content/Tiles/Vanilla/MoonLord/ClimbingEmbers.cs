using BossForgiveness.Common;
using Terraria.ID;

namespace BossForgiveness.Content.Tiles.Vanilla.MoonLord;

internal class ClimbingEmbers : ModTile, IKelpTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileSolid[Type] = false;
		Main.tileLighted[Type] = true;

		DustType = DustID.Torch;

		AddMapEntry(new Color(255, 97, 58));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		if (Main.tile[i, j].TileFrameNumber != 0)
			(r, g, b) = (0.25f, 0.25f, 0.075f);
	}
}
