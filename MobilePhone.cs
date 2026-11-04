using System;
using System.Collections.Generic;
using System.Text;

namespace AssetTrackingLevel2
{
    // Subklass for mobiltelefoner
    public class MobilePhone : Asset
    {
        public override string AssetType => "Phone";

        public MobilePhone(string brand, string model, DateTime purchaseDate, double priceUSD, string office)
        : base(brand, model, purchaseDate, priceUSD, office) { }
    }
}
