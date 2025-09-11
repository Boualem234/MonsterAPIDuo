// (8,8)  (9,8)  (10,8)  (11,8)  (12,8)
// (8,9)  (9,9)  (10,9)  (11,9)  (12,9)
// (8,10) (9,10) (10,10) (11,10) (12,10)
// (8,11) (9,11) (10,11) (11,11) (12,11)
// (8,12) (9,12) (10,12) (11,12) (12,12)

//mettre le radius dans l'api
async function getTilesAsync(x, y) {
  try {
      const url = `https://localhost:7039/api/Tuiles/${x}/${y}`;
      const response = await fetch(url);
    
    if (!response.ok) {
      throw new Error(`Erreur HTTP: ${response.status}`);
    }
    
    const tiles = await response.json();
    displayTiles(tiles);
    
  } catch (error) {
    console.error('Erreur lors du chargement:', error);
    showErrorMessage('Impossible de charger les tuiles');
  }
}

async function getBaseTitles(){
  for (let x = 9; x <= 11; x++) {
      for (let y = 9; y <= 11; y++) {
          await getTilesAsync(x, y);
      }
  }
}