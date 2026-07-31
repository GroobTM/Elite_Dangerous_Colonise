-- Run second, after creating extensions in DDL
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

-- Run fourth, after running rest of DDL
BEGIN TRANSACTION;

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


CREATE OR REPLACE FUNCTION "InsertStarSystemsBulk"("inputStarSystems" "StarSystemInsertType"[])
RETURNS VOID AS $$
	INSERT INTO "StarSystems" (
		"systemID",
		"systemName",
		"systemCoords",
		"isColonised"
	)
	SELECT
		"systemID",
		"systemName",
		ST_MakePoint("coordinateX", "coordinateY", "coordinateZ"),
		"isColonised"
	FROM unnest("inputStarSystems") AS inss
	ON CONFLICT ("systemID") DO UPDATE
	SET
		"isColonised" = EXCLUDED."isColonised"
	WHERE "StarSystems"."isColonised" = FALSE
	AND EXCLUDED."isColonised" = TRUE;
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
		"ringCount"
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
		"ringCount"
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
		"ringCount" = EXCLUDED."ringCount"
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
		"ColonyOverrideCounts"."ringCount"
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
		EXCLUDED."ringCount"
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

CREATE OR REPLACE FUNCTION "ClaimStarSystem"("inputSystemID" BIGINT, "inputClaimDate" TIMESTAMPTZ)
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
	"inputMaxDistanceToRegionCentre" INT,
	"inputHotspotTypes" "HotspotType"[],
	"inputRemovedSystemIDs" BIGINT[]
)
RETURNS jsonb AS $$
	WITH "TopResults" AS (
		SELECT 
			duss."uncolonisedSystemID",
			ss."systemName",
			ss."systemCoords",
			uss."lastUpdated",
			uss."reserveLevel",
			uss."landableCount",
			uss."walkableCount",
			ST_3DDistance(ss."systemCoords", reg."regionCentreCoords")::INT AS "distanceToRegionCentre",
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
			coc."ringCount"
		FROM "DistinctUncolonisedStarSystems" duss
		INNER JOIN "StarSystems" ss ON duss."uncolonisedSystemID" = ss."systemID"
		INNER JOIN "Regions" reg ON reg."regionName" = "inputRegionName"
			AND ST_3DIntersects(ss."systemCoords", "GetRegionCube"(reg."regionCentreCoords", reg."regionRange"))
		INNER JOIN "UncolonisedStarSystems" uss ON duss."uncolonisedSystemID" = uss."systemID"
		INNER JOIN "ColonyOverrideCounts" coc ON duss."uncolonisedSystemID" = coc."systemID"
		INNER JOIN "UncolonisedStarSystemsAvailability" ussa ON duss."uncolonisedSystemID" = ussa."systemID"
		WHERE ussa."isLocked" = FALSE
			AND ussa."isClaimed" = FALSE
			AND reg."regionName" = "inputRegionName"
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
			AND uss."landableCount" BETWEEN "inputMinLandables" AND "inputMaxLandables"
			AND uss."walkableCount" BETWEEN "inputMinWalkables" AND "inputMaxWalkables"
			AND ST_3DDistance(ss."systemCoords", reg."regionCentreCoords") <= "inputMaxDistanceToRegionCentre"
			AND ("inputRemovedSystemIDs" IS NULL OR NOT (duss."uncolonisedSystemID" = ANY("inputRemovedSystemIDs")))
			AND (
				("inputSystemName" IS NULL AND "inputFactionName" IS NULL)
				OR EXISTS (
					SELECT 1
					FROM "ColonisableStarSystems" css
					INNER JOIN "DistinctColonisedStarSystems" dcss ON css."colonisedSystemID" = dcss."colonisedSystemID"
					INNER JOIN "Stations" s ON dcss."colonisedSystemID" = s."systemID"
					INNER JOIN "Factions" f ON s."controllingFaction" = f."factionID"
					WHERE css."uncolonisedSystemID" = duss."uncolonisedSystemID"
						AND ("inputSystemName" IS NULL OR dcss."systemName" = "inputSystemName")
						AND ("inputFactionName" IS NULL OR f."factionName" = "inputFactionName")
				)
			)
			AND (
				("inputHotspotTypes" IS NULL OR CARDINALITY("inputHotspotTypes") = 0)
				OR EXISTS (
					SELECT 1
					FROM "Rings" r
					INNER JOIN "Hotspots" h ON r."ringID" = h."ringID"
					WHERE r."systemID" = duss."uncolonisedSystemID"
						AND h."hotspotType" = ANY("inputHotspotTypes")
				)
			)
		ORDER BY
			CASE WHEN "sortOrder" = 'SystemValue' THEN uss."systemValue" END DESC,
			CASE WHEN "sortOrder" = 'MostWalkables' THEN uss."walkableCount" END DESC,
			CASE WHEN "sortOrder" = 'DistanceToRegionCentre' THEN ST_3DDistance(ss."systemCoords", reg."regionCentreCoords")::INT END ASC,
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
							'stations', (
								SELECT jsonb_agg(
									jsonb_build_object(
										'stationID', s."stationID",
										'stationName', s."stationName",
										'controllingFaction', f."factionName"
									)
								)
								FROM "Stations" s
								INNER JOIN "Factions" f ON s."controllingFaction" = f."factionID"
								WHERE s."systemID" = css."colonisedSystemID"
							)
						)
					)
					FROM "ColonisableStarSystems" css
					INNER JOIN "StarSystems" ss ON css."colonisedSystemID" = ss."systemID"
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
	) tr;
$$ LANGUAGE sql;

CREATE OR REPLACE FUNCTION "SelectFactionNamesJson"("name" VARCHAR(75))
RETURNS jsonb AS $$
	SELECT jsonb_agg(
		jsonb_build_object(
			'name', "factionName"
		)
	)
	FROM (
		SELECT "factionName"
		FROM "Factions"
		WHERE "factionName" ILIKE '%' || "name" || '%'
		LIMIT 20
	) as "factionNames";
$$ LANGUAGE sql;

CREATE OR REPLACE FUNCTION "SelectColonisedSystemNamesJson"("name" VARCHAR(75))
RETURNS jsonb AS $$
	SELECT jsonb_agg(
		jsonb_build_object(
			'name', "systemName"
		)
	)
	FROM (
		SELECT "systemName"
		FROM "StarSystems"
		WHERE "isColonised" = TRUE
		AND "systemName" ILIKE '%' || "name" || '%'
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

CREATE OR REPLACE FUNCTION "ReportStarSystem"("inputSystemID" BIGINT, "isLockReport" BOOLEAN)
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

COMMIT TRANSACTION;