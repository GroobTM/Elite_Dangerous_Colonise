var regions = window.serverRegions;
var mapScene = null;

var originalScene = THREE.Scene;
THREE.Scene = function () {
    originalScene.apply(this, arguments);

    if (!mapScene) {
        mapScene = this;
    }
};
THREE.Scene.prototype = Object.create(originalScene.prototype);
THREE.Scene.prototype.constructor = THREE.Scene;

$(function () {
    Ed3d.init({
        container: "edmap",
        json: [ { "name": "Sol", "coords": { "x": 0, "y": 0, "z": 0 } } ],
        basePath: "/lib/ed3d/",
        startAnim: false,
        cameraPos: [0, 50000, 0]
    });
});

setTimeout(function () {
    if (mapScene) {
        regions.forEach(region => {
            console.log(region.name);

            var regionMesh = createRegionMesh(region.range, region.centre, "#00FF00");

            mapScene.add(regionMesh);
        });
    }
}, 200);

function createRegionMesh(size, centre, colour) {
    var geometry = new THREE.BoxGeometry(size * 2, size * 2, size * 2);

    var material = new THREE.MeshBasicMaterial({
        color: colour,
        transparent: true,
        opacity: 0.3,
        side: THREE.OneSided
    })

    var regionMesh = new THREE.Mesh(geometry, material);

    regionMesh.position.set(centre.x, centre.y, -centre.z);

    return regionMesh;
}