using System;
using System.Collections.Generic;
using System.Linq;
using ItemChecklist.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader;
using static ItemChecklist.Utilities;

// Copied from my Recipe Browser mod.
namespace ItemChecklist
{
	internal class SharedUI
	{
		internal static SharedUI instance;
		internal bool updateNeeded;
		internal List<ModCategory> modCategories = new(); // TODO: copy over call? make it automatic somehow?
		internal List<ModCategory> modFilters = new(); // TODO: copy over call? make it automatic somehow?

		internal UIPanel sortsAndFiltersPanel;
		internal UIHorizontalGrid categoriesGrid;
		internal UIHorizontalGrid subCategorySortsFiltersGrid;
		internal InvisibleFixedUIHorizontalScrollbar lootGridScrollbar2;

		private Sort selectedSort;
		internal Sort SelectedSort
		{
			get { return selectedSort; }
			set
			{
				if (selectedSort != value)
				{
					updateNeeded = true;
					ItemChecklistUI.instance.UpdateNeeded();
				}
				selectedSort = value;
			}
		}

		private Category selectedCategory;
		internal Category SelectedCategory
		{
			get { return selectedCategory; }
			set
			{
				if (selectedCategory != value)
				{
					updateNeeded = true;
					ItemChecklistUI.instance.UpdateNeeded();
				}
				selectedCategory = value;
				if (selectedCategory != null && selectedCategory.sorts.Count > 0)
					SelectedSort = selectedCategory.sorts[0];
				else if (selectedCategory != null && selectedCategory.parent != null && selectedCategory.parent.sorts.Count > 0)
					SelectedSort = selectedCategory.parent.sorts[0];
			}
		}

		public SharedUI()
		{
			instance = this;
		}

		internal void Initialize()
		{
			// Sorts
			// Filters: Categories?
			// Craft and Loot Badges as well!
			// Hide with alt click?
			// show hidden toggle
			// Favorite: Only affects sort order?

			sortsAndFiltersPanel = new UIPanel();
			sortsAndFiltersPanel.SetPadding(6);
			sortsAndFiltersPanel.Top.Set(0, 0f);
			sortsAndFiltersPanel.Width.Set(-275, 1);
			sortsAndFiltersPanel.Height.Set(60, 0f);
			sortsAndFiltersPanel.BackgroundColor = new Color(73, 94, 221);
			//sortsAndFiltersPanel.SetPadding(4);
			//mainPanel.Append(sortsAndFiltersPanel);
			//additionalDragTargets.Add(sortsAndFiltersPanel);
			//SetupSortsAndCategories();

			//PopulateSortsAndFiltersPanel();

			updateNeeded = true;
		}

		internal void Update()
		{
			if (!updateNeeded) { return; }
			updateNeeded = false;

			// Delay this so we can integrate mod categories.
			if (sorts == null)
			{
				SetupSortsAndCategories();
			}

			PopulateSortsAndFiltersPanel();
		}

		internal List<Filter> availableFilters;
		private void PopulateSortsAndFiltersPanel()
		{
			var availableSorts = new List<Sort>(sorts);
			availableFilters = new List<Filter>(filters);
			//sortsAndFiltersPanel.RemoveAllChildren();
			if (subCategorySortsFiltersGrid != null)
			{
				sortsAndFiltersPanel.RemoveChild(subCategorySortsFiltersGrid);
				sortsAndFiltersPanel.RemoveChild(lootGridScrollbar2);
			}

			bool doTopRow = false;
			if (categoriesGrid == null)
			{
				doTopRow = true;

				categoriesGrid = new UIHorizontalGrid();
				categoriesGrid.Width.Set(0, 1f);
				categoriesGrid.Height.Set(26, 0f);
				categoriesGrid.ListPadding = 2f;
				categoriesGrid.OnScrollWheel += ItemChecklistUI.OnScrollWheel_FixHotbarScroll;
				sortsAndFiltersPanel.Append(categoriesGrid);
				categoriesGrid.drawArrows = true;

				var lootGridScrollbar = new InvisibleFixedUIHorizontalScrollbar(UISystem.ItemChecklistInterface);
				lootGridScrollbar.SetView(100f, 1000f);
				lootGridScrollbar.Width.Set(0, 1f);
				lootGridScrollbar.Top.Set(0, 0f);
				sortsAndFiltersPanel.Append(lootGridScrollbar);
				categoriesGrid.SetScrollbar(lootGridScrollbar);
			}

			subCategorySortsFiltersGrid = new UIHorizontalGrid();
			subCategorySortsFiltersGrid.Width.Set(0, 1f);
			subCategorySortsFiltersGrid.Top.Set(26, 0f);
			subCategorySortsFiltersGrid.Height.Set(26, 0f);
			subCategorySortsFiltersGrid.ListPadding = 2f;
			subCategorySortsFiltersGrid.OnScrollWheel += ItemChecklistUI.OnScrollWheel_FixHotbarScroll;
			sortsAndFiltersPanel.Append(subCategorySortsFiltersGrid);
			subCategorySortsFiltersGrid.drawArrows = true;

			lootGridScrollbar2 = new InvisibleFixedUIHorizontalScrollbar(UISystem.ItemChecklistInterface);
			lootGridScrollbar2.SetView(100f, 1000f);
			lootGridScrollbar2.Width.Set(0, 1f);
			lootGridScrollbar2.Top.Set(28, 0f);
			sortsAndFiltersPanel.Append(lootGridScrollbar2);
			subCategorySortsFiltersGrid.SetScrollbar(lootGridScrollbar2);

			//sortsAndFiltersPanelGrid = new UIGrid();
			//sortsAndFiltersPanelGrid.Width.Set(0, 1);
			//sortsAndFiltersPanelGrid.Height.Set(0, 1);
			//sortsAndFiltersPanel.Append(sortsAndFiltersPanelGrid);

			//sortsAndFiltersPanelGrid2 = new UIGrid();
			//sortsAndFiltersPanelGrid2.Width.Set(0, 1);
			//sortsAndFiltersPanelGrid2.Height.Set(0, 1);
			//sortsAndFiltersPanel.Append(sortsAndFiltersPanelGrid2);

			int count = 0;

			var visibleCategories = new List<Category>();
			var visibleSubCategories = new List<Category>();
			int left = 0;
			foreach (var category in categories)
			{
				category.button.selected = false;
				visibleCategories.Add(category);
				bool meOrChildSelected = SelectedCategory == category;
				foreach (var subcategory in category.subCategories)
				{
					subcategory.button.selected = false;
					meOrChildSelected |= subcategory == SelectedCategory;
				}
				if (meOrChildSelected)
				{
					visibleSubCategories.AddRange(category.subCategories);
					category.button.selected = true;
				}
			}

			if (doTopRow)
				foreach (var category in visibleCategories)
				{
					var container = new UISortableElement(++count);
					container.Width.Set(24, 0);
					container.Height.Set(24, 0);
					//category.button.Left.Pixels = left;
					//if (category.parent != null)
					//	container.OrderIndex
					//	category.button.Top.Pixels = 12;
					//sortsAndFiltersPanel.Append(category.button);
					container.Append(category.button);
					categoriesGrid.Add(container);
					left += 26;
				}

			//UISortableElement spacer = new UISortableElement(++count);
			//spacer.Width.Set(0, 1);
			//sortsAndFiltersPanelGrid2.Add(spacer);

			foreach (var category in visibleSubCategories)
			{
				var container = new UISortableElement(++count);
				container.Width.Set(24, 0);
				container.Height.Set(24, 0);
				container.Append(category.button);
				subCategorySortsFiltersGrid.Add(container);
				left += 26;
			}

			if (visibleSubCategories.Count > 0)
			{
				var container2 = new UISortableElement(++count);
				container2.Width.Set(24, 0);
				container2.Height.Set(24, 0);
				var image = new UIImage(ItemChecklist.instance.GetTexture("UIElements/spacer"));
				//image.Left.Set(6, 0);
				image.HAlign = 0.5f;
				container2.Append(image);
				subCategorySortsFiltersGrid.Add(container2);
			}

			// add to sorts here
			if (SelectedCategory != null)
			{
				SelectedCategory.button.selected = true;
				SelectedCategory.ParentAddToSorts(availableSorts);
			}

			left = 0;
			foreach (var sort in availableSorts)
			{
				sort.button.selected = SelectedSort == sort; // TODO: SelectedSort no longer valid
				//sort.button.Left.Pixels = left;
				//sort.button.Top.Pixels = 24;
				//sort.button.Width
				//grid.Add(sort.button);
				var container = new UISortableElement(++count);
				container.Width.Set(24, 0);
				container.Height.Set(24, 0);
				container.Append(sort.button);
				subCategorySortsFiltersGrid.Add(container);
				//sortsAndFiltersPanel.Append(sort.button);
				left += 26;
			}
			if (!availableSorts.Contains(instance.SelectedSort))
			{
				availableSorts[0].button.selected = true;
				instance.SelectedSort = availableSorts[0];
				updateNeeded = false;
			}

			if (instance.filters.Count > 0)
			{
				var container2 = new UISortableElement(++count);
				container2.Width.Set(24, 0);
				container2.Height.Set(24, 0);
				var image = new UIImage(ItemChecklist.instance.GetTexture("UIElements/spacer"));
				image.HAlign = 0.5f;
				container2.Append(image);
				subCategorySortsFiltersGrid.Add(container2);

				foreach (var item in instance.filters)
				{
					var container = new UISortableElement(++count);
					container.Width.Set(24, 0);
					container.Height.Set(24, 0);
					container.Append(item.button);
					subCategorySortsFiltersGrid.Add(container);
				}
			}
		}

		private List<int> preloadItemTypes = new()
		{
			ItemID.MetalDetector,
			ItemID.SpellTome,
			ItemID.GoldBroadsword,
			ItemID.GoldenShower,
			ItemID.FlintlockPistol,
			ItemID.Shuriken,
			ItemID.SlimeStaff,
			ItemID.DD2LightningAuraT1Popper,
			ItemID.SilverHelmet,
			ItemID.SilverChainmail,
			ItemID.SilverGreaves,
			ItemID.Sign,
			ItemID.IronAnvil,
			ItemID.PearlstoneBrickWall,
			ItemID.EoCShield,
			ItemID.ZephyrFish,
			ItemID.FairyBell,
			ItemID.MechanicalSkull,
			ItemID.SlimySaddle,
			ItemID.OrangeDye,
			ItemID.BiomeHairDye,
			ItemID.FallenStarfish,
			ItemID.HermesBoots,
			ItemID.LeafWings,
			ItemID.Minecart,
			ItemID.HealingPotion,
			ItemID.ManaPotion,
			ItemID.RagePotion,
			ItemID.AlphabetStatueA,
			ItemID.GoldChest,
			ItemID.PaintingMartiaLisa,
			ItemID.HeartStatue,
			ItemID.Wire,
			ItemID.PurificationPowder,
			ItemID.Extractinator,
			ItemID.UnicornonaStick,
			ItemID.SilverHelmet,
			ItemID.SilverChainmail,
			ItemID.SilverGreaves,
			ItemID.ZephyrFish,
			ItemID.FairyBell,
			ItemID.Sign,
			ItemID.PearlstoneBrickWall,
			ItemID.FallenStarfish,
			ItemID.HealingPotion,
			ItemID.ManaPotion,
			ItemID.RagePotion,
			ItemID.OrangeDye,
			ItemID.BiomeHairDye,
			ItemID.Candelabra,
			ItemID.GrandfatherClock
		};

		internal List<Category> categories;
		internal List<Filter> filters;
		internal List<Sort> sorts;
		internal List<int> craftingTiles;
		private void SetupSortsAndCategories()
		{
			var tileUsageCounts = new Dictionary<int, int>();
			for (int i = 0; i < Recipe.numRecipes; i++)
			{
				for (int j = 0; j < 15; j++)
				{
					if (Main.recipe[i].requiredTile[j] == -1)
						break;
					tileUsageCounts.TryGetValue(Main.recipe[i].requiredTile[j], out int currentCount);
					tileUsageCounts[Main.recipe[i].requiredTile[j]] = currentCount + 1;
				}
			}
			craftingTiles = tileUsageCounts.Select(x => x.Key).ToList();

			List<int> yoyos = new();
			for (int i = 0; i < ItemID.Sets.Yoyo.Length; ++i)
			{
				if (ItemID.Sets.Yoyo[i])
				{
					yoyos.Add(i);
				}
			}

			foreach (int type in preloadItemTypes.Concat(yoyos))
				Main.instance.LoadItem(type);

			Texture2D terrariaSort = ResizeImage(TextureAssets.InventorySort[1].Value, 24, 24);
			Texture2D rarity = ResizeImage(TextureAssets.Item[ItemID.MetalDetector].Value, 24, 24);

			sorts = new List<Sort>
			{
				new Sort("ItemID", "Images/sortItemID", (x,y)=>x.type.CompareTo(y.type), x=>x.type.ToString()),
				new Sort("Value", "Images/sortValue", (x,y)=>x.value.CompareTo(y.value), x=>x.value.ToString()),
				new Sort("Alphabetical", "Images/sortAZ", (x,y)=>x.Name.CompareTo(y.Name), x=>x.Name.ToString()),
				new Sort("Rarity", rarity, (x,y)=> x.rare==y.rare ? x.value.CompareTo(y.value) : Math.Abs(x.rare).CompareTo(Math.Abs(y.rare)), x=>x.rare.ToString()),
				new Sort("Terraria Sort", terrariaSort, (x,y)=> -ItemChecklistUI.vanillaIDsInSortOrder[x.type].CompareTo(ItemChecklistUI.vanillaIDsInSortOrder[y.type]), x=>ItemChecklistUI.vanillaIDsInSortOrder[x.type].ToString()),
			};

			Texture2D materialsIcon = StackResizeImage(new[] { TextureAssets.Item[ItemID.SpellTome] .Value}, 24, 24);
			filters = new List<Filter>
			{
				new Filter("Materials", x=>ItemID.Sets.IsAMaterial[x.type], materialsIcon),
			};

			// TODOS: Vanity armor, grapple, cart, potions buffs
			// 24x24 pixels

			Texture2D smallMelee = ResizeImage(TextureAssets.Item[ItemID.GoldBroadsword].Value, 24, 24);
			Texture2D smallYoyo = ResizeImage(TextureAssets.Item[Main.rand.Next(yoyos)].Value, 24, 24); //Main.rand.Next(ItemID.Sets.Yoyo) ItemID.Yelets
			Texture2D smallMagic = ResizeImage(TextureAssets.Item[ItemID.GoldenShower].Value, 24, 24);
			Texture2D smallRanged = ResizeImage(TextureAssets.Item[ItemID.FlintlockPistol].Value, 24, 24);
			Texture2D smallThrown = ResizeImage(TextureAssets.Item[ItemID.Shuriken].Value, 24, 24);
			Texture2D smallSummon = ResizeImage(TextureAssets.Item[ItemID.SlimeStaff].Value, 24, 24);
			Texture2D smallSentry = ResizeImage(TextureAssets.Item[ItemID.DD2LightningAuraT1Popper].Value, 24, 24);
			Texture2D smallHead = ResizeImage(TextureAssets.Item[ItemID.SilverHelmet].Value, 24, 24);
			Texture2D smallBody = ResizeImage(TextureAssets.Item[ItemID.SilverChainmail].Value, 24, 24);
			Texture2D smallLegs = ResizeImage(TextureAssets.Item[ItemID.SilverGreaves].Value, 24, 24);
			Texture2D smallTiles = ResizeImage(TextureAssets.Item[ItemID.Sign].Value, 24, 24);
			Texture2D smallCraftingStation = ResizeImage(TextureAssets.Item[ItemID.IronAnvil].Value, 24, 24);
			Texture2D smallWalls = ResizeImage(TextureAssets.Item[ItemID.PearlstoneBrickWall].Value, 24, 24);
			Texture2D smallExpert = ResizeImage(TextureAssets.Item[ItemID.EoCShield].Value, 24, 24);
			Texture2D smallPets = ResizeImage(TextureAssets.Item[ItemID.ZephyrFish].Value, 24, 24);
			Texture2D smallLightPets = ResizeImage(TextureAssets.Item[ItemID.FairyBell].Value, 24, 24);
			Texture2D smallBossSummon = ResizeImage(TextureAssets.Item[ItemID.MechanicalSkull].Value, 24, 24);
			Texture2D smallMounts = ResizeImage(TextureAssets.Item[ItemID.SlimySaddle].Value, 24, 24);
			Texture2D smallDyes = ResizeImage(TextureAssets.Item[ItemID.OrangeDye].Value, 24, 24);
			Texture2D smallHairDye = ResizeImage(TextureAssets.Item[ItemID.BiomeHairDye].Value, 24, 24);
			Texture2D smallQuestFish = ResizeImage(TextureAssets.Item[ItemID.FallenStarfish].Value, 24, 24);
			Texture2D smallAccessories = ResizeImage(TextureAssets.Item[ItemID.HermesBoots].Value, 24, 24);
			Texture2D smallWings = ResizeImage(TextureAssets.Item[ItemID.LeafWings].Value, 24, 24);
			Texture2D smallCarts = ResizeImage(TextureAssets.Item[ItemID.Minecart].Value, 24, 24);
			Texture2D smallHealth = ResizeImage(TextureAssets.Item[ItemID.HealingPotion].Value, 24, 24);
			Texture2D smallMana = ResizeImage(TextureAssets.Item[ItemID.ManaPotion].Value, 24, 24);
			Texture2D smallBuff = ResizeImage(TextureAssets.Item[ItemID.RagePotion].Value, 24, 24);
			Texture2D smallAll = ResizeImage(TextureAssets.Item[ItemID.AlphabetStatueA].Value, 24, 24);
			Texture2D smallContainer = ResizeImage(TextureAssets.Item[ItemID.GoldChest].Value, 24, 24);
			Texture2D smallPaintings = ResizeImage(TextureAssets.Item[ItemID.PaintingMartiaLisa].Value, 24, 24);
			Texture2D smallStatue = ResizeImage(TextureAssets.Item[ItemID.HeartStatue].Value, 24, 24);
			Texture2D smallWiring = ResizeImage(TextureAssets.Item[ItemID.Wire].Value, 24, 24);
			Texture2D smallConsumables = ResizeImage(TextureAssets.Item[ItemID.PurificationPowder].Value, 24, 24);
			Texture2D smallExtractinator = ResizeImage(TextureAssets.Item[ItemID.Extractinator].Value, 24, 24);
			Texture2D smallOther = ResizeImage(TextureAssets.Item[ItemID.UnicornonaStick].Value, 24, 24);

			Texture2D smallArmor = StackResizeImage(new[] { TextureAssets.Item[ItemID.SilverHelmet].Value, TextureAssets.Item[ItemID.SilverChainmail].Value, TextureAssets.Item[ItemID.SilverGreaves] .Value}, 24, 24);
			Texture2D smallPetsLightPets = StackResizeImage(new[] { TextureAssets.Item[ItemID.ZephyrFish].Value, TextureAssets.Item[ItemID.FairyBell] .Value}, 24, 24);
			Texture2D smallPlaceables = StackResizeImage(new[] { TextureAssets.Item[ItemID.Sign].Value, TextureAssets.Item[ItemID.PearlstoneBrickWall] .Value}, 24, 24);
			Texture2D smallWeapons = StackResizeImage(new[] { smallMelee, smallMagic, smallThrown }, 24, 24);
			Texture2D smallTools = StackResizeImage(new[] { ItemChecklist.instance.GetTexture("Images/sortPick"), ItemChecklist.instance.GetTexture("Images/sortAxe"), ItemChecklist.instance.GetTexture("Images/sortHammer") }, 24, 24);
			Texture2D smallFishing = StackResizeImage(new[] { ItemChecklist.instance.GetTexture("Images/sortFish"), ItemChecklist.instance.GetTexture("Images/sortBait"), TextureAssets.Item[ItemID.FallenStarfish] .Value}, 24, 24);
			Texture2D smallPotions = StackResizeImage(new[] { TextureAssets.Item[ItemID.HealingPotion].Value, TextureAssets.Item[ItemID.ManaPotion].Value, TextureAssets.Item[ItemID.RagePotion] .Value}, 24, 24);
			Texture2D smallBothDyes = StackResizeImage(new[] { TextureAssets.Item[ItemID.OrangeDye].Value, TextureAssets.Item[ItemID.BiomeHairDye] .Value}, 24, 24);
			Texture2D smallSortTiles = StackResizeImage(new[] { TextureAssets.Item[ItemID.Candelabra].Value, TextureAssets.Item[ItemID.GrandfatherClock] .Value}, 24, 24);

			// Potions, other?
			// should inherit children?
			// should have other category?
			if (WorldGen.statueList == null)
				WorldGen.SetupStatueList();

			categories = new List<Category>
			{
				new("All", _ => true, smallAll),
				new("Weapons" /*, x=>x.damage>0*/, _ => false, smallWeapons)
				{
					//"Images/sortDamage"
					subCategories = new List<Category>
					{
						new("Melee", x => x.CountsAsClass(DamageClass.Melee), smallMelee),
						new("Yoyo", x => ItemID.Sets.Yoyo[x.type], smallYoyo),
						new("Magic", x => x.CountsAsClass(DamageClass.Magic), smallMagic),
						new("Ranged", x => x.CountsAsClass(DamageClass.Ranged) && x.ammo == 0, smallRanged) // TODO and ammo no
						{
							sorts = new List<Sort>
								{ new("Use Ammo Type", "Images/sortAmmo", (x, y) => x.useAmmo.CompareTo(y.useAmmo), x => x.useAmmo.ToString()) }
						},
						new("Throwing", x => x.CountsAsClass(DamageClass.Throwing), smallThrown),
						new("Summon", x => x.CountsAsClass(DamageClass.Summon) && !x.sentry, smallSummon),
						new("Sentry", x => x.CountsAsClass(DamageClass.Summon) && x.sentry, smallSentry)
					},
					sorts = new List<Sort> { new("Damage", "Images/sortDamage", (x, y) => x.damage.CompareTo(y.damage), x => x.damage.ToString()) } // ascending
				},
				new("Tools" /*,x=>x.pick>0||x.axe>0||x.hammer>0*/, _ => false, smallTools)
				{
					subCategories = new List<Category>
					{
						new("Pickaxes", x => x.pick > 0, "Images/sortPick")
							{ sorts = new List<Sort> { new("Pick Power", "Images/sortPick", (x, y) => x.pick.CompareTo(y.pick), x => x.pick.ToString()) } },
						new("Axes", x => x.axe > 0, "Images/sortAxe")
							{ sorts = new List<Sort> { new("Axe Power", "Images/sortAxe", (x, y) => x.axe.CompareTo(y.axe), x => (x.axe * 5).ToString()) } },
						new("Hammers", x => x.hammer > 0, "Images/sortHammer")
						{
							sorts = new List<Sort>
								{ new("Hammer Power", "Images/sortHammer", (x, y) => x.hammer.CompareTo(y.hammer), x => x.hammer.ToString()) }
						}
					}
				},
				new("Armor" /*,  x=>x.headSlot!=-1||x.bodySlot!=-1||x.legSlot!=-1*/, _ => false, smallArmor)
				{
					subCategories = new List<Category>
					{
						new("Head", x => x.headSlot != -1, smallHead),
						new("Body", x => x.bodySlot != -1, smallBody),
						new("Legs", x => x.legSlot != -1, smallLegs)
					},
					sorts = new List<Sort> { new("Defense", "Images/sortDefense", (x, y) => x.defense.CompareTo(y.defense), x => x.defense.ToString()) }
				},
				new("Tiles", x => x.createTile != -1, smallTiles)
				{
					subCategories = new List<Category>
					{
						new("Crafting Stations", x => craftingTiles.Contains(x.createTile), smallCraftingStation),
						new("Containers", x => x.createTile != -1 && Main.tileContainer[x.createTile], smallContainer),
						new("Wiring", x => ItemID.Sets.SortingPriorityWiring[x.type] > -1, smallWiring),
						new("Statues", x => WorldGen.statueList.Any(point => point.X == x.createTile && point.Y == x.placeStyle), smallStatue)
						//new Category("Paintings", x=>ItemID.Sets.SortingPriorityPainting[x.type] > -1, smallPaintings), // oops, this is painting tools not painting tiles
						//new Category("5x4", x=>{
						//	if(x.createTile!=-1)
						//	{
						//		var tod = Terraria.ObjectData.TileObjectData.GetTileData(x.createTile, x.placeStyle);
						//		return tod != null && tod.Width == 5 && tod.Height == 4;
						//	}
						//	return false;
						//} , smallContainer),
					},
					// wires

					// Banners
					sorts = new List<Sort>
					{
						new("Place Tile", smallSortTiles,
							(x, y) => x.createTile == y.createTile ? x.placeStyle.CompareTo(y.placeStyle) : x.createTile.CompareTo(y.createTile),
							x => $"{x.createTile},{x.placeStyle}")
					}
				},
				new("Walls", x => x.createWall != -1, smallWalls),
				new("Accessories", x => x.accessory, smallAccessories)
				{
					subCategories = new List<Category>
					{
						new("Wings", x => x.wingSlot > 0, smallWings)
					}
				},
				new("Ammo", x => x.ammo != 0, ItemChecklist.instance.GetTexture("Images/sortAmmo"))
				{
					sorts = new List<Sort> { new("Ammo Type", "Images/sortAmmo", (x, y) => x.ammo.CompareTo(y.ammo), x => $"{x.ammo}") }
					// TODO: Filters/Subcategories for all ammo types?
				},
				new("Potions", x => x.UseSound is { Style: 3 }, smallPotions)
				{
					subCategories = new List<Category>
					{
						new("Health Potions", x => x.healLife > 0, smallHealth)
							{ sorts = new List<Sort> { new("Heal Life", smallHealth, (x, y) => x.healLife.CompareTo(y.healLife), x => $"{x.healLife}") } },
						new("Mana Potions", x => x.healMana > 0, smallMana)
							{ sorts = new List<Sort> { new("Heal Mana", smallMana, (x, y) => x.healMana.CompareTo(y.healMana), x => $"{x.healMana}") } },
						new("Buff Potions", x => x.UseSound is { Style: 3 } && x.buffType > 0, smallBuff)
						// Todo: Automatic other category?
					}
				},
				new("Expert", x => x.expert, smallExpert),
				new("Pets" /*, x=> x.buffType > 0 && (Main.vanityPet[x.buffType] || Main.lightPet[x.buffType])*/, _ => false, smallPetsLightPets)
				{
					subCategories = new List<Category>
					{
						new("Pets", x => Main.vanityPet[x.buffType], smallPets),
						new("Light Pets", x => Main.lightPet[x.buffType], smallLightPets)
					}
				},
				new("Mounts", x => x.mountType != -1, smallMounts)
				{
					subCategories = new List<Category>
					{
						new("Carts", x => x.mountType != -1 && MountID.Sets.Cart[x.mountType],
							smallCarts) // TODO: need mountType check? inherited parent logic or parent unions children?
					}
				},
				new("Dyes", _ => false, smallBothDyes)
				{
					subCategories = new List<Category>
					{
						new("Dyes", x => x.dye != 0, smallDyes),
						new("Hair Dyes", x => x.hairDye != -1, smallHairDye)
					}
				},
				new("Boss Summons",
					x => ItemID.Sets.SortingPriorityBossSpawns[x.type] != -1 &&
						 x.type != ItemID.LifeCrystal &&
						 x.type != ItemID.ManaCrystal &&
						 x.type != ItemID.CellPhone &&
						 x.type != ItemID.IceMirror &&
						 x.type != ItemID.MagicMirror &&
						 x.type != ItemID.LifeFruit &&
						 x.netID != ItemID.TreasureMap ||
						 x.netID == ItemID.PirateMap, smallBossSummon)
				{
					// vanilla bug.
					sorts = new List<Sort>
					{
						new("Progression Order", "Images/sortDamage",
							(x, y) => ItemID.Sets.SortingPriorityBossSpawns[x.type].CompareTo(ItemID.Sets.SortingPriorityBossSpawns[y.type]),
							x => $"{ItemID.Sets.SortingPriorityBossSpawns[x.type]}")
					}
				},
				new("Consumables", x => x.consumable, smallConsumables),
				new("Fishing" /*, x=> x.fishingPole > 0 || x.bait>0|| x.questItem*/, _ => false, smallFishing)
				{
					subCategories = new List<Category>
					{
						new("Poles", x => x.fishingPole > 0, "Images/sortFish")
						{
							sorts = new List<Sort>
								{ new("Pole Power", "Images/sortFish", (x, y) => x.fishingPole.CompareTo(y.fishingPole), x => $"{x.fishingPole}") }
						},
						new("Bait", x => x.bait > 0, "Images/sortBait")
							{ sorts = new List<Sort> { new("Bait Power", "Images/sortBait", (x, y) => x.bait.CompareTo(y.bait), x => $"{x.bait}") } },
						new("Quest Fish", x => x.questItem, smallQuestFish)
					}
				},
				new("Extractinator", x => ItemID.Sets.ExtractinatorMode[x.type] > -1, smallExtractinator),
				//modCategory,
				new("Other", BelongsInOther, smallOther)
			};

			/* Think about this one.
			foreach (var modCategory in RecipeBrowser.instance.modCategories)
			{
				if (string.IsNullOrEmpty(modCategory.parent))
				{
					categories.Insert(categories.Count - 2, new Category(modCategory.name, modCategory.belongs, modCategory.icon));
				}
				else
				{
					foreach (var item in categories)
					{
						if (item.name == modCategory.parent)
						{
							item.subCategories.Add(new Category(modCategory.name, modCategory.belongs, modCategory.icon));
						}
					}
				}
			}

			foreach (var modCategory in RecipeBrowser.instance.modFilters)
			{
				filters.Add(new Filter(modCategory.name, modCategory.belongs, modCategory.icon));
			}
			*/

			foreach (Category parent in categories)
			foreach (Category child in parent.subCategories)
				child.parent = parent; // 3 levels?

			SelectedSort = sorts[0];
			SelectedCategory = categories[0];
		}

		private bool BelongsInOther(Item item)
		{
			var cats = categories.Skip(1).Take(categories.Count - 2);
			return cats.All(category => !category.BelongsRecursive(item));
		}
	}

	internal class Filter
	{
		internal string name;
		internal Predicate<Item> belongs;
		internal List<Category> subCategories; //
		internal List<Sort> sorts;
		internal UISilentImageButton button;
		internal Category parent;

		public Filter(string name, Predicate<Item> belongs, Texture2D texture)
		{
			this.name = name;
			subCategories = new List<Category>();
			sorts = new List<Sort>();
			this.belongs = belongs;

			button = new UISilentImageButton(texture, name);
			button.OnClick += (_, _) =>
			{
				button.selected = !button.selected;
				ItemChecklistUI.instance.UpdateNeeded();
				//Main.NewText("clicked on " + button.hoverText);
			};
		}
	}

	internal class Sort
	{
		internal Func<Item, Item, int> sort;
		internal Func<Item, string> badge;
		internal UISilentImageButton button;

		public Sort(string hoverText, Texture2D texture, Func<Item, Item, int> sort, Func<Item, string> badge)
		{
			this.sort = sort;
			this.badge = badge;
			button = new UISilentImageButton(texture, hoverText);
			button.OnClick += (_, _) =>
			{
				SharedUI.instance.SelectedSort = this;
			};
		}

		public Sort(string hoverText, string textureFileName, Func<Item, Item, int> sort, Func<Item, string> badge) : this(hoverText, ItemChecklist.instance.GetTexture(textureFileName), sort, badge)
		{
		}
	}

	// Represents a requested Category or Filter.
	internal class ModCategory
	{
		internal string name;
		internal string parent;
		internal Texture2D icon;
		internal Predicate<Item> belongs;
		public ModCategory(string name, string parent, Texture2D icon, Predicate<Item> belongs)
		{
			this.name = name;
			this.parent = parent;
			this.icon = icon;
			this.belongs = belongs;
		}
	}

	// Can belong to 2 Category? -> ??
	// Separate filter? => yes, but Separate conditional filters?
	// All children belong to parent -> yes.
	internal class Category // Filter
	{
		internal string name;
		internal Predicate<Item> belongs;
		internal List<Category> subCategories;
		internal List<Sort> sorts;
		internal UISilentImageButton button;
		internal Category parent;

		public Category(string name, Predicate<Item> belongs, Texture2D texture = null)
		{
			texture ??= ItemChecklist.instance.GetTexture("Images/sortAmmo");
			this.name = name;
			subCategories = new List<Category>();
			sorts = new List<Sort>();
			this.belongs = belongs;

			button = new UISilentImageButton(texture, name);
			button.OnClick += (_, _) =>
			{
				//Main.NewText("clicked on " + button.hoverText);
				SharedUI.instance.SelectedCategory = this;
			};
		}

		public Category(string name, Predicate<Item> belongs, string textureFileName) : this(name, belongs, ItemChecklist.instance.GetTexture(textureFileName))
		{
		}

		internal bool BelongsRecursive(Item item)
		{
			return belongs(item) || subCategories.Any(x => x.belongs(item));
		}

		internal void ParentAddToSorts(List<Sort> availableSorts)
		{
			parent?.ParentAddToSorts(availableSorts);
			availableSorts.AddRange(sorts);
		}
	}
}
