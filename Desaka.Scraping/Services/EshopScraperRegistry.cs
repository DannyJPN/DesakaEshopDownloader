namespace Desaka.Scraping.Services;

public sealed record EshopScraperDefinition(string Name, string BaseUrl, string SitemapUrl);

public sealed class EshopScraperRegistry
{
    private readonly IReadOnlyDictionary<string, EshopScraperDefinition> _definitions;

    public EshopScraperRegistry()
    {
        var list = new List<EshopScraperDefinition>
        {
            new("Gewo", "https://www.contra.de/", "https://www.contra.de/sitemap.xml"),
            new("Nittaku", "https://nittaku.tt/", "https://nittaku.tt/sitemap.xml"),
            new("Pincesobchod", "https://www.pincesobchod.cz", "https://www.pincesobchod.cz/sitemap.xml"),
            new("Spinway", "https://www.spinway.sk/", "https://www.spinway.sk/sitemap.xml"),
            new("Sportspin", "https://www.sportspin.cz/", "https://www.sportspin.cz/sitemap.xml"),
            new("Stoten", "https://stoten.cz", "https://stoten.cz/sitemap.xml"),
            new("VseNaStolniTenis", "https://www.vsenastolnitenis.cz/", "https://www.vsenastolnitenis.cz/sitemap.xml"),
            new("Adidas", "https://www.adidas.cz/", "https://www.adidas.cz/sitemap.xml"),
            new("Andro", "https://andro.de", "https://andro.de/sitemap.xml"),
            new("Armstrong", "https://armstrong.tokyo.jp/", "https://armstrong.tokyo.jp/sitemap.xml"),
            new("Asics", "https://asics.com", "https://asics.com/sitemap.xml"),
            new("Avalox", "https://avalox.com", "https://avalox.com/sitemap.xml"),
            new("Butterfly", "https://butterfly.tt", "https://butterfly.tt/sitemap.xml"),
            new("Carlton", "https://carlton-sports.com", "https://carlton-sports.com/sitemap.xml"),
            new("Cornilleau", "https://cornilleau.com", "https://cornilleau.com/sitemap.xml"),
            new("Dawei", "https://daweisports.com", "https://daweisports.com/sitemap.xml"),
            new("DerMaterialspezialist", "https://der-materialspezialist.com", "https://der-materialspezialist.com/sitemap.xml"),
            new("Desaka", "https://desaka.cz", "https://desaka.cz/sitemap.xml"),
            new("DHS", "https://dhs-sports.com", "https://dhs-sports.com/sitemap.xml"),
            new("DingoSwiss", "https://dingyitt.com", "https://dingyitt.com/sitemap.xml"),
            new("Donic", "https://donic.com", "https://donic.com/sitemap.xml"),
            new("DrNeubauer", "https://drneubauer.com", "https://drneubauer.com/sitemap.xml"),
            new("Friendship", "https://en.729sports.com", "https://en.729sports.com/sitemap.xml"),
            new("Enebe", "https://enebetenisdemesa.com", "https://enebetenisdemesa.com/sitemap.xml"),
            new("FastPong", "https://fastpong.com/", "https://fastpong.com/sitemap.xml"),
            new("Gambler", "https://gamblertt.com/shop/", "https://gamblertt.com/sitemap.xml"),
            new("GiantDragon", "https://giant-dragon.com", "https://giant-dragon.com/sitemap.xml"),
            new("Hallmark", "https://hallmarktabletennis.co.uk", "https://hallmarktabletennis.co.uk/sitemap.xml"),
            new("Hanno", "https://hanno-tischtennis.de/en/", "https://hanno-tischtennis.de/sitemap.xml"),
            new("Japsko", "https://japsko.se", "https://japsko.se/sitemap.xml"),
            new("Joola", "https://joola.com", "https://joola.com/sitemap.xml"),
            new("Juic", "https://www.juic.co.jp/", "https://www.juic.co.jp/sitemap.xml"),
            new("Kokutaku", "https://www.kokutaku.net/", "https://www.kokutaku.net/sitemap.xml"),
            new("Lion", "https://lionmfg.com", "https://lionmfg.com/sitemap.xml"),
            new("Mizuno", "https://mizuno.com", "https://mizuno.com/sitemap.xml"),
            new("Nexy", "https://nexy.com", "https://nexy.com/sitemap.xml"),
            new("Palio", "https://palioett.com", "https://palioett.com/sitemap.xml"),
            new("PimplePark", "https://pimplepark.com", "https://pimplepark.com/sitemap.xml"),
            new("Sanwei", "https://sanweisport.com", "https://sanweisport.com/sitemap.xml"),
            new("SauerTroeger", "https://sauer-troeger.com", "https://sauer-troeger.com/sitemap.xml"),
            new("Schildkrot", "https://www.xn--schildkrt-sport-gtb.com/", "https://www.xn--schildkrt-sport-gtb.com/sitemap.xml"),
            new("Spinlord", "https://spinlord-tt.de", "https://spinlord-tt.de/sitemap.xml"),
            new("Sponeta", "https://www.sponeta.de/en/", "https://www.sponeta.de/sitemap.xml"),
            new("Stag", "https://stagiconic.com/", "https://stagiconic.com/sitemap.xml"),
            new("Stiga", "https://stigasports.com", "https://stigasports.com/sitemap.xml"),
            new("SunFlex", "https://www.sunflex-sport.com/en/home", "https://www.sunflex-sport.com/sitemap.xml"),
            new("Sword", "https://swordtt.com", "https://swordtt.com/sitemap.xml"),
            new("Tibhar", "https://tibhar.info", "https://tibhar.info/sitemap.xml"),
            new("Tuttle", "https://tuttle-tabletennis.com", "https://tuttle-tabletennis.com/sitemap.xml"),
            new("Victas", "https://victas.com", "https://victas.com/sitemap.xml"),
            new("Enlio", "https://www.cnenlio.com/", "https://www.cnenlio.com/sitemap.xml"),
            new("Contra", "https://www.contra.de/", "https://www.contra.de/sitemap.xml"),
            new("DoubleFish", "https://www.doublefish.com/", "https://www.doublefish.com/sitemap.xml"),
            new("Sportspin", "https://www.sportspin.cz/", "https://www.sportspin.cz/sitemap.xml"),
            new("Spinway", "https://www.spinway.sk/", "https://www.spinway.sk/sitemap.xml"),
            new("Xiom", "https://xiom.eu", "https://xiom.eu/sitemap.xml"),
            new("Yasaka", "https://yasaka-jp.com", "https://yasaka-jp.com/sitemap.xml"),
            new("Xushaofa", "https://www.xushaofa-sports.com/", "https://www.xushaofa-sports.com/sitemap.xml"),
            new("ChinaMeteot", "http://www.china-meteot.com", "http://www.china-meteot.com/sitemap.xml"),
            new("YinHe", "http://www.yinhe1986.cn/", "http://www.yinhe1986.cn/sitemap.xml")
        };

        _definitions = list
            .GroupBy(x => GetHost(x.BaseUrl))
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
    }

    public EshopScraperDefinition? Resolve(string baseUrl)
    {
        var host = GetHost(baseUrl);
        return _definitions.TryGetValue(host, out var def) ? def : null;
    }

    public IReadOnlyList<EshopScraperDefinition> All() => _definitions.Values.ToList();

    private static string GetHost(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return uri.Host.ToLowerInvariant();
        }

        return url.ToLowerInvariant();
    }
}
