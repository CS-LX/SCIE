namespace Game
{
	public class LitEngineHBlock : EngineHBlock
	{
		public new const int Index = 534;

		public override int GetFaceTextureSlot(int face, int value)
		{
			if (face != 4 && face != 5)
			{
				switch (Terrain.ExtractData(value))
				{
				case 0:
					switch (face)
					{
					case 0:
						return 187;
					default:
						return 159;
					case 2:
						return 107;
					}
				case 1:
					switch (face)
					{
					case 1:
						return 187;
					default:
						return 159;
					case 3:
						return 107;
					}
				case 2:
					switch (face)
					{
					case 2:
						return 187;
					default:
						return 159;
					case 0:
						return 107;
					}
				default:
					switch (face)
					{
					case 3:
						return 187;
					default:
						return 159;
					case 1:
						return 107;
					}
				}
			}
			return 107;
		}
	}
}