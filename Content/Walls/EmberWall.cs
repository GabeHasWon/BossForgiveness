namespace BossForgiveness.Content.Walls;

public class EmberWall : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;

		AddMapEntry(new Color(91, 0, 31));
	}
}