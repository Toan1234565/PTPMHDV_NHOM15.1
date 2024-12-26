using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using BaiTap.Models;
using HtmlAgilityPack;
using System.Net;
using System.Runtime.Caching;

public class ProductService
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly MemoryCache cache = MemoryCache.Default;

    private readonly string apiKey = "AIzaSyCYRZEQ1wohBsBPSpiwWuP8EOlSOA82XoE"; // Thay thế bằng API Key của bạn
    private readonly string searchEngineId = "54a2eefbf7c49446b"; // Thay thế bằng Search Engine ID của bạn

    public ProductService()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
    }

    public async Task<string> GetProductImageAsync(string productName)
    {
        string cacheKey = $"ProductImage_{productName}";
        if (cache.Contains(cacheKey))
        {
            return (string)cache.Get(cacheKey);
        }

        string searchUrl = $"https://www.googleapis.com/customsearch/v1?q={productName}&cx={searchEngineId}&searchType=image&key={apiKey}";

        HttpResponseMessage response = await client.GetAsync(searchUrl);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            JObject searchResults = JObject.Parse(responseBody);
            if (searchResults["items"] != null && searchResults["items"].HasValues)
            {
                string imageUrl = searchResults["items"][0]["link"].ToString();

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30) };
                cache.Add(cacheKey, imageUrl, policy);

                return imageUrl;
            }
        }
        else
        {
            Console.WriteLine("Error: " + response.StatusCode + " - " + response.ReasonPhrase);
        }
        return null;
    }

    public async Task<ChiTietSanPham> GetProductDetailsFromWebAsync(string productName)
    {
        string cacheKey = $"ProductDetails_{productName}";
        if (cache.Contains(cacheKey))
        {
            return (ChiTietSanPham)cache.Get(cacheKey);
        }

        string searchUrl = $"https://www.googleapis.com/customsearch/v1?q={productName}&cx={searchEngineId}&key={apiKey}";

        HttpResponseMessage response = await client.GetAsync(searchUrl);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            JObject searchResults = JObject.Parse(responseBody);
            if (searchResults["items"] != null && searchResults["items"].HasValues)
            {
                string productUrl = searchResults["items"][0]["link"].ToString();
                var productDetails = await ScrapeProductDetailsFromUrl(productUrl);

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30) };
                cache.Add(cacheKey, productDetails, policy);

                return productDetails;
            }
        }
        else
        {
            Console.WriteLine("Error: " + response.StatusCode + " - " + response.ReasonPhrase);
        }
        return null;
    }

    private async Task<ChiTietSanPham> ScrapeProductDetailsFromUrl(string url)
    {
        var productDetails = new ChiTietSanPham();

        HttpResponseMessage response = await client.GetAsync(url);
        string responseBody = await response.Content.ReadAsStringAsync();

        var doc = new HtmlDocument();
        doc.LoadHtml(responseBody);

        var manHinhNode = doc.DocumentNode.SelectSingleNode("//div[@class='parameter']//td[@data-title='Màn hình']");
        if (manHinhNode != null)
        {
            productDetails.ManHinh = manHinhNode.InnerText.Trim();
        }

        var heDieuHanhNode = doc.DocumentNode.SelectSingleNode("//div[@class='parameter']//td[@data-title='Hệ điều hành']");
        if (heDieuHanhNode != null)
        {
            productDetails.HeDieuHanh = heDieuHanhNode.InnerText.Trim();
        }

        var cameraTruocNode = doc.DocumentNode.SelectSingleNode("//div[@class='parameter']//td[@data-title='Camera trước']");
        if (cameraTruocNode != null)
        {
            productDetails.CameraTruoc = cameraTruocNode.InnerText.Trim();
        }

        var cameraSauNode = doc.DocumentNode.SelectSingleNode("//div[@class='parameter']//td[@data-title='Camera sau']");
        if (cameraSauNode != null)
        {
            productDetails.CameraSau = cameraSauNode.InnerText.Trim();
        }

        var chipNode = doc.DocumentNode.SelectSingleNode("//div[@class='parameter']//td[@data-title='Chip']");
        if (chipNode != null)
        {
            productDetails.Chip = chipNode.InnerText.Trim();
        }

        var ramNode = doc.DocumentNode.SelectSingleNode("//div[@class='parameter']//td[@data-title='RAM']");
        if (ramNode != null)
        {
            productDetails.RAM = ramNode.InnerText.Trim();
        }

        var boNhoTrongNode = doc.DocumentNode.SelectSingleNode("//div[@class='parameter']//td[@data-title='Bộ nhớ trong']");
        if (boNhoTrongNode != null)
        {
            productDetails.BoNhoTrong = boNhoTrongNode.InnerText.Trim();
        }

        var simNode = doc.DocumentNode.SelectSingleNode("//div[@class='parameter']//td[@data-title='Sim']");
        if (simNode != null)
        {
            productDetails.Sim = simNode.InnerText.Trim();
        }

        var pinNode = doc.DocumentNode.SelectSingleNode("//div[@class='parameter']//td[@data-title='Pin']");
        if (pinNode != null)
        {
            productDetails.Pin = pinNode.InnerText.Trim();
        }

        return productDetails;
    }
}
