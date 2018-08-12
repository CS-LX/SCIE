using Engine;
using GameEntitySystem;
using System.Globalization;
using TemplatesDatabase;

namespace Game
{
	public class ComponentPresser : ComponentInventoryBase, IUpdateable
	{
		private ComponentBlockEntity m_componentBlockEntity;

		private float m_fireTimeRemaining;

		private int m_furnaceSize;

		private readonly string[] m_matchedIngredients = new string[9];

		private SubsystemExplosions m_subsystemExplosions;

		private SubsystemTerrain m_subsystemTerrain;

		private bool m_updateSmeltingRecipe;

		private string m_smeltingRecipe;

		private SubsystemAudio m_subsystemAudio;

		private int m_music;

		private string m_smeltingRecipe2;

		public int RemainsSlotIndex
		{
			get
			{
				return SlotsCount;
			}
		}

		public int ResultSlotIndex
		{
			get
			{
				return SlotsCount - 1;
			}
		}

		public int FuelSlotIndex
		{
			get
			{
				return SlotsCount;
			}
		}

		public float HeatLevel
		{
			get;
			private set;
		}

		public float SmeltingProgress
		{
			get;
			private set;
		}

		public int UpdateOrder
		{
			get
			{
				return 0;
			}
		}

		public void Update(float dt)
		{
			Point3 coordinates = m_componentBlockEntity.Coordinates;
			if ((double)HeatLevel > 0.0)
			{
				m_fireTimeRemaining = MathUtils.Max(0f, m_fireTimeRemaining - dt);
				if ((double)m_fireTimeRemaining == 0.0)
				{
					HeatLevel = 0f;
				}
			}
			if (m_updateSmeltingRecipe)
			{
				m_updateSmeltingRecipe = false;
				string text = m_smeltingRecipe2 = FindSmeltingRecipe(1f);
				if (text != m_smeltingRecipe)
				{
					m_smeltingRecipe = text;
					m_smeltingRecipe2 = text;
					SmeltingProgress = 0f;
					m_music = 0;
				}
			}
			if (m_smeltingRecipe2 != null)
			{
				int num = 0;
				for (int i = -1; i < 2; i++)
				{
					for (int j = -1; j < 2; j++)
					{
						for (int k = -1; k < 2; k++)
						{
							int cellContents = m_subsystemTerrain.Terrain.GetCellContents(coordinates.X + i, coordinates.Y + j, coordinates.Z + k);
							if (i * i + j * j + k * k <= 1 && (cellContents == 509 || cellContents == 534))
							{
								num = 1;
								break;
							}
						}
					}
				}
				if (num == 0)
				{
					m_smeltingRecipe = null;
				}
				if (num == 1 && m_smeltingRecipe == null)
				{
					m_smeltingRecipe = m_smeltingRecipe2;
				}
			}
			if (m_smeltingRecipe == null)
			{
				HeatLevel = 0f;
				m_fireTimeRemaining = 0f;
				m_music = -1;
			}
			if (m_smeltingRecipe != null)
			{
				m_fireTimeRemaining = 0f;
				float fireTimeRemaining = m_fireTimeRemaining;
				m_fireTimeRemaining = 100f;
			}
			if ((double)m_fireTimeRemaining <= 0.0)
			{
				m_smeltingRecipe = null;
				SmeltingProgress = 0f;
				m_music = -1;
			}
			if (m_smeltingRecipe != null)
			{
				SmeltingProgress = MathUtils.Min(SmeltingProgress + 0.1f * dt, 1f);
				if ((double)SmeltingProgress >= 1.0)
				{
					for (int l = 0; l < m_furnaceSize; l++)
					{
						if (m_slots[l].Count > 0)
						{
							m_slots[l].Count--;
						}
					}
					int value = 0;
					if (m_smeltingRecipe == "ironplate")
					{
						value = 510;
					}
					if (m_smeltingRecipe == "copperplate")
					{
						value = 511;
					}
					if (m_smeltingRecipe == "steelplate")
					{
						value = 520;
					}
					m_slots[ResultSlotIndex].Value = value;
					m_slots[ResultSlotIndex].Count++;
					m_smeltingRecipe = null;
					SmeltingProgress = 0f;
					m_updateSmeltingRecipe = true;
				}
			}
		}

		public override int GetSlotCapacity(int slotIndex, int value)
		{
			if (slotIndex != FuelSlotIndex)
			{
				return base.GetSlotCapacity(slotIndex, value);
			}
			if ((double)BlocksManager.Blocks[Terrain.ExtractContents(value)].FuelHeatLevel > 0.0)
			{
				return base.GetSlotCapacity(slotIndex, value);
			}
			return 0;
		}

		public override void AddSlotItems(int slotIndex, int value, int count)
		{
			base.AddSlotItems(slotIndex, value, count);
			m_updateSmeltingRecipe = true;
		}

		public override int RemoveSlotItems(int slotIndex, int count)
		{
			m_updateSmeltingRecipe = true;
			return base.RemoveSlotItems(slotIndex, count);
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			base.Load(valuesDictionary, idToEntityMap);
			m_subsystemTerrain = base.Project.FindSubsystem<SubsystemTerrain>(true);
			m_subsystemExplosions = base.Project.FindSubsystem<SubsystemExplosions>(true);
			m_componentBlockEntity = base.Entity.FindComponent<ComponentBlockEntity>(true);
			m_subsystemAudio = base.Project.FindSubsystem<SubsystemAudio>(true);
			m_furnaceSize = SlotsCount - 1;
			m_fireTimeRemaining = valuesDictionary.GetValue<float>("FireTimeRemaining");
			HeatLevel = valuesDictionary.GetValue<float>("HeatLevel");
			m_updateSmeltingRecipe = true;
		}

		public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
		{
			base.Save(valuesDictionary, entityToIdMap);
			valuesDictionary.SetValue("FireTimeRemaining", m_fireTimeRemaining);
			valuesDictionary.SetValue("HeatLevel", HeatLevel);
		}

		private string FindSmeltingRecipe(float heatLevel)
		{
			string text = null;
			for (int i = 0; i < m_furnaceSize; i++)
			{
				int slotValue = GetSlotValue(i);
				int num = Terrain.ExtractContents(slotValue);
				int num2 = Terrain.ExtractData(slotValue);
				if (GetSlotCount(i) > 0)
				{
					Block block = BlocksManager.Blocks[num];
					m_matchedIngredients[i] = block.CraftingId + ":" + num2.ToString(CultureInfo.InvariantCulture);
					if (block.CraftingId.ToString() == "ironingot")
					{
						text = "ironplate";
					}
					if (block.CraftingId.ToString() == "copperingot")
					{
						text = "copperplate";
					}
					if (block.CraftingId.ToString() == "steelingot")
					{
						text = "steelplate";
					}
				}
				else
				{
					m_matchedIngredients[i] = null;
				}
			}
			if (text != null)
			{
				Slot slot = m_slots[ResultSlotIndex];
				int num3 = Terrain.ExtractContents(GetSlotValue(1));
				string craftingId = BlocksManager.Blocks[num3].CraftingId;
				if (slot.Count != 0 && (craftingId != text || 1 + slot.Count > 40))
				{
					text = null;
				}
			}
			return text;
		}
	}
}
