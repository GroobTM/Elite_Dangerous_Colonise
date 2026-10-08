const regions = window.serverRegions;
const backgroundBounds = [
    [-20000, -45000],
    [70000, 45000]
];
const scaleX = 1 / 640;
const scaleY = -1 / 640;
const shiftX = 70.3125;
const shiftY = 109.375;

L.CRS.Galaxy = L.extend({}, L.CRS.Simple, {
    transformation: new L.Transformation(scaleX, shiftX, scaleY, shiftY)
});

const map = new L.map("map", {
    crs: L.CRS.Galaxy,
    minZoom: 2,
    maxZoom: 6,
    zoomSnap: 0.25,
    maxBounds: backgroundBounds,
    maxBoundsViscosity: 1.0
});

map.attributionControl.addAttribution("Map by ~");

map.createPane("overlays");
map.getPane("overlays").style.zIndex = 400;
map.getPane("overlays").style.pointerEvents = "none";

const backgroundLayer = L.tileLayer("/images/game-galaxy/{z}/{x}/{y}.webp", {
    minZoom: 2,
    maxNativeZoom: 6,
    noWrap: true,
    bounds: backgroundBounds,
    errorTileUrl: "/images/empty-tile.webp"
});

const galRegionsOverlay = L.tileLayer("/images/region-lines/{z}/{x}/{y}.webp", {
    minZoom: 2,
    maxNativeZoom: 6,
    noWrap: true,
    bounds: backgroundBounds,
    errorTileUrl: "/images/empty-tile.webp",
    pane: "overlays"
});

backgroundLayer.addTo(map);

const overlaysControl = {
    "Galactic Regions": galRegionsOverlay
};

L.control.layers(null, overlaysControl, {
    collapsed: false
}).addTo(map);

const regionGroup = L.featureGroup();

regions.forEach(region => {
    const rectBounds = [
        [region.centre.z - region.range, region.centre.x - region.range],
        [region.centre.z + region.range, region.centre.x + region.range]
    ];
    const rect = L.rectangle(rectBounds, {
        color: region.colour,
        weight: 2,
        fillColor: region.colour,
        fillOpacity: 0.4
    });

    rect.on("click", () => {
        window.location.href = `/Regions/${encodeURIComponent(region.name)}`;
    });

    rect.on("mouseover", function() {
        this.setStyle({
            fillOpacity: 0.6,
            weight: 3
        });
    });

    rect.on("mouseout", function () {
        this.setStyle({
            fillOpacity: 0.4,
            weight: 2
        });
    });

    rect.bindTooltip(region.name, {
        direction: "center",
        permanent: false,
        className: "rounded-lg !bg-web-black px-3 py-2 text-base !text-web-white opacity-0 shadow-xs !border-0 transition-opacity duration-200"
    });

    rect.addTo(regionGroup);
});

regionGroup.addTo(map);

if (regions.length > 0) {
    map.fitBounds(regionGroup.getBounds(), { padding: [50, 50] });
}
else {
    map.fitBounds(backgroundBounds);
}