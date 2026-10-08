const regions = window.serverRegions;
const background = "/images/origin-lines/{z}/{x}/{y}.webp"
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
    minZoom: 0,
    maxZoom: 7,
    zoomSnap: 0.25,
    maxBounds: backgroundBounds,
    maxBoundsViscosity: 1.0
});

L.tileLayer(background, {
    minZoom: 0,
    maxNativeZoom: 6,
    noWrap: true,
    bounds: backgroundBounds,
    errorTileUrl: "/images/empty-tile.webp"
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