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

    const card = AddRegionToCards(region);

    const activateCard = () => {
        card.classList.add("bg-top", "scale-102", "!text-web-white");
    }
    const deactivateCard = () => {
        card.classList.remove("bg-top", "scale-102", "!text-web-white");
    }

    rect.on("mouseover", function() {
        this.setStyle({
            fillOpacity: 0.6,
            weight: 3
        });

        activateCard();
    });

    rect.on("mouseout", function () {
        this.setStyle({
            fillOpacity: 0.4,
            weight: 2
        });

        deactivateCard();
    });

    $(card).on("mouseenter", () => {
        rect.fire("mouseover");
    }).on("mouseleave", () => {
        rect.fire("mouseout");
    });

    rect.on("click", () => {
        window.location.href = `/Regions/${encodeURIComponent(region.name)}`;
    });

    rect.bindTooltip(region.name, {
        direction: "center",
        permanent: false,
        className: "rounded-lg !bg-web-black px-3 py-2 text-base !text-web-white opacity-0 shadow-xs !border-0 transition-opacity duration-200 font-[EUROCAPS]"
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

function AddRegionToCards (region) {
    const card = document.createElement("a");
    card.className = "text-web-black rounded-lg border-t-10 bg-bottom pt-1 pb-2 shadow-sm transition-all duration-300 ease-in-out hover:text-web-white hover:scale-102 hover:bg-top bg-[length:100%_200%]";
    card.href = `/Regions/${region.name}`;

    const hexColor = `${region.colour}`;
    card.style.borderColor = hexColor;
    card.style.backgroundImage = `linear-gradient(to bottom, ${hexColor} 50%, white 50%)`;

    card.innerHTML = `
        <h2 class="text-center text-2xl font-[EUROCAPS]">${region.name}</h2>
        <p class="text-center">(${Math.round(region.centre.x * 100) / 100}, ${Math.round(region.centre.y * 100) / 100}, ${Math.round(region.centre.z * 100) / 100})</p>
        <p class="text-center">±${(region.range).toLocaleString()}ly</p>
    `;

    document.getElementById("card_grid").appendChild(card);

    return card;
}