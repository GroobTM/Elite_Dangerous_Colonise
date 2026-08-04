-- Run first, then create "GetRegionCube" from DML
CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Run third
BEGIN TRANSACTION;

CREATE TYPE "ResultOrderType" AS ENUM (
	'SystemValue',
	'MostWalkables',
	'DistanceToRegionCentre',
	'MostHotspots'
);

CREATE TYPE "ReserveType" AS ENUM (
	'None',
	'Major',
	'Common',
	'Low',
	'Depleted',
	'Pristine'
);

CREATE TYPE "RingType" AS ENUM (
	'Rocky',
	'MetalRich',
	'Icy',
	'Metallic'
);

CREATE TYPE "HotspotType" AS ENUM (
	'Alexandrite',
	'Bauxite',
	'Benitoite',
	'Bromellite',
	'Cobalt',
	'Coltan',
	'Gallite',
	'Gold',
	'Grandidierite',
	'HydrogenPeroxide',
	'Indite',
	'Lepidolite',
	'LiquidOxygen',
	'LithiumHydroxide',
	'LowTemperatureDiamond',
	'MethaneClathrate',
	'MethanolMonohydrateCrystals',
	'Monazite',
	'Musgravite',
	'Opal',
	'Osmium',
	'Painite',
	'Palladium',
	'Platinum',
	'Praseodymium',
	'Rhodplumsite',
	'Rutile',
	'Samarium',
	'Serendibite',
	'Silver',
	'Tritium',
	'Uraninite',
	'Water'
);

CREATE TABLE "Regions" (
	"regionID" SERIAL PRIMARY KEY,
	"regionName" VARCHAR(75) UNIQUE NOT NULL,
	"regionRange" SMALLINT NOT NULL,
	"regionCentreCoords" GEOMETRY(PointZ, 0) NOT NULL
);

CREATE TABLE "Factions" (
	"factionID" SERIAL PRIMARY KEY,
	"factionName" VARCHAR(75) UNIQUE NOT NULL
);

CREATE TABLE "StarSystems" (
	"systemID" NUMERIC(20, 0) PRIMARY KEY,
	"systemName" VARCHAR(75) NOT NULL,
	"systemCoords" GEOMETRY(PointZ, 0) NOT NULL,
	"isColonised" BOOLEAN NOT NULL
);

CREATE TABLE "StarSystemsByRegion" (
	"systemID" NUMERIC(20, 0),
	"regionID" INT,
	"distanceToCentre" INT NOT NULL,
	PRIMARY KEY ("systemID", "regionID"),
	FOREIGN KEY ("systemID") REFERENCES "StarSystems"("systemID") ON DELETE CASCADE,
	FOREIGN KEY ("regionID") REFERENCES "Regions"("regionID") ON DELETE CASCADE
);

CREATE TABLE "Stations" (
	"stationID" NUMERIC(20, 0) PRIMARY KEY,
	"systemID" NUMERIC(20, 0) NOT NULL,
	"stationName" VARCHAR(75) NOT NULL,
	"controllingFaction" INT NOT NULL,
	FOREIGN KEY ("systemID") REFERENCES "StarSystems"("systemID") ON DELETE CASCADE,
	FOREIGN KEY ("controllingFaction") REFERENCES "Factions"("factionID")
);

CREATE TABLE "UncolonisedStarSystems" (
	"systemID" NUMERIC(20, 0) PRIMARY KEY,
	"lastUpdated" TIMESTAMPTZ NOT NULL,
	"reserveLevel" "ReserveType" NOT NULL,
	"landableCount" SMALLINT NOT NULL,
	"walkableCount" SMALLINT NOT NULL,
	"totalHotspots" SMALLINT NOT NULL,
	"systemValue" NUMERIC(5,2) NOT NULL,
	FOREIGN KEY ("systemID") REFERENCES "StarSystems"("systemID") ON DELETE CASCADE
);

CREATE TABLE "UncolonisedStarSystemsAvailability" (
	"systemID" NUMERIC(20, 0) PRIMARY KEY,
	"isLocked" BOOLEAN NOT NULL DEFAULT FALSE,
	"isClaimed" BOOLEAN NOT NULL DEFAULT FALSE,
	"lockReportCount" SMALLINT NOT NULL DEFAULT 0,
	"claimReportCount" SMALLINT NOT NULL DEFAULT 0,
	"lockReportDate" TIMESTAMPTZ DEFAULT NULL,
	"claimReportDate" TIMESTAMPTZ DEFAULT NULL,
	FOREIGN KEY ("systemID") REFERENCES "UncolonisedStarSystems"("systemID") ON DELETE CASCADE
);

CREATE TABLE "ColonyOverrideCounts" (
	"systemID" NUMERIC(20, 0) PRIMARY KEY,
	"blackHoleCount" SMALLINT NOT NULL,
	"neutronStarCount" SMALLINT NOT NULL,
	"whiteDwarves" SMALLINT NOT NULL,
	"otherStarCount" SMALLINT NOT NULL,
	"earthLikeCount" SMALLINT NOT NULL,
	"waterWorldCount" SMALLINT NOT NULL,
	"ammoniaWorldCount" SMALLINT NOT NULL,
	"gasGiantCount" SMALLINT NOT NULL,
	"highMetalContentCount" SMALLINT NOT NULL,
	"metalRichCount" SMALLINT NOT NULL,
	"rockyIceBodyCount" SMALLINT NOT NULL,
	"rockBodyCount" SMALLINT NOT NULL,
	"icyBodyCount" SMALLINT NOT NULL,
	"organicCount" SMALLINT NOT NULL,
	"geologicalsCount" SMALLINT NOT NULL,
	"ringCount" SMALLINT NOT NULL,
	FOREIGN KEY ("systemID") REFERENCES "UncolonisedStarSystems"("systemID") ON DELETE CASCADE
);

CREATE TABLE "Rings" (
	"ringID" SERIAL PRIMARY KEY,
	"systemID" NUMERIC(20, 0) NOT NULL,
	"ringName" VARCHAR(75) NOT NULL,
	"ringType" "RingType" NOT NULL,
	UNIQUE ("systemID", "ringName"),
	FOREIGN KEY ("systemID") REFERENCES "UncolonisedStarSystems"("systemID") ON DELETE CASCADE
);

CREATE TABLE "Hotspots" (
	"ringID" INT,
	"hotspotType" "HotspotType",
	"hotspotCount" SMALLINT NOT NULL,
	PRIMARY KEY ("ringID", "hotspotType"),
	FOREIGN KEY ("ringID") REFERENCES "Rings"("ringID") ON DELETE CASCADE
);

CREATE TABLE "ColonisableStarSystems" (
	"colonisedSystemID" NUMERIC(20, 0),
	"uncolonisedSystemID" NUMERIC(20, 0),
	PRIMARY KEY ("colonisedSystemID", "uncolonisedSystemID"),
	FOREIGN KEY ("colonisedSystemID") REFERENCES "StarSystems"("systemID") ON DELETE CASCADE,
	FOREIGN KEY ("uncolonisedSystemID") REFERENCES "UncolonisedStarSystems"("systemID") ON DELETE CASCADE
);

CREATE TABLE "StagedStarSystems" (
	"systemID" NUMERIC(20, 0) PRIMARY KEY,
	FOREIGN KEY ("systemID") REFERENCES "StarSystems"("systemID") ON DELETE CASCADE
);

CREATE MATERIALIZED VIEW "DistinctColonisedStarSystems" AS
SELECT DISTINCT ON (css."colonisedSystemID")
	css."colonisedSystemID",
	ss."systemName"
FROM "ColonisableStarSystems" css
INNER JOIN "StarSystems" ss ON css."colonisedSystemID" = ss."systemID";

CREATE MATERIALIZED VIEW "DistinctUncolonisedStarSystems" AS
SELECT DISTINCT "uncolonisedSystemID" FROM "ColonisableStarSystems";

CREATE MATERIALIZED VIEW "MaxSearchValues" AS
SELECT
	ssbr."regionID",
	MAX(uss."landableCount") "landableCount",
	MAX(uss."walkableCount") "walkableCount",
	MAX(ssbr."distanceToCentre") "distanceToRegionCentre",
	MAX(uss."totalHotspots") "totalHotspots",
	MAX(coc."blackHoleCount") "blackHoleCount",
	MAX(coc."neutronStarCount") "neutronStarCount",
	MAX(coc."whiteDwarves") "whiteDwarves",
	MAX(coc."otherStarCount") "otherStarCount",
	MAX(coc."earthLikeCount") "earthLikeCount",
	MAX(coc."waterWorldCount") "waterWorldCount",
	MAX(coc."ammoniaWorldCount") "ammoniaWorldCount",
	MAX(coc."gasGiantCount") "gasGiantCount",
	MAX(coc."highMetalContentCount") "highMetalContentCount",
	MAX(coc."metalRichCount") "metalRichCount",
	MAX(coc."rockyIceBodyCount") "rockyIceBodyCount",
	MAX(coc."rockBodyCount") "rockBodyCount",
	MAX(coc."icyBodyCount") "icyBodyCount",
	MAX(coc."organicCount") "organicCount",
	MAX(coc."geologicalsCount") "geologicalsCount",
	MAX(coc."ringCount") "ringCount"
FROM "DistinctUncolonisedStarSystems" duss
INNER JOIN "StarSystemsByRegion" ssbr ON duss."uncolonisedSystemID" = ssbr."systemID"
INNER JOIN "ColonyOverrideCounts" coc ON duss."uncolonisedSystemID" = coc."systemID"
INNER JOIN "UncolonisedStarSystems" uss  ON duss."uncolonisedSystemID" = uss."systemID"
GROUP BY ssbr."regionID";

CREATE MATERIALIZED VIEW "SystemsByRegion" AS
SELECT DISTINCT
	ss."systemName",
	reg."regionName"
FROM "StarSystems" ss
INNER JOIN "StarSystemsByRegion" ssbr ON ss."systemID" = ssbr."systemID"
INNER JOIN "Regions" reg ON ssbr."regionID" = reg."regionID"
WHERE ss."isColonised" = TRUE;

CREATE MATERIALIZED VIEW "FactionsByRegion" AS
SELECT DISTINCT
	f."factionName",
	reg."regionName"
FROM "Factions" f
INNER JOIN "Stations" s ON f."factionID" = s."controllingFaction"
INNER JOIN "StarSystems" ss ON s."systemID" = ss."systemID"
INNER JOIN "StarSystemsByRegion" ssbr ON ss."systemID" = ssbr."systemID"
INNER JOIN "Regions" reg ON ssbr."regionID" = reg."regionID";

CREATE INDEX "idx_F_factionName" ON "Factions"("factionName");
CREATE INDEX "idx_SS_systemName" ON "StarSystems"("systemName");
CREATE INDEX "idx_SS_isColonised" ON "StarSystems"("isColonised");
CREATE INDEX "idx_SSBR_systemID" ON "StarSystemsByRegion"("systemID");
CREATE INDEX "idx_SSBR_regionID" ON "StarSystemsByRegion"("regionID");
CREATE INDEX "idx_SSBR_distanceToCentre" ON "StarSystemsByRegion"("distanceToCentre");
CREATE INDEX "idx_S_systemID" ON "Stations"("systemID");
CREATE INDEX "idx_S_controllingFaction" ON "Stations"("controllingFaction");
CREATE INDEX "idx_USS_lastUpdated" ON "UncolonisedStarSystems"("lastUpdated");
CREATE INDEX "idx_USS_landableCount" ON "UncolonisedStarSystems"("landableCount");
CREATE INDEX "idx_USS_walkableCount" ON "UncolonisedStarSystems"("walkableCount");
CREATE INDEX "idx_USS_totalHotspots" ON "UncolonisedStarSystems"("totalHotspots");
CREATE INDEX "idx_USS_systemValue" ON "UncolonisedStarSystems"("systemValue");
CREATE INDEX "idx_COC_blackHoleCount" ON "ColonyOverrideCounts"("blackHoleCount");
CREATE INDEX "idx_COC_neutronStarCount" ON "ColonyOverrideCounts"("neutronStarCount");
CREATE INDEX "idx_COC_whiteDwarves" ON "ColonyOverrideCounts"("whiteDwarves");
CREATE INDEX "idx_COC_otherStarCount" ON "ColonyOverrideCounts"("otherStarCount");
CREATE INDEX "idx_COC_earthLikeCount" ON "ColonyOverrideCounts"("earthLikeCount");
CREATE INDEX "idx_COC_waterWorldCount" ON "ColonyOverrideCounts"("waterWorldCount");
CREATE INDEX "idx_COC_ammoniaWorldCount" ON "ColonyOverrideCounts"("ammoniaWorldCount");
CREATE INDEX "idx_COC_gasGiantCount" ON "ColonyOverrideCounts"("gasGiantCount");
CREATE INDEX "idx_COC_highMetalContentCount" ON "ColonyOverrideCounts"("highMetalContentCount");
CREATE INDEX "idx_COC_metalRichCount" ON "ColonyOverrideCounts"("metalRichCount");
CREATE INDEX "idx_COC_rockyIceBodyCount" ON "ColonyOverrideCounts"("rockyIceBodyCount");
CREATE INDEX "idx_COC_rockBodyCount" ON "ColonyOverrideCounts"("rockBodyCount");
CREATE INDEX "idx_COC_icyBodyCount" ON "ColonyOverrideCounts"("icyBodyCount");
CREATE INDEX "idx_COC_organicCount" ON "ColonyOverrideCounts"("organicCount");
CREATE INDEX "idx_COC_geologicalsCount" ON "ColonyOverrideCounts"("geologicalsCount");
CREATE INDEX "idx_COC_ringCount" ON "ColonyOverrideCounts"("ringCount");
CREATE INDEX "idx_R_systemID" ON "Rings"("systemID");
CREATE INDEX "idx_H_ringID" ON "Hotspots"("ringID");
CREATE INDEX "idx_CSS_colonisedSystemID" ON "ColonisableStarSystems"("colonisedSystemID");
CREATE INDEX "idx_CSS_uncolonisedSystemID" ON "ColonisableStarSystems"("uncolonisedSystemID");
CREATE INDEX "idx_DCSS_systemName" ON "DistinctColonisedStarSystems"("systemName");
CREATE INDEX "idx_SBR_regionName" ON "SystemsByRegion"("regionName");
CREATE INDEX "idx_FBR_regionName" ON "FactionsByRegion"("regionName");

CREATE INDEX "idx_USSA_isLocked_isClaimed" ON "UncolonisedStarSystemsAvailability"("isLocked", "isClaimed");
CREATE INDEX "idx_SSBR_regionID_systemID" ON "StarSystemsByRegion" ("regionID", "systemID");
CREATE INDEX "idx_SSBR_regionID_distance" ON "StarSystemsByRegion"("regionID", "distanceToCentre");

CREATE INDEX "idx_SS_systemCoords" ON "StarSystems" USING GIST("systemCoords" gist_geometry_ops_nd);

CREATE INDEX "idx_SBR_systemName" ON "SystemsByRegion" USING GIN("systemName" gin_trgm_ops);
CREATE INDEX "idx_FBR_factionName" ON "FactionsByRegion" USING GIN("factionName" gin_trgm_ops);

CREATE UNIQUE INDEX "idx_DCSS_colonisedSystemID" ON "DistinctColonisedStarSystems"("colonisedSystemID");
CREATE UNIQUE INDEX "idx_DUSS_uncolonisedSystemID" ON "DistinctUncolonisedStarSystems"("uncolonisedSystemID");

CREATE STATISTICS "stats_SSBR_region_dist" ON "regionID", "distanceToCentre" FROM "StarSystemsByRegion";

CREATE TYPE "StarSystemInsertType" AS (
    "systemID" NUMERIC(20, 0),
    "systemName" VARCHAR(75),
    "isColonised" BOOLEAN,
    "coordinateX" NUMERIC(11, 5),
    "coordinateY" NUMERIC(11, 5),
    "coordinateZ" NUMERIC(11, 5)
);

CREATE TYPE "StationInsertType" AS (
    "stationID" NUMERIC(20, 0),
    "systemID" NUMERIC(20, 0),
    "stationName" VARCHAR(75),
    "controllingFaction" VARCHAR(75)
);

CREATE TYPE "UncolonisedDetailsInsertType" AS (
	"systemID" NUMERIC(20, 0),
	"lastUpdated" TIMESTAMPTZ,
	"reserveLevel" "ReserveType",
	"landableCount" SMALLINT,
	"walkableCount" SMALLINT,
	"totalHotspots" SMALLINT,
	"systemValue" NUMERIC(5,2),
	"blackHoleCount" SMALLINT,
	"neutronStarCount" SMALLINT,
	"whiteDwarves" SMALLINT,
	"otherStarCount" SMALLINT,
	"earthLikeCount" SMALLINT,
	"waterWorldCount" SMALLINT,
	"ammoniaWorldCount" SMALLINT,
	"gasGiantCount" SMALLINT,
	"highMetalContentCount" SMALLINT,
	"metalRichCount" SMALLINT,
	"rockyIceBodyCount" SMALLINT,
	"rockBodyCount" SMALLINT,
	"icyBodyCount" SMALLINT,
	"organicCount" SMALLINT,
	"geologicalsCount" SMALLINT,
	"ringCount" SMALLINT
);

CREATE TYPE "RingInsertType" AS (
	"systemID" NUMERIC(20, 0),
	"ringName" VARCHAR(75),
	"ringType" "RingType"
);

CREATE TYPE "HotspotInsertType" AS (
	"systemID" NUMERIC(20, 0),
	"ringName" VARCHAR(75),
	"hotspotType" "HotspotType",
	"hotspotCount" SMALLINT
);

CREATE TYPE "ColonisableInsertType" AS (
	"colonisedSystemID" NUMERIC(20, 0),
	"uncolonisedSystemID" NUMERIC(20, 0)
);

COMMIT TRANSACTION;