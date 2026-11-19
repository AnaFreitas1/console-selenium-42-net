using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace ConsoleSelenium.Tests;

[TestClass]
public class BuscaDesafioTest
{
    private IWebDriver _driver = null!;
    private WebDriverWait _wait = null!;
    private const string BaseUrl = "https://duckduckgo.com";

    [TestInitialize]
    public void Setup()
    {
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");

        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
    }

    [TestCleanup]
    public void TearDown()
    {
        _driver?.Quit();
        _driver?.Dispose();
    }

    [TestMethod]
    public void Exercicio1_PesquisaBasica_ValidarTituloOuUrl()
    {
        _driver.Navigate().GoToUrl(BaseUrl);

        var campoBusca = _wait.Until(d =>
        {
            var el = d.FindElement(By.Name("q"));
            return el.Displayed && el.Enabled ? el : null;
        })!;

        campoBusca.SendKeys("Selenium WebDriver C#" + Keys.Enter);

        _wait.Until(d =>
            d.Title.Contains("Selenium", StringComparison.OrdinalIgnoreCase) ||
            d.Url.Contains("Selenium", StringComparison.OrdinalIgnoreCase));

        bool tituloContem = _driver.Title.Contains("Selenium", StringComparison.OrdinalIgnoreCase);
        bool urlContem = _driver.Url.Contains("Selenium", StringComparison.OrdinalIgnoreCase);

        Assert.IsTrue(
            tituloContem || urlContem,
            $"Esperava encontrar o termo nos resultados. Título: '{_driver.Title}' | URL: '{_driver.Url}'"
        );
    }

    [TestMethod]
    public void Exercicio2_Autocompletar_SelecionarTerceiraSugestao()
    {
        _driver.Navigate().GoToUrl(BaseUrl);

        var campoBusca = _wait.Until(d =>
        {
            var el = d.FindElement(By.Name("q"));
            return el.Displayed && el.Enabled ? el : null;
        })!;

        campoBusca.SendKeys("C# unit testing");

        By seletorSugestoes = By.CssSelector(".search-suggestions-list li, .sbox-suggestions li, [role='option']");

        var sugestoes = _wait.Until(d =>
        {
            var itens = d.FindElements(seletorSugestoes)
                         .Where(el => el.Displayed && !string.IsNullOrWhiteSpace(el.Text))
                         .ToList();
            return itens.Count >= 3 ? itens : null;
        });

        sugestoes![2].Click();

        _wait.Until(d =>
            d.Url.Contains("q=", StringComparison.OrdinalIgnoreCase) ||
            d.Title.Contains("DuckDuckGo", StringComparison.OrdinalIgnoreCase));

        Assert.IsTrue(
            _driver.Url.Contains("unit", StringComparison.OrdinalIgnoreCase) ||
            _driver.Title.Contains("unit", StringComparison.OrdinalIgnoreCase),
            $"A navegação após clicar na sugestão não refletiu o termo esperado. URL: '{_driver.Url}' | Título: '{_driver.Title}'"
        );
    }

    [TestMethod]
    public void Exercicio3_NavegacaoHistorico_VoltarParaResultados()
    {
        _driver.Navigate().GoToUrl(BaseUrl);

        var campoBusca = _wait.Until(d =>
        {
            var el = d.FindElement(By.Name("q"));
            return el.Displayed && el.Enabled ? el : null;
        })!;

        campoBusca.SendKeys("Automated Testing" + Keys.Enter);

        string urlResultados = _wait.Until(d =>
        {
            var links = d.FindElements(By.CssSelector("article h2 a, a[data-testid='result-title-a']"));
            return links.Any(l => l.Displayed) ? d.Url : null;
        })!;

        var primeiroLink = _wait.Until(d =>
            d.FindElements(By.CssSelector("article h2 a, a[data-testid='result-title-a']"))
             .FirstOrDefault(l => l.Displayed && !string.IsNullOrWhiteSpace(l.GetAttribute("href")))
        );

        Assert.IsNotNull(primeiroLink, "Nenhum link orgânico foi encontrado na página de resultados.");
        primeiroLink.Click();

        _wait.Until(d => d.Url != urlResultados);
        string urlPaginaExterna = _driver.Url;
        Assert.AreNotEqual(urlResultados, urlPaginaExterna, "A URL deveria ter mudado após clicar no resultado.");

        _driver.Navigate().Back();

        _wait.Until(d =>
            d.Url.Contains("Automated", StringComparison.OrdinalIgnoreCase) ||
            d.FindElements(By.Name("q")).Any(e => e.Displayed));

        Assert.IsTrue(
            _driver.Url.Contains("Automated", StringComparison.OrdinalIgnoreCase) ||
            _driver.Title.Contains("Automated", StringComparison.OrdinalIgnoreCase),
            $"A página de resultados não foi restaurada corretamente. URL atual: '{_driver.Url}'"
        );
    }
}