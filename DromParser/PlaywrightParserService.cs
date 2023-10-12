using DromParser.Models;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace DromParser
{
    public class PlaywrightParserService
    {
        public async Task<List<Brand>> ParseAllCarsDataAsync()
        {
            var result = GetAllBrands();
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });
            var page = await browser.NewPageAsync();
            
            foreach(var brand in result)
            {
                Console.WriteLine($"Начало сборки моделей по бренду {brand.Name}");
                var brandModels = await GetAllBrandModelsAsync(page, brand.UrlToPageWithModels);
                brand.BrandModels = brandModels;
                Console.WriteLine($"Конец сборки моделей по бренду {brand.Name}");

            }

            foreach (var brand in result)
            {
                foreach(var brandModel in brand.BrandModels)
                {
                    Console.WriteLine($"Начало сборки поколений по бренду {brand.Name}, модель {brandModel.Name}");
                    var modelGenerations = await GetAllModelGenerationsAsync(page, brandModel.UrlToSearchByModel);
                    brandModel.ModelGenerations = modelGenerations;
                    Console.WriteLine($"Начало сборки поколений по бренду {brand.Name}, модель {brandModel.Name}");
                }
            }

            return result;
        }

        private List<Brand> GetAllBrands()
        {
            var result = new List<Brand>();
            var rawHtmlModelList = File.ReadAllText("ModelsList.txt");
            
            var regex = new Regex("<a\\s+href=\"(?<url>.*?)\">(?<text>.*?)</a>");

            var matches = regex.Matches(rawHtmlModelList);

            foreach (Match match in matches)
            {
                string url = match.Groups["url"].Value;
                string modelName = match.Groups["text"].Value;

                result.Add(new Brand
                {
                    Name= modelName,
                    UrlToPageWithModels = url
                });
            }

            return result;
        }

        private async Task<List<BrandModel>> GetAllBrandModelsAsync(IPage page, string urlToBrandPage)
        {
            var result = new List<BrandModel>();
            await page.GotoAsync(urlToBrandPage);
            await page.WaitForLoadStateAsync();

            var showAllButtonExists = await page.QuerySelectorAsync("//div[contains(text(),'Показать все')]");
            if (showAllButtonExists != null) 
            {
                await showAllButtonExists.ClickAsync();
            }
            //await page.GetByText("Показать все").ClickAsync();

            var nameElements = await page.QuerySelectorAllAsync("[data-ftid=component_cars-list-item_name]");

            foreach (var nameElement in nameElements)
            {
                var modelName = await nameElement.TextContentAsync();

                var modelLink = await page.EvaluateAsync<string>(
                @"(element) => {
                    const parentElement = element.parentElement.parentElement;
                    const linkElement = parentElement.querySelector('a[data-ftid=component_cars-list-item_hidden-link]');
                    return linkElement ? linkElement.getAttribute('href') : null;
                }",
                nameElement);

                result.Add(new BrandModel
                {
                    Name = modelName,
                    UrlToSearchByModel = modelLink
                });
            }

            return result;
        }

        private async Task<List<ModelGeneration>> GetAllModelGenerationsAsync(IPage page, string urlToSearchByModel)
        {
            var result = new List<ModelGeneration>();
            await page.GotoAsync(urlToSearchByModel);
            await page.WaitForLoadStateAsync();

            var generationElements = await page.QuerySelectorAllAsync("[data-ga-stats-name=generation_card]");

            foreach (var generationElement in generationElements)
            {
                var yearsAndNameElement = await generationElement.QuerySelectorAsync("//div[3]/div/div[1]");
                var descriptionElement = await generationElement.QuerySelectorAsync("//div[3]/div/div[2]");

                result.Add(new ModelGeneration
                {
                    YearsAndName = yearsAndNameElement != null ? await yearsAndNameElement.TextContentAsync() : "",
                    Description = descriptionElement != null ? await descriptionElement.TextContentAsync() : "",
                });
            }

            return result;
        }
    }
}
