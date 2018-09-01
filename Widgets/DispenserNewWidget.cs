using Engine;
using System.Xml.Linq;

namespace Game
{
	public class DispenserNewWidget : CanvasWidget
	{
		protected readonly CheckboxWidget m_acceptsDropsBox;

		protected readonly ComponentBlockEntity m_componentBlockEntity;

		protected readonly ComponentDispenserNew m_componentDispenser;

		protected readonly ButtonWidget m_dispenseButton;

		protected readonly GridPanelWidget m_dispenserGrid;

		protected readonly GridPanelWidget m_inventoryGrid;

		protected readonly ButtonWidget m_shootButton;

		protected readonly SubsystemTerrain m_subsystemTerrain;

		protected readonly InventorySlotWidget m_drillSlot;

		public DispenserNewWidget(IInventory inventory, ComponentDispenserNew componentDispenser)
		{
			m_componentDispenser = componentDispenser;
			m_componentBlockEntity = componentDispenser.Entity.FindComponent<ComponentBlockEntity>(true);
			m_subsystemTerrain = componentDispenser.Project.FindSubsystem<SubsystemTerrain>(true);
			WidgetsManager.LoadWidgetContents(this, this, ContentManager.Get<XElement>("Widgets/DispenserNewWidget"));
			m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid", true);
			m_dispenserGrid = Children.Find<GridPanelWidget>("DispenserGrid", true);
			m_dispenseButton = Children.Find<ButtonWidget>("DispenseButton", true);
			m_shootButton = Children.Find<ButtonWidget>("ShootButton", true);
			m_acceptsDropsBox = Children.Find<CheckboxWidget>("AcceptsDropsBox", true);
			m_drillSlot = Children.Find<InventorySlotWidget>("DrillSlot", true);
			int num = 6, y, x;
			for (y = 0; y < m_dispenserGrid.RowsCount; y++)
			{
				for (x = 0; x < m_dispenserGrid.ColumnsCount; x++)
				{
					var inventorySlotWidget = new InventorySlotWidget();
					inventorySlotWidget.AssignInventorySlot(componentDispenser, num++);
					m_dispenserGrid.Children.Add(inventorySlotWidget);
					m_dispenserGrid.SetWidgetCell(inventorySlotWidget, new Point2(x, y));
				}
			}
			num = 0;
			for (y = 0; y < m_inventoryGrid.RowsCount; y++)
			{
				for (x = 0; x < m_inventoryGrid.ColumnsCount; x++)
				{
					var inventorySlotWidget2 = new InventorySlotWidget();
					inventorySlotWidget2.AssignInventorySlot(inventory, num++);
					m_inventoryGrid.Children.Add(inventorySlotWidget2);
					m_inventoryGrid.SetWidgetCell(inventorySlotWidget2, new Point2(x, y));
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
				data = DispenserNewBlock.SetMode(data, DispenserNewBlock.Mode.Dispense);
				m_subsystemTerrain.ChangeCell(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z, Terrain.ReplaceData(value, data), true);
			}
			if (m_shootButton.IsClicked)
			{
				data = DispenserNewBlock.SetMode(data, DispenserNewBlock.Mode.Shoot);
				m_subsystemTerrain.ChangeCell(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z, Terrain.ReplaceData(value, data), true);
			}
			if (m_acceptsDropsBox.IsClicked)
			{
				data = DispenserNewBlock.SetAcceptsDrops(data, !DispenserNewBlock.GetAcceptsDrops(data));
				m_subsystemTerrain.ChangeCell(m_componentBlockEntity.Coordinates.X, m_componentBlockEntity.Coordinates.Y, m_componentBlockEntity.Coordinates.Z, Terrain.ReplaceData(value, data), true);
			}
			var mode = DispenserNewBlock.GetMode(data);
			m_dispenseButton.IsChecked = mode == DispenserNewBlock.Mode.Dispense;
			m_shootButton.IsChecked = mode == DispenserNewBlock.Mode.Shoot;
			m_acceptsDropsBox.IsChecked = DispenserNewBlock.GetAcceptsDrops(data);
			if (!m_componentDispenser.IsAddedToProject)
			{
				ParentWidget.Children.Remove(this);
			}
		}
	}
}
