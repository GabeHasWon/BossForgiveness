namespace BossForgiveness.Content.Walls;

public class EmberOffburnWall : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;

		AddMapEntry(new Color(55, 66, 73));
	}
}