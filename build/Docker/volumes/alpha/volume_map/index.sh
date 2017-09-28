

rm data/barts/street/street.shp
gdaltindex data/barts/street/street.shp /maps/data/barts/street/*.tif

rm data/barts/main/main.shp
gdaltindex data/barts/main/main.shp /maps/data/barts/main/*.tif

rm data/barts/road/road.shp
gdaltindex data/barts/road/road.shp /maps/data/barts/road/*.tif
