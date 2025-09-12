// (8,8)  (9,8)  (10,8)  (11,8)  (12,8)
// (8,9)  (9,9)  (10,9)  (11,9)  (12,9)
// (8,10) (9,10) (10,10) (11,10) (12,10)
// (8,11) (9,11) (10,11) (11,11) (12,11)
// (8,12) (9,12) (10,12) (11,12) (12,12)

var tuile;

document.addEventListener("DOMContentLoaded", async function () {
  const url = `https://localhost:7039/api/Tuiles`;

  for (let i = 1; i < 6; i++) {
    for (let j = 1; j < 6; j++) {
      try {
        const response = await fetch(`${url}/${i}/${j}`, {
          method: "GET",
        });
        const data = await response.json();

        const imageURL = data.imageURL || "";
        const tuileGrid = document.createElement("img");
        tuileGrid.src = `./${imageURL}`;
        tuileGrid.classList.add("grid-tile");
        document.getElementById("gridtuiles").appendChild(tuileGrid);
      } catch (error) {
        console.error(`Failed to fetch tile (${i}, ${j}):`, error);
      }
    }
  }
});

async function getTilesAsync(x, y) {
  try {
      const url = `https://localhost:7039/api/Tuiles/${x}/${y}`;
      await fetch(url, {
        method: "GET"
      }).then((response) => response.json())
      .then(response => {
        tuile = response.split("\"imageURL\":")[1];
      });
    
    if (!response.ok) {
      throw new Error(`Erreur HTTP: ${response.status}`);
    }
    
    const tiles = await response.json();
    console.log('Tuiles reçues:', tiles); //DEBUG
    displayTiles(tiles);
    
  } catch (error) {
    console.error('Erreur lors du chargement:', error);
    showErrorMessage('Impossible de charger les tuiles');
  }
}

//displqy tiles img avec imageUrl

function displayTiles(tiles) {
  const container = document.getElementById('tilesContainer');
  container.innerHTML = ""; // Nettoie avant d'ajouter
  tiles.forEach(tile => {
      const img = document.createElement('img');
      img.src = tile.imageUrl;
      img.alt = `Tile ${tile.id}`;
      img.classList.add('tile-image');
      container.appendChild(img);
  });
}

async function getBaseTitles(){
  for (let x = 9; x <= 11; x++) {
      for (let y = 9; y <= 11; y++) {
          await getTilesAsync(x, y);
      }
  }
}