using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Market
{
    public class CryptocurrencySearchResult
    {
        [Required]
        public string Symbol { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        
        public decimal Change24h { get; set; }
        
        public decimal ChangePercent24h { get; set; }
        
        public decimal Volume24h { get; set; }
        
        public decimal MarketCap { get; set; }
        
        public int Rank { get; set; }
        
        public string Logo { get; set; } = string.Empty;
    }

    public class SearchResultsData
    {
        public List<CryptocurrencySearchResult> Data { get; set; } = new();
    }

    public class SearchResponse
    {
        public int Status { get; set; } = 200;
        public SearchResultsData Data { get; set; } = new();
    }

    public class ProductDetailsResponse
    {
        [Required]
        public string Symbol { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        
        public decimal Change24h { get; set; }
        
        public decimal ChangePercent24h { get; set; }
        
        public decimal Volume24h { get; set; }
        
        public decimal MarketCap { get; set; }
        
        public decimal CirculatingSupply { get; set; }
        
        public decimal TotalSupply { get; set; }
        
        public int Rank { get; set; }
        
        public string Logo { get; set; } = string.Empty;
        
        public string Website { get; set; } = string.Empty;
        
        public string Whitepaper { get; set; } = string.Empty;
    }

    public class ProductDetailsDataResponse
    {
        public int Status { get; set; } = 200;
        public ProductDetailsResponse Data { get; set; } = new();
    }
}
