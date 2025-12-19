namespace BossForgiveness.Content.Walls;

public class OffburnWall : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;

		AddMapEntry(new Color(55, 66, 73));
	}
}