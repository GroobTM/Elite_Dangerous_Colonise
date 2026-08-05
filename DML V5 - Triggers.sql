-- Run fifth after
BEGIN TRANSACTION;

CREATE OR REPLACE FUNCTION "TriggerAddNewSystemsToAvailabilityOnInsert"()
RETURNS TRIGGER AS $$
BEGIN
	INSERT INTO "UncolonisedStarSystemsAvailability" ("systemID")
	SELECT "systemID"
	FROM "NewlyInserted"
	ON CONFLICT("systemID") DO NOTHING;
	
	RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER "TriggerAddNewSystemsToAvailabilityOnInsert"
AFTER INSERT ON "UncolonisedStarSystems"
REFERENCING NEW TABLE AS "NewlyInserted"
FOR EACH STATEMENT
EXECUTE FUNCTION "TriggerAddNewSystemsToAvailabilityOnInsert"();

CREATE OR REPLACE FUNCTION "TriggerRemoveUncolonisedSystemAndStageColonisedOnUpdate"()
RETURNS TRIGGER AS $$
BEGIN
	DELETE FROM "UncolonisedStarSystems" WHERE "systemID" = NEW."systemID";
	
	INSERT INTO "StagedStarSystems" ("systemID")
	VALUES (NEW."systemID")
	ON CONFLICT("systemID") DO NOTHING;
	
	RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER "TriggerRemoveUncolonisedSystemAndStageColonisedOnUpdate"
AFTER UPDATE OF "isColonised" ON "StarSystems"
FOR EACH ROW
WHEN (OLD."isColonised" = FALSE AND NEW."isColonised" = TRUE)
EXECUTE FUNCTION "TriggerRemoveUncolonisedSystemAndStageColonisedOnUpdate"();

CREATE OR REPLACE FUNCTION "TriggerAddNewSystemToStaging"()
RETURNS TRIGGER AS $$
BEGIN
	INSERT INTO "StagedStarSystems" ("systemID")
	SELECT "systemID"
	FROM "NewlyInserted"
	ON CONFLICT("systemID") DO NOTHING;
	
	RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER "TriggerAddNewSystemToStaging"
AFTER INSERT ON "StarSystems"
REFERENCING NEW TABLE AS "NewlyInserted"
FOR EACH STATEMENT
EXECUTE FUNCTION "TriggerAddNewSystemToStaging"();

CREATE OR REPLACE FUNCTION "TriggerUpdateAvailabilityLock"()
RETURNS TRIGGER AS $$
BEGIN
	IF NEW."isLocked" = FALSE AND NEW."lockReportCount" >= 10 THEN
		NEW."isLocked" := TRUE;
		NEW."lockReportDate" := NOW();
	END IF;
	
	RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER "TriggerUpdateAvailabilityLock"
BEFORE UPDATE OF "lockReportCount" ON "UncolonisedStarSystemsAvailability"
FOR EACH ROW
WHEN (OLD."lockReportCount" IS DISTINCT FROM NEW."lockReportCount")
EXECUTE FUNCTION "TriggerUpdateAvailabilityLock"();

CREATE OR REPLACE FUNCTION "TriggerUpdateAvailabilityClaim"()
RETURNS TRIGGER AS $$
BEGIN
	IF NEW."isClaimed" = FALSE AND NEW."claimReportCount" >= 10 THEN
		NEW."isClaimed" := TRUE;
		NEW."claimReportDate" := NOW();
	END IF;
	
	RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER "TriggerUpdateAvailabilityClaim"
BEFORE UPDATE OF "claimReportCount" ON "UncolonisedStarSystemsAvailability"
FOR EACH ROW
WHEN (OLD."claimReportCount" IS DISTINCT FROM NEW."claimReportCount")
EXECUTE FUNCTION "TriggerUpdateAvailabilityClaim"();

COMMIT TRANSACTION;