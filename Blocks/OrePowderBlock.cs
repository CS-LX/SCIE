using Engine;
using Engine.Graphics;
using System;

namespace Game
{
	public class IronOrePowder : OrePowderBlock
	{
		public IronOrePowder() : base(OreType.IronOre)
		{
		}
	}
    public class CopperOrePowder : OrePowderBlock
    {
        public CopperOrePowder() : base(OreType.CopperOre)
        {
        }
    }
    public class GermaniumOrePowder : OrePowderBlock
    {
        public GermaniumOrePowder() : base(OreType.GermaniumOre)
        {
        }
    }
    public class GoldOrePowder : OrePowderBlock
    {
        public GoldOrePowder() : base(OreType.GoldOre)
        {
        }
    }
    public class SliverOrePowder : OrePowderBlock
    {
        public SliverOrePowder() : base(OreType.SliverOre)
        {
        }
    }
    public class PlatinumOrePowder : OrePowderBlock
    {
        public PlatinumOrePowder() : base(OreType.PlatinumOre)
        {
        }
    }
    public class LeadOrePowder : OrePowderBlock
    {
        public LeadOrePowder() : base(OreType.LeadOre)
        {
        }
    }
    public class StannaryOrePowder : OrePowderBlock
    {
        public StannaryOrePowder() : base(OreType.StannaryOre)
        {
        }
    }
    public class ZincOrePowder : OrePowderBlock
    {
        public ZincOrePowder() : base(OreType.ZincOre)
        {
        }
    }
    public class ChromiumOrePowder : OrePowderBlock
    {
        public ChromiumOrePowder() : base(OreType.ChromiumOre)
        {
        }
    }
    public class NickelOrePowder : OrePowderBlock
    {
        public NickelOrePowder() : base(OreType.NickelOre)
        {
        }
    }
    public class AluminumOrePowder : OrePowderBlock
    {
        public AluminumOrePowder() : base(OreType.AluminumOre)
        {
        }
    }
    public abstract class OrePowderBlock : FlatItem
	{
		[Serializable]
		public enum OreType
		{
			IronOre,
            CopperOre,
            GermaniumOre,
            GoldOre,
            SliverOre,
            PlatinumOre,
            LeadOre,
            ZincOre,
            ChromiumOre,
            NickelOre,
            AluminumOre,
            StannaryOre
		}
		public readonly OreType Type;
		protected OrePowderBlock(OreType type)
		{
			DefaultTextureSlot = 198;
			DefaultDisplayName = type.ToString()+"Powder";
			Type = type;
		}
		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			switch (Type)
			{
				case OreType.IronOre:
					color = new Color(139, 69, 19);
					break;
                case OreType.CopperOre:
                    color = new Color(34, 139, 34);
                    break;
                case OreType.GermaniumOre:
                    color = new Color(205, 190, 112);
                    break;
                case OreType.GoldOre:
                    color = new Color(255, 215, 0);
                    break;
                case OreType.SliverOre:
                    color = new Color(212, 212, 212);
                    break;
                case OreType.PlatinumOre:
                    color = new Color(232, 232, 232);
                    break;
                case OreType.LeadOre:
                    color = new Color(87, 86, 85);
                    break;
                case OreType.ZincOre:
                    color = new Color(64, 224, 205);
                    break;
                case OreType.StannaryOre:
                    color = new Color(225, 225, 225);
                    break;
                case OreType.ChromiumOre:
                    color = new Color(60, 60, 60);
                    break;
                case OreType.NickelOre:
                    color = new Color(120, 120, 120);
                    break;
                case OreType.AluminumOre:
                    color = new Color(199, 97, 20);
                    break;
                default:
					color = Color.White;
					break;
			}
			BlocksManager.DrawFlatBlock(primitivesRenderer, value, size, ref matrix, null, color, false, environmentData);
		}
		public override string GetDescription(int value)
		{
			return Type.ToString()+"Powder is Ore powder obtained by crushing "+Type.ToString()+".";
		}
	}
}
