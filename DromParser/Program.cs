using DromParser;
using DromParser.Models;
using Newtonsoft.Json;

public class Program
{
    public static void Main(string[] args)
    {
        var parserService = new PlaywrightParserService();
        var result = parserService.ParseAllCarsDataAsync().Result;
        File.WriteAllText("result.json", JsonConvert.SerializeObject(result));
    }
}
