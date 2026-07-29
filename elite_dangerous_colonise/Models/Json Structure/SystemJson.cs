using Newtonsoft.Json;
using elite_dangerous_colonise.Models.Database_Types;
using elite_dangerous_colonise.Models.Internal;

namespace elite_dangerous_colonise.Models.Json_Structure
{
    /// <summary> Represents the Json structure of a star system. </summary>
    public class SystemJson
    {
        readonly string[] COLONISED_STATION_TYPES = new string[]
        {
            "Coriolis Starport",
            "Ocellus Starport",
            "Orbis Starport",
            "Outposts"
        };

        private readonly string[] INVALID_STATIONS =
        {
            "Drake-Class Carrier",
            "Mega ship",
            "Settlement",
            "Planetary Construction Depot",
            "Space Construction Depot",
            null
        };

        private readonly string[] SPECIAL_MEGASHIP_NAMES =
        {
            "$EXT_PANEL_ColonisationShip:#index=1;",
            "$EXT_PANEL_ColonisationShip; [inactive]",
            "System Colonisation Ship",
            "Stronghold Carrier"
        };

        [JsonProperty("id64")]
        public required long SystemID { get; set; }
        [JsonProperty("name")]
        public required string Name { get; set; }
        [JsonProperty("coords")]
        public required CoordinatesJson Coordinates { get; set; }
        [JsonProperty("government")]
        public string? Government { get; set; } = "None";
        [JsonProperty("date")]
        public DateTimeOffset LastUpdate { get; set; }
        [JsonProperty("bodies")]
        public required List<BodyJson> Bodies { get; set; }
        [JsonProperty("stations")]
        public List<StationJson>? Stations { get; set; }

        private bool IsColonised()
        {
            return (Government != null && Government != "None") 
                || (Stations != null && Stations.Any(station => COLONISED_STATION_TYPES.Contains(station.StationType)));
        }

        private bool HasInvalidSignals()
        {
            foreach (BodyJson body in Bodies)
            {
                if (body.SignalCategory?.SignalTypes != null)
                {
                    foreach (string key in body.SignalCategory.SignalTypes.Keys)
                    {
                        if (key == "$SAA_SignalType_Human;")
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void MergeSystemAndBodyStationLists()
        {
            foreach (BodyJson body in Bodies)
            {
                if (body.Stations != null && body.Stations.Count > 0)
                {
                    if (Stations == null)
                    {
                        Stations = new List<StationJson>();
                    }

                    Stations.AddRange(body.Stations);
                }
            }
        }

        private void RemoveInvalidStations()
        {
            if (Stations != null && Stations.Count() > 0)
            {
                Stations.RemoveAll(station => INVALID_STATIONS.Contains(station.StationType) || SPECIAL_MEGASHIP_NAMES.Contains(station.Name));
            }
        }

        private ReserveType GetSystemReserveLevel()
        {
            ReserveType systemReserveLevel = ReserveType.None;

            foreach (BodyJson body in Bodies)
            {
                if (Enum.TryParse<ReserveType>(body.ReserveLevel, out ReserveType bodyReserveType))
                {
                    if (bodyReserveType != ReserveType.None)
                    {
                        systemReserveLevel = bodyReserveType;
                        return systemReserveLevel;
                    }
                }
            }

            return systemReserveLevel;
        }

        private (short, short) CountLandableAndWalkables()
        {
            short landableCount = 0;
            short walkableCount = 0;

            foreach (BodyJson body in Bodies)
            {
                if (body.IsLandable)
                {
                    landableCount++;
                    
                    if (body.IsDisembarkable())
                    {
                        walkableCount++;
                    }
                }
            }

            return (landableCount, walkableCount);
        }

        private List<Ring> CreateRingList()
        {
            List<Ring> rings = new List<Ring>();

            foreach (BodyJson body in Bodies)
            {
                rings.AddRange(RingJson.ConvertToRingList(body.Rings));
            }

            return rings;
        }

        private BodyCount CountBodyDetails()
        {
            BodyCount bodyCount = new BodyCount();

            foreach (BodyJson body in Bodies)
            {
                bodyCount.BinBodyTypes(body.BodyType);
                bodyCount.OrganicCount += (body.SignalCategory?.SignalTypes.ContainsKey("$SAA_SignalType_Biological;") ?? false) ? (short)1 : (short)0;
                bodyCount.GeologicalsCount += (body.SignalCategory?.SignalTypes.ContainsKey("$SAA_SignalType_Geological;") ?? false) ? (short)1 : (short)0;
                bodyCount.RingCount += (short)(body.Rings?.Count() ?? 0);
            }

            return bodyCount;
        }

        /// <summary> Converts the SystemJson object into a StarSystem object. </summary>
        public StarSystem? ConvertToStarSystem()
        {
            MergeSystemAndBodyStationLists();
            RemoveInvalidStations();

            if (IsColonised())
            {
                return new ColonisedStarSystem(SystemID, Name, Coordinates.ConvertToVector(), StationJson.ConvertToStationList(Stations));
            }
            else
            {
                if (HasInvalidSignals())
                {
                    return null;
                }
                else
                {
                    (short, short) landableAndWalkableCounts = CountLandableAndWalkables();
                    return new UncolonisedStarSystem(
                        SystemID,
                        Name,
                        Coordinates.ConvertToVector(),
                        LastUpdate.UtcDateTime,
                        GetSystemReserveLevel(),
                        landableAndWalkableCounts.Item1,
                        landableAndWalkableCounts.Item2,
                        CreateRingList(),
                        CountBodyDetails()
                    );
                }
            }
        }
    }
}
