using System;
using System.Collections.Generic;
using System.Text;



namespace AssetTrackingLevel2
{
    // Abstrakt basklass som håller gemensamma egenskaper för alla tillgångar
    public abstract class Asset
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public DateTime PurchaseDate { get; set; }
        public double PriceUSD { get; set; }
        public string Office { get; set; }

        public abstract string AssetType { get; }

        
        // Hämtar lokal valuta baserat på kontorets land
        public string Currency =>
        Office?.ToLower() == "sweden" ? "SEK" :
        Office?.ToLower() == "turkey" ? "TRY" : "USD";

        // Beräknar det lokala priset baserat på växelkursen
        public double LocalPrice =>
        Office?.ToLower() == "sweden" ? PriceUSD * 10 :
        Office?.ToLower() == "turkey" ? PriceUSD * 34.5 : PriceUSD;
        protected Asset(string brand, string model, DateTime purchaseDate, double priceUSD, string office)
        {
            Brand = brand;
            Model = model;
            PurchaseDate = purchaseDate;
            PriceUSD = priceUSD;
            Office = office;
        }
    }
}
