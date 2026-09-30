using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Emby.Plugin.Danmu.Core.Extensions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;
using Emby.Plugin.Danmu.Scraper.Entity;

static void Check(bool result, string name)
{
    if (!result) throw new Exception(name);
    Console.WriteLine("PASS " + name);
}

var directory = Path.Combine(Path.GetTempPath(), "danmu-regression-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(directory);
try
{
    DanmuProviderStore.Initialize(directory);
    var item = new Episode
    {
        Id = Guid.NewGuid(), Name = "第10集", Path = "/media/Season 2/S02E10.mkv",
        IndexNumber = 10, ParentIndexNumber = 2, ProductionYear = 2024,
        ProviderIds = new ProviderIdDictionary { { "Tvdb", "correct-tvdb" } }
    };
    var season = new Season { Id = Guid.NewGuid(), Name = "第3季", IndexNumber = 3, ProductionYear = 2024 };
    var episodeSearch = item.CreateDanmuSearchItem("节目全名", item.ProductionYear);
    var seasonSearch = season.CreateDanmuSearchItem("节目全名", season.ProductionYear);
    episodeSearch.Name = "搜索临时名称";
    seasonSearch.ProductionYear = 2025;
    Check(item.Name == "第10集" && season.Name == "第3季" && season.ProductionYear == 2024,
        "search proxies preserve original episode/season metadata");
    Check(episodeSearch is Episode e && e.IndexNumber == 10 && e.ParentIndexNumber == 2 && e.Path == item.Path,
        "search proxy preserves source identity and episode numbers");
    var media = new ScraperMedia { Episodes = Enumerable.Range(1, 26)
        .Select(i => new ScraperEpisode { Id = "episode-" + i, CommentId = "comment-" + i }).ToList() };
    var sparseEpisodes = new[] { 1, 2, 3, 4, 5, 6, 7, 9, 12, 26 }
        .Select(i => new Episode { IndexNumber = i, ParentIndexNumber = 2 }).ToArray();
    Check(sparseEpisodes.All(e => e.GetDanmuEpisode(media).Id == "episode-" + e.IndexNumber),
        "missing episodes do not shift later danmu matches");
    Check(new Episode { IndexNumber = 27, ParentIndexNumber = 2 }.GetDanmuEpisode(media) == null &&
        new Episode { IndexNumber = 1, ParentIndexNumber = 0 }.GetDanmuEpisode(media) == null &&
        new Episode { ParentIndexNumber = 2 }.GetDanmuEpisode(media) == null,
        "invalid, special and out-of-range episodes are skipped");

    item.SetDanmuProviderId("TencentID", "episode-10");
    season.SetDanmuProviderId("TencentID", "season-3");
    Check(item.ProviderIds.Count == 1 && item.GetProviderId("Tvdb") == "correct-tvdb" &&
        item.IndexNumber == 10 && item.ParentIndexNumber == 2 && item.Name == "第10集" &&
        season.Name == "第3季" && season.IndexNumber == 3,
        "saving danmu IDs does not mutate metadata or external IDs");
    Check(item.GetDanmuProviderId("TencentID") == "episode-10", "new IDs readable from isolated cache");
    DanmuProviderStore.Initialize(directory);
    Check(item.GetDanmuProviderId("TencentID") == "episode-10", "IDs survive cache reinitialization");
    item.ProviderIds["TencentID"] = "legacy";
    Check(item.GetDanmuProviderId("TencentID") == "episode-10", "cache takes precedence over stale NFO ID");
    var legacy = new Episode { Id = Guid.NewGuid(), ProviderIds = new ProviderIdDictionary { { "TencentID", "legacy" } } };
    Check(legacy.GetDanmuProviderId("TencentID") == "legacy", "legacy NFO ID remains usable");
    Parallel.For(0, 32, i => DanmuProviderStore.Set(item.Id, "source-" + i, "id-" + i));
    Check(Enumerable.Range(0, 32).All(i => DanmuProviderStore.Get(item.Id, "source-" + i) == "id-" + i),
        "concurrent saves preserve every source");
    DanmuProviderStore.Set(item.Id, "TencentID", "replacement");
    Check(DanmuProviderStore.Get(item.Id, "TencentID") == "replacement" && !Directory.EnumerateFiles(directory, "*.tmp").Any(),
        "atomic replacement leaves no temporary files");
    var rejected = false;
    try { DanmuProviderStore.Set(Guid.Empty, "TencentID", "x"); } catch (ArgumentException) { rejected = true; }
    Check(rejected, "invalid item IDs fail without writing metadata");
    Console.WriteLine("All regression tests passed.");
}
finally
{
    Directory.Delete(directory, true);
}
