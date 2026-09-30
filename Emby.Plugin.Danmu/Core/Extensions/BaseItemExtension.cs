using System;
using System.Threading;
using System.Threading.Tasks;
using Emby.Plugin.Danmu.Core.Singleton;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Emby.Plugin.Danmu.Scraper.Entity;

namespace Emby.Plugin.Danmu.Core.Extensions
{
    public static class BaseItemExtension
    {
        public static ScraperEpisode GetDanmuEpisode(this Episode episode, ScraperMedia media)
        {
            var number = episode.IndexNumber ?? 0;
            if ((episode.ParentIndexNumber ?? 0) <= 0 || number < 1 ||
                media?.Episodes == null || number > media.Episodes.Count) return null;
            return media.Episodes[number - 1];
        }

        public static void SetDanmuProviderId(this BaseItem item, string providerId, string value)
        {
            var target = item;
            if (item is Season && item.Id == Guid.Empty) target = item.GetParent();
            if (target == null) throw new InvalidOperationException("Cannot cache an unpersisted season without its series.");
            DanmuProviderStore.Set(target.Id, providerId, value);
        }

        public static BaseItem CreateDanmuSearchItem(this BaseItem item, string name, int? year)
        {
            BaseItem result;
            if (item is Episode episode)
                result = new Episode { IndexNumber = episode.IndexNumber, ParentIndexNumber = episode.ParentIndexNumber };
            else if (item is Season season)
                result = new Season { IndexNumber = season.IndexNumber };
            else
                throw new ArgumentException("Only episode and season search proxies are supported.");
            result.Id = item.Id;
            result.Name = name;
            result.Path = item.Path;
            result.ProductionYear = year;
            return result;
        }

        public static string GetDanmuXmlPath(this BaseItem item, string providerId)
        {
            return item.FileNameWithoutExtension + "_" + providerId + ".xml";
        }

        /**
         * 获取弹幕id
         */
        public static string GetDanmuProviderId(this BaseItem item, string providerId)
        {
            if (item == null) return null;
            string providerVal = DanmuProviderStore.Get(item.Id, providerId) ?? item.GetProviderId(providerId);
            if (!string.IsNullOrEmpty(providerVal))
            {
                return providerVal;
            }

            if (item is Season)
            {   
                var parent = item.GetParent();
                return parent == null ? null : DanmuProviderStore.Get(parent.Id, providerId) ?? parent.GetProviderId(providerId);
            }
            return providerVal;
        }

        /**
         * season 获取id问题，可能存在没有season的问题，需要使用SeriesId
         */
        public static Guid GetSeasonId(this Season season)
        {
            Guid seasonId = season.Id;
            if (!Guid.Empty.Equals(seasonId))
            {
                return seasonId;
            }

            return season.GetParent().Id;
        }

        /**
         * 是否存在相应的id
         */
        public static bool HasAnyDanmuProviderIds(this BaseItem item)
        {
            var scrapers = SingletonManager.ScraperManager.All();
            if (scrapers == null || scrapers.Count == 0)
            {
                return false;
            }

            foreach (var scraper in scrapers)
            {
                if (!string.IsNullOrEmpty(item.GetDanmuProviderId(scraper.ProviderId)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
