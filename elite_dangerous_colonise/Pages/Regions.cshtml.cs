using elite_dangerous_colonise.Models.Internal;
using elite_dangerous_colonise.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace elite_dangerous_colonise.Pages
{
    public class RegionsModel : PageModel
    {
        private readonly AppLogger logger;
        private readonly RegionStore regionStore;

        public List<Region> Regions { get; private set; }

        public RegionsModel (AppLogger logger, RegionStore regionStore)
        {
            this.logger = logger;
            this.regionStore = regionStore;
        }

        public void OnGet()
        {
            Regions = new List<Region>(regionStore.GetRegions());
            Regions.RemoveAll(region => region.Name == "Colonia" || region.Name == "Sagittarius A*");
        }
    }
}
