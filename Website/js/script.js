// (8,8)  (9,8)  (10,8)  (11,8)  (12,8)
// (8,9)  (9,9)  (10,9)  (11,9)  (12,9)
// (8,10) (9,10) (10,10) (11,10) (12,10)
// (8,11) (9,11) (10,11) (11,11) (12,11)
// (8,12) (9,12) (10,12) (11,12) (12,12)

var tuile;

document.addEventListener("DOMContentLoaded", function () {
  const grid = document.getElementById("gridtuiles");
  const url = `https://localhost:7039/api/Tuiles`;

  for (let i = 8; i <= 12; i++) {
    for (let j = 8; j <= 12; j++) {

      // Créer le bouton
      const button = document.createElement("button");
      button.classList.add("tile-button");
      button.style.border = "none";
      button.style.background = "transparent";
      button.style.padding = "0";
      button.style.cursor = "pointer";

      button.dataset.x = i;
      button.dataset.y = j;

      // Image par défaut
      const tuileGrid = document.createElement("img");
      tuileGrid.src = "images/rien.png"; // image par défaut
      tuileGrid.classList.add("grid-tile");

      button.appendChild(tuileGrid);

      // Au clic, fetch pour remplacer l'image
      button.addEventListener("click", async () => {
        try {
          const response = await fetch(`${url}/${i}/${j}`);
          const data = await response.json();
          const imageURL = data.imageURL || "images/rien.png";

          tuileGrid.src = `./${imageURL}`;

        } catch (error) {
          console.error(`Failed to fetch tile (${i}, ${j}):`, error);
        }
      });

      grid.appendChild(button);

      // Précharger les 9 tuiles centrales (9→11) car joueur au milieu
      if (i >= 9 && i <= 11 && j >= 9 && j <= 11) {
        (async () => {
          try {
            const response = await fetch(`${url}/${i}/${j}`);
            const data = await response.json();
            const imageURL = data.imageURL || "images/rien.png";
            tuileGrid.src = `./${imageURL}`;
          } catch (error) {
            console.error(`Failed to fetch central tile (${i}, ${j}):`, error);
          }
        })();
      }

    }
  }
});
