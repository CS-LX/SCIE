using Engine;
using System.Collections.Generic;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemFurnaceNBlockBehavior : SubsystemBlockBehavior
	{
		protected readonly Dictionary<Point3, FireParticleSystem> m_particleSystemsByCell = new Dictionary<Point3, FireParticleSystem>();

		protected SubsystemParticles m_subsystemParticles;

		public override int[] HandledBlocks
		{
			get
			{
				return new []
				{
					FurnaceNBlock.Index
				};
			}
		}

		public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
		{
			if (Terrain.ExtractContents(oldValue) != FurnaceNBlock.Index)
			{
				DatabaseObject databaseObject = Project.GameDatabase.Database.FindDatabaseObject("FurnaceN", Project.GameDatabase.EntityTemplateType, true);
				var valuesDictionary = new ValuesDictionary();
				valuesDictionary.PopulateFromDatabaseObject(databaseObject);
				valuesDictionary.GetValue<ValuesDictionary>("BlockEntity").SetValue("Coordinates", new Point3(x, y, z));
				Project.AddEntity(Project.CreateEntity(valuesDictionary));
			}
			if (FurnaceNBlock.GetHeatLevel(value) != 0)
			{
				AddFire(value, x, y, z);
			}
		}

		public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
		{
			ComponentBlockEntity blockEntity = Project.FindSubsystem<SubsystemBlockEntities>(true).GetBlockEntity(x, y, z);
			if (blockEntity != null)
			{
				var position = new Vector3((float)x, (float)y, (float)z) + new Vector3(0.5f);
				for (var i = blockEntity.Entity.FindComponents<IInventory>().GetEnumerator(); i.MoveNext();)
				{
					i.Current.DropAllItems(position);
				}
				Project.RemoveEntity(blockEntity.Entity, true);
			}
			if (FurnaceNBlock.GetHeatLevel(value) != 0)
			{
				RemoveFire(x, y, z);
			}
		}

		public override void OnBlockModified(int value, int oldValue, int x, int y, int z)
		{
			switch (FurnaceNBlock.GetHeatLevel(oldValue).CompareTo(FurnaceNBlock.GetHeatLevel(value)))
			{
				case -1:
					AddFire(value, x, y, z);
					break;
				case 1:
					RemoveFire(x, y, z);
					break;
			}
		}

		public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
		{
			if (FurnaceNBlock.GetHeatLevel(value) != 0)
			{
				AddFire(value, x, y, z);
			}
		}

		public override void OnChunkDiscarding(TerrainChunk chunk)
		{
			int originX = chunk.Origin.X, originY = chunk.Origin.Y;
			var list = new List<Point3>();
			foreach (var key in m_particleSystemsByCell.Keys)
			{
				if (key.X >= originX && key.X < originX + 16 && key.Z >= originY && key.Z < originY + 16)
				{
					list.Add(key);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				Point3 item = list[i];
				RemoveFire(item.X, item.Y, item.Z);
			}
		}

		public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
		{
			ComponentBlockEntity blockEntity = Project.FindSubsystem<SubsystemBlockEntities>(true).GetBlockEntity(raycastResult.CellFace.X, raycastResult.CellFace.Y, raycastResult.CellFace.Z);
			if (blockEntity == null || componentMiner.ComponentPlayer == null)
			{
				return false;
			}
			ComponentFurnaceN componentFurnace = blockEntity.Entity.FindComponent<ComponentFurnaceN>(true);
			componentMiner.ComponentPlayer.ComponentGui.ModalPanelWidget = new FurnaceNWidget(componentMiner.Inventory, componentFurnace);
			AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
			return true;
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemParticles = Project.FindSubsystem<SubsystemParticles>(true);
		}

		protected void AddFire(int value, int x, int y, int z)
		{
			var v = new Vector3(0.5f, 0.2f, 0.5f);
			const float size = 0.15f;
			var fireParticleSystem = new FireParticleSystem(new Vector3((float)x, (float)y, (float)z) + v, size, 16f);
			m_subsystemParticles.AddParticleSystem(fireParticleSystem);
			m_particleSystemsByCell[new Point3(x, y, z)] = fireParticleSystem;
		}

		protected void RemoveFire(int x, int y, int z)
		{
			var key = new Point3(x, y, z);
			m_subsystemParticles.RemoveParticleSystem(m_particleSystemsByCell[key]);
			m_particleSystemsByCell.Remove(key);
		}
	}
}
