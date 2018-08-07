﻿using ItemChecklist.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using static ItemChecklist.Utilities;

// Copied from my Recipe Browser mod.
namespace ItemChecklist
{
	class SharedUI
	{
		internal static SharedUI instance;
		internal bool updateNeeded;
		internal List<ModCategory> modCategories = new List<ModCategory>(); // TODO: copy over call? make it automatic somehow?
		internal List<ModCategory> modFilters = new List<ModCategory>(); // TODO: copy over call? make it automatic somehow?

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

				var lootGridScrollbar = new InvisibleFixedUIHorizontalScrollbar(ItemChecklist.ItemChecklistInterface);
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

			lootGridScrollbar2 = new InvisibleFixedUIHorizontalScrollbar(ItemChecklist.ItemChecklistInterface);
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
				sort.button.selected = false;
				if (SelectedSort == sort) // TODO: SelectedSort no longwe valid
					sort.button.selected = true;
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
			if (!availableSorts.Contains(SharedUI.instance.SelectedSort))
			{
				availableSorts[0].button.selected = true;
				SharedUI.instance.SelectedSort = availableSorts[0];
				updateNeeded = false;
			}

			if (SharedUI.instance.filters.Count > 0)
			{
				var container2 = new UISortableElement(++count);
				container2.Width.Set(24, 0);
				container2.Height.Set(24, 0);
				var image = new UIImage(ItemChecklist.instance.GetTexture("UIElements/spacer"));
				image.HAlign = 0.5f;
				container2.Append(image);
				subCategorySortsFiltersGrid.Add(container2);

				foreach (var item in SharedUI.instance.filters)
				{
					var container = new UISortableElement(++count);
					container.Width.Set(24, 0);
					container.Height.Set(24, 0);
					container.Append(item.button);
					subCategorySortsFiltersGrid.Add(container);
				}
			}
		}

		internal List<Category> categories;
		internal List<Filter> filters;
		internal List<Sort> sorts;
		internal List<int> craftingTiles;
		private void SetupSortsAndCategories()
		{
			var tileUsageCounts = new Dictionary<int, int>();
			int currentCount;
			for (int i = 0; i < Recipe.numRecipes; i++)
			{
				for (int j = 0; j < 15; j++)
				{
					if (Main.recipe[i].requiredTile[j] == -1)
						break;
					tileUsageCounts.TryGetValue(Main.recipe[i].requiredTile[j], out currentCount);
					tileUsageCounts[Main.recipe[i].requiredTile[j]] = currentCount + 1;
				}
			}
			craftingTiles = tileUsageCounts.Select(x => x.Key).ToList();

			Texture2D terrariaSort = ResizeImage(Main.inventorySortTexture[1], 24, 24); 
			Texture2D rarity = ResizeImage(Main.itemTexture[ItemID.MetalDetector], 24, 24);

			sorts = new List<Sort>()
			{
				new Sort("ItemID", "Images/sortItemID", (x,y)=>x.type.CompareTo(y.type), x=>x.type.ToString()),
				new Sort("Value", "Images/sortValue", (x,y)=>x.value.CompareTo(y.value), x=>x.value.ToString()),
				new Sort("Alphabetical", "Images/sortAZ", (x,y)=>x.Name.CompareTo(y.Name), x=>x.Name.ToString()),
				new Sort("Rarity", rarity, (x,y)=> x.rare==y.rare ? x.value.CompareTo(y.value) : Math.Abs(x.rare).CompareTo(Math.Abs(y.rare)), x=>x.rare.ToString()),
				new Sort("Terraria Sort", terrariaSort, (x,y)=> -ItemChecklistUI.vanillaIDsInSortOrder[x.type].CompareTo(ItemChecklistUI.vanillaIDsInSortOrder[y.type]), x=>ItemChecklistUI.vanillaIDsInSortOrder[x.type].ToString()),
			};

			Texture2D materialsIcon = Utilities.StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.SpellTome] }, 24, 24);
			filters = new List<Filter>()
			{
				new Filter("Materials", x=>ItemID.Sets.IsAMaterial[x.type], materialsIcon),
			};

			// TODOS: Vanity armor, grapple, cart, potions buffs
			// 24x24 pixels

			List<int> yoyos = new List<int>();
			for (int i = 0; i < ItemID.Sets.Yoyo.Length; ++i)
			{
				if (ItemID.Sets.Yoyo[i])
				{
					yoyos.Add(i);
				}
			}

			Texture2D smallMelee = ResizeImage(Main.itemTexture[ItemID.GoldBroadsword], 24, 24);
			Texture2D smallYoyo = ResizeImage(Main.itemTexture[Main.rand.Next(yoyos)], 24, 24); //Main.rand.Next(ItemID.Sets.Yoyo) ItemID.Yelets
			Texture2D smallMagic = ResizeImage(Main.itemTexture[ItemID.GoldenShower], 24, 24);
			Texture2D smallRanged = ResizeImage(Main.itemTexture[ItemID.FlintlockPistol], 24, 24);
			Texture2D smallThrown = ResizeImage(Main.itemTexture[ItemID.Shuriken], 24, 24);
			Texture2D smallSummon = ResizeImage(Main.itemTexture[ItemID.SlimeStaff], 24, 24);
			Texture2D smallSentry = ResizeImage(Main.itemTexture[ItemID.DD2LightningAuraT1Popper], 24, 24);
			Texture2D smallHead = ResizeImage(Main.itemTexture[ItemID.SilverHelmet], 24, 24);
			Texture2D smallBody = ResizeImage(Main.itemTexture[ItemID.SilverChainmail], 24, 24);
			Texture2D smallLegs = ResizeImage(Main.itemTexture[ItemID.SilverGreaves], 24, 24);
			Texture2D smallTiles = ResizeImage(Main.itemTexture[ItemID.Sign], 24, 24);
			Texture2D smallCraftingStation = ResizeImage(Main.itemTexture[ItemID.IronAnvil], 24, 24);
			Texture2D smallWalls = ResizeImage(Main.itemTexture[ItemID.PearlstoneBrickWall], 24, 24);
			Texture2D smallExpert = ResizeImage(Main.itemTexture[ItemID.EoCShield], 24, 24);
			Texture2D smallPets = ResizeImage(Main.itemTexture[ItemID.ZephyrFish], 24, 24);
			Texture2D smallLightPets = ResizeImage(Main.itemTexture[ItemID.FairyBell], 24, 24);
			Texture2D smallBossSummon = ResizeImage(Main.itemTexture[ItemID.MechanicalSkull], 24, 24);
			Texture2D smallMounts = ResizeImage(Main.itemTexture[ItemID.SlimySaddle], 24, 24);
			Texture2D smallDyes = ResizeImage(Main.itemTexture[ItemID.OrangeDye], 24, 24);
			Texture2D smallHairDye = ResizeImage(Main.itemTexture[ItemID.BiomeHairDye], 24, 24);
			Texture2D smallQuestFish = ResizeImage(Main.itemTexture[ItemID.FallenStarfish], 24, 24);
			Texture2D smallAccessories = ResizeImage(Main.itemTexture[ItemID.HermesBoots], 24, 24);
			Texture2D smallWings = ResizeImage(Main.itemTexture[ItemID.LeafWings], 24, 24);
			Texture2D smallCarts = ResizeImage(Main.itemTexture[ItemID.Minecart], 24, 24);
			Texture2D smallHealth = ResizeImage(Main.itemTexture[ItemID.HealingPotion], 24, 24);
			Texture2D smallMana = ResizeImage(Main.itemTexture[ItemID.ManaPotion], 24, 24);
			Texture2D smallBuff = ResizeImage(Main.itemTexture[ItemID.RagePotion], 24, 24);
			Texture2D smallAll = ResizeImage(Main.itemTexture[ItemID.AlphabetStatueA], 24, 24);
			Texture2D smallContainer = ResizeImage(Main.itemTexture[ItemID.GoldChest], 24, 24);
			Texture2D smallPaintings = ResizeImage(Main.itemTexture[ItemID.PaintingMartiaLisa], 24, 24);
			Texture2D smallStatue = ResizeImage(Main.itemTexture[ItemID.HeartStatue], 24, 24);
			Texture2D smallWiring = ResizeImage(Main.itemTexture[ItemID.Wire], 24, 24);
			Texture2D smallConsumables = ResizeImage(Main.itemTexture[ItemID.PurificationPowder], 24, 24);
			Texture2D smallExtractinator = ResizeImage(Main.itemTexture[ItemID.Extractinator], 24, 24);
			Texture2D smallOther = ResizeImage(Main.itemTexture[ItemID.UnicornonaStick], 24, 24);

			Texture2D smallArmor = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.SilverHelmet], Main.itemTexture[ItemID.SilverChainmail], Main.itemTexture[ItemID.SilverGreaves] }, 24, 24);
			Texture2D smallPetsLightPets = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.ZephyrFish], Main.itemTexture[ItemID.FairyBell] }, 24, 24);
			Texture2D smallPlaceables = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.Sign], Main.itemTexture[ItemID.PearlstoneBrickWall] }, 24, 24);
			Texture2D smallWeapons = StackResizeImage(new Texture2D[] { smallMelee, smallMagic, smallThrown }, 24, 24);
			Texture2D smallTools = StackResizeImage(new Texture2D[] { ItemChecklist.instance.GetTexture("Images/sortPick"), ItemChecklist.instance.GetTexture("Images/sortAxe"), ItemChecklist.instance.GetTexture("Images/sortHammer") }, 24, 24);
			Texture2D smallFishing = StackResizeImage(new Texture2D[] { ItemChecklist.instance.GetTexture("Images/sortFish"), ItemChecklist.instance.GetTexture("Images/sortBait"), Main.itemTexture[ItemID.FallenStarfish] }, 24, 24);
			Texture2D smallPotions = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.HealingPotion], Main.itemTexture[ItemID.ManaPotion], Main.itemTexture[ItemID.RagePotion] }, 24, 24);
			Texture2D smallBothDyes = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.OrangeDye], Main.itemTexture[ItemID.BiomeHairDye] }, 24, 24);
			Texture2D smallSortTiles = StackResizeImage(new Texture2D[] { Main.itemTexture[ItemID.Candelabra], Main.itemTexture[ItemID.GrandfatherClock] }, 24, 24);

			// Potions, other?
			// should inherit children?
			// should have other category?
			if (WorldGen.statueList == null)
				WorldGen.SetupStatueList();

			categories = new List<Category>() {
				new Category("All", x=> true, smallAll),
				new Category("Weapons"/*, x=>x.damage>0*/, x=> false, smallWeapons) { //"Images/sortDamage"
					subCategories = new List<Category>() {
						new Category("Melee", x=>x.melee, smallMelee),
						new Category("Yoyo", x=>ItemID.Sets.Yoyo[x.type], smallYoyo),
						new Category("Magic", x=>x.magic, smallMagic),
						new Category("Ranged", x=>x.ranged && x.ammo == 0, smallRanged) // TODO and ammo no
						{
							sorts = new List<Sort>() { new Sort("Use Ammo Type", "Images/sortAmmo", (x,y)=>x.useAmmo.CompareTo(y.useAmmo), x => x.useAmmo.ToString()), }
						},
						new Category("Throwing", x=>x.thrown, smallThrown),
						new Category("Summon", x=>x.summon && !x.sentry, smallSummon),
						new Category("Sentry", x=>x.summon && x.sentry, smallSentry),
					},
					sorts = new List<Sort>() { new Sort("Damage", "Images/sortDamage", (x,y)=>x.damage.CompareTo(y.damage), x => x.damage.ToString()), } // ascending 
				},
				new Category("Tools"/*,x=>x.pick>0||x.axe>0||x.hammer>0*/, x=>false, smallTools) {
					subCategories = new List<Category>() {
						new Category("Pickaxes", x=>x.pick>0, "Images/sortPick") { sorts = new List<Sort>() { new Sort("Pick Power", "Images/sortPick", (x,y)=>x.pick.CompareTo(y.pick), x => x.pick.ToString()), } },
						new Category("Axes", x=>x.axe>0, "Images/sortAxe"){ sorts = new List<Sort>() { new Sort("Axe Power", "Images/sortAxe", (x,y)=>x.axe.CompareTo(y.axe), x => (x.axe*5).ToString()), } },
						new Category("Hammers", x=>x.hammer>0, "Images/sortHammer"){ sorts = new List<Sort>() { new Sort("Hammer Power", "Images/sortHammer", (x,y)=>x.hammer.CompareTo(y.hammer), x => x.hammer.ToString()), } },
					},
				},
				new Category("Armor"/*,  x=>x.headSlot!=-1||x.bodySlot!=-1||x.legSlot!=-1*/, x => false, smallArmor) {
					subCategories = new List<Category>() {
						new Category("Head", x=>x.headSlot!=-1, smallHead),
						new Category("Body", x=>x.bodySlot!=-1, smallBody),
						new Category("Legs", x=>x.legSlot!=-1, smallLegs),
					},
					sorts = new List<Sort>() { new Sort("Defense", "Images/sortDefense", (x,y)=>x.defense.CompareTo(y.defense), x => x.defense.ToString()), }
				},
				new Category("Tiles", x=>x.createTile!=-1, smallTiles)
				{
					subCategories = new List<Category>()
					{
						new Category("Crafting Stations", x=>craftingTiles.Contains(x.createTile), smallCraftingStation),
						new Category("Containers", x=>x.createTile!=-1 && Main.tileContainer[x.createTile], smallContainer),
						new Category("Wiring", x=>ItemID.Sets.SortingPriorityWiring[x.type] > -1, smallWiring),
						new Category("Statues", x=>WorldGen.statueList.Any(point => point.X == x.createTile && point.Y == x.placeStyle), smallStatue), 
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
					sorts = new List<Sort>() {
						new Sort("Place Tile", smallSortTiles, (x,y)=> x.createTile == y.createTile ? x.placeStyle.CompareTo(y.placeStyle) : x.createTile.CompareTo(y.createTile), x=>$"{x.createTile},{x.placeStyle}"),
					}
				},
				new Category("Walls", x=>x.createWall!=-1, smallWalls),
				new Category("Accessories", x=>x.accessory, smallAccessories)
				{
					subCategories = new List<Category>()
					{
						new Category("Wings", x=>x.wingSlot > 0, smallWings)
					}
				},
				new Category("Ammo", x=>x.ammo!=0, ItemChecklist.instance.GetTexture("Images/sortAmmo"))
				{
					sorts = new List<Sort>() { new Sort("Ammo Type", "Images/sortAmmo", (x,y)=>x.ammo.CompareTo(y.ammo), x => $"{x.ammo}"), }
					// TODO: Filters/Subcategories for all ammo types?
				},
				new Category("Potions", x=>(x.UseSound != null && x.UseSound.Style == 3), smallPotions)
				{
					subCategories = new List<Category>() {
						new Category("Health Potions", x=>x.healLife > 0, smallHealth) { sorts = new List<Sort>() { new Sort("Heal Life", smallHealth, (x,y)=>x.healLife.CompareTo(y.healLife), x => $"{x.healLife}"), } },
						new Category("Mana Potions", x=>x.healMana > 0, smallMana) { sorts = new List<Sort>() { new Sort("Heal Mana", smallMana, (x,y)=>x.healMana.CompareTo(y.healMana), x => $"{x.healMana}"),   }},
						new Category("Buff Potions", x=>(x.UseSound != null && x.UseSound.Style == 3) && x.buffType > 0, smallBuff),
						// Todo: Automatic other category?
					}
				},
				new Category("Expert", x=>x.expert, smallExpert),
				new Category("Pets"/*, x=> x.buffType > 0 && (Main.vanityPet[x.buffType] || Main.lightPet[x.buffType])*/, x=>false, smallPetsLightPets){
					subCategories = new List<Category>() {
						new Category("Pets", x=>Main.vanityPet[x.buffType], smallPets),
						new Category("Light Pets", x=>Main.lightPet[x.buffType], smallLightPets),
					}
				},
				new Category("Mounts", x=>x.mountType != -1, smallMounts)
				{
					subCategories = new List<Category>()
					{
						new Category("Carts", x=>x.mountType != -1 && MountID.Sets.Cart[x.mountType], smallCarts) // TODO: need mountType check? inherited parent logic or parent unions children?
					}
				},
				new Category("Dyes", x=>false, smallBothDyes)
				{
					subCategories = new List<Category>()
					{
						new Category("Dyes", x=>x.dye != 0, smallDyes),
						new Category("Hair Dyes", x=>x.hairDye != -1, smallHairDye),
					}
				},
				new Category("Boss Summons", x=>ItemID.Sets.SortingPriorityBossSpawns[x.type] != -1 && x.type != ItemID.LifeCrystal && x.type != ItemID.ManaCrystal && x.type != ItemID.CellPhone && x.type != ItemID.IceMirror && x.type != ItemID.MagicMirror && x.type != ItemID.LifeFruit && x.netID != ItemID.TreasureMap || x.netID == ItemID.PirateMap, smallBossSummon) { // vanilla bug.
					sorts = new List<Sort>() { new Sort("Progression Order", "Images/sortDamage", (x,y)=>ItemID.Sets.SortingPriorityBossSpawns[x.type].CompareTo(ItemID.Sets.SortingPriorityBossSpawns[y.type]), x => $"{ItemID.Sets.SortingPriorityBossSpawns[x.type]}"), }
				},
				new Category("Consumables", x=>x.consumable, smallConsumables),
				new Category("Fishing"/*, x=> x.fishingPole > 0 || x.bait>0|| x.questItem*/, x=>false, smallFishing){
					subCategories = new List<Category>() {
						new Category("Poles", x=>x.fishingPole > 0, "Images/sortFish") {sorts = new List<Sort>() { new Sort("Pole Power", "Images/sortFish", (x,y)=>x.fishingPole.CompareTo(y.fishingPole), x => $"{x.fishingPole}"), } },
						new Category("Bait", x=>x.bait>0, "Images/sortBait") {sorts = new List<Sort>() { new Sort("Bait Power", "Images/sortBait", (x,y)=>x.bait.CompareTo(y.bait), x => $"{x.bait}"), } },
						new Category("Quest Fish", x=>x.questItem, smallQuestFish),
					}
				},
				new Category("Extractinator", x=>ItemID.Sets.ExtractinatorMode[x.type] > -1, smallExtractinator),
				//modCategory,
				new Category("Other", x=>BelongsInOther(x), smallOther),
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

			foreach (var parent in categories)
			{
				foreach (var child in parent.subCategories)
				{
					child.parent = parent; // 3 levels?
				}
			}

			SelectedSort = sorts[0];
			SelectedCategory = categories[0];
		}

		private bool BelongsInOther(Item item)
		{
			var cats = categories.Skip(1).Take(categories.Count - 2);
			foreach (var category in cats)
			{
				if (category.BelongsRecursive(item))
					return false;
			}
			return true;
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

			this.button = new UISilentImageButton(texture, name);
			button.OnClick += (a, b) =>
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
			button.OnClick += (a, b) =>
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
			if (texture == null)
				texture = ItemChecklist.instance.GetTexture("Images/sortAmmo");
			this.name = name;
			subCategories = new List<Category>();
			sorts = new List<Sort>();
			this.belongs = belongs;

			this.button = new UISilentImageButton(texture, name);
			button.OnClick += (a, b) =>
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
			if (belongs(item))
				return true;
			return subCategories.Any(x => x.belongs(item));
		}

		internal void ParentAddToSorts(List<Sort> availableSorts)
		{
			if (parent != null)
				parent.ParentAddToSorts(availableSorts);
			availableSorts.AddRange(sorts);
		}
	}
}
