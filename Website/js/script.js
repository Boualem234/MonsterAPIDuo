// (8,8)  (9,8)  (10,8)  (11,8)  (12,8)
// (8,9)  (9,9)  (10,9)  (11,9)  (12,9)
// (8,10) (9,10) (10,10) (11,10) (12,10)
// (8,11) (9,11) (10,11) (11,11) (12,11)
// (8,12) (9,12) (10,12) (11,12) (12,12)


document.addEventListener("DOMContentLoaded", () => {

  // 1 CODE TUILES (index.html)

  const grid = document.getElementById("gridtuiles");
  if (grid) {
    const urlTuiles = "https://localhost:7039/api/Tuiles";

    for (let i = 8; i <= 12; i++) {
      for (let j = 8; j <= 12; j++) {
        // Création du bouton
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

        // Click sur la tuile
        button.addEventListener("click", async () => {
          try {
            const response = await fetch(`${urlTuiles}/${i}/${j}`);
            const data = await response.json();
            const imageURL = data.imageURL || "images/rien.png";
            tuileGrid.src = `./${imageURL}`;
          } catch (error) {
            console.error(`Failed to fetch tile (${i}, ${j}):`, error);
          }
        });

        grid.appendChild(button);

        // Précharger les tuiles centrales (9→11)
        if (i >= 9 && i <= 11 && j >= 9 && j <= 11) {
          (async () => {
            try {
              const response = await fetch(`${urlTuiles}/${i}/${j}`);
              const data = await response.json();
              tuileGrid.src = data.imageURL ? `./${data.imageURL}` : "images/rien.png";
            } catch (error) {
              console.error(`Failed to fetch central tile (${i}, ${j}):`, error);
            }
          })();
        }
      }
    }
  }


  // 2 REGISTER (register.html)
 
  const registerForm = document.getElementById("registerForm");
  if (registerForm) {
    registerForm.addEventListener("submit", async (event) => {
      event.preventDefault();

      const email = document.getElementById("email").value;
      const username = document.getElementById("username").value;
      const password = document.getElementById("password").value;

      const userData = {
        email: email,
        mdp: password,
        pseudo: username,
        dateInscription: new Date().toISOString(),
        isConnected: true
      };

      try {
        const response = await fetch("https://localhost:7039/api/Users/Register", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(userData)
        });

        if (response.ok) {
          alert("Registration successful! You can now log in.");
          window.location.href = "login.html";
        } else {
          const errorText = await response.text();
          alert("Registration failed: " + errorText);
        }
      } catch (error) {
        console.error("Error during registration:", error);
        alert("An error occurred. Please try again.");
      }
    });
  }

  // 3 LOGIN

  const loginForm = document.getElementById("loginForm");
  if (loginForm) {
    loginForm.addEventListener("submit", async (event) => {
      event.preventDefault();

      const email = document.getElementById("email").value;
      const password = document.getElementById("password").value;

      try {
        const response = await fetch(`https://localhost:7039/api/Users/Login/${email}/${password}`, {
          method: "GET",
          headers: { "Content-Type": "application/json" }
        });

        if (response.ok) {
          alert("Login successful!");
          window.location.href = "../index.html";
        } else {
          const errorText = await response.text();
          alert("Login failed: " + errorText);
        }
      } catch (error) {
        console.error("Error during login:", error);
        alert("An error occurred. Please try again.");
      }
    });
  }

});

