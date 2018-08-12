using Engine;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemDispenserNew2BlockBehavior : SubsystemBlockBehavior
	{
		private SubsystemAudio m_subsystemAudio;

		private SubsystemBlockEntities m_subsystemBlockEntities;

		private SubsystemGameInfo m_subsystemGameInfo;

		private SubsystemTerrain m_subsystemTerrain;

		public override int[] HandledBlocks
		{
			get
			{
				return new int[1]
				{
					537
				};
			}
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemTerrain = base.Project.FindSubsystem<SubsystemTerrain>(true);
			m_subsystemBlockEntities = base.Project.FindSubsystem<SubsystemBlockEntities>(true);
			m_subsystemGameInfo = base.Project.FindSubsystem<SubsystemGameInfo>(true);
			m_subsystemAudio = base.Project.FindSubsystem<SubsystemAudio>(true);
		}

		public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
		{
			DatabaseObject databaseObject = base.Project.GameDatabase.Database.FindDatabaseObject("DispenserNew2", base.Project.GameDatabase.EntityTemplateType, true);
			ValuesDictionary valuesDictionary = new ValuesDictionary();
			valuesDictionary.PopulateFromDatabaseObject(databaseObject);
			valuesDictionary.GetValue<ValuesDictionary>("BlockEntity").SetValue("Coordinates", new Point3(x, y, z));
			base.Project.AddEntity(base.Project.CreateEntity(valuesDictionary));
		}

		public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
		{
			ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(x, y, z);
			if (blockEntity != null)
			{
				Vector3 position = new Vector3((float)x, (float)y, (float)z) + new Vector3(0.5f);
				foreach (IInventory item in blockEntity.Entity.FindComponents<IInventory>())
				{
					item.DropAllItems(position);
				}
				base.Project.RemoveEntity(blockEntity.Entity, true);
			}
		}

		public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
		{
			if (m_subsystemGameInfo.WorldSettings.GameMode != GameMode.Adventure)
			{
				ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(raycastResult.CellFace.X, raycastResult.CellFace.Y, raycastResult.CellFace.Z);
				if (blockEntity != null && componentMiner.ComponentPlayer != null)
				{
					ComponentDispenserNew2 componentDispenser = blockEntity.Entity.FindComponent<ComponentDispenserNew2>(true);
					componentMiner.ComponentPlayer.ComponentGui.ModalPanelWidget = new DispenserNew2Widget(componentMiner.Inventory, componentDispenser);
					AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
					return true;
				}
			}
			return false;
		}

		public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
		{
			if (!worldItem.ToRemove)
			{
				ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(cellFace.X, cellFace.Y, cellFace.Z);
				if (blockEntity != null && DispenserNew2Block.GetAcceptsDrops(Terrain.ExtractData(m_subsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z))))
				{
					ComponentDispenserNew inventory = blockEntity.Entity.FindComponent<ComponentDispenserNew>(true);
					Pickable pickable = worldItem as Pickable;
					int num = (pickable == null) ? 1 : pickable.Count;
					int value = worldItem.Value;
					int count = num;
					int num2 = ComponentInventoryBase.AcquireItems(inventory, value, count);
					if (num2 < num)
					{
						m_subsystemAudio.PlaySound("Audio/PickableCollected", 1f, 0f, worldItem.Position, 3f, true);
					}
					if (num2 <= 0)
					{
						worldItem.ToRemove = true;
					}
					else if (pickable != null)
					{
						pickable.Count = num2;
					}
				}
			}
		}
	}
}