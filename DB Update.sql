DROP FUNCTION "SelectRegion";
CREATE OR REPLACE VIEW "SelectRegionsView" AS
SELECT 
	"regionName",
	"regionRange",
	ST_X("regionCentreCoords") AS "centreCoordinateX",
	ST_Y("regionCentreCoords") AS "centreCoordinateY",
	ST_Z("regionCentreCoords") AS "centreCoordinateZ",
	"regionColour",
	"regionActive"
FROM "Regions";