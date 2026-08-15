BEGIN TRANSACTION;

CREATE OR REPLACE FUNCTION "GetRegionCube"("centre" PUBLIC.GEOMETRY, "range" SMALLINT)
RETURNS PUBLIC.GEOMETRY AS $$
	SELECT PUBLIC.ST_3DMakeBox(
		PUBLIC.ST_MakePoint(
			PUBLIC.ST_X("centre") - "range",
			PUBLIC.ST_Y("centre") - "range",
			PUBLIC.ST_Z("centre") - "range"
		),
		PUBLIC.ST_MakePoint(
			PUBLIC.ST_X("centre") + "range",
			PUBLIC.ST_Y("centre") + "range",
			PUBLIC.ST_Z("centre") + "range"
		)
	)::PUBLIC.GEOMETRY
$$ LANGUAGE sql IMMUTABLE PARALLEL SAFE;

CREATE OR REPLACE FUNCTION "InsertRegion"(
	"inputRegionName" VARCHAR(75),
	"inputRegionRange" SMALLINT,
	"inputCoordinateX" NUMERIC(11, 5),
    "inputCoordinateY" NUMERIC(11, 5),
    "inputCoordinateZ" NUMERIC(11, 5)
)
RETURNS VOID AS $$
	INSERT INTO "Regions" (
		"regionName",
		"regionRange",
		"regionCentreCoords"
	)
	VALUES (
		"inputRegionName",
		"inputRegionRange",
		ST_MakePoint("inputCoordinateX", "inputCoordinateY", "inputCoordinateZ")
	)
	ON CONFLICT ("regionName") DO NOTHING;
$$ LANGUAGE sql;

CREATE OR REPLACE FUNCTION "SelectRegions"()
RETURNS TABLE (
	"regionName" VARCHAR(75),
	"regionRange" SMALLINT,
	"centreCoordinateX" NUMERIC(11, 5),
    "centreCoordinateY" NUMERIC(11, 5),
    "centreCoordinateZ" NUMERIC(11, 5)
) AS $$
	SELECT 
		"regionName",
		"regionRange",
		ST_X("regionCentreCoords"),
		ST_Y("regionCentreCoords"),
		ST_Z("regionCentreCoords")
	FROM "Regions";
$$ LANGUAGE sql;

CREATE OR REPLACE FUNCTION "InsertStarSystemsBulk"("inputStarSystems" "StarSystemInsertType"[])
RETURNS VOID AS $$
	-- Insert Factions
	INSERT INTO "Factions" ("factionName")
	SELECT DISTINCT "controllingFaction"
	FROM unnest("inputStarSystems") AS inss
	WHERE "controllingFaction" IS NOT NULL
	ON CONFLICT ("factionName") DO NOTHING;
	
	-- Insert Star Systems
	INSERT INTO "StarSystems" (
		"systemID",
		"systemName",
		"systemCoords",
		"isColonised",
		"controllingFaction"
	)
	SELECT
		inss."systemID",
		inss."systemName",
		ST_MakePoint(inss."coordinateX", inss."coordinateY", inss."coordinateZ"),
		inss."isColonised",
		f."factionID"
	FROM unnest("inputStarSystems") AS inss
	LEFT JOIN "Factions" f ON inss."controllingFaction" = f."factionName"
	ON CONFLICT ("systemID") DO UPDATE
	SET
		"isColonised" = EXCLUDED."isColonised",
		"controllingFaction" = EXCLUDED."controllingFaction"
	WHERE ("StarSystems"."isColonised" = FALSE AND EXCLUDED."isColonised" = TRUE)
	OR "StarSystems"."controllingFaction" IS DISTINCT FROM EXCLUDED."controllingFaction";
	
	-- Insert Star System Region
	INSERT INTO "StarSystemsByRegion" (
		"systemID",
		"regionID",
		"distanceToCentre"
	)
	SELECT
		pt."systemID",
		reg."regionID",
		ST_3DDistance(pt."point", reg."regionCentreCoords")
	FROM (
		SELECT 
			inss."systemID",
			ST_MakePoint(inss."coordinateX", inss."coordinateY", inss."coordinateZ") AS "point"
		FROM unnest("inputStarSystems") AS inss
	) pt
	INNER JOIN "Regions" reg 
		ON ST_3DIntersects(pt."point", "GetRegionCube"(reg."regionCentreCoords", reg."regionRange"))
	ON CONFLICT ("systemID", "regionID") DO NOTHING;
$$ LANGUAGE sql;

CREATE OR REPLACE FUNCTION "InsertStationsBulk"("inputStations" "StationInsertType"[])
RETURNS VOID AS $$
	-- Insert Factions
	INSERT INTO "Factions" ("factionName")
	SELECT DISTINCT "controllingFaction"
	FROM unnest("inputStations") AS ins
	ON CONFLICT ("factionName") DO NOTHING;
	
	-- Insert Stations
	INSERT INTO "Stations" (
		"stationID",
		"systemID",
		"stationName",
		"controllingFaction"
	)
	SELECT
		ins."stationID",
		ins."systemID",
		ins."stationName",
		f."factionID"
	FROM unnest("inputStations") AS ins
	INNER JOIN "Factions" f ON ins."controllingFaction" = f."factionName"
	ON CONFLICT ("stationID") DO UPDATE
	SET
		"stationName" = EXCLUDED."stationName",
		"controllingFaction" = EXCLUDED."controllingFaction"
	WHERE (
		"Stations"."stationName",
		"Stations"."controllingFaction"
	)
	IS DISTINCT FROM (
		EXCLUDED."stationName",
		EXCLUDED."controllingFaction"
	);
$$ LANGUAGE sql;

CREATE OR REPLACE FUNCTION "InsertUncolonisedStarSystemDetailsBulk"("inputDetails" "UncolonisedDetailsInsertType"[])
RETURNS VOID AS $$	
	-- Insert UncolonisedStarSystems
	INSERT INTO "UncolonisedStarSystems" (
		"systemID",
		"lastUpdated",
		"reserveLevel",
		"landableCount",
		"walkableCount",
		"totalHotspots",
		"systemValue"
	)
	SELECT
		"systemID",
		"lastUpdated",
		"reserveLevel",
		"landableCount",
		"walkableCount",
		"totalHotspots",
		"systemValue"
	FROM unnest("inputDetails") AS ind
	ON CONFLICT ("systemID") DO UPDATE
	SET
		"lastUpdated" = EXCLUDED."lastUpdated",
		"reserveLevel" = EXCLUDED."reserveLevel",
		"landableCount" = EXCLUDED."landableCount",
		"walkableCount" = EXCLUDED."walkableCount",
		"totalHotspots" = EXCLUDED."totalHotspots",
		"systemValue" = EXCLUDED."systemValue"
	WHERE (
		"UncolonisedStarSystems"."lastUpdated",
		"UncolonisedStarSystems"."reserveLevel",
		"UncolonisedStarSystems"."landableCount",
		"UncolonisedStarSystems"."walkableCount",
		"UncolonisedStarSystems"."totalHotspots",
		"UncolonisedStarSystems"."systemValue"
	)
	IS DISTINCT FROM (
		EXCLUDED."lastUpdated",
		EXCLUDED."reserveLevel",
		EXCLUDED."landableCount",
		EXCLUDED."walkableCount",
		EXCLUDED."totalHotspots",
		EXCLUDED."systemValue"
	);
	
	
	-- Insert ColonyOverrideCounts
	INSERT INTO "ColonyOverrideCounts" (
		"systemID",
		"blackHoleCount",
		"neutronStarCount",
		"whiteDwarves",
		"otherStarCount",
		"earthLikeCount",
		"waterWorldCount",
		"ammoniaWorldCount",
		"gasGiantCount",
		"highMetalContentCount",
		"metalRichCount",
		"rockyIceBodyCount",
		"rockBodyCount",
		"icyBodyCount",
		"organicCount",
		"geologicalsCount",
		"ringCount",
		"terraformableCount",
		"volcanicsCount"
	)
	SELECT
		"systemID",
		"blackHoleCount",
		"neutronStarCount",
		"whiteDwarves",
		"otherStarCount",
		"earthLikeCount",
		"waterWorldCount",
		"ammoniaWorldCount",
		"gasGiantCount",
		"highMetalContentCount",
		"metalRichCount",
		"rockyIceBodyCount",
		"rockBodyCount",
		"icyBodyCount",
		"organicCount",
		"geologicalsCount",
		"ringCount",
		"terraformableCount",
		"volcanicsCount"
	FROM unnest("inputDetails") AS ind
	ON CONFLICT ("systemID") DO UPDATE
	SET
		"blackHoleCount" = EXCLUDED."blackHoleCount",
		"neutronStarCount" = EXCLUDED."neutronStarCount",
		"whiteDwarves" = EXCLUDED."whiteDwarves",
		"otherStarCount" = EXCLUDED."otherStarCount",
		"earthLikeCount" = EXCLUDED."earthLikeCount",
		"waterWorldCount" = EXCLUDED."waterWorldCount",
		"ammoniaWorldCount" = EXCLUDED."ammoniaWorldCount",
		"gasGiantCount" = EXCLUDED."gasGiantCount",
		"highMetalContentCount" = EXCLUDED."highMetalContentCount",
		"metalRichCount" = EXCLUDED."metalRichCount",
		"rockyIceBodyCount" = EXCLUDED."rockyIceBodyCount",
		"rockBodyCount" = EXCLUDED."rockBodyCount",
		"icyBodyCount" = EXCLUDED."icyBodyCount",
		"organicCount" = EXCLUDED."organicCount",
		"geologicalsCount" = EXCLUDED."geologicalsCount",
		"ringCount" = EXCLUDED."ringCount",
		"terraformableCount" = EXCLUDED."terraformableCount",
		"volcanicsCount" = EXCLUDED."volcanicsCount"
	WHERE (
		"ColonyOverrideCounts"."blackHoleCount",
		"ColonyOverrideCounts"."neutronStarCount",
		"ColonyOverrideCounts"."whiteDwarves",
		"ColonyOverrideCounts"."otherStarCount",
		"ColonyOverrideCounts"."earthLikeCount",
		"ColonyOverrideCounts"."waterWorldCount",
		"ColonyOverrideCounts"."ammoniaWorldCount",
		"ColonyOverrideCounts"."gasGiantCount",
		"ColonyOverrideCounts"."highMetalContentCount",
		"ColonyOverrideCounts"."metalRichCount",
		"ColonyOverrideCounts"."rockyIceBodyCount",
		"ColonyOverrideCounts"."rockBodyCount",
		"ColonyOverrideCounts"."icyBodyCount",
		"ColonyOverrideCounts"."organicCount",
		"ColonyOverrideCounts"."geologicalsCount",
		"ColonyOverrideCounts"."ringCount",
		"ColonyOverrideCounts"."terraformableCount",
		"ColonyOverrideCounts"."volcanicsCount"
	)
	IS DISTINCT FROM (
		EXCLUDED."blackHoleCount",
		EXCLUDED."neutronStarCount",
		EXCLUDED."whiteDwarves",
		EXCLUDED."otherStarCount",
		EXCLUDED."earthLikeCount",
		EXCLUDED."waterWorldCount",
		EXCLUDED."ammoniaWorldCount",
		EXCLUDED."gasGiantCount",
		EXCLUDED."highMetalContentCount",
		EXCLUDED."metalRichCount",
		EXCLUDED."rockyIceBodyCount",
		EXCLUDED."rockBodyCount",
		EXCLUDED."icyBodyCount",
		EXCLUDED."organicCount",
		EXCLUDED."geologicalsCount",
		EXCLUDED."ringCount",
		EXCLUDED."terraformableCount",
		EXCLUDED."volcanicsCount"
	);
$$ LANGUAGE sql;

CREATE OR REPLACE FUNCTION "InsertRingsBulk"("inputRings" "RingInsertType"[])
RETURNS VOID AS $$	
	INSERT INTO "Rings" (
		"systemID",
		"ringName",
		"ringType"
	)
	SELECT 
		"systemID",
		"ringName",
		"ringType"
	FROM unnest("inputRings") AS inr
	ON CONFLICT ("systemID", "ringName") DO NOTHING;
$$ LANGUAGE sql;

CREATE OR REPLACE FUNCTION "InsertHotspotsBulk"("inputHotspots" "HotspotInsertType"[])
RETURNS VOID AS $$	
	INSERT INTO "Hotspots" (
		"ringID",
		"hotspotType",
		"hotspotCount"
	)
	SELECT 
		r."ringID",
		inh."hotspotType",
		inh."hotspotCount"
	FROM unnest("inputHotspots") AS inh
	INNER JOIN "Rings" r ON inh."systemID" = r."systemID" AND inh."ringName" = r."ringName"
	ON CONFLICT ("ringID", "hotspotType") DO UPDATE
	SET
		"hotspotCount" = EXCLUDED."hotspotCount"
	WHERE "Hotspots"."hotspotCount" != EXCLUDED."hotspotCount";
$$ LANGUAGE sql;

CREATE OR REPLACE FUNCTION "InsertColonisableStarSystemsBulk"("inputColonisables" "ColonisableInsertType"[])
RETURNS VOID AS $$
	INSERT INTO "ColonisableStarSystems" (
		"colonisedSystemID",
		"uncolonisedSystemID"
	)
	SELECT
		"colonisedSystemID",
		"uncolonisedSystemID"
	FROM unnest ("inputColonisables") AS inc
	ON CONFLICT ("colonisedSystemID", "uncolonisedSystemID") DO NOTHING;
$$ LANGUAGE sql;

CREATE OR REPLACE FUNCTION "InsertColonisableStarSystemsFromStaged"("insertColonised" BOOLEAN)
RETURNS VOID AS $$
	INSERT INTO "ColonisableStarSystems" (
		"colonisedSystemID",
		"uncolonisedSystemID"
	)
	SELECT
		CASE
			WHEN "insertColonised"
				THEN source."systemID"
				ELSE target."systemID"
		END AS "colonisedSystemID",
		
		CASE
			WHEN "insertColonised"
				THEN target."systemID"
				ELSE source."systemID"
		END AS "uncolonisedSystemID"
	FROM "StagedStarSystems" sss
	INNER JOIN "StarSystems" source ON sss."systemID" = source."systemID"
	INNER JOIN "StarSystems" target ON ST_3DDWithin(source."systemCoords", target."systemCoords", 15)
	WHERE source."isColonised" = "insertColonised"
	AND target."isColonised" = NOT "insertColonised"
	AND source."systemID" != target."systemID"
	ON CONFLICT("colonisedSystemID", "uncolonisedSystemID") DO NOTHING;
$$ LANGUAGE sql;

CREATE OR REPLACE FUNCTION "ClaimStarSystem"("inputSystemID" NUMERIC(20, 0), "inputClaimDate" TIMESTAMPTZ)
RETURNS VOID AS $$
	UPDATE "UncolonisedStarSystemsAvailability"
	SET
		"isClaimed" = TRUE,
		"claimReportDate" = "inputClaimDate"
	WHERE "systemID" = "inputSystemID"
	AND "isClaimed" = FALSE
	AND ("inputClaimDate" > "claimReportDate" OR "claimReportDate" IS NULL);
$$ LANGUAGE sql;

CREATE OR REPLACE FUNCTION "SelectSearchResults" (
	"inputRegionName" VARCHAR(75),
	"sortOrder" "ResultOrderType",
	"pageNo" INT,
	"resultsPerPage" SMALLINT,
	"inputSystemName" VARCHAR(75),
	"factionSearchMode" BOOLEAN,
	"inputFactionName" VARCHAR(75),
	"inputMinBlackHoles" SMALLINT,
	"inputMaxBlackHoles" SMALLINT,
	"inputMinNeutronStars" SMALLINT,
	"inputMaxNeutronStars" SMALLINT,
	"inputMinWhiteDwarves" SMALLINT,
	"inputMaxWhiteDwarves" SMALLINT,
	"inputMinOtherStars" SMALLINT,
	"inputMaxOtherStars" SMALLINT,
	"inputMinEarthLikes" SMALLINT,
	"inputMaxEarthLikes" SMALLINT,
	"inputMinWaterWorlds" SMALLINT,
	"inputMaxWaterWorlds" SMALLINT,
	"inputMinAmmoniaWorlds" SMALLINT,
	"inputMaxAmmoniaWorlds" SMALLINT,
	"inputMinGasGiants" SMALLINT,
	"inputMaxGasGiants" SMALLINT,
	"inputMinHighMetalContents" SMALLINT,
	"inputMaxHighMetalContents" SMALLINT,
	"inputMinMetalRiches" SMALLINT,
	"inputMaxMetalRiches" SMALLINT,
	"inputMinRockyIces" SMALLINT,
	"inputMaxRockyIces" SMALLINT,
	"inputMinRocks" SMALLINT,
	"inputMaxRocks" SMALLINT,
	"inputMinIcys" SMALLINT,
	"inputMaxIcys" SMALLINT,
	"inputMinOrganics" SMALLINT,
	"inputMaxOrganics" SMALLINT,
	"inputMinGeologicals" SMALLINT,
	"inputMaxGeologicals" SMALLINT,
	"inputMinRings" SMALLINT,
	"inputMaxRings" SMALLINT,
	"inputMinLandables" SMALLINT,
	"inputMaxLandables" SMALLINT,
	"inputMinWalkables" SMALLINT,
	"inputMaxWalkables" SMALLINT,
	"inputMinTerraformables" SMALLINT,
	"inputMaxTerraformables" SMALLINT,
	"inputMinVolcanics" SMALLINT,
	"inputMaxVolcanics" SMALLINT,
	"inputMaxDistanceToRegionCentre" INT,
	"inputHotspotTypes" "HotspotType"[],
	"inputRemovedSystemIDs" NUMERIC(20, 0)[]
)
RETURNS jsonb
SET plan_cache_mode = force_custom_plan
AS $$
DECLARE
    "savedRegionID" INT;
	"targetSystemIDs" NUMERIC(20, 0)[] := NULL;
    "result" jsonb;
BEGIN
	SELECT "regionID" INTO "savedRegionID" FROM "Regions" WHERE "regionName" = "inputRegionName";
	
	IF "inputSystemName" IS NOT NULL AND "inputFactionName" IS NOT NULL THEN
		IF NOT "factionSearchMode" THEN
			SELECT array_agg(DISTINCT css."uncolonisedSystemID") INTO "targetSystemIDs"
			FROM "ColonisableStarSystems" css
			INNER JOIN "DistinctColonisedStarSystems" dcss ON css."colonisedSystemID" = dcss."colonisedSystemID"
			INNER JOIN "Stations" s ON dcss."colonisedSystemID" = s."systemID"
			INNER JOIN "Factions" f ON s."controllingFaction" = f."factionID"
			WHERE dcss."systemName" = "inputSystemName" AND f."factionName" = "inputFactionName";
			
		ELSE
			SELECT array_agg(DISTINCT css."uncolonisedSystemID") INTO "targetSystemIDs"
			FROM "ColonisableStarSystems" css
			INNER JOIN "DistinctColonisedStarSystems" dcss ON css."colonisedSystemID" = dcss."colonisedSystemID"
			WHERE dcss."systemName" = "inputSystemName" AND dcss."factionName" = "inputFactionName";
		END IF;
		
	ELSIF "inputSystemName" IS NOT NULL THEN
		SELECT array_agg(DISTINCT css."uncolonisedSystemID") INTO "targetSystemIDs"
		FROM "ColonisableStarSystems" css
		INNER JOIN "DistinctColonisedStarSystems" dcss ON css."colonisedSystemID" = dcss."colonisedSystemID"
		WHERE dcss."systemName" = "inputSystemName";
		
	ELSIF "inputFactionName" IS NOT NULL THEN
		IF NOT "factionSearchMode" THEN
			SELECT array_agg(DISTINCT css."uncolonisedSystemID") INTO "targetSystemIDs"
			FROM "ColonisableStarSystems" css
			INNER JOIN "DistinctColonisedStarSystems" dcss ON css."colonisedSystemID" = dcss."colonisedSystemID"
			INNER JOIN "Stations" s ON dcss."colonisedSystemID" = s."systemID"
			INNER JOIN "Factions" f ON s."controllingFaction" = f."factionID"
			WHERE f."factionName" = "inputFactionName";
		ELSE
			SELECT array_agg(DISTINCT css."uncolonisedSystemID") INTO "targetSystemIDs"
			FROM "ColonisableStarSystems" css
			INNER JOIN "DistinctColonisedStarSystems" dcss ON css."colonisedSystemID" = dcss."colonisedSystemID"
			WHERE dcss."factionName" = "inputFactionName";
		END IF;
	END IF;
	
	IF ("inputSystemName" IS NOT NULL OR "inputFactionName" IS NOT NULL) AND "targetSystemIDs" IS NULL THEN
		"targetSystemIDs" := ARRAY[-1]::NUMERIC(20, 0)[];
	END IF;
	
	IF "inputHotspotTypes" IS NOT NULL AND CARDINALITY("inputHotspotTypes") > 0 THEN
		IF "targetSystemIDs" IS NULL OR "targetSystemIDs" <> ARRAY[-1]::NUMERIC(20, 0)[] THEN
			DECLARE
				"hotspotSystemIDs" NUMERIC(20, 0)[];
			BEGIN
				SELECT array_agg(DISTINCT r."systemID") INTO "hotspotSystemIDs"
				FROM "Rings" r
				INNER JOIN "Hotspots" h ON r."ringID" = h."ringID"
				INNER JOIN "StarSystemsByRegion" ssbr ON ssbr."systemID" = r."systemID"
				WHERE h."hotspotType" = ANY("inputHotspotTypes")
					AND ssbr."regionID" = "savedRegionID"
					AND ssbr."distanceToCentre" <= "inputMaxDistanceToRegionCentre"
					AND ("targetSystemIDs" IS NULL OR r."systemID" = ANY("targetSystemIDs"));
					
				"targetSystemIDs" := "hotspotSystemIDs";
			END;
		
			IF "targetSystemIDs" IS NULL THEN
				"targetSystemIDs" := ARRAY[-1]::NUMERIC(20, 0)[];
			END IF;
		END IF;
	END IF;
	
	IF "inputRemovedSystemIDs" IS NULL THEN
		"inputRemovedSystemIDs" := '{}'::NUMERIC(20, 0)[];
	END IF;
	
	IF "targetSystemIDs" IS NOT NULL AND "targetSystemIDs" <> ARRAY[-1]::NUMERIC(20, 0)[] AND CARDINALITY("inputRemovedSystemIDs") > 0 THEN
		SELECT array_agg("pruned") INTO "targetSystemIDs"
		FROM unnest("targetSystemIDs") AS "pruned"
		WHERE NOT ("pruned" = ANY("inputRemovedSystemIDs"));
		
		IF "targetSystemIDs" IS NULL THEN
			"targetSystemIDs" := ARRAY[-1]::NUMERIC(20, 0)[];
		END IF;
		
		"inputRemovedSystemIDs" := '{}'::NUMERIC(20, 0)[];
	END IF;

	WITH "TopResults" AS (
		SELECT 
			duss."uncolonisedSystemID",
			ss."systemName",
			ss."systemCoords",
			uss."lastUpdated",
			uss."reserveLevel",
			uss."landableCount",
			uss."walkableCount",
			ssbr."distanceToCentre" AS "distanceToRegionCentre",
			uss."totalHotspots",
			uss."systemValue",
			coc."blackHoleCount",
			coc."neutronStarCount",
			coc."whiteDwarves",
			coc."otherStarCount",
			coc."earthLikeCount",
			coc."waterWorldCount",
			coc."ammoniaWorldCount",
			coc."gasGiantCount",
			coc."highMetalContentCount",
			coc."metalRichCount",
			coc."rockyIceBodyCount",
			coc."rockBodyCount",
			coc."icyBodyCount",
			coc."organicCount",
			coc."geologicalsCount",
			coc."ringCount",
			coc."terraformableCount",
			coc."volcanicsCount"
		FROM "DistinctUncolonisedStarSystems" duss
		INNER JOIN "StarSystems" ss ON duss."uncolonisedSystemID" = ss."systemID"
		INNER JOIN "StarSystemsByRegion" ssbr ON ss."systemID" = ssbr."systemID"
		INNER JOIN "UncolonisedStarSystems" uss ON duss."uncolonisedSystemID" = uss."systemID"
		INNER JOIN "ColonyOverrideCounts" coc ON duss."uncolonisedSystemID" = coc."systemID"
		INNER JOIN "UncolonisedStarSystemsAvailability" ussa ON duss."uncolonisedSystemID" = ussa."systemID"
		WHERE ssbr."regionID" = "savedRegionID"
			AND ssbr."distanceToCentre" <= "inputMaxDistanceToRegionCentre"
			AND ussa."isLocked" = FALSE
			AND ussa."isClaimed" = FALSE
			AND coc."blackHoleCount" BETWEEN "inputMinBlackHoles" AND "inputMaxBlackHoles"
			AND coc."neutronStarCount" BETWEEN "inputMinNeutronStars" AND "inputMaxNeutronStars"
			AND coc."whiteDwarves" BETWEEN "inputMinWhiteDwarves" AND "inputMaxWhiteDwarves"
			AND coc."otherStarCount" BETWEEN "inputMinOtherStars" AND "inputMaxOtherStars"
			AND coc."earthLikeCount" BETWEEN "inputMinEarthLikes" AND "inputMaxEarthLikes"
			AND coc."waterWorldCount" BETWEEN "inputMinWaterWorlds" AND "inputMaxWaterWorlds"
			AND coc."ammoniaWorldCount" BETWEEN "inputMinAmmoniaWorlds" AND "inputMaxAmmoniaWorlds"
			AND coc."gasGiantCount" BETWEEN "inputMinGasGiants" AND "inputMaxGasGiants"
			AND coc."highMetalContentCount" BETWEEN "inputMinHighMetalContents" AND "inputMaxHighMetalContents"
			AND coc."metalRichCount" BETWEEN "inputMinMetalRiches" AND "inputMaxMetalRiches"
			AND coc."rockyIceBodyCount" BETWEEN "inputMinRockyIces" AND "inputMaxRockyIces"
			AND coc."rockBodyCount" BETWEEN "inputMinRocks" AND "inputMaxRocks"
			AND coc."icyBodyCount" BETWEEN "inputMinIcys" AND "inputMaxIcys"
			AND coc."organicCount" BETWEEN "inputMinOrganics" AND "inputMaxOrganics"
			AND coc."geologicalsCount" BETWEEN "inputMinGeologicals" AND "inputMaxGeologicals"
			AND coc."ringCount" BETWEEN "inputMinRings" AND "inputMaxRings"
			AND coc."terraformableCount" BETWEEN "inputMinTerraformables" AND "inputMaxTerraformables"
			AND coc."volcanicsCount" BETWEEN "inputMinVolcanics" AND "inputMaxVolcanics"
			AND uss."landableCount" BETWEEN "inputMinLandables" AND "inputMaxLandables"
			AND uss."walkableCount" BETWEEN "inputMinWalkables" AND "inputMaxWalkables"
			AND ("inputRemovedSystemIDs" IS NULL OR NOT (duss."uncolonisedSystemID" = ANY("inputRemovedSystemIDs")))
			AND ("targetSystemIDs" IS NULL OR duss."uncolonisedSystemID" = ANY ("targetSystemIDs"))
			AND NOT (duss."uncolonisedSystemID" = ANY("inputRemovedSystemIDs"))
		ORDER BY
			CASE WHEN "sortOrder" = 'SystemValue' THEN uss."systemValue" END DESC,
			CASE WHEN "sortOrder" = 'MostWalkables' THEN uss."walkableCount" END DESC,
			CASE WHEN "sortOrder" = 'DistanceToRegionCentre' THEN ssbr."distanceToCentre" END ASC,
			CASE WHEN "sortOrder" = 'MostHotspots' THEN uss."totalHotspots" END DESC
		OFFSET (("pageNo" - 1) * "resultsPerPage") ROWS
		LIMIT "resultsPerPage" * 11
	)
	SELECT jsonb_build_object(
		'minFollwingPages', MIN(tr."minFollwingPages"),
		'results', jsonb_agg(
			jsonb_build_object(
				'systemID', tr."uncolonisedSystemID",
				'systemName', tr."systemName",
				'lastUpdate', tr."lastUpdated",
				'distanceToRegionCentre', tr."distanceToRegionCentre",
				'coordinates', jsonb_build_object(
						'coordinateX', ST_X(tr."systemCoords"),
						'coordinateY', ST_Y(tr."systemCoords"),
						'coordinateZ', ST_Z(tr."systemCoords")
					),
				'reserveLevel', tr."reserveLevel",
				'landableCount', tr."landableCount",
				'walkableCount', tr."walkableCount",
				'systemCounts', jsonb_build_object(
					'blackHoleCount', tr."blackHoleCount",
					'neutronStarCount', tr."neutronStarCount",
					'whiteDwarves', tr."whiteDwarves",
					'otherStarCount', tr."otherStarCount",
					'earthLikeCount', tr."earthLikeCount",
					'waterWorldCount', tr."waterWorldCount",
					'ammoniaWorldCount', tr."ammoniaWorldCount",
					'gasGiantCount', tr."gasGiantCount",
					'highMetalContentCount', tr."highMetalContentCount",
					'metalRichCount', tr."metalRichCount",
					'rockyIceBodyCount',tr."rockyIceBodyCount",
					'rockBodyCount', tr."rockBodyCount",
					'icyBodyCount', tr."icyBodyCount",
					'organicCount', tr."organicCount",
					'geologicalsCount', tr."geologicalsCount",
					'ringCount', tr."ringCount",
					'terraformableCount', tr."terraformableCount",
					'volcanicsCount', tr."volcanicsCount",
					'totalHotspots', tr."totalHotspots"
				),
				'rings', (
					SELECT jsonb_agg(
						jsonb_build_object(
							'ringName', r."ringName",
							'ringType', r."ringType",
							'hotspots', (
								SELECT jsonb_agg(
									jsonb_build_object(
										'hotspotType', h."hotspotType",
										'hotspotCount', h."hotspotCount"
									)
								)
								FROM "Hotspots" h
								WHERE h."ringID" = r."ringID"
							)
						)
					)
					FROM "Rings" r
					WHERE r."systemID" = tr."uncolonisedSystemID"
				),
				'colonisedSystems', (
					SELECT jsonb_agg(
						jsonb_build_object(
							'colonisedSystemID', css."colonisedSystemID",
							'systemName', ss."systemName",
							'controllingFaction', f1."factionName",
							'stations', (
								SELECT jsonb_agg(
									jsonb_build_object(
										'stationID', s."stationID",
										'stationName', s."stationName",
										'controllingFaction', f2."factionName"
									)
								)
								FROM "Stations" s
								INNER JOIN "Factions" f2 ON s."controllingFaction" = f2."factionID"
								WHERE s."systemID" = css."colonisedSystemID"
							)
						)
					)
					FROM "ColonisableStarSystems" css
					INNER JOIN "StarSystems" ss ON css."colonisedSystemID" = ss."systemID"
					LEFT JOIN "Factions" f1 ON ss."controllingFaction" = f1."factionID"
					WHERE css."uncolonisedSystemID" = tr."uncolonisedSystemID"
				)
			)
		)
	)
	FROM (
		SELECT 
			*,
			GREATEST((COUNT(*) OVER()) / "resultsPerPage", 1) - 1 AS "minFollwingPages"
			FROM "TopResults"
			LIMIT "resultsPerPage"
	) tr INTO "result";
	
	RETURN "result";
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION "SelectFactionNamesJson"("inputFactionName" VARCHAR(75), "inputRegionName" VARCHAR(75))
RETURNS jsonb AS $$
	SELECT jsonb_agg(
		jsonb_build_object(
			'name', "factionName"
		)
	)
	FROM (
		SELECT DISTINCT "factionName"
		FROM "FactionsByRegion"
		WHERE "factionName" ILIKE '%' || "inputFactionName" || '%'
		AND "regionName" = "inputRegionName"
		LIMIT 20
	) as "factionNames";
$$ LANGUAGE sql;

CREATE OR REPLACE FUNCTION "SelectColonisedSystemNamesJson"("inputSystemName" VARCHAR(75), "inputRegionName" VARCHAR(75))
RETURNS jsonb AS $$
	SELECT jsonb_agg(
		jsonb_build_object(
			'name', "systemName"
		)
	)
	FROM (
		SELECT DISTINCT "systemName"
		FROM "SystemsByRegion"
		WHERE "systemName" ILIKE '%' || "inputSystemName" || '%'
		AND "regionName" = "inputRegionName"
		LIMIT 20
	) as "systemNames";
$$ LANGUAGE sql;

CREATE OR REPLACE FUNCTION "RefreshDistinctColonisedStarSystems"()
RETURNS void AS $$
BEGIN
    REFRESH MATERIALIZED VIEW "DistinctColonisedStarSystems";
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

CREATE OR REPLACE FUNCTION "RefreshDistinctUncolonisedStarSystems"()
RETURNS void AS $$
BEGIN
    REFRESH MATERIALIZED VIEW "DistinctUncolonisedStarSystems";
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

CREATE OR REPLACE FUNCTION "RefreshMaxSearchValues"()
RETURNS void AS $$
BEGIN
    REFRESH MATERIALIZED VIEW "MaxSearchValues";
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

CREATE OR REPLACE FUNCTION "RefreshSystemsByRegion"()
RETURNS void AS $$
BEGIN
    REFRESH MATERIALIZED VIEW "SystemsByRegion";
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

CREATE OR REPLACE FUNCTION "RefreshFactionsByRegion"()
RETURNS void AS $$
BEGIN
    REFRESH MATERIALIZED VIEW "FactionsByRegion";
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

CREATE OR REPLACE FUNCTION "ReportStarSystem"("inputSystemID" NUMERIC(20, 0), "isLockReport" BOOLEAN)
RETURNS VOID AS $$
BEGIN
	IF "isLockReport" THEN
		UPDATE "UncolonisedStarSystemsAvailability"
		SET "lockReportCount" = "lockReportCount" + 1
		WHERE "systemID" = "inputSystemID"
		AND NOT "isLocked";
	
	ELSE
		UPDATE "UncolonisedStarSystemsAvailability"
		SET "claimReportCount" = "claimReportCount" + 1
		WHERE "systemID" = "inputSystemID"
		AND NOT "isClaimed";
	END IF;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION "SelectMaxValues"("inputRegionName" VARCHAR(75))
RETURNS "MaxSearchValues" AS $$
	SELECT msv.* FROM "MaxSearchValues" msv
	INNER JOIN "Regions" r ON msv."regionID" = r."regionID"
	WHERE r."regionName" = "inputRegionName"
	LIMIT 1;
$$ LANGUAGE sql;

COMMIT TRANSACTION;