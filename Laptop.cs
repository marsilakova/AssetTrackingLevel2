using System;
using System.Collections.Generic;
using System.Text;

namespace AssetTrackingLevel2
{
    // Subklass för datorer
    public class Laptop : Asset
    {
        public override string AssetType => "Laptop";

        public Laptop(string brand, string model, DateTime purchaseDate, double priceUSD, string office)
        : base(brand, model, purchaseDate, priceUSD, office) { }
    }
}