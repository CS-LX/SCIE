using Engine;
using GameEntitySystem;
using System.Globalization;
using TemplatesDatabase;

namespace Game
{
	public class ComponentPresserNN : ComponentMachine, IUpdateable
	{
		protected float m_fireTimeRemaining;

		protected int m_furnaceSize;

		protected readonly string[] m_matchedIngredients = new string[9];

		protected string m_smeltingRecipe;

		protected SubsystemAudio m_subsystemAudio;

		//protected int m_music;

		protected string m_smeltingRecipe2;

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

		public override int FuelSlotIndex
		{
			get
			{
				return SlotsCount;
			}
		}

		public float HeatLevel;

		public float SmeltingProgress;

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
			if (HeatLevel > 0f)
			{
				m_fireTimeRemaining = MathUtils.Max(0f, m_fireTimeRemaining - dt);
				if (m_fireTimeRemaining == 0f)
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
					//m_music = 0;
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
							int cellValue = m_subsystemTerrain.Terrain.GetCellValue(coordinates.X + i, coordinates.Y + j, coordinates.Z + k);
							if (i * i + j * j + k * k <= 1 && FurnaceNBlock.GetHeatLevel(cellValue) != 0)
							{
								cellValue = Terrain.ExtractContents(cellValue);
								if (cellValue == EngineBlock.Index || cellValue == EngineHBlock.Index)
								{
									num = 1;
									break;
								}
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
				//m_music = -1;
			}
			if (m_smeltingRecipe != null)
			{
				m_fireTimeRemaining = 0f;
				float fireTimeRemaining = m_fireTimeRemaining;
				m_fireTimeRemaining = 100f;
			}
			if (m_fireTimeRemaining <= 0f)
			{
				m_smeltingRecipe = null;
				SmeltingProgress = 0f;
				//m_music = -1;
			}
			if (m_smeltingRecipe != null)
			{
				SmeltingProgress = MathUtils.Min(SmeltingProgress + 0.01f * dt, 1f);
				if (SmeltingProgress >= 1f)
				{
					for (int l = 0; l < m_furnaceSize; l++)
					{
						if (m_slots[l].Count > 0)
						{
							m_slots[l].Count--;
						}
					}
					//int value = 0;
					m_slots[ResultSlotIndex].Value = ItemBlock.IdTable[m_smeltingRecipe];
					m_slots[ResultSlotIndex].Count++;
					m_smeltingRecipe = null;
					SmeltingProgress = 0f;
					m_updateSmeltingRecipe = true;
				}
			}
		}

		public override int GetSlotCapacity(int slotIndex, int value)
		{
			if (slotIndex != FuelSlotIndex || BlocksManager.Blocks[Terrain.ExtractContents(value)].FuelHeatLevel > 0f)
			{
				return base.GetSlotCapacity(slotIndex, value);
			}
			return 0;
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			base.Load(valuesDictionary, idToEntityMap);
			m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(true);
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

		protected string FindSmeltingRecipe(float heatLevel)
		{
			string text = null;
			for (int i = 0; i < m_furnaceSize; i++)
			{
				int slotValue = GetSlotValue(i);
				int num = Terrain.ExtractContents(slotValue);
				int num2 = Terrain.ExtractData(slotValue);
				if (GetSlotCount(i) > 0)
				{
					if (slotValue == ItemBlock.IdTable["SteelRod"])
					{
						text = "RifleBarrel";
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
				if (slot.Count != 0 && (slot.Value != ItemBlock.IdTable[text] || 1 + slot.Count > 40))
				{
					text = null;
				}
			}
			return text;
		}
	}
}
