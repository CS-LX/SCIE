namespace Game
{
	public class SteelBlock : CubeBlock
	{
		public const int Index = 519;

		public override int GetFaceTextureSlot(int face, int value)
		{
			return 107;
		}
	}
}
