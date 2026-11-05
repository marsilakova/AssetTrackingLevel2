using System.Globalization;

namespace AssetTrackingLevel2;

internal static class Program
{
    private const string StorageFileName = "assets_storage.txt";
    private static readonly string StoragePath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", StorageFileName));

    private static void Main()
    {
        List<Asset> assets = LoadFromFile();

        Console.WriteLine("--- Asset Tracking System (Level 3 - Final) ---");
        if (assets.Count > 0)
        {
            Console.WriteLine($"[System] Loaded {assets.Count} assets from {StorageFileName}");
        }

        Console.WriteLine("Enter assets. Type 'exit' to save and view the list.\n");

        if (assets.Count == 0)
        {
            AddDefaultAssets(assets);
        }

        ReadAssetsFromConsole(assets);
        SaveToFile(assets);
        DisplayAssets(assets);
    }

    private static void AddDefaultAssets(List<Asset> assets)
    {
        assets.Add(new Laptop("Apple", "MacBook Pro", new DateTime(2022, 8, 10), 1650, "Sweden"));
        assets.Add(new MobilePhone("Samsung", "Galaxy S23", new DateTime(2024, 11, 1), 950, "Sweden"));
        assets.Add(new Laptop("Lenovo", "ThinkPad X1", new DateTime(2023, 1, 5), 2099, "Turkey"));
        assets.Add(new MobilePhone("Apple", "iPhone 15", new DateTime(2025, 2, 1), 1200, "USA"));
    }

    private static void ReadAssetsFromConsole(List<Asset> assets)
    {
        while (true)
        {
            string type = ReadLine("Enter Asset Type (Laptop / Phone) or 'exit': ").Trim();
            if (type.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!type.Equals("laptop", StringComparison.OrdinalIgnoreCase) &&
                !type.Equals("phone", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Invalid asset type. Enter 'Laptop' or 'Phone'.\n");
                continue;
            }

            string office = ReadRequired("Enter Office (e.g. Sweden, USA, Turkey): ");
            string brand = ReadRequired("Enter Brand: ");
            string model = ReadRequired("Enter Model: ");
            DateTime purchaseDate = ReadDate();
            double priceUsd = ReadPrice();

            Asset asset = type.Equals("laptop", StringComparison.OrdinalIgnoreCase)
                ? new Laptop(brand, model, purchaseDate, priceUsd, office)
                : new MobilePhone(brand, model, purchaseDate, priceUsd, office);

            assets.Add(asset);
            Console.WriteLine("Asset added!\n");
        }
    }

    private static DateTime ReadDate()
    {
        while (true)
        {
            string value = ReadLine("Enter Purchase Date (yyyy-MM-dd): ");
            if (DateTime.TryParseExact(
                value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime date))
            {
                return date;
            }

            Console.WriteLine("Invalid format. Use YYYY-MM-DD.");
        }
    }

    private static double ReadPrice()
    {
        while (true)
        {
            string value = ReadLine("Enter Price (USD): ");
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out double price) &&
                price >= 0)
            {
                return price;
            }

            Console.WriteLine("Invalid price. Enter a non-negative number.");
        }
    }

    private static string ReadRequired(string prompt)
    {
        while (true)
        {
            string value = ReadLine(prompt).Trim();
            if (value.Length > 0)
            {
                return value;
            }

            Console.WriteLine("Value cannot be empty.");
        }
    }

    private static string ReadLine(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? string.Empty;
    }

    private static void DisplayAssets(List<Asset> assets)
    {
        if (assets.Count == 0)
        {
            return;
        }

        Console.WriteLine("\nFINAL ASSET LIST (Level 3)");
        Console.WriteLine(new string('-', 92));
        Console.WriteLine("Office       Type       Brand      Model           Price           Purchase Date   Status");
        Console.WriteLine(new string('-', 92));

        DateTime today = DateTime.Today;
        foreach (Asset asset in assets.OrderBy(a => a.Office).ThenBy(a => a.PurchaseDate))
        {
            double daysLeft = (asset.PurchaseDate.AddYears(3) - today).TotalDays;
            string status = GetStatus(daysLeft, out ConsoleColor? color);

            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }

            string price = $"{asset.LocalPrice:F0} {asset.Currency}";
            Console.WriteLine(
                $"{asset.Office,-12} {asset.AssetType,-10} {asset.Brand,-10} {asset.Model,-15} " +
                $"{price,-15} {asset.PurchaseDate,-15:yyyy-MM-dd} {status,-10}");
        }

        Console.ResetColor();
        Console.WriteLine(new string('-', 92));
    }

    private static string GetStatus(double daysLeft, out ConsoleColor? color)
    {
        if (daysLeft < 90)
        {
            color = ConsoleColor.Red;
            return "RED";
        }

        if (daysLeft < 180)
        {
            color = ConsoleColor.Yellow;
            return "YELLOW";
        }

        color = null;
        return string.Empty;
    }

    private static void SaveToFile(List<Asset> assets)
    {
        try
        {
            using StreamWriter writer = new(StoragePath, false);
            foreach (Asset asset in assets)
            {
                writer.WriteLine(
                    string.Join(';',
                        asset.AssetType,
                        asset.Office,
                        asset.Brand,
                        asset.Model,
                        asset.PurchaseDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        asset.PriceUSD.ToString(CultureInfo.InvariantCulture)));
            }

            Console.WriteLine($"\n[File System] Data successfully saved to {StorageFileName}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error saving to file: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Error saving to file: {ex.Message}");
        }
    }

    private static List<Asset> LoadFromFile()
    {
        List<Asset> assets = [];
        if (!File.Exists(StoragePath))
        {
            return assets;
        }

        try
        {
            foreach (string line in File.ReadLines(StoragePath))
            {
                if (TryParseAsset(line, out Asset? asset))
                {
                    assets.Add(asset);
                }
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error loading file: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Error loading file: {ex.Message}");
        }

        return assets;
    }

    private static bool TryParseAsset(string line, out Asset? asset)
    {
        asset = null;
        string[] fields = line.Split(';');
        if (fields.Length != 6 || string.IsNullOrWhiteSpace(fields[0]))
        {
            return false;
        }

        bool validDate = DateTime.TryParseExact(
            fields[4],
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime purchaseDate);
        bool validPrice = double.TryParse(
            fields[5],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out double priceUsd);

        if (!validDate || !validPrice || priceUsd < 0)
        {
            return false;
        }

        asset = fields[0].Trim().Equals("laptop", StringComparison.OrdinalIgnoreCase)
            ? new Laptop(fields[2], fields[3], purchaseDate, priceUsd, fields[1])
            : fields[0].Trim().Equals("phone", StringComparison.OrdinalIgnoreCase)
                ? new MobilePhone(fields[2], fields[3], purchaseDate, priceUsd, fields[1])
                : null;

        return asset is not null;
    }
}