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

      // Créer les bouton
      const button = document.createElement("button");
      button.classList.add("tile-button");
      button.style.border = "none";
      button.style.background = "transparent";
      button.style.padding = "0";
      button.style.cursor = "pointer";

      button.dataset.x = i;
      button.dataset.y = j;

      // Image de base
      const tuileGrid = document.createElement("img");
      tuileGrid.src = "images/rien.png";
      tuileGrid.classList.add("grid-tile");

      button.appendChild(tuileGrid);

      // Quand on clique, fetch pour remplacer l'image
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

      // Précharger les 9 tuiles centrales (9→11) (car joueur au milieu)
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

// fonction pour register via l'api
async function register() {
  const email = document.getElementById("email").value;
  const username = document.getElementById("username").value;
  const password = document.getElementById("password").value;
  const url = "https://localhost:7039/api/Users/Register";

  const credentials = {
    username: email,
    mdp: password,
    pseudo: username,
    dateInscription: new Date().toISOString(),
    isConnected: false
  };

  try {
    const response = await fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(credentials)
    });

    if (response.ok) {
      const data = await response.json();
      console.log("Registration successful:", data);
      alert("Registration successful!");
      window.location.href = "login.html";
    } else {
      console.error("Registration failed:", response.statusText);
      alert("Registration failed. Please try again.");
    }
  } catch (error) {
    console.error("Error during registration:", error);
    alert("An error occurred. Please try again.");
  }
}

document.getElementById("registerForm").addEventListener("submit", function (event) {
  event.preventDefault();
  register();
});

// fonction pour login via l'api (avec email et mdp)
async function login() {
  const email = document.getElementById("email").value;
  const password = document.getElementById("password").value;

  try {
    const response = await fetch(`https://localhost:7039/api/Users/Login/${email}/${password}`, {
      method: "GET",
      headers: { "Content-Type": "application/json" }
    });
    if (response.ok) {
      const data = await response.json();
      console.log("Login successful:", data);
      alert("Login successful!");
      window.location.href = "index.html";
    } else {
      console.error("Login failed:", response.statusText);
      alert("Login failed. Please check your credentials and try again.");
    }
  } catch (error) {
    console.error("Error during login:", error);
    alert("An error occurred. Please try again.");
  }
}

document.getElementById("loginForm").addEventListener("submit", function (event) {
  event.preventDefault();
  login();
});
