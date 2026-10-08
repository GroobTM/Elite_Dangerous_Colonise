const regions = window.serverRegions;
const background = "/images/origin-lines/origin-lines.png"
const backgroundBounds = [
    [-20000, -45000],
    [70000, 45000]
];

const map = new L.map('map', {
    crs: L.CRS.Simple,
    minZoom: -8,
    maxZoom: 0,
    zoomSnap: 0.25,
    maxBounds: backgroundBounds,
    maxBoundsViscosity: 1.0
});

L.imageOverlay(background, backgroundBounds).addTo(map);

//L.tileLayer(background, {
//    minZoom: -8,
//    maxZoom: 0,
//    zoomOffset: 8,
//    noWrap: true,
//    bounds: backgroundBounds
//}).addTo(map);

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