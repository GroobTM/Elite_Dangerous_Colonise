var formData;
var resultsPerPage = 10;
var currentPage = 1;
var maxPages = 1;

// Countdown Events
var conn = new signalR.HubConnectionBuilder()
    .withUrl("/updateHub")
    .build();

conn.start().catch(err => console.error(err.toString()));

conn.on("SystemUpdateStarted", function () {
    document.getElementById("update_countdown").innerText = "Update in Progress";
});

conn.on("SearchBlockEnabled", function () {
    ToggleSearch(true);
    alert("The update is being cleaned up. Searching is disabled for a few minutes.")
});

conn.on("SystemUpdateComplete", function () {
    ToggleSearch(false);
    StartCountdown();
});

conn.on("SystemUpdateStatus", function (updateStatus, searchStatus) {
    if (updateStatus === "inProgress") {
        document.getElementById("update_countdown").innerText = "Update in Progress";

        if (searchStatus === "blocked") {
            ToggleSearch(true);
            alert("The update is being cleaned up. Searching is disabled for a few minutes.")
        }
    }
    else {
        StartCountdown();
    }
});

function StartCountdown() {
    if (interval) {
        clearInterval(interval);
    }

    var now = new Date();
    var target = new Date();

    target.setUTCHours(5, 0, 0, 0);

    if (now >= target) {
        target.setUTCDate(target.getUTCDate() + 1);
    }

    var interval = setInterval(function () {
        var diff = target - new Date();
        if (diff <= 0) {
            clearInterval(interval);
            document.getElementById("update_countdown").innerText = "Update in Progress";
        }
        else {
            var hours = Math.floor(diff / (1000 * 60 * 60));
            var minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
            var seconds = Math.floor((diff % (1000 * 60)) / 1000);
            document.getElementById("update_countdown").innerText = `Time Until Next Update: ${hours}h ${minutes}m ${seconds}s`;
        }
    }, 1000);
}

$(function () {
    StarSession();
});

async function StarSession() {
    var storedIDs = localStorage.getItem("ReportedIDs");
    var storedIDsJson = [];

    if (storedIDs) {
        storedIDsJson = JSON.parse(storedIDs);
    }

    const response = await fetch("/api/ConfigSession", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(storedIDsJson)
    });

    if (!response.ok) {
        console.error("Error configuring session.")
    }
}


// Search Bar Functions

function SetupSearchInput(id, api) {
    $("#" + id).each(function () {
        new HSComboBox(this, {
            apiUrl: api,
            apiSearchQuery: "query",
            outputItemTemplate: `
                <div class="w-full cursor-pointer px-4 py-2 text-[#0F0F0F] hover:bg-[#E1E1E1]" data-hs-combo-box-output-item>
                    <div class="flex justify-between items-center w-full">
                        <div>
                            <div data-hs-combo-box-output-item-field="name" data-hs-combo-box-search-text data-hs-combo-box-value></div>
                        </div>
                    </div>
                </div>
            `
        });
    });

    const comboBoxInput = $("#" + id + " [data-hs-combo-box-input]");

    comboBoxInput.on("input", function () {
        if ($(this).val() === "") {
            HSComboBox.getInstance("#" + id).setValue(null);
        }
    });
}

function SetupMaxDistanceFromSolSlider() {
    const distanceFromSolSlider = document.querySelector("#distance_from_sol_slider");
    const distanceFromSolValue = document.querySelector("#distance_from_sol_value");
    const distanceFromSolSliderInstance = new HSRangeSlider(distanceFromSolSlider);

    distanceFromSolSlider.noUiSlider.on("update", function (values) {
        distanceFromSolValue.textContent = Math.trunc(values[0]) + " ly";
    });
}

function SetupGenericSlider(sliderID, valueID) {
    const slider = document.querySelector("#" + sliderID);
    const value = document.querySelector("#" + valueID);
    const sliderInstance = new HSRangeSlider(slider);

    slider.noUiSlider.on("update", function (values) {
        value.textContent = Math.trunc(values[0]) + " - " + Math.trunc(values[1]);
    });
}

$(window).on("load", function () {
    window.HSStaticMethods.autoInit();

    $("#hotspot_select").each(function () {
        new HSSelect(this, {
            placeholder: "Select (Optional)",
            dropdownClasses: "!mt-0 z-50 w-full max-h-55 p-1 space-y-0.5 bg-white border-1 border-[#BCBCBC] rounded-lg overflow-hidden overflow-y-auto shadow-lg",
            optionClasses: "py-2 px-4 w-full text-[#0F0F0F] cursor-pointer hover:bg-[#E1E1E1] rounded-lg focus:outline-hidden focus:bg-gray-100 hs-select-disabled:pointer-events-none hs-select-disabled:opacity-50",
            mode: "tags",
            wrapperClasses: "relative ps-0.5 pe-9 min-h-20 flex items-center flex-wrap text-nowrap w-full border border-[#0F0F0F] rounded-lg text-start focus:border-[#F07B05] focus:ring-[#F07B05] bg-white shadow-sm mt-2",
            tagsItemTemplate: `
            <div class="flex flex-nowrap items-center relative z-10 bg-[#F0F0F0] border border-[#0F0F0F] rounded-full p-3 m-1">
                <div class="whitespace-nowrap text-[#0F0F0F]" data-title></div>
                <div class="inline-flex shrink-0 justify-center items-center size-5 ms-2 rounded-full text-[#0F0F0F] text-2xl -translate-y-0.5 hover:text-[#F07B05] cursor-pointer" data-remove>&times;</div>
            </div>`,
            tagsInputId: "hs-tags-input",
            tagsInputClasses: "py-2.5 px-2 min-w-20 rounded-lg order-1 border-transparent focus:ring-0 outline-hidden",
            optionTemplate: `
            <div class="flex items-center text-[#0F0F0F]">
                <div>
                    <div data-title></div>
                </div>
                <div class="ms-auto">
                    <span class="hidden hs-selected:block">
                        <svg class="shrink-0 size-4" xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                            <path d="M12.736 3.97a.733.733 0 0 1 1.047 0c.286.289.29.756.01 1.05L7.88 12.01a.733.733 0 0 1-1.065.02L3.217 8.384a.757.757 0 0 1 0-1.06.733.733 0 0 1 1.047 0l3.052 3.093 5.4-6.425a.247.247 0 0 1 .02-.022Z"/>
                        </svg>
                    </span>
                </div>
            </div>`
        });
    });

    SetupSearchInput("colonised_system_search", "/api/ColonisedSystemNames");
    SetupSearchInput("faction_search", "/api/FactionNames");
    SetupMaxDistanceFromSolSlider();
    SetupGenericSlider("landable_bodies_slider", "landable_bodies_value");
    SetupGenericSlider("walkable_bodies_slider", "walkable_bodies_value");
    SetupGenericSlider("black_holes_slider", "black_holes_value");
    SetupGenericSlider("neutron_stars_slider", "neutron_stars_value");
    SetupGenericSlider("white_dwarves_slider", "white_dwarves_value");
    SetupGenericSlider("other_stars_slider", "other_stars_value");
    SetupGenericSlider("earth_likes_slider", "earth_likes_value");
    SetupGenericSlider("water_worlds_slider", "water_worlds_value");
    SetupGenericSlider("ammonia_worlds_slider", "ammonia_worlds_value");
    SetupGenericSlider("gas_giants_slider", "gas_giants_value");
    SetupGenericSlider("high_metal_content_slider", "high_metal_content_value");
    SetupGenericSlider("metal_rich_slider", "metal_rich_value");
    SetupGenericSlider("rocky_ice_world_slider", "rocky_ice_world_value");
    SetupGenericSlider("rocky_bodies_slider", "rocky_bodies_value");
    SetupGenericSlider("icy_bodies_slider", "icy_bodies_value");
    SetupGenericSlider("rings_slider", "rings_value");
    SetupGenericSlider("geologicals_slider", "geologicals_value");
    SetupGenericSlider("organics_slider", "organics_value");
});

$("#more_options_button").on("click", function () {
    $("#more_options_icon").toggleClass("rotate-180");
    $("#more_options").toggleClass("hidden grid");
});

$(document).on("click", "#clear_button", function () {
    $("#" + $(this).attr("for")).val("");
});

$("#systems_search").on("submit", function (e) {
    e.preventDefault();
    currentPage = 1;

    SetFormData();
    LoadResults();
});

function UpdateFormFromParams(searchParams) {
    $("#colonised_system").val(searchParams.systemName);
    $("#sort_order").val(searchParams.sortOrder);
    $("#faction").val(searchParams.factionName);

    document.querySelector("#distance_from_sol_slider").noUiSlider.set(searchParams.maxDistanceToSol);
    document.querySelector("#landable_bodies_slider").noUiSlider.set([searchParams.minLandables, searchParams.maxLandables]);
    document.querySelector("#walkable_bodies_slider").noUiSlider.set([searchParams.minWalkables, searchParams.maxWalkables]);
    document.querySelector("#black_holes_slider").noUiSlider.set([searchParams.minBlackHoles, searchParams.maxBlackHoles]);
    document.querySelector("#neutron_stars_slider").noUiSlider.set([searchParams.minNeutronStars, searchParams.maxNeutronStars]);
    document.querySelector("#white_dwarves_slider").noUiSlider.set([searchParams.minWhiteDwarves, searchParams.maxWhiteDwarves]);
    document.querySelector("#other_stars_slider").noUiSlider.set([searchParams.minOtherStars, searchParams.maxOtherStars]);
    document.querySelector("#earth_likes_slider").noUiSlider.set([searchParams.minEarthLikes, searchParams.maxEarthLikes]);
    document.querySelector("#water_worlds_slider").noUiSlider.set([searchParams.minWaterWorlds, searchParams.maxWaterWorlds]);
    document.querySelector("#ammonia_worlds_slider").noUiSlider.set([searchParams.minAmmoniaWorlds, searchParams.maxAmmoniaWorlds]);
    document.querySelector("#gas_giants_slider").noUiSlider.set([searchParams.minGasGiants, searchParams.maxGasGiants]);
    document.querySelector("#high_metal_content_slider").noUiSlider.set([searchParams.minHighMetalContents, searchParams.maxHighMetalContents]);
    document.querySelector("#metal_rich_slider").noUiSlider.set([searchParams.minMetalRiches, searchParams.maxMetalRiches]);
    document.querySelector("#rocky_ice_world_slider").noUiSlider.set([searchParams.minRockyIces, searchParams.maxRockyIces]);
    document.querySelector("#rocky_bodies_slider").noUiSlider.set([searchParams.minRocks, searchParams.maxRocks]);
    document.querySelector("#icy_bodies_slider").noUiSlider.set([searchParams.minIces, searchParams.maxIces]);
    document.querySelector("#rings_slider").noUiSlider.set([searchParams.minRings, searchParams.maxRings]);
    document.querySelector("#geologicals_slider").noUiSlider.set([searchParams.minGeologicals, searchParams.maxGeologicals]);
    document.querySelector("#organics_slider").noUiSlider.set([searchParams.minOrganics, searchParams.maxOrganics]);

    const hotspotSelect = document.querySelector("#hotspot_select");

    const hotspotValues = searchParams.hotspotTypes ? searchParams.hotspotTypes.split(",") : [];

    Array.from(hotspotSelect.options).forEach(option => {
        option.selected = hotspotValues.includes(option.value);
    });

    window.HSStaticMethods.autoInit(['select']);
}

window.addEventListener("popstate", (event) => {
    if (event.state && event.state.encodedParams) {
        const searchParams = new URLSearchParams(atob(event.state.encodedParams));

        UpdateFormFromParams(Object.fromEntries(searchParams));

        currentPage = parseInt(searchParams.get("pageNo")) || 1;

        SetFormData();

        LoadResults(false);
    }
});

$(window).on("load", function () {
    const encodedParams = new URLSearchParams(window.location.search).get("q");

    if (encodedParams) {
        try {
            const initialParams = new URLSearchParams(atob(encodedParams));

            if (initialParams.has("sortOrder") && initialParams.has("pageNo") && initialParams.has("resultsPerPage") && initialParams.has("systemName")
                && initialParams.has("factionName") && initialParams.has("minBlackHoles") && initialParams.has("maxBlackHoles") && initialParams.has("minNeutronStars")
                && initialParams.has("maxNeutronStars") && initialParams.has("minWhiteDwarves") && initialParams.has("maxWhiteDwarves")
                && initialParams.has("minOtherStars") && initialParams.has("maxOtherStars") && initialParams.has("minEarthLikes") && initialParams.has("maxEarthLikes")
                && initialParams.has("minWaterWorlds") && initialParams.has("maxWaterWorlds") && initialParams.has("minAmmoniaWorlds")
                && initialParams.has("maxAmmoniaWorlds") && initialParams.has("minGasGiants") && initialParams.has("maxGasGiants") && initialParams.has("minHighMetalContents")
                && initialParams.has("maxHighMetalContents") && initialParams.has("minMetalRiches") && initialParams.has("maxMetalRiches")
                && initialParams.has("minRockyIces") && initialParams.has("maxRockyIces") && initialParams.has("minRocks") && initialParams.has("maxRocks")
                && initialParams.has("minIces") && initialParams.has("maxIces") && initialParams.has("minOrganics") && initialParams.has("maxOrganics")
                && initialParams.has("minGeologicals") && initialParams.has("maxGeologicals") && initialParams.has("minRings") && initialParams.has("maxRings")
                && initialParams.has("minLandables") && initialParams.has("maxLandables") && initialParams.has("minWalkables") && initialParams.has("maxWalkables")
                && initialParams.has("maxDistanceToSol") && initialParams.has("hotspotTypes")) {

                UpdateFormFromParams(Object.fromEntries(initialParams));

                currentPage = parseInt(initialParams.get("pageNo")) || 1;

                SetFormData();

                LoadResults(false);
            }
        } catch (e) {
            console.error("Failed to decode URL parameters:", e);
        }
    }
});

// Search Results Functions
function CopyToClipboard(obj) {
    var text = obj.innerText;

    navigator.clipboard.writeText(text);
}

$("#pagination_next").on("click", function (e) {
    e.preventDefault();
    currentPage++;
    LoadResults(true);
});

$("#pagination_previous").on("click", function (e) {
    e.preventDefault();
    currentPage--;
    LoadResults(true);
});

$("#results_per_page").change(function () {
    currentPage = 1;
    resultsPerPage = parseInt($(this).val(), 10);

    if (isNaN(resultsPerPage)) {
        resultsPerPage = 10;
    }

    LoadResults();
});

$(".pagination_numbers").on("click", function (e) {
    e.preventDefault();
    currentPage = parseInt($(this).text());
    LoadResults(true);
});

$("#results_per_page").prop("disabled", true);
$("#results_per_page_lbl").addClass("opacity-50", true);

function ToggleSelectPage(isSelecting) {
    $("#select_page_btn").toggleClass("hidden", isSelecting);
    $("#select_page_form").toggleClass("hidden", !isSelecting);
}

function ToggleSearch(isSearching) {
    $("#search")
        .prop("disabled", isSearching)
        .toggleClass("cursor-progress", isSearching)
        .toggleClass("cursor-pointer", !isSearching);
}

function AddSliderDataToForm(sliderID, sliderName) {
    const sliderValues = document.querySelector("#" + sliderID).noUiSlider.get();
    formData.append("Min" + sliderName, sliderValues[0]);
    formData.append("Max" + sliderName, sliderValues[1]);
}

function SetFormData() {
    formData = new FormData(document.getElementById("systems_search"));

    const distanceFromSol = document.querySelector("#distance_from_sol_slider").noUiSlider.get();
    formData.append("MaxDistanceFromSol", distanceFromSol);

    AddSliderDataToForm("landable_bodies_slider", "Landables");
    AddSliderDataToForm("walkable_bodies_slider", "Walkables");
    AddSliderDataToForm("black_holes_slider", "BlackHoles");
    AddSliderDataToForm("neutron_stars_slider", "NeutronStars");
    AddSliderDataToForm("white_dwarves_slider", "WhiteDwarves");
    AddSliderDataToForm("other_stars_slider", "OtherStars");
    AddSliderDataToForm("earth_likes_slider", "EarthLikes");
    AddSliderDataToForm("water_worlds_slider", "WaterWorlds");
    AddSliderDataToForm("ammonia_worlds_slider", "AmmoniaWorlds");
    AddSliderDataToForm("gas_giants_slider", "GasGiants");
    AddSliderDataToForm("high_metal_content_slider", "HighMetalContents");
    AddSliderDataToForm("metal_rich_slider", "MetalRiches");
    AddSliderDataToForm("rocky_ice_world_slider", "RockyIces");
    AddSliderDataToForm("rocky_bodies_slider", "Rocks");
    AddSliderDataToForm("icy_bodies_slider", "Icys");
    AddSliderDataToForm("rings_slider", "Rings");
    AddSliderDataToForm("geologicals_slider", "Geologicals");
    AddSliderDataToForm("organics_slider", "Organics");

    const hotspotTypes = document.querySelector("#hotspot_select");
    formData.append(
        "HotspotTypes",
        Array.from(hotspotTypes.selectedOptions).map(option => option.value)
    );
}

async function LoadResults(updateUrl = true) {
    ToggleSearch(true);        
    $("#loading").addClass("flex").removeClass("hidden");

    const searchParams = new URLSearchParams({
        sortOrder: formData.get("SortOrder"),
        pageNo: currentPage,
        resultsPerPage: resultsPerPage,
        systemName: formData.get("ColonisedSystem"),
        factionName: formData.get("Faction"),
        minBlackHoles: parseInt(formData.get("MinBlackHoles")),
        maxBlackHoles: parseInt(formData.get("MaxBlackHoles")),
        minNeutronStars: parseInt(formData.get("MinNeutronStars")),
        maxNeutronStars: parseInt(formData.get("MaxNeutronStars")),
        minWhiteDwarves: parseInt(formData.get("MinWhiteDwarves")),
        maxWhiteDwarves: parseInt(formData.get("MaxWhiteDwarves")),
        minOtherStars: parseInt(formData.get("MinOtherStars")),
        maxOtherStars: parseInt(formData.get("MaxOtherStars")),
        minEarthLikes: parseInt(formData.get("MinEarthLikes")),
        maxEarthLikes: parseInt(formData.get("MaxEarthLikes")),
        minWaterWorlds: parseInt(formData.get("MinWaterWorlds")),
        maxWaterWorlds: parseInt(formData.get("MaxWaterWorlds")),
        minAmmoniaWorlds: parseInt(formData.get("MinAmmoniaWorlds")),
        maxAmmoniaWorlds: parseInt(formData.get("MaxAmmoniaWorlds")),
        minGasGiants: parseInt(formData.get("MinGasGiants")),
        maxGasGiants: parseInt(formData.get("MaxGasGiants")),
        minHighMetalContents: parseInt(formData.get("MinHighMetalContents")),
        maxHighMetalContents: parseInt(formData.get("MaxHighMetalContents")),
        minMetalRiches: parseInt(formData.get("MinMetalRiches")),
        maxMetalRiches: parseInt(formData.get("MaxMetalRiches")),
        minRockyIces: parseInt(formData.get("MinRockyIces")),
        maxRockyIces: parseInt(formData.get("MaxRockyIces")),
        minRocks: parseInt(formData.get("MinRocks")),
        maxRocks: parseInt(formData.get("MaxRocks")),
        minIces: parseInt(formData.get("MinIcys")),
        maxIces: parseInt(formData.get("MaxIcys")),
        minOrganics: parseInt(formData.get("MinOrganics")),
        maxOrganics: parseInt(formData.get("MaxOrganics")),
        minGeologicals: parseInt(formData.get("MinGeologicals")),
        maxGeologicals: parseInt(formData.get("MaxGeologicals")),
        minRings: parseInt(formData.get("MinRings")),
        maxRings: parseInt(formData.get("MaxRings")),
        minLandables: parseInt(formData.get("MinLandables")),
        maxLandables: parseInt(formData.get("MaxLandables")),
        minWalkables: parseInt(formData.get("MinWalkables")),
        maxWalkables: parseInt(formData.get("MaxWalkables")),
        maxDistanceToSol: parseInt(formData.get("MaxDistanceFromSol")),
        hotspotTypes: formData.get("HotspotTypes")
    });

    if (updateUrl) {
        const encodedParams = btoa(searchParams.toString());
        const newUrl = `${window.location.pathname}?q=${encodedParams}`;

        history.pushState({ encodedParams: encodedParams }, "", newUrl);
    }

    const response = await fetch(`/api/StarSystemSearch?${searchParams}`);

    if (!response.ok) {
        console.error("Error loading search results.")
        return;
    }

    const results = await response.json();

    if (results?.results?.length > 0) {
        FormatResults(results.results);
    }
    else {
        FormatNoResults();
    }

    FormatPagination(results.minFollwingPages);

    $("#loading").addClass("hidden").removeClass("flex");
    ToggleSearch(false);
}

function FormatResults(results) {
    const resultsDiv = document.getElementById("results");
    var count = 0;
    resultsDiv.innerHTML = ``;

    results.forEach(system => {
        resultsDiv.innerHTML += `
            <div id="results-heading-${system.systemID}" class="grid">
                <button type="button" class="${FormatTopBoarder(count)} flex w-full cursor-pointer items-center justify-between gap-3 border border-[#0F0F0F] bg-white p-5 text-[#0F0F0F] shadow-sm hover:bg-[#ff9305] rtl:text-right" data-accordion-target="#results-body-${system.systemID}" aria-expanded="false" aria-controls="results-body-${system.systemID}">
                    <span class="flex items-center text-[#0F0F0F]">${system.systemName}</span>
                    <svg data-accordion-icon class="h-3 w-3 shrink-0 rotate-180 transition-transform duration-200" aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 10 6">
                        <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5 5 1 1 5"/>
                    </svg>
                </button>
            </div>
            
            <div id="results-body-${system.systemID}" class="hidden" aria-labelledby="results-heading-${system.systemID}">
                <div class="grid-rows-auto grid grid-cols-1 gap-y-5 border border-t-0 border-[#0F0F0F] bg-white p-5 shadow-sm lg:grid-cols-2 lg:gap-x-5 xl:grid-cols-3 xl:gap-y-0">
                    <h2 class="row-start-1 border-b border-gray-300 pb-3 text-center text-lg drop-shadow-xs lg:col-start-1 lg:text-left">Details</h2>
                    <div class="row-start-2 mt-3 lg:col-start-1">
                        <div class="grid grid-cols-2 items-end gap-x-2">
                            <h3 class="mt-3 font-bold">Name:</h3>
                            <p class="mt-3"><span data-tooltip-target="${system.systemID}_tooltip" class="cursor-copy" onclick="CopyToClipboard(this);">${system.systemName}</span></p>
                            <h3 class="mt-3 font-bold">Coordinates:</h3>
                            <p class="mt-3">${FormatCoordinates(system.coordinates)}</p>
                            <h3 class="mt-3 font-bold">Last Updated:</h3>
                            <p class="mt-3">${FormatDate(system.lastUpdate)}</p>
                            <h3 class="mt-3 font-bold">Distance to Sol:</h3>
                            <p class="mt-3">${system.distanceToSol} ly</p>
                            <h3 class="mt-3 font-bold">System Reserve:</h3>
                            <p class="mt-3">${system.reserveLevel}</p>
                            <h3 class="mt-3 font-bold">Landable Bodies:</h3>
                            <p class="mt-3">${system.landableCount}</p>
                            <h3 class="mt-3 font-bold">Walkable Bodies:</h3>
                            <p class="mt-3">${system.walkableCount}</p>
                        </div>
                    </div>
                    <h2 class="row-start-3 border-b border-gray-300 pt-5 pb-3 text-center text-lg drop-shadow-xs lg:col-start-2 lg:row-start-1 lg:pt-0 lg:text-left">Rings</h2>
                    <div class="row-start-4 mt-3 max-h-100 overflow-auto lg:col-start-2 lg:row-start-2">
                        <ul class="list-inside list-disc font-bold">
                            ${FormatRings(system.rings)}
                        </ul>
                    </div>
                    <h2 class="row-start-5 border-b border-gray-300 pt-5 pb-3 text-center text-lg drop-shadow-xs lg:col-start-1 lg:col-end-3 lg:row-start-3 lg:text-left xl:col-start-3 xl:col-end-5 xl:row-start-1 xl:pt-0">Nearby Stations</h2>
                    <div class="row-start-6 mt-3 max-h-100 overflow-auto lg:col-start-1 lg:col-end-3 lg:row-start-4 xl:col-start-3 xl:col-end-5 xl:row-start-2">
                        <ul class="list-inside list-disc">
                            ${FormatColonisedSystems(system.systemID, system.colonisedSystems)}
                        </ul>
                    </div>
                    <h2 class="col-start-1 row-start-7 border-b border-gray-300 pt-5 pb-3 text-center text-lg drop-shadow-xs lg:col-end-3 lg:row-start-5 lg:text-left xl:col-end-4 xl:row-start-3">Colony Influences</h2>
                    <div class="col-start-1 row-start-8 mt-3 grid grid-cols-2 gap-x-2 overflow-auto lg:col-end-3 lg:row-start-6 lg:grid-cols-4 xl:col-end-4 xl:row-start-4 2xl:grid-cols-8">
                        <div class="mt-3 flex gap-3">
                            ${FormatInfluenceIcons(system.systemID, "#00FFFF", "#6600E5", "#00FFFF", "#6600E5")}
                            <h3 class="font-bold">Black Holes:</h3>
                            <div id="${system.systemID}_black_holes_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                                High Tech and Tourism
                            </div>
                        </div>
                        <p class="mt-3">${system.systemCounts.blackHoleCount}</p>
                        <div class="mt-3 flex gap-3">
                            ${FormatInfluenceIcons(system.systemID, "#00FFFF", "#6600E5", "#00FFFF", "#6600E5")}
                            <h3 class="font-bold">Neutron Stars:</h3>
                            <div id="${system.systemID}_neutron_stars_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                                High Tech and Tourism
                            </div>
                        </div>
                        <p class="mt-3">${system.systemCounts.neutronStarCount}</p>
                        <div class="mt-3 flex gap-3">
                            ${FormatInfluenceIcons(system.systemID, "#00FFFF", "#6600E5", "#00FFFF", "#6600E5")}
                            <h3 class="font-bold">White Dwarves:</h3>
                            <div id="${system.systemID}_white_dwarves_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                                High Tech and Tourism
                            </div>
                        </div>
                        <p class="mt-3">${system.systemCounts.whiteDwarves}</p>
                        <div class="mt-3 flex gap-3">
                            ${FormatInfluenceIcons(system.systemID, "#E500E5", "#E500E5", "#E500E5", "#E500E5")}
                            <h3 class="font-bold">Other Stars:</h3>
                            <div id="${system.systemID}_other_stars_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                                Military
                            </div>
                        </div>
                        <p class="mt-3">${system.systemCounts.otherStarCount}</p>
                        <div class="mt-3 flex gap-3">
                            ${FormatInfluenceIcons(system.systemID, "#E500E5", "#6600E5", "#80FF00", "#00FFFF")}
                            <h3 class="font-bold">Earth-like Worlds:</h3>
                            <div id="${system.systemID}_earth_likes_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                                Argriculture, High Tech, Tourism, and Military
                            </div>
                        </div>
                        <p class="mt-3">${system.systemCounts.earthLikeCount}</p>
                        <div class="mt-3 flex gap-3">
                            ${FormatInfluenceIcons(system.systemID, "#80FF00", "#6600E5", "#80FF00", "#6600E5")}
                            <h3 class="font-bold">Water Worlds:</h3>
                            <div id="${system.systemID}_water_worlds_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                                Argriculture and Tourism
                            </div>
                        </div>
                        <p class="mt-3">${system.systemCounts.waterWorldCount}</p>
                        <div class="mt-3 flex gap-3">
                            ${FormatInfluenceIcons(system.systemID, "#00FFFF", "#6600E5", "#00FFFF", "#6600E5")}
                            <h3 class="font-bold">Ammonia Worlds:</h3>
                            <div id="${system.systemID}_ammonia_worlds_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                                High Tech and Tourism
                            </div>
                        </div>
                        <p class="mt-3">${system.systemCounts.ammoniaWorldCount}</p>
                        <div class="mt-3 flex gap-3">
                            ${FormatInfluenceIcons(system.systemID, "#00FFFF", "#FFFF00", "#00FFFF", "#FFFF00")}
                            <h3 class="font-bold">Gas Giants:</h3>
                            <div id="${system.systemID}_gas_giants_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                                High Tech and Industrial
                            </div>
                        </div>
                        <p class="mt-3">${system.systemCounts.gasGiantCount}</p>
                        <div class="mt-3 flex gap-3">
                            ${FormatInfluenceIcons(system.systemID, "#FF0000", "#FF0000", "#FF0000", "#FF0000")}
                            <h3 class="font-bold">HMC Worlds:</h3>
                            <div id="${system.systemID}_high_metal_content_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                                Extraction
                            </div>
                        </div>
                        <p class="mt-3">${system.systemCounts.highMetalContentCount}</p>
                        <div class="mt-3 flex gap-3">
                            ${FormatInfluenceIcons(system.systemID, "#FF0000", "#FF0000", "#FF0000", "#FF0000")}
                            <h3 class="font-bold">Metal Rich Bodies:</h3>
                            <div id="${system.systemID}_metal_rich_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                                Extraction
                            </div>
                        </div>
                        <p class="mt-3">${system.systemCounts.metalRichCount}</p>
                        <div class="mt-3 flex gap-3">
                            ${FormatInfluenceIcons(system.systemID, "#FF8000", "#FFFF00", "#FF8000", "#FFFF00")}
                            <h3 class="font-bold">Rocky Ice Worlds:</h3>
                            <div id="${system.systemID}_rocky_ice_world_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                                Refinery and Industrial
                            </div>
                        </div>
                        <p class="mt-3">${system.systemCounts.rockyIceBodyCount}</p>
                        <div class="mt-3 flex gap-3">
                            ${FormatInfluenceIcons(system.systemID, "#FF8000", "#FF8000", "#FF8000", "#FF8000")}
                            <h3 class="font-bold">Rocky Bodies:</h3>
                            <div id="${system.systemID}_rocky_bodies_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                                Refinery
                            </div>
                        </div>
                        <p class="mt-3">${system.systemCounts.rockBodyCount}</p>
                        <div class="mt-3 flex gap-3">
                            ${FormatInfluenceIcons(system.systemID, "#FFFF00", "#FFFF00", "#FFFF00", "#FFFF00")}
                            <h3 class="font-bold">Icy Bodies:</h3>
                            <div id="${system.systemID}_icy_bodies_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                                Industrial
                            </div>
                        </div>
                        <p class="mt-3">${system.systemCounts.icyBodyCount}</p>
                        <div class="mt-3 flex gap-3">
                            ${FormatInfluenceIcons(system.systemID, "#FF0000", "#FF0000", "#FF0000", "#FF0000")}
                            <h3 class="font-bold">Rings:</h3>
                            <div id="${system.systemID}_rings_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                                Extraction
                            </div>
                        </div>
                        <p class="mt-3">${system.systemCounts.ringCount}</p>
                        <div class="mt-3 flex gap-3">
                            ${FormatInfluenceIcons(system.systemID, "#FF0000", "#FFFF00", "#FF0000", "#FFFF00")}
                            <h3 class="font-bold">Geologicals:</h3>
                            <div id="${system.systemID}_geologicals_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                                Extraction and Industrial
                            </div>
                        </div>
                        <p class="mt-3">${system.systemCounts.geologicalsCount}</p>
                        <div class="mt-3 flex gap-3">
                            ${FormatInfluenceIcons(system.systemID, "#80FF00", "#009900", "#80FF00", "#009900")}
                            <h3 class="font-bold">Organics:</h3>
                            <div id="${system.systemID}_organics_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                                Agriculture and Terraforming
                            </div>
                        </div>
                        <p class="mt-3">${system.systemCounts.organicCount}</p>
                    </div>
                    <h2 class="col-start-1 row-start-9 border-b border-gray-300 pt-5 text-center text-lg drop-shadow-xs lg:col-end-3 lg:row-start-7 lg:text-left xl:col-end-5 xl:row-start-5"></h2>
                    <div class="mt-5 flex flex-col justify-between gap-y-5 sm:flex-row lg:col-span-2 xl:col-span-4">
                        <a href="https://spansh.co.uk/system/${system.systemID}" target="_blank" class="text-center text-blue-800 underline sm:text-left">View on Spansh</a>
                        <div class="flex justify-center gap-x-3 text-center sm:text-left">
                            <a id="${system.systemID}_reportLocked" class="text-blue-800 underline cursor-pointer">Report as Locked</a>
                            <a id="${system.systemID}_reportClaimed" class="text-blue-800 underline cursor-pointer">Report as Claimed</a>
                        </div>
                    </div>
                </div>
                <div id="${system.systemID}_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
                    Click to copy.
                </div>
                ${FormatColonisedSystemTooltips(system.systemID, system.colonisedSystems)}
            </div>
        `;
        count++;
    });

    results.forEach(system => {
        ReinitializeReportLinks(system.systemID);
    });

    ReinitializeAccordion();
    ReinitializeTooltips();
}

function FormatTopBoarder(count) {
    if (count == 0) {
        return "rounded-t-lg border-b-1 border-t-1";
    }
    else {
        return "border-b-1 border-t-0";
    }
}

function FormatCoordinates(inputCoords) {
    return `
        ${Math.round(inputCoords.coordinateX * 100) / 100},
        ${Math.round(inputCoords.coordinateY * 100) / 100},
        ${Math.round(inputCoords.coordinateZ * 100) / 100}
    `;
}

function FormatDate(inputDate) {
    const date = new Date(inputDate);
    return date.toISOString().slice(0, 10).replace(/-/g, '/');
}

function FormatRings(inputRings) {
    var ringList = ``;

    if (inputRings != null) {

        inputRings.sort((a, b) => a.ringName.localeCompare(b.ringName, undefined, { numeric: true }));

        inputRings.forEach(ring => {
            ringList += `
        <li>
            ${ring.ringName} (${ring.ringType.replace(/([A-Z])/g, ' $1').trim()})
            <ul class="items-center ps-7 font-normal">
                ${FormatHotspots(ring.hotspots)}
            </ul>
        </li>
        `;
        });
    }

    return ringList;
}

function FormatHotspots(inputHotspots) {
    var hotspotList = ``;

    if (inputHotspots != null) {
        inputHotspots.sort((a, b) => a.hotspotType.localeCompare(b.hotspotType, undefined, { numeric: true }));

        inputHotspots.forEach(hotspot => {
            hotspotList += `
        <li>&#10551;&nbsp;${hotspot.hotspotType}&nbsp;-&nbsp;${hotspot.hotspotCount}</li>
        `;
        });

    }
    
    return hotspotList;
}

function FormatColonisedSystems(systemID, inputColonisedSystems) {
    var systemsList = ``;

    inputColonisedSystems.sort((a, b) => a.systemName.localeCompare(b.systemName, undefined, { numeric: true }));

    inputColonisedSystems.forEach(system => {
        systemsList += `
        <li class="list-inside list-disc font-bold">
            <span data-tooltip-target="${systemID}_${system.colonisedSystemID}_tooltip" class="cursor-copy" onclick="CopyToClipboard(this);">${system.systemName}</span>
            <ul class="items-center ps-7 font-normal">
                ${FormatStations(system.colonisedSystemID, system.stations)}
            </ul>
        </li>
        `;
    });

    return systemsList;
}

function FormatInfluenceIcons(systemID, bottomLeft, bottomRight, topLeft, topRight) {
    return `
        <svg data-tooltip-target="${systemID}_geologicals_tooltip" class="h-6 w-6" version="1.1" id="Layer_1" x="0px" y="0px" width="924" height="924" viewBox="0 0 924 924" enable-background="new 0 0 924 924" xml:space="preserve" xmlns="http://www.w3.org/2000/svg" xmlns:svg="http://www.w3.org/2000/svg"><defs id="defs3" />
            <g id="g1">
                <path class="fill-[${bottomLeft}]" d="M 458.18182,917.9913 8.1454545,468.46402 46.836364,883.88221 Z" id="path7" />
                <path class="fill-[${bottomRight}]" d="M 458.84246,917.9913 908.87883,468.46402 870.18792,883.88221 Z" id="path8" />
                <path class="fill-[${topLeft}]" d="M 458.18182,12.727273 8.1454545,462.25455 46.836364,46.836364 Z" id="path9" />
                <path class="fill-[${topRight}]" d="M 458.84246,12.727273 908.87883,462.25455 870.18792,46.836364 Z" id="path10" />
            </g>
            <g id="g3"><path d="M 462,29.698 894.302,462 462,894.302 29.698,462 462,29.698 M 462,0 0,462 462,924 924,462 Z" id="path2" style="display:inline" /><path d="M 462,21.083 866.97,57.031 902.918,462 866.97,866.97 462,902.918 57.031,866.97 21.083,462 57.031,57.031 462,21.083 M 462,0 37.667,37.667 0,462 37.667,886.333 462,924 886.333,886.333 924,462 886.333,37.667 Z" id="path1" /><rect x="327" y="433" width="270" height="58" id="rect1" /></g>
        </svg>
    `;
}

function FormatStations(colonisedSystemID, inputStations) {
    var stationList = ``;

    if (inputStations != null) {
        inputStations.sort((a, b) => {
            const factionCompare = a.controllingFaction.localeCompare(b.controllingFaction, undefined, { numeric: true });

            if (factionCompare !== 0) {
                return factionCompare;
            }
            else {
                return a.stationName.localeCompare(b.stationName, undefined, { numeric: true });
            }
        });

        inputStations.forEach(station => {
            stationList += `
                <li>&#10551;&nbsp;<a href="https://spansh.co.uk/station/${station.stationID}" target="_blank" class="text-blue-800 underline">${station.stationName}</a>&nbsp;-&nbsp;${station.controllingFaction}</li>
            `;
        });
    }
    else {
        stationList += `
                <li>&#10551;&nbsp;<a href="https://spansh.co.uk/system/${colonisedSystemID}" target="_blank" class="text-blue-800 underline">Unknown</a></li>
            `;
    }
    
    return stationList;
}

function FormatTrailblazers(inputTrailblazers) {
    var trailblazersList = ``;

    inputTrailblazers.sort((a, b) => a.trailblazerName.localeCompare(b.trailblazerName, undefined, { numeric: true }));

    inputTrailblazers.forEach(trailblazer => {
        trailblazersList += `
        <li class="list-inside list-disc">
            <a href="https://spansh.co.uk/station/${trailblazer.trailblazerID}" target="_blank" class="text-blue-800 underline">${trailblazer.trailblazerName}</a>&nbsp;-&nbsp;${trailblazer.distanceBetween}&nbsp;ly
        </li>
        `;
    });

    return trailblazersList;
}

function FormatColonisedSystemTooltips(systemID, inputColonisedSystems) {
    var tooltipList = ``;

    inputColonisedSystems.forEach(system => {
        tooltipList += `
        <div id="${systemID}_${system.colonisedSystemID}_tooltip" role="tooltip" class="tooltip invisible absolute z-10 inline-block rounded-lg bg-[#0F0F0F] px-3 py-2 text-sm text-[#F0F0F0] opacity-0 shadow-xs transition-opacity duration-200">
            Click to copy.
        </div>
        `;
    });

    return tooltipList;
}

function AddToLocalStorage(systemID) {
    var storedIDs = localStorage.getItem("ReportedIDs");
    var storedIDsJson = [];

    if (storedIDs) {
        storedIDsJson = JSON.parse(storedIDs);
    }

    if (!storedIDsJson.includes(systemID)) {
        storedIDsJson.push(systemID);
    }

    localStorage.setItem("ReportedIDs", JSON.stringify(storedIDsJson));
}

async function ReportSystem(systemID, isLocked) {
    const reportParams = {
        ReportedSystemID: systemID,
        IsLocked: isLocked
    };

    const response = await fetch("/api/StarSystemReport", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(reportParams)
    });

    if (response.status == 401) {
        HSOverlay.open("#error_modal");
    }
    else if (!response.ok) {
        console.error("Error sending report.")
    }
    else {
        AddToLocalStorage(systemID);
    }
}

function ReinitializeReportLinks(systemID) {
    $(`#${systemID}_reportLocked`).on("click", async function () {
        await ReportSystem(systemID, true);
        await LoadResults(false);
    });

    $(`#${systemID}_reportClaimed`).on("click", async function () {
        await ReportSystem(systemID, false);
        await LoadResults(false);
    });
}

function ReinitializeAccordion() {
    if (typeof window.initAccordions === 'function') {
        window.initAccordions();
    }

    $("[data-accordion-target]").on("click", function () {
        const button = $(this);
        const span = button.find("span");
        const svg = button.find("svg");

        if (button.attr("aria-expanded") === "true") {
            button.addClass("bg-[#F07B05] text-[#F0F0F0]")
                .removeClass("bg-white text-[#0F0F0F]");
            span.addClass("text-[#F0F0F0] font-bold")
                .removeClass("text-[#0F0F0F]");
            svg.addClass("text-[#F0F0F0]")
                .removeClass("text-[#0F0F0F]");
        }
        else {
            button.addClass("bg-white text-[#0F0F0F]")
                .removeClass("bg-[#F07B05] text-[#F0F0F0]");
            span.addClass("text-[#0F0F0F]")
                .removeClass("text-[#F0F0F0] font-bold");
            svg.addClass("text-[#0F0F0F]")
                .removeClass("text-[#F0F0F0]");
        }
    });
}

function ReinitializeTooltips() {
    if (typeof window.initTooltips === 'function') {
        window.initTooltips();
    }
}

function TogglePaginationControls(enable) {
    $("#pagination").toggleClass("hidden", !enable);
    $("#pagination").toggleClass("flex", enable);
    $("#results_per_page").prop("disabled", !enable);
    $("#results_per_page_lbl").toggleClass("opacity-50", !enable);
}

function TogglePaginationNumberButtons(remainingPages) {
    const totalPages = remainingPages + currentPage;

    $("#pagination").toggleClass("hidden", totalPages == 1);
    $("#pagination_1").toggleClass("hidden", totalPages <= 4);
    $("#pagination_2").toggleClass("hidden", totalPages <= 3);
    $("#pagination_3").toggleClass("hidden", totalPages <= 2);
}

function SetPaginationNumberButtons(pages) {
    if (currentPage == 1 && pages >= 4) {
        $("#pagination_1").text(currentPage);
        $("#pagination_2").text(currentPage + 1);
        $("#pagination_3").text(currentPage + 2);
        $("#pagination_4").text(currentPage + 3);
        $("#pagination_5").text(currentPage + 4);
    }
    else if (currentPage <= 2 && pages >= 3) {
        $("#pagination_1").text(currentPage - 1);
        $("#pagination_2").text(currentPage);
        $("#pagination_3").text(currentPage + 1);
        $("#pagination_4").text(currentPage + 2);
        $("#pagination_5").text(currentPage + 3);
    }
    else if (pages == 1) {
        $("#pagination_1").text(currentPage - 3);
        $("#pagination_2").text(currentPage - 2);
        $("#pagination_3").text(currentPage - 1);
        $("#pagination_4").text(currentPage);
        $("#pagination_5").text(currentPage + 1);
    }
    else if (pages == 0) {
        $("#pagination_1").text(currentPage - 4);
        $("#pagination_2").text(currentPage - 3);
        $("#pagination_3").text(currentPage - 2);
        $("#pagination_4").text(currentPage - 1);
        $("#pagination_5").text(currentPage);
    }
    else {
        $("#pagination_1").text(currentPage - 2);
        $("#pagination_2").text(currentPage - 1);
        $("#pagination_3").text(currentPage);
        $("#pagination_4").text(currentPage + 1);
        $("#pagination_5").text(currentPage + 2);
    }
}

function SetPaginationNumberButtonAsSelected(buttonID, selected) {
    $("#" + buttonID).toggleClass("bg-[#F07B05] text-[#F0F0F0]", selected);
    $("#" + buttonID).toggleClass("cursor-pointer text-[#0F0F0F] hover:bg-[#0F0F0F] hover:text-[#F0F0F0] focus:bg-[#0F0F0F] focus:text-[#F0F0F0]", !selected);
}

function SetSelectedPaginationNumberButton(pages) {
    if (currentPage == 1 && pages >= 4) {
        SetPaginationNumberButtonAsSelected("pagination_1", true);
        SetPaginationNumberButtonAsSelected("pagination_2", false);
        SetPaginationNumberButtonAsSelected("pagination_3", false);
        SetPaginationNumberButtonAsSelected("pagination_4", false);
        SetPaginationNumberButtonAsSelected("pagination_5", false);
    }
    else if (currentPage <= 2 && pages >= 3) {
        SetPaginationNumberButtonAsSelected("pagination_1", false);
        SetPaginationNumberButtonAsSelected("pagination_2", true);
        SetPaginationNumberButtonAsSelected("pagination_3", false);
        SetPaginationNumberButtonAsSelected("pagination_4", false);
        SetPaginationNumberButtonAsSelected("pagination_5", false);
    }
    else if (pages == 1) {
        SetPaginationNumberButtonAsSelected("pagination_1", false);
        SetPaginationNumberButtonAsSelected("pagination_2", false);
        SetPaginationNumberButtonAsSelected("pagination_3", false);
        SetPaginationNumberButtonAsSelected("pagination_4", true);
        SetPaginationNumberButtonAsSelected("pagination_5", false);
    }
    else if (pages == 0) {
        SetPaginationNumberButtonAsSelected("pagination_1", false);
        SetPaginationNumberButtonAsSelected("pagination_2", false);
        SetPaginationNumberButtonAsSelected("pagination_3", false);
        SetPaginationNumberButtonAsSelected("pagination_4", false);
        SetPaginationNumberButtonAsSelected("pagination_5", true);
    }
    else {
        SetPaginationNumberButtonAsSelected("pagination_1", false);
        SetPaginationNumberButtonAsSelected("pagination_2", false);
        SetPaginationNumberButtonAsSelected("pagination_3", true);
        SetPaginationNumberButtonAsSelected("pagination_4", false);
        SetPaginationNumberButtonAsSelected("pagination_5", false);
    }
}

function TogglePaginationButtons(remainingPages) {
    $("#pagination_previous_jump").prop("disabled", currentPage == 1);
    $("#pagination_previous").prop("disabled", currentPage == 1);
    $("#pagination_next").prop("disabled", remainingPages == 0);
    $("#pagination_next_jump").prop("disabled", remainingPages == 0);
}

function SetPaginationButttonFunctions(remainingPages) {
    $("#pagination_previous_jump").off("click").on("click", function (e) {
        e.preventDefault();
        if (currentPage <= 10) {
            currentPage = 1;
        }
        else {
            currentPage -= 10;
        }

        LoadResults(true);
    });

    $("#pagination_next_jump").off("click").on("click", function (e) {
        e.preventDefault();
        if (remainingPages < 10) {
            currentPage += remainingPages;
        }
        else {
            currentPage += 10;
        }

        LoadResults(true);
    });
}

function FormatPagination(minFollowingPages) {
    if (minFollowingPages != null) {
        TogglePaginationControls(true);

        TogglePaginationNumberButtons(minFollowingPages);
        SetPaginationNumberButtons(minFollowingPages);
        SetSelectedPaginationNumberButton(minFollowingPages);

        TogglePaginationButtons(minFollowingPages);
        SetPaginationButttonFunctions(minFollowingPages);
    }
    else {
        TogglePaginationControls(false);
    }
}

function FormatNoResults() {
    document.getElementById("results").innerHTML = `
    <div class="w-full">
    <h1 class="p-30 text-xl text-center">No systems found.</h1>
    </div>
    `;
}

$("#error_modal_close").on("click", function (e) {
    e.preventDefault();
    HSOverlay.close("#error_modal");
});