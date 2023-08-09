using PuppeteerSharp;
using System;
using System.Text;
using System.Threading.Tasks;

namespace GorillaTypist;

internal class Program
{
    private const string Stats = "#result";
    private const string WordInput = "#wordsInput";
    private const string WordActive = "#words > div.word.active";
    static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();
    static async Task DownloadBrowser()
    {
        using var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
    }
    static async Task<IPage> RunPuppeteer()
    {
        var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = false
        });
        var page = await browser.NewPageAsync();
        return page;
    }
    static async Task MainAsync()
    {
        Console.Title = "Gorilla Typist || Auto MonkeyType Typer";
        await DownloadBrowser();

        var page = await RunPuppeteer();
        await page.GoToAsync("https://monkeytype.com/");
        await Task.Delay(3000); //Wait 3 seconds minimum to load

        Console.WriteLine("Accepting Cookies..");
        AcceptCookies(page);

        Console.WriteLine("Ensuring stats are hidden...");
        await page.WaitForSelectorAsync(Stats);

        Console.WriteLine("Waiting for 3 seconds...");
        await Task.Delay(3000);

        Console.WriteLine("Typing...");
        while (await IsHidden(page, Stats))
        {
            StringBuilder lettersBuilder = new StringBuilder();
            var activeLetters = await page.QuerySelectorAllAsync(WordActive);
            foreach (var letter in activeLetters)
            {
                string letterText = await page.EvaluateFunctionAsync<string>("element => element.textContent", letter);
                lettersBuilder.Append(letterText);
            }
            lettersBuilder.Append(" ");
            var letters = lettersBuilder.ToString();
            await page.TypeAsync(WordInput, letters);
        }

        Console.WriteLine("Finished.");
        Console.ReadKey();
    }

    static async Task<bool> IsHidden(IPage page, string selector)
        => await page.EvaluateFunctionAsync<bool>("selector => !document.querySelector(selector).offsetParent", selector);
    static async void AcceptCookies(IPage page)
    {
        string cookieAccept = "#cookiePopup > div.main > div.buttons > div.button.active.acceptAll";
        await page.WaitForSelectorAsync(cookieAccept);
        await page.ClickAsync(cookieAccept);
    }
}