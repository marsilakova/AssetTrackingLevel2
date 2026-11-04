using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;

namespace AssetTrackingLevel2
{
    class Program
    {
        // Den här raden skapar filen direkt i mappen på Skrivbordet (Desktop)
        private static readonly string FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "assets_storage.txt");

        static void Main(string[] args)
        {
            // 1. LÄSER FRÅN FIL: Hämtar sparade tillgångar vid start (Krav för Level 3)
            List<Asset> assets = LoadFromFile();

            Console.WriteLine("--- Asset Tracking System (Level 3 - Final) ---");
            if (assets.Count > 0)
            {
                Console.WriteLine($"[System] Loaded {assets.Count} assets from assets_storage.txt");
            }
            Console.WriteLine("Enter assets. Type 'exit' to save and view the list.\n");

            // Standard testdata om filen startas tom för första gången
            if (assets.Count == 0)
            {
                assets.Add(new Laptop("Apple", "MacBook Pro", new DateTime(2022, 08, 10), 1650, "Sweden"));
                assets.Add(new MobilePhone("Samsung", "Galaxy S23", new DateTime(2024, 11, 01), 950, "Sweden"));
                assets.Add(new Laptop("Lenovo", "ThinkPad X1", new DateTime(2023, 01, 05), 2099, "Turkey"));
                assets.Add(new MobilePhone("Apple", "iPhone 15", new DateTime(2025, 02, 01), 1200, "USA"));
                // 🟥 RED: Köpt i augusti 2023. 3 år gick ut i augusti 2026 (livslängden har tagit slut).
               // assets.Add(new Laptop("Apple", "MacBook Pro", new DateTime(2023, 08, 10), 1650, "Sweden"));

                // 🟨 YELLOW: Куплен в марте 2024. 3 года истекают в марте 2027. До дедлайна меньше 6 месяцев!

                //assets.Add(new Laptop("Lenovo", "ThinkPad X1", new DateTime(2024, 03, 15), 1300, "Sweden"));

                // ⬜ БЕЛЫЙ: Köpt i juni 2025. Löper ut först 2028. Elementet är stabilt.
                //assets.Add(new MobilePhone("Samsung", "Galaxy S23", new DateTime(2025, 06, 01), 950, "Sweden"));
            }

        

            while (true)
            {
                Console.Write("Enter Asset Type (Laptop / Phone) or 'exit': ");
                string typeInput = Console.ReadLine()?.Trim().ToLower() ??"";
                if (typeInput == "exit") break;

                if (typeInput != "laptop" && typeInput != "phone")
                {
                    Console.WriteLine("Invalid asset type. Enter 'Laptop' or 'Phone'.\n");
                    continue;
                }

                Console.Write("Enter Office (e.g. Sweden, USA, Turkey): ");
                string office = Console.ReadLine() ??"";

                Console.Write("Enter Brand: ");
                string brand = Console.ReadLine() ??"";

                Console.Write("Enter Model: ");
                string model = Console.ReadLine() ??"";

                DateTime purchaseDate;
                while (true)
                {
                    Console.Write("Enter Purchase Date (yyyy-MM-dd): ");
                    if (DateTime.TryParse(Console.ReadLine(), out purchaseDate)) break;
                    Console.WriteLine("Invalid format. Use YYYY-MM-DD.");
                }

                double priceUSD;
                while (true)
                {
                    Console.Write("Enter Price (USD): ");
                    if (double.TryParse(Console.ReadLine(), out priceUSD)) break;
                    Console.WriteLine("Invalid price. Enter a number.");
                }

                Asset newAsset = typeInput == "laptop"
                ? new Laptop(brand, model, purchaseDate, priceUSD, office)
                : new MobilePhone(brand, model, purchaseDate, priceUSD, office);

                assets.Add(newAsset);
                Console.WriteLine("Asset added!\n");
            }

            // 2. SPARAR TILL FIL: Skriver ner listan till textfilen (Krav för Level 3)
            SaveToFile(assets);

            // Visar din fina sorterade tabell
            DisplayAssets(assets);
        }

        static void DisplayAssets(List<Asset> assets)
        {
            if (assets.Count == 0) return;

            // Sortering enligt Level 3: Först efter kontor, sedan efter inköpsdatum
            var sortedAssets = assets
            .OrderBy(a => a.Office)
            .ThenBy(a => a.PurchaseDate)
            .ToList();

            Console.WriteLine("\nFINAL ASSET LIST (Level 3)");
            Console.WriteLine(new string('-', 92));
            // Formaterar hela raden genom att omvandla datumet till text först
            // return $"{Office,-10} {AssetType,-10} {Brand,-12} {Model,-15} {(LocalPrice + " " + Currency),-12} {PurchaseDate.ToString("yyyy-MM-dd"),-15} {Status}";
            //Console.WriteLine($"{"Office",-12} {"Type",-10} {"Brand",-10} {"Model",-15} {"Price",-15} {"Purchase Date",-15} {"Status",-10}");
            Console.WriteLine(new string('-', 92));

            DateTime today = DateTime.Today;

            foreach (var asset in sortedAssets)
            {
                DateTime endOfLifeDate = asset.PurchaseDate.AddYears(3);
                TimeSpan timeLeft = endOfLifeDate - today;
                double daysLeft = timeLeft.TotalDays;

                string statusText = "";

                if (daysLeft < 90)
                {
                    statusText = "RED";
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else if (daysLeft < 180)
                {
                    statusText = "YELLOW";
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else
                {
                    Console.ResetColor();
                }

                // Formatera priset till "VÄRDE VALUTA" (t.ex. 16500 SEK) enligt skärmbilden
                string priceText = $"{asset.LocalPrice:F0} {asset.Currency}";

                // Utskrift av raden med exakt kolumnordning
                Console.WriteLine($"{asset.Office,-12} {asset.AssetType,-10} {asset.Brand,-10} {asset.Model,-15} {priceText,-15} {asset.PurchaseDate,-15:yyyy-MM-dd} {statusText,-10}");
               

                //string priceText = $"{asset.Office,-12} {asset.AssetType,-10} {asset.Brand,-10} {asset.Model,-15} {priceText,-15} {asset.PurchaseDate:yyyy-MM-dd,-15} {statusText,-10}";
            }

            Console.ResetColor();
            Console.WriteLine(new string('-', 92));
        }
        // =========================================================================
        // CORRECTED FILE HANDLING (Level 3 - Textbaserad Databas
        // =========================================================================
        private static void SaveToFile(List<Asset> assets)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(FileName, false))
                {
                    foreach (var a in assets)
                    {
                        // Vi skriver data genom separatorn ';', tar de exakta egenskaperna
                        writer.WriteLine(
                        $"{a.AssetType};{a.Office};{a.Brand};{a.Model};{a.PurchaseDate:yyyy-MM-dd};{a.PriceUSD}");
                    }
                }
                Console.WriteLine("\n[File System] Data successfully saved to assets_storage.txt");
            }
            catch (Exception)
            {
                Console.WriteLine(

                        "Error saving to file: {ex.Message}");
            }
        }

        private static List<Asset> LoadFromFile()
        {
            List<Asset> list = new List<Asset>();
            if (!File.Exists(FileName)) return list;

            try
            {
                string[] lines = File.ReadAllLines(FileName);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Разбиваем строку по знаку ';' в массив элементов
                    string[] p = line.Split(';');
                    if (p.Length < 6) continue;

                    // ИСПОЛЬЗУЕМ ИНДЕКСЫ, [1], [2]... ДЛЯ КАЖДОГО СВОЙСТВА
                    string type = p[0].ToLower();
                    string office = p[1];
                    string brand = p[2];
                    string model = p[3];
                    DateTime date = DateTime.Parse(p[4]);
                    double price = double.Parse(p[5]);

                    Asset asset = type == "laptop"
                    ? new Laptop(brand, model, date, price, office)
                    : new MobilePhone(brand, model, date, price, office);

                    if (asset != null) list.Add(asset);
                }
            }
            catch
            {
                // Если файл поврежден, просто возвращаем пустой список, чтобы не падать
            }
            return list;
        }
        // =========================================================================
        // NYA METODER FÖR FILHANTERING (Level 3 - Textbaserad Databas)
        // =========================================================================
        /* private static void SaveToFile(List<Asset> assets)
         {
             try
             {
                 using (StreamWriter writer = new StreamWriter(FileName, false))
                 {
                     foreach (var a in assets)
                     {
                         writer.WriteLine(
                         "{a.AssetType};{a.Office};{a.Brand};{a.Model};{a.PurchaseDate:yyyy-MM-dd};{a.PriceUSD}");
                     }
                 }
                 Console.WriteLine("\n[File System] Data successfully saved to assets_storage.txt");
             }
             catch (Exception ex)
             {
                 Console.WriteLine(

                         "Error saving to file: {ex.Message}");
             }
         }*/

        /* private static List<Asset> LoadFromFile()
         {
             List<Asset> list = new List<Asset>();
             if (!File.Exists(FileName)) return list;

             try
             {
                 string[] lines = File.ReadAllLines(FileName);
                 foreach (string line in lines)
                 {
                     if (string.IsNullOrWhiteSpace(line)) continue;
                     string[] p = line.Split(';');
                     if (p.Length < 6) continue;

                     string type = p[0].ToLower();
                     string office = p[1];
                     string brand = p[2];
                     string model = p[3];
                     DateTime date = DateTime.Parse(p[4]);
                     double price = double.Parse(p[5]);

                     Asset asset = type == "laptop"
                     ? new Laptop(brand, model, date, price, office)
                     : new MobilePhone(brand, model, date, price, office);

                     list.Add(asset);
                 }
             }
             catch
             {
                 // Returnerar tom lista vid fel så att programmet inte kraschar
             }
             return list;
         }*/
    }
}