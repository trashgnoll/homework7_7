using System.Net.NetworkInformation;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

static string? InputString(string Prompt, bool AllowEmptyInput = false)
{
    Console.Write(Prompt);
    string? result;
    if (AllowEmptyInput)
        result = Console.ReadLine();
    else
        while ((result = Console.ReadLine()) is null || result == string.Empty)
            Console.Write(Prompt);
    return result;
}

static int InputInt(string Prompt)
{
    string? input;
    int result;
    do
        input = InputString(Prompt);
    while( !int.TryParse(input, out result) );
    return result;
}
// Доступные продукты
TProduct[] Products = { new TProduct(0, 50, "Хлеб"),
                        new TProduct(1, 80, "Молоко"),
                        new TProduct(2, 100, "Сосиска"),
                        new TProduct(3, 160, "Мясо")};
TCart cart = new(); // корзина для заказа
TOrder<TDelivery, TCart> order = new(1, "Описание заказа"); // заказ
// Сообщение для вывода при выборе продукта:
string ChooseProductPrompt = "Выберите продукт чтобы добавить в заказ:" +
                      "\n\t0: Хлеб" +
                      "\n\t1: Молоко" +
                      "\n\t2: Сосиска" +
                      "\n\t3: Мясо";
// Выбор первого, нельзя не выбрать:
int answer = InputInt(ChooseProductPrompt + "\n?:");
while( answer < 0 || answer > 3)
    answer = InputInt(ChooseProductPrompt + "\n?:");
int quantity = InputInt("Сколько штук?");
cart += new TOrderProduct(Products[answer], Math.Max(1, quantity) );
// Выбор последующих, можно пропустить:
string? answerS = InputString(ChooseProductPrompt + "\nИли нажмите Enter\n?:", true);
while ( answerS is not null && answerS != string.Empty && int.TryParse(answerS, out answer) )
{
    if (answer >= 0 && answer <= 3)
    {
        quantity = InputInt("Сколько штук?");
        cart += new TOrderProduct(Products[answer], Math.Max(1, quantity));
    }
    answerS = InputString(ChooseProductPrompt + "\nИли нажмите Enter\n?:", true);
}
// Вывод информации о заказе:
Console.WriteLine(cart.GetInfo());
order.Cart = cart;
// Выбор доставки:
answerS = InputString("Введите адрес для доставки:");
answer = InputInt("Выберите способ доставки\n\t1: Курьером\n\t2: В постомат\n\t3: В магазин\n?:");
switch (answer)
{
    case 1:
        order.Delivery = new THomeDelivery(answerS, DateTime.Now, DateTime.Now.AddHours(3), "DHL");
        break;
    case 2:
        order.Delivery = new TPickPointDelivery(answerS, "CDEK");
        break;
    case 3:
        order.Delivery = new TShopDelivery(answerS, "Пятёрочка");
        break;
    default: return;
}
// Готово:
Console.WriteLine(order.GetInfo());
if (!order.IsValid())
    Console.WriteLine("Заказ некорректный. Попробуйте снова.");
else
    Console.WriteLine("Заказ оформлен");
Console.ReadKey();

abstract class TDelivery
{
    public string Address;

    public TDelivery()
    {
        Address = string.Empty;
    }
    public abstract bool IsValid(); // is delivery data correct
    public abstract string GetInfo();
}

class THomeDelivery : TDelivery
{
    public DateTime ReceiveFrom, ReceiveTo; // deliver interval
    public string? DeliveryService;
    public THomeDelivery(string? address, DateTime receiveFrom, DateTime receiveTo, string? deliveryService)
    {
        Address = (address is not null? address : string.Empty);
        ReceiveFrom = receiveFrom;
        ReceiveTo = receiveTo;
        DeliveryService = deliveryService;
    }
    public override bool IsValid()
    {
        return (DeliveryService is not null) && (ReceiveFrom < ReceiveTo);
    }
    public override string GetInfo()
    {
        return this.IsValid() ?
                    "Адрес: " + this.Address +
                    "\nСлужба доставки: " + this.DeliveryService +
                    "\nДоставить с " + this.ReceiveFrom.ToString() +
                    " до " + this.ReceiveTo.ToString()
                : 
                    "Некорректные данные доставки курьером";
    }
}

class TPickPointDelivery : TDelivery
{
    public string? PickPointName;
    public TPickPointDelivery(string? address, string? pickPointName)
    {
        Address = (address is not null ? address : string.Empty);
        PickPointName = pickPointName;
    }

    public override bool IsValid()
    {
        return (PickPointName is not null);
    }
    public override string GetInfo()
    {
        return this.IsValid() ?
                    "Адрес: " + this.Address +
                    "\nПостомат: " + this.PickPointName
                :
                    "Некорректные данные доставки в постомат";
    }
}

class TShopDelivery : TDelivery
{
    public string? ShopName;
    public TShopDelivery(string? address, string? shopName)
    {
        Address = (address is not null ? address : string.Empty);
        ShopName = shopName;
    }
    public override bool IsValid()
    {
        return (ShopName is not null);
    }
    public override string GetInfo()
    {
        return this.IsValid() ?
                    "Адрес: " + this.Address +
                    "\nМагазин: " + this.ShopName
                :
                    "Некорректные данные доставки в магазин";
    }
}

abstract class TGenericProduct
{
    public int ProductId;
    public float Price;

    public abstract string GetInfo();
}

class TProduct: TGenericProduct
{
    public string? Name;
    public TProduct(int productId, float price, string name = "")
    {
        ProductId = productId;
        Price = price;
        Name = name;
    }
    public override string GetInfo()
    {
        return "Номер: " + ProductId.ToString() +
                "\nЦена: " + Price.ToString() +
                "\n" + (Name is null ? "Без имени" : Name);
    }
}

class TOrderProduct: TProduct
{
    public TGenericProduct Product;
    public float Quantity;

    public TOrderProduct( TGenericProduct product, float quantity = 1 )
            : base(product.ProductId, product.Price)
    {
        if (product is not null)
        {
            if (product is TProduct)
                Name = ((TProduct)product).Name;
            else
                Name = string.Empty;
            Product = product;
        }
        else
        {
            Name = string.Empty;
            Product = new TProduct(0, 0, string.Empty);
        }
        Quantity = quantity;
    }
    public override string GetInfo()
    {
        return base.GetInfo() + "\nКол-во: " + Quantity.ToString();
    }
    public float Total()
    {
        return Product.Price * Quantity;
    }
}

class TCart
{
    private protected readonly Dictionary<int, TOrderProduct> FProducts;
    public TCart()
    {
        FProducts = new Dictionary<int, TOrderProduct>();
    }
    public bool Add(TOrderProduct Product)
    {
        try
        {
            if (FProducts.ContainsKey(Product.ProductId))
            {
                FProducts[Product.ProductId].Quantity += Product.Quantity;
                return true;
            }
            else
            {
                FProducts.Add(Product.ProductId, Product);
                return true;
            }
        }
        catch
        { return false; }
    }
    public bool Add(TGenericProduct Product, float quantity)
    {
        TOrderProduct product = new(Product, quantity);
        return Add(product);
    }
    public bool Remove(int productId, float quantity)
    {
        try
        {
            if (FProducts.ContainsKey(productId))
            {
                if (FProducts[productId].Quantity > quantity)
                    FProducts[productId].Quantity -= quantity;
                else
                    FProducts.Remove(productId);
            }
            return true;
        }
        catch
        { return false; };
    }
    public bool Remove(TOrderProduct product, float quantity)
    {
        return Remove(product.ProductId, quantity);
    }
    public bool Remove(TGenericProduct product, float quantity)
    {
        return Remove(product.ProductId, quantity);
    }
    public static TCart operator +(TCart a, TOrderProduct b)
    {
        a.Add(b);
        return a;
    }
    public static TCart operator -(TCart a, TOrderProduct b)
    {
        a.Remove(b, b.Quantity);
        return a;
    }
    public string GetInfo()
    {
        string result = "Корзина: ";
        if( FProducts.Count == 0)
            return result + "пусто";
        result += "Позиций: " + FProducts.Count.ToString();
        float total = 0;
        TOrderProduct[] products = FProducts.Values.ToArray();
        foreach (TOrderProduct item in products )
        {
            result += "\n\t" + item.Quantity.ToString() + " шт. " +
                               item.Name + " = " + item.Total().ToString() + " руб.";
            total += item.Total();
        }
        result += "\nИтого: " + total.ToString() + " руб.";
        return result;
    }
    public void Clear()
    {
        FProducts.Clear();
    }
    public bool IsEmpty()
    {
        return ( FProducts.Count == 0 );
    }
}

abstract class TAction
{
    public string Name;
    public abstract TOrder<TDelivery, TCart> Apply(TOrder<TDelivery, TCart> order);

    public TAction()
    {
        Name = string.Empty;
    }
}

class TOrder<ADelivery, ACart> where ADelivery : TDelivery where ACart : TCart
{
    public TDelivery? Delivery;
    public TCart? Cart;
    public int Number;
    public string Description;

    public TOrder( int number, string description = "" )
    {
        Delivery = null;
        Cart = null;
        Number = number;
        Description = description;
    }

    public void DisplayAddress()
    {
        if (Delivery is not null)
            Console.WriteLine(Delivery.Address);
        else
            Console.WriteLine("-");
    }
    public bool IsValid()
    {
        return (Delivery is not null && Delivery.IsValid() &&
                Cart is not null && !Cart.IsEmpty());
    }
    public string GetInfo()
    {
        return "Доставка: " + (Delivery is not null ? Delivery.GetInfo() : " нет данных") + "\n" +
               (Cart is not null ? Cart.GetInfo() : "Корзина: нет");

    }

    // ... Другие поля
}