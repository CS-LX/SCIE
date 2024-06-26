using Engine;
using System.Xml.Linq;

namespace Game
{
	public class DrillerWidget : CanvasWidget
	{
		protected readonly CheckboxWidget m_acceptsDropsBox;

		protected readonly ComponentBlockEntity m_componentBlockEntity;

		protected readonly ComponentInventoryBase m_componentDispenser;

		protected readonly ButtonWidget m_dispenseButton;

		protected readonly GridPanelWidget m_dispenserGrid;

		protected readonly GridPanelWidget m_inventoryGrid;

		protected readonly ButtonWidget m_shootButton;

		protected readonly SubsystemTerrain m_subsystemTerrain;

		protected readonly InventorySlotWidget m_drillSlot;

		public DrillerWidget(IInventory inventory, ComponentInventoryBase componentDispenser)
		{
			m_componentDispenser = componentDispenser;
			m_componentBlockEntity = componentDispenser.Entity.FindComponent<ComponentBlockEntity>(true);
			m_subsystemTerrain = componentDispenser.Project.FindSubsystem<SubsystemTerrain>(true);
            LoadContents( this, ContentManager.Get<XElement>("Widgets/DrillerWidget"));
			m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
			m_dispenserGrid = Children.Find<GridPanelWidget>("DispenserGrid");
			m_dispenseButton = Children.Find<ButtonWidget>("DispenseButton");
			m_shootButton = Children.Find<ButtonWidget>("ShootButton");
			m_acceptsDropsBox = Children.Find<CheckboxWidget>("AcceptsDropsBox");
			m_drillSlot = Children.Find<InventorySlotWidget>("DrillSlot");
			int num = 6, y, x;
			InventorySlotWidget inventorySlotWidget;
			for (y = 0; y < m_inventoryGrid.RowsCount; y++)
			{
				for (x = 0; x < m_inventoryGrid.ColumnsCount; x++)
				{
					inventorySlotWidget = new InventorySlotWidget();
					inventorySlotWidget.AssignInventorySlot(inventory, num++);
					m_inventoryGrid.Children.Add(inventorySlotWidget);
					m_inventoryGrid.SetWidgetCell(inventorySlotWidget, new Point2(x, y));
				}
			}
			num = 0;
			for (y = 0; y < m_dispenserGrid.RowsCount; y++)
			{
				for (x = 0; x < m_dispenserGrid.ColumnsCount; x++)
				{
					inventorySlotWidget = new InventorySlotWidget();
					inventorySlotWidget.AssignInventorySlot(componentDispenser, num++);
					m_dispenserGrid.Children.Add(inventorySlotWidget);
					m_dispenserGrid.SetWidgetCell(inventorySlotWidget, new Point2(x, y));
				}
			}
			m_drillSlot.AssignInventorySlot(componentDispenser, 8);
		}

		public override void Update()
		{
			int value = m_subsystemTerrain.Terrain.GetCellValue(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z);
			int data = Terrain.ExtractData(value);
			if (m_dispenseButton.IsClicked)
			{
				data = DrillerBlock.SetMode(data, MachineMode.Dispense);
				m_subsystemTerrain.ChangeCell(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z, Terrain.ReplaceData(value, data));
			}
			if (m_shootButton.IsClicked)
			{
				data = DrillerBlock.SetMode(data, MachineMode.Shoot);
				m_subsystemTerrain.ChangeCell(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z, Terrain.ReplaceData(value, data));
			}
			if (m_acceptsDropsBox.IsClicked)
			{
				data = SixDirectionalBlock.SetAcceptsDrops(data, !SixDirectionalBlock.GetAcceptsDrops(data));
				m_subsystemTerrain.ChangeCell(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z, Terrain.ReplaceData(value, data));
			}
			var mode = DrillerBlock.GetMode(data);
			m_dispenseButton.IsChecked = mode == MachineMode.Dispense;
			m_shootButton.IsChecked = mode == MachineMode.Shoot;
			m_acceptsDropsBox.IsChecked = SixDirectionalBlock.GetAcceptsDrops(data);
			if (!m_componentDispenser.IsAddedToProject)
				ParentWidget.Children.Remove(this);
		}
	}
}