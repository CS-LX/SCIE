﻿using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;
using XmlUtilities;
using System;

namespace Game
{
	[PluginLoader("IndustrialMod", "", 0u)]
	public class Item : IAnimatedItem, IUnstableItem, IFood, IExplosive, IWeapon, IScalableItem, ICollidableItem
	{
		internal static readonly BoundingBox[] m_defaultCollisionBoxes = new BoundingBox[]
		{
			new BoundingBox(Vector3.Zero, Vector3.One)
		};
		public static bool Loaded;
		static void Initialize()
		{
			BlocksManager.DamageItem1 = DamageItem;
			BlocksManager.FindBlocksByCraftingId1 = FindBlocksByCraftingId;
			CraftingRecipesManager.Initialize1 = CRInitialize;
			CraftingRecipesManager.MatchRecipe1 = MatchRecipe;
			CraftingRecipesManager.TransformRecipe1 = TransformRecipe;
		}
		public static Block[] FindBlocksByCraftingId(string craftingId)
		{
			if (ItemBlock.IdTable.TryGetValue(craftingId, out int value))
			{
				return new Block[1];// { BlocksManager.Blocks[ItemBlock.Index] };
			}
			var c__DisplayClass = new BlocksManager.c__DisplayClass6
			{
				craftingId = craftingId
			};
			return BlocksManager.Blocks.Where(c__DisplayClass.FindBlocksByCraftingId_b__5).ToArray();
		}
		public static int DamageItem(int value, int damageCount)
		{
			Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
			if (block.Durability < 0)
			{
				return value;
			}
			damageCount += block.GetDamage(value);
			return damageCount <= (block is IDurability item ? item.GetDurability(value) : block.Durability)
				? block.SetDamage(value, damageCount)
				: block.GetDamageDestructionValue(value);
		}
		public static void CRInitialize()
		{
			CraftingRecipesManager.m_recipes = new List<CraftingRecipe>();
			foreach (XElement xelement in ContentManager.Get<XElement>("CraftingRecipes").Descendants("Recipe"))
			{
				CraftingRecipe craftingRecipe = new CraftingRecipe
				{
					Ingredients = new string[36]
				};
				string attributeValue = XmlUtils.GetAttributeValue<string>(xelement, "Result");
				craftingRecipe.ResultValue = CraftingRecipesManager.DecodeResult(attributeValue);
				craftingRecipe.ResultCount = XmlUtils.GetAttributeValue<int>(xelement, "ResultCount");
				string attributeValue2 = XmlUtils.GetAttributeValue<string>(xelement, "Remains", string.Empty);
				if (!string.IsNullOrEmpty(attributeValue2))
				{
					craftingRecipe.RemainsValue = CraftingRecipesManager.DecodeResult(attributeValue2);
					craftingRecipe.RemainsCount = XmlUtils.GetAttributeValue<int>(xelement, "RemainsCount");
				}
				craftingRecipe.RequiredHeatLevel = XmlUtils.GetAttributeValue<float>(xelement, "RequiredHeatLevel");
				craftingRecipe.Description = XmlUtils.GetAttributeValue<string>(xelement, "Description");
				if (craftingRecipe.ResultCount > BlocksManager.Blocks[Terrain.ExtractContents(craftingRecipe.ResultValue)].MaxStacking)
				{
					throw new InvalidOperationException(string.Format("In recipe for \"{0}\" ResultCount is larger than max stacking of result block.", attributeValue));
				}
				if (craftingRecipe.RemainsValue != 0 && craftingRecipe.RemainsCount > BlocksManager.Blocks[Terrain.ExtractContents(craftingRecipe.RemainsValue)].MaxStacking)
				{
					throw new InvalidOperationException(string.Format("In Recipe for \"{0}\" RemainsCount is larger than max stacking of remains block.", attributeValue2));
				}
				Dictionary<char, string> dictionary = new Dictionary<char, string>();
				foreach (XAttribute xattribute in xelement.Attributes().Where(CraftingRecipesManager.Initialize_b__0))
				{
					CraftingRecipesManager.DecodeIngredient(xattribute.Value, out string craftingId, out int? num);
					if (BlocksManager.FindBlocksByCraftingId(craftingId).Length == 0)
					{
						throw new InvalidOperationException(string.Format("Block with craftingId \"{0}\" not found.", xattribute.Value));
					}
					if (num != null && (num.Value < 0 || num.Value > 262143))
					{
						throw new InvalidOperationException(string.Format("Data in recipe ingredient \"{0}\" must be between 0 and 0x3FFFF.", xattribute.Value));
					}
					dictionary.Add(xattribute.Name.LocalName[0], xattribute.Value);
				}
				string[] array = xelement.Value.Trim().Split('\n');
				for (int i = 0; i < array.Length; i++)
				{
					int num2 = array[i].IndexOf('"');
					int num3 = array[i].LastIndexOf('"');
					if (num2 < 0 || num3 < 0 || num3 <= num2)
					{
						throw new InvalidOperationException("Invalid recipe line.");
					}
					string text = array[i].Substring(num2 + 1, num3 - num2 - 1);
					for (int j = 0; j < text.Length; j++)
					{
						char c = text[j];
						if (char.IsLower(c))
						{
							string text2 = dictionary[c];
							craftingRecipe.Ingredients[j + i * 6] = text2;
						}
					}
				}
				CraftingRecipesManager.m_recipes.Add(craftingRecipe);
			}
			var blocks = BlocksManager.Blocks;
			for (int i = 0; i < blocks.Length; i++)
			{
				using (var enumerator2 = blocks[i].GetProceduralCraftingRecipes().GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						var old = enumerator2.Current.Ingredients;
						var ingredients = new string[36];
						ingredients[0] = old[0];
						ingredients[1] = old[1];
						ingredients[2] = old[2];
						ingredients[6] = old[3];
						ingredients[7] = old[4];
						ingredients[8] = old[5];
						ingredients[12] = old[6];
						ingredients[13] = old[7];
						ingredients[14] = old[8];
						enumerator2.Current.Ingredients = ingredients;
						CraftingRecipesManager.m_recipes.Add(enumerator2.Current);
					}
				}
			}
			CraftingRecipesManager.m_recipes.Sort(CraftingRecipesManager.Initialize_b__1);
		}

		private static bool MatchRecipe(string[] requiredIngredients, string[] actualIngredients)
		{
			string[] array = new string[36];
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j <= 6; j++)
				{
					for (int k = 0; k <= 6; k++)
					{
						bool flip = i != 0;
						if (CraftingRecipesManager.TransformRecipe(array, requiredIngredients, k, j, flip))
						{
							bool flag = true;
							for (int l = 0; l < 36; l++)
							{
								if (!CraftingRecipesManager.CompareIngredients(array[l], actualIngredients[l]))
								{
									flag = false;
									break;
								}
							}
							if (flag)
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		private static bool TransformRecipe(string[] transformedIngredients, string[] ingredients, int shiftX, int shiftY, bool flip)
		{
			for (int i = 0; i < 36; i++)
			{
				transformedIngredients[i] = null;
			}
			for (int j = 0; j < 6; j++)
			{
				for (int k = 0; k < 6; k++)
				{
					int num = (flip ? (6 - k - 1) : k) + shiftX;
					int num2 = j + shiftY;
					string text = ingredients[k + j * 6];
					if (num >= 0 && num2 >= 0 && num < 6 && num2 < 6)
					{
						transformedIngredients[num + num2 * 6] = text;
					}
					else if (!string.IsNullOrEmpty(text))
					{
						return false;
					}
				}
			}
			return true;
		}
		public virtual string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			return GetType().ToString().Substring(5);
		}
		public virtual string GetDescription(int value)
		{
			return string.Empty;
		}
		public virtual string GetCategory(int value)
		{
			return "Items";
		}
		public virtual bool IsInteractive(SubsystemTerrain subsystemTerrain, int value)
		{
			return false;
		}
		public virtual IEnumerable<CraftingRecipe> GetProceduralCraftingRecipes()
		{
			yield break;
		}
		public virtual CraftingRecipe GetAdHocCraftingRecipe(SubsystemTerrain subsystemTerrain, string[] ingredients, float heatLevel)
		{
			return null;
		}
		public virtual bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
		{
			return false;
		}
		public virtual bool ShouldGenerateFace(SubsystemTerrain subsystemTerrain, int face, int value, int neighborValue)
		{
			return BlocksManager.Blocks[Terrain.ExtractContents(neighborValue)].IsFaceTransparent(subsystemTerrain, CellFace.OppositeFace(face), neighborValue);
		}
		public virtual int GetShadowStrength(int value)
		{
			return 0;
		}
		public virtual int GetFaceTextureSlot(int face, int value)
		{
			return 0;
		}
		public virtual string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
		{
			return string.Empty;
		}
		public virtual void GenerateTerrainVertices(Block block, BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
		{
			generator.GenerateCubeVertices(block, value, x, y, z, Color.White, geometry.OpaqueSubsetsByFace);
		}
		public virtual void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			BlocksManager.DrawCubeBlock(primitivesRenderer, value, new Vector3(size), ref matrix, color, color, environmentData);
		}
		public virtual BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
		{
			return new BlockPlacementData
			{
				Value = 0,
				CellFace = raycastResult.CellFace
			};
		}
		public virtual BlockPlacementData GetDigValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, int toolValue, TerrainRaycastResult raycastResult)
		{
			return new BlockPlacementData
			{
				Value = 0,
				CellFace = raycastResult.CellFace
			};
		}
		public virtual void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
		{
			showDebris = true;
			dropValues.Add(new BlockDropValue
			{
				Value = oldValue,
				Count = 1
			});
		}
		public virtual int GetDamage(int value)
		{
			return (Terrain.ExtractData(value) >> 4) & 0xFFF;
		}
		public virtual int SetDamage(int value, int damage)
		{
			int num = Terrain.ExtractData(value);
			num &= 0xF;
			num |= MathUtils.Clamp(damage, 0, 4095) << 4;
			return Terrain.ReplaceData(value, num);
		}
		public virtual int GetDamageDestructionValue(int value)
		{
			return 0;
		}
		public virtual int GetRotPeriod(int value)
		{
			return 0;
		}
		public virtual float GetSicknessProbability(int value)
		{
			return 0f;
		}
		public virtual float GetMeleePower(int value)
		{
			return 1;
		}
		public virtual float GetMeleeHitProbability(int value)
		{
			return 0.66f;
		}
		public virtual float GetProjectilePower(int value)
		{
			return 1;
		}
		public virtual float GetHeat(int value)
		{
			return 0f;
		}
		public virtual float GetExplosionPressure(int value)
		{
			return 0f;
		}
		public virtual bool GetExplosionIncendiary(int value)
		{
			return false;
		}
		public virtual Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
		{
			return Vector3.Zero;
		}
		public virtual Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
		{
			return new Vector3(1f);
		}
		public virtual float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
		{
			return 1f;
		}
		public virtual BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
		{
			return new BlockDebrisParticleSystem(subsystemTerrain, position, strength, 1f, Color.White, GetFaceTextureSlot(4, value));
		}
		public virtual BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
		{
			return m_defaultCollisionBoxes;
		}
		public virtual BoundingBox[] GetCustomInteractionBoxes(SubsystemTerrain terrain, int value)
		{
			return GetCustomCollisionBoxes(terrain, value);
		}
		public virtual int GetEmittedLightAmount(int value)
		{
			return 0;
		}
		public virtual float GetNutritionalValue(int value)
		{
			return 0;
		}
		public virtual bool ShouldAvoid(int value)
		{
			return false;
		}
		public virtual bool IsSwapAnimationNeeded(int oldValue, int newValue)
		{
			return true;
		}
		public virtual bool IsHeatBlocker(int value)
		{
			return true;
		}
	}
	public class BlockItem : Item
	{
		public string DefaultDisplayName;
		public string DefaultDescription = string.Empty;
		public string DefaultCategory = "Items";

		public BlockItem()
		{
			DefaultDisplayName = GetType().ToString().Substring(5);
		}

		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			return DefaultDisplayName;
		}
		public override string GetDescription(int value)
		{
			return DefaultDescription;
		}
		public override string GetCategory(int value)
		{
			return DefaultCategory;
		}
	}
	public class FlatItem : BlockItem
	{
		public int DefaultTextureSlot;
		public override int GetFaceTextureSlot(int face, int value)
		{
			return DefaultTextureSlot;
		}
		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			BlocksManager.DrawFlatBlock(primitivesRenderer, value, size, ref matrix, null, color, false, environmentData);
		}
		public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
		{
			return new Vector3
			{
				Z = 1
			};
		}
	}
	public class MeshItem : FlatItem
	{
		public BlockMesh m_standaloneBlockMesh = new BlockMesh();
		public MeshItem(string description)
		{
			DefaultDescription = description;
		}
		public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
		{
			return Vector3.One;
		}
		public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
		{
			return 0.85f;
		}
	}
	public abstract partial class ItemBlock : CubeBlock, IItemBlock
	{
		public const int Index = 246;
		public static bool Loaded;
		public static Item[] Items;
		public static Dictionary<string, int> IdTable;
		public static Item DefaultItem;
		//public Item this[int index] => Items[index];
		//public int Count => Items.Length;
		public virtual Item GetItem(ref int value)
		{
			if (Terrain.ExtractContents(value) != BlockIndex)
				return DefaultItem;
			int data = Terrain.ExtractData(value);
			return data < Items.Length ? Items[data] : DefaultItem;
		}
		public virtual int DecodeResult(string result)
		{
			if (IdTable.TryGetValue(result, out int value))
			{
				return value;
			}
			string[] array = result.Split(':');
			return Terrain.MakeBlockValue(BlocksManager.FindBlockByTypeName(array[0], true).BlockIndex, 0, array.Length >= 2 ? int.Parse(array[1], CultureInfo.InvariantCulture) : 0);
		}
		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			return GetItem(ref value).GetDisplayName(subsystemTerrain, value);
		}
		public override string GetDescription(int value)
		{
			return GetItem(ref value).GetDescription(value);
		}
		public override string GetCategory(int value)
		{
			return GetItem(ref value).GetCategory(value);
		}
		public override bool IsInteractive(SubsystemTerrain subsystemTerrain, int value)
		{
			return GetItem(ref value).IsInteractive(subsystemTerrain, value);
		}
		public override IEnumerable<CraftingRecipe> GetProceduralCraftingRecipes()
		{
			if (!Loaded)
			{
				for (int i = 0; i < CraftingRecipesManager.Recipes.Count; i++)
				{
					var ingredients = CraftingRecipesManager.Recipes[i].Ingredients;
					for (int j = 0; j < ingredients.Length; j++)
					{
						if (!string.IsNullOrEmpty(ingredients[j]) && IdTable.TryGetValue(ingredients[j], out int value))
						{
							ingredients[j] = "item:" + Terrain.ExtractData(value);
						}
					}
				}
				Loaded = true;
			}
			return base.GetProceduralCraftingRecipes();
		}
		/*public override CraftingRecipe GetAdHocCraftingRecipe(SubsystemTerrain subsystemTerrain, string[] ingredients, float heatLevel)
		{
			for (int i = 0; i < ingredients.Length; i++)
			{
				if (!string.IsNullOrEmpty(ingredients[i]))
				{
					CraftingRecipesManager.DecodeIngredient(ingredients[i], out string craftingId, out int? data);
					if (craftingId == "item")
					{
						ingredients[i] = Items[data ?? 0].ToString().Substring(5);
					}
				}
			}
			return null;
		}*/
		public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
		{
			return GetItem(ref value).IsFaceTransparent(subsystemTerrain, face, value);
		}
		public override bool ShouldGenerateFace(SubsystemTerrain subsystemTerrain, int face, int value, int neighborValue)
		{
			return GetItem(ref value).ShouldGenerateFace(subsystemTerrain, face, value, neighborValue);
		}
		public override int GetShadowStrength(int value)
		{
			return GetItem(ref value).GetShadowStrength(value);
		}
		public override int GetFaceTextureSlot(int face, int value)
		{
			return GetItem(ref value).GetFaceTextureSlot(face, value);
		}
		public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
		{
			return GetItem(ref value).GetSoundMaterialName(subsystemTerrain, value);
		}
		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
		{
			GetItem(ref value).GenerateTerrainVertices(this, generator, geometry, value, x, y, z);
		}
		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			GetItem(ref value).DrawBlock(primitivesRenderer, value, color, size, ref matrix, environmentData);
		}
		public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
		{
			int value2 = value;
			return GetItem(ref value2).GetPlacementValue(subsystemTerrain, componentMiner, value, raycastResult);
		}
		public override BlockPlacementData GetDigValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, int toolValue, TerrainRaycastResult raycastResult)
		{
			return GetItem(ref value).GetDigValue(subsystemTerrain, componentMiner, value, toolValue, raycastResult);
		}
		public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
		{
			GetItem(ref oldValue).GetDropValues(subsystemTerrain, oldValue, newValue, toolLevel, dropValues, out showDebris);
		}
		public override int GetDamage(int value)
		{
			return GetItem(ref value).GetDamage(value);
		}
		public override int SetDamage(int value, int damage)
		{
			return GetItem(ref value).SetDamage(value, damage);
		}
		public override int GetDamageDestructionValue(int value)
		{
			return GetItem(ref value).GetDamageDestructionValue(value);
		}
		public override int GetRotPeriod(int value)
		{
			return GetItem(ref value).GetRotPeriod(value);
		}
		public override float GetSicknessProbability(int value)
		{
			return GetItem(ref value).GetSicknessProbability(value);
		}
		public override float GetMeleePower(int value)
		{
			return GetItem(ref value).GetMeleePower(value);
		}
		public override float GetMeleeHitProbability(int value)
		{
			return GetItem(ref value).GetMeleeHitProbability(value);
		}
		public override float GetProjectilePower(int value)
		{
			return GetItem(ref value).GetProjectilePower(value);
		}
		public override float GetHeat(int value)
		{
			return GetItem(ref value).GetHeat(value);
		}
		public override float GetExplosionPressure(int value)
		{
			return GetItem(ref value).GetExplosionPressure(value);
		}
		public override bool GetExplosionIncendiary(int value)
		{
			return GetItem(ref value).GetExplosionIncendiary(value);
		}
		public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
		{
			return GetItem(ref value).GetIconBlockOffset(value, environmentData);
		}
		public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
		{
			return GetItem(ref value).GetIconViewOffset(value, environmentData);
		}
		public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
		{
			return GetItem(ref value).GetIconViewScale(value, environmentData);
		}
		public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
		{
			return GetItem(ref value).CreateDebrisParticleSystem(subsystemTerrain, position, value, strength);
		}
		public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
		{
			return GetItem(ref value).GetCustomCollisionBoxes(terrain, value);
		}
		public override BoundingBox[] GetCustomInteractionBoxes(SubsystemTerrain terrain, int value)
		{
			return GetItem(ref value).GetCustomInteractionBoxes(terrain, value);
		}
		public override int GetEmittedLightAmount(int value)
		{
			return GetItem(ref value).GetEmittedLightAmount(value);
		}
		public override float GetNutritionalValue(int value)
		{
			return GetItem(ref value).GetNutritionalValue(value);
		}
		public override bool ShouldAvoid(int value)
		{
			return GetItem(ref value).ShouldAvoid(value);
		}
		public override bool IsSwapAnimationNeeded(int oldValue, int newValue)
		{
			return GetItem(ref oldValue).IsSwapAnimationNeeded(oldValue, newValue);
		}
		public override bool IsHeatBlocker(int value)
		{
			return GetItem(ref value).IsHeatBlocker(value);
		}
		public override IEnumerable<int> GetCreativeValues()
		{
			if (DefaultCreativeData < 0)
			{
				return base.GetCreativeValues();
			}
			var list = new List<int>(8);
			var set = new HashSet<Item>()
			{
				DefaultItem
			};
			for (int i = 0, value = BlockIndex; set.Add(GetItem(ref value)); value = Terrain.MakeBlockValue(BlockIndex, 0, ++i))
			{
				list.Add(value);
			}
			return list;
		}
		/*public bool Equals(object other, IEqualityComparer comparer)
		{
			return ((IStructuralEquatable)Items).Equals(other, comparer);
		}
		public int GetHashCode(IEqualityComparer comparer)
		{
			return ((IStructuralEquatable)Items).GetHashCode(comparer);
		}
		public int CompareTo(object other, IComparer comparer)
		{
			return ((IStructuralComparable)Items).CompareTo(other, comparer);
		}
		public IEnumerator<Item> GetEnumerator()
		{
			return ((IEnumerable<Item>)Items).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return Items.GetEnumerator();
		}*/
	}
	public abstract class PaintableItemBlock : ItemBlock, IPaintableBlock
	{
		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			var item = GetItem(ref value);
			if (item != DefaultItem)
			{
				return SubsystemPalette.GetName(subsystemTerrain, GetPaintColor(value), item.GetDisplayName(subsystemTerrain, value));
			}
			return DefaultDisplayName;
		}
		public int? GetPaintColor(int value)
		{
			return GetColor(Terrain.ExtractData(value));
		}
		public int Paint(SubsystemTerrain subsystemTerrain, int value, int? color)
		{
			int data = Terrain.ExtractData(value);
			return Terrain.ReplaceData(value, SetColor(data, color));
		}
		public static int? GetColor(int data)
		{
			return (data & 0b1000000) != 0 ? data >> 7 & 15 : default(int?);
		}
		public static int SetColor(int data, int? color)
		{
			if (color.HasValue)
			{
				return (data & -0b1111100000111111) | 0b1000000 | (color.Value & 15) << 7;
			}
			return data & -0b1111100000111111;
		}
	}
	public class RottenEggBlock : ItemBlock
	{
		public new const int Index = 246;
	}
}
