using System;
using System.Collections.Generic;

namespace Tired
{
    class Program
    {
        static void Main(string[] args)
        {
            InventoriableEntity playerInventory = new InventoriableEntity(10);
            InventoriableEntity traderInventory = new InventoriableEntity(10);
            UserInputMenu menu = new UserInputMenu(traderInventory, playerInventory);

            traderInventory.CurrentInventory.TryAddItem(ItemBase.Meet, 28);
            traderInventory.CurrentInventory.TryAddItem(ItemBase.Bow, 3);
            traderInventory.CurrentInventory.TryAddItem(ItemBase.Axe, 1);
            traderInventory.CurrentInventory.TryAddItem(ItemBase.Sword, 5);
            traderInventory.CurrentInventory.TryAddItem(ItemBase.Beer, 13);
            traderInventory.CurrentInventory.TryAddItem(ItemBase.Bread, 45);
            traderInventory.CurrentInventory.TryAddItem(ItemBase.Stick, 18);

            menu.Update();
        }
    }

    class UserInputMenu
    {
        private const string WordToExit = "exit";
        private const string WordToBuy = "buy";
        private const string WordToSell = "sell";
        private const string WordToShow = "show";

        private string _wordToRead;
        private InventoriableEntity _trader;
        private InventoriableEntity _player;
        private bool _isPlayerInventoryShown = false;

        public UserInputMenu(InventoriableEntity trader, InventoriableEntity player)
        {
            _player = player;
            _trader = trader;
        }

        public void Update()
        {
            while (_player != null && _trader != null && _wordToRead != WordToExit)
            {
                ShowMenu();

                Console.Write("\nВвод:");

                _wordToRead = Console.ReadLine();

                Console.WriteLine();

                switch (_wordToRead)
                {
                    case WordToExit:
                        break;

                    case WordToBuy:
                        BuyItemByUser();
                        break;

                    case WordToSell:
                        SellItemByUser();
                        break;

                    case WordToShow:
                        SetPlayerInventoryShowModeByUser();
                        break;
                }

                Console.Clear();
            }
        }

        public void SetPlayerInventoryShowModeByUser()
        {
            Console.Write("\nПоказать инвентарь(да,нет):");

            if (Console.ReadLine() == "да")
            {
                _isPlayerInventoryShown = true;
            }
            else if (Console.ReadLine() == "нет")
            {
                _isPlayerInventoryShown = false;
            }
        }

        public void ReadItemByUser(out int itemID, out int amount)
        {
            Console.Write("\nВведите имя предмета:");

            itemID = ItemBase.GetItemIDByName(Console.ReadLine());

            if (itemID == Item.EmptyID)
            {
                Console.WriteLine("\nТакого предмета не существует в игре!");

                amount = 0;

                return;
            }

            Console.Write("\nВведите количество:");

            if (Int32.TryParse(Console.ReadLine(), out amount) == true)
            {
                Console.WriteLine("\nДанные верны");
            }
        }

        public void BuyItemByUser()
        {
            Console.WriteLine("Покупка предмета:");

            ReadItemByUser(out int itemID, out int amount);

            if (_trader.TryGiveItemTo(itemID, amount, _player) == true)
            {
                Console.WriteLine("Покупка совершена!");
            }
            else
            {
                Console.WriteLine("Произошла ошибка..");
            }
        }

        public void SellItemByUser()
        {
            Console.WriteLine("Продажа предмета:");

            ReadItemByUser(out int itemID, out int amount);

            if (_player.TryGiveItemTo(itemID, amount, _trader))
            {
                Console.WriteLine("Произошла ошибка..");
            }
        }

        public void ShowMenu()
        {
            Console.WriteLine("Вы классическом магазине 15-го века и торгуете с продавцом!");
            Console.WriteLine(" *Введите " + WordToBuy + " для покупки предмета");
            Console.WriteLine(" *Введите " + WordToSell + " для продажи предмета");
            Console.WriteLine(" *Введите " + WordToShow + " чтобы посмотреть свой инвентарь");
            Console.WriteLine(" *Введите " + WordToExit + " чтобы выйти");
            Console.WriteLine("\nСписок предметов для покупки у продовца");
            _trader.CurrentInventory.ShowItems();

            if (_isPlayerInventoryShown == true)
            {
                Console.WriteLine("Ваш инвентарь:");
                _player.CurrentInventory.ShowItems();
            }

            Console.WriteLine();
        }
    }

    class InventoriableEntity
    {
        public Inventory CurrentInventory { get; private set; }

        public InventoriableEntity(int inventorySize)
        {
            CurrentInventory = new Inventory(inventorySize);
        }

        public bool TryGiveItemTo(int itemID,int amount,InventoriableEntity otherEntity)
        {
            return CurrentInventory.TryMoveItemTo(itemID, amount, otherEntity.CurrentInventory);
        }
    }

    class Inventory
    {
        private const int DefaultMaxCells = 16;

        private List<ItemCell> _cells = new List<ItemCell>();

        public int MaxCells { get; private set; }

        public Inventory(int maxCells = DefaultMaxCells)
        {
            MaxCells = maxCells;

            Clear();
        }

        public void Clear()
        {
            _cells.Clear();

            for (int i = 0; i < MaxCells; i++)
            {
                _cells.Add(new ItemCell(64));
            }
        }

        public bool TryAddItem(int itemID, int amount)
        {
            ItemCell neededCell = null;

            if (itemID == Item.EmptyID)
            {
                return false;
            }

            foreach (ItemCell cell in _cells)
            {
                if ((cell.SavedItemID == itemID) || (neededCell == null && cell.SavedItemID == Item.EmptyID))
                {
                    neededCell = cell;
                }
            }

            if (neededCell != null && neededCell.TryAddItem(itemID, amount, out int overAmount) == true)
            {
                if (overAmount > 0)
                {
                    TryAddItem(itemID, overAmount);
                }

                return true;
            }

            return false;
        }

        public bool TryRemoveItem(int itemID, int amount)
        {
            ItemCell cell = GetFirstItemCellByItemID(itemID);

            if (cell != null)
            {
                cell.SetAmount(cell.Amount - amount);

                return true;
            }

            return false;
        }

        public bool TryMoveItemTo(int itemID, int amount, Inventory otherInventory)
        {
            ItemCell cell = GetFirstItemCellByItemID(itemID);

            if (cell == null || otherInventory == null || cell.Amount < amount)
            {
                return false;
            }

            if (otherInventory.TryAddItem(cell.SavedItemID, amount) == true)
            {
                cell.SetAmount(cell.Amount - amount);

                return true;
            }

            return false;
        }

        public void ShowItems()
        {
            Console.WriteLine("Содержимое инвентаря:");

            foreach (ItemCell cell in _cells)
            {
                Console.WriteLine(cell.GetItemInfo());
            }
        }

        private ItemCell GetFirstItemCellByItemID(int itemID)
        {
            if (itemID == Item.EmptyID)
            {
                return null;
            }

            foreach (ItemCell cell in _cells)
            {
                if (cell.SavedItemID == itemID)
                {
                    return cell;
                }
            }

            return null;
        }
    }

    class ItemCell
    {
        private const int DefaultAmount = 16;

        private int _amount;

        public int SavedItemID { get; private set; } = Item.EmptyID;
        public int MaxAmount { get; private set; }
        public int Amount
        {
            get
            {
                return _amount;
            }

            private set
            {
                if (SavedItemID == Item.EmptyID)
                {
                    _amount = 0;
                }
                else if (value > MaxAmount)
                {
                    _amount = MaxAmount;
                }
                else if (value <= 0)
                {
                    SavedItemID = Item.EmptyID;
                    _amount = 0;
                }
                else
                {
                    _amount = value;
                }
            }
        }

        public ItemCell(int maxCount = DefaultAmount)
        {
            MaxAmount = maxCount;
        }

        public void SetItemID(int itemID) => SavedItemID = itemID;

        public void SetAmount(int amount) => Amount = amount;

        public void DeleteItem() => SetAmount(0);

        public bool TryMoveItemTo(ItemCell itemCell)
        {
            if (itemCell != null)
            {
                itemCell.SetItemID(SavedItemID);
                itemCell.SetAmount(Amount);
                DeleteItem();

                return true;
            }

            return false;
        }

        public bool TryAddItem(int itemID, int amount, out int overAmount)
        {
            if (SavedItemID != itemID && SavedItemID != Item.EmptyID)
            {
                overAmount = 0;

                return false;
            }

            int resultAmount = Amount + amount;

            overAmount = resultAmount > MaxAmount ? (resultAmount - MaxAmount) : 0;
            SetItemID(itemID);
            SetAmount(resultAmount);

            return true;
        }

        public void IncrementAmount() => Amount++;

        public void DecrementAmount() => Amount--;

        public Item GetItem() => ItemBase.GetItemByID(SavedItemID);

        public string GetItemInfo() => (SavedItemID == Item.EmptyID) ? "Пустой слот" : GetItem().Name + ":" + Amount;
    }

    static class ItemBase
    {
        /* Example Test WriteTable */
        public static readonly int Bread;
        public static readonly int Beer;
        public static readonly int Meet;
        public static readonly int Sword;
        public static readonly int Bow;
        public static readonly int Stick;
        public static readonly int Axe;

        private static Dictionary<int, Item> _itemsDictionary = new Dictionary<int, Item>();

        static ItemBase()
        {
            Bread = CreateNewItem(new Item("Хлеб"));
            Beer = CreateNewItem(new Item("Пиво"));
            Meet = CreateNewItem(new Item("Мясо"));
            Sword = CreateNewItem(new Item("Меч"));
            Bow = CreateNewItem(new Item("Лук"));
            Stick = CreateNewItem(new Item("Палка"));
            Axe = CreateNewItem(new Item("Топор"));
        }

        public static int CreateNewItem(Item item)
        {
            if (item == null)
            {
                return Item.EmptyID;
            }

            _itemsDictionary.Add(item.ID, item);

            return item.ID;
        }

        public static Item GetItemByID(int itemID)
        {
            if (_itemsDictionary.TryGetValue(itemID, out Item item) == true)
            {
                return item;
            }

            return null;
        }

        public static int GetItemIDByName(string name)
        {
            foreach (KeyValuePair<int, Item> pair in _itemsDictionary)
            {
                if (pair.Value.Name == name)
                {
                    return pair.Value.ID;
                }
            }

            return Item.EmptyID;
        }
    }

    class Item
    {
        public const int EmptyID = -1;

        private static int NextID = 0;

        public string Name { get; private set; }
        public int ID { get; private set; }

        public Item(string name)
        {
            Name = name;
            ID = NextID;
            NextID++;
        }
    }
}
