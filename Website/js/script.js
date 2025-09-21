// (8,8)  (9,8)  (10,8)  (11,8)  (12,8)
// (8,9)  (9,9)  (10,9)  (11,9)  (12,9)
// (8,10) (9,10) (10,10) (11,10) (12,10)
// (8,11) (9,11) (10,11) (11,11) (12,11)
// (8,12) (9,12) (10,12) (11,12) (12,12)

function showNotif(message, type = "info") {
    const notif = document.getElementById("notif");
    notif.textContent = message;

    notif.style.background = 
        type === "success" ? "green" :
        type === "error" ? "crimson" : "rgba(0,0,0,0.8)";

    notif.style.display = "block";

    setTimeout(() => {
        notif.style.display = "none";
    }, 3000);
}


document.addEventListener("DOMContentLoaded", () => {

    const grid = document.getElementById("gridtuiles");
    const viewportSize = 5;
    const half = Math.floor(viewportSize / 2);
    const playerRadius = 1; // rayon autour du joueur à charger automatiquement
    const urlTuiles = "https://localhost:7039/api/Tuiles";
    let playerPosGlobal = { x: 10, y: 10 }; // position globale du joueur
    const minX = 0, minY = 0, maxX = 50, maxY = 50; // limites du monde

    // Création de la grille 5x5
    function createGrid() {
        grid.innerHTML = "";
        for (let row = 0; row < viewportSize; row++) {
            for (let col = 0; col < viewportSize; col++) {
                const btn = document.createElement("button");
                btn.classList.add("tile-button");
                btn.style.border = "none";
                btn.style.background = "transparent";
                btn.style.padding = "0";
                btn.style.cursor = "pointer";
                btn.style.position = "relative";

                const img = document.createElement("img");
                img.src = "images/rien.png";
                img.classList.add("grid-tile");
                btn.appendChild(img);

                btn.dataset.row = row;
                btn.dataset.col = col;

                // Click sur la tuile pour charger depuis l'API
                btn.addEventListener("click", async () => {
                    const worldX = playerPosGlobal.x + col - half;
                    const worldY = playerPosGlobal.y + row - half;
                    try {
                        const response = await fetch(`${urlTuiles}/${worldX}/${worldY}`);
                        const data = await response.json();
                        img.src = data.imageURL || "images/rien.png";
                    } catch (err) {
                        console.error(`Erreur tuile (${worldX},${worldY})`, err);
                        img.src = "images/rien.png";
                    }
                });

                grid.appendChild(btn);
            }
        }
    }

    async function updateViewport() {
        const buttons = grid.querySelectorAll("button");

        for (let row = 0; row < viewportSize; row++) {
            for (let col = 0; col < viewportSize; col++) {
                const index = row * viewportSize + col;
                const btn = buttons[index];
                const img = btn.querySelector("img");

                // Supprimer ancien point joueur
                const oldPlayer = btn.querySelector(".player-dot");
                if (oldPlayer) oldPlayer.remove();

                // Joueur toujours au centre
                if (row === half && col === half) {
                    const dot = document.createElement("div");
                    dot.classList.add("player-dot");
                    dot.style.width = "15px";
                    dot.style.height = "15px";
                    dot.style.borderRadius = "50%";
                    dot.style.backgroundColor = "red";
                    dot.style.position = "absolute";
                    dot.style.top = "50%";
                    dot.style.left = "50%";
                    dot.style.transform = "translate(-50%, -50%)";
                    dot.style.pointerEvents = "none";
                    btn.appendChild(dot);
                }

                // Charger uniquement les tuiles autour du joueur 
                const distance = Math.max(Math.abs(row - half), Math.abs(col - half));
                if (distance <= playerRadius) {
                    const worldX = playerPosGlobal.x + col - half;
                    const worldY = playerPosGlobal.y + row - half;
                    try {
                        const response = await fetch(`${urlTuiles}/${worldX}/${worldY}`);
                        const data = await response.json();
                        img.src = data.imageURL || "images/rien.png";
                    } catch (err) {
                        img.src = "images/rien.png";
                    }
                } else {
                    img.src = "images/rien.png"; // les tuiles plus loin restent "rien"
                }
            }
        }
    }

    async function movePlayer(dx, dy) {
        const newX = playerPosGlobal.x + dx;
        const newY = playerPosGlobal.y + dy;

        if (newX >= minX && newX <= maxX && newY >= minY && newY <= maxY) {
            playerPosGlobal.x = newX;
            playerPosGlobal.y = newY;

            // MAJ la vue
            updateViewport();

            // MAJ en DB
            await updatePlayerPositionInDB(newX, newY);
        }
    }

    document.getElementById("btn-up")?.addEventListener("click", () => movePlayer(0, -1));
    document.getElementById("btn-down")?.addEventListener("click", () => movePlayer(0, 1));
    document.getElementById("btn-left")?.addEventListener("click", () => movePlayer(-1, 0));
    document.getElementById("btn-right")?.addEventListener("click", () => movePlayer(1, 0));

    document.addEventListener("keydown", (e) => {
        if (e.key === "ArrowUp") movePlayer(0, -1);
        if (e.key === "ArrowDown") movePlayer(0, 1);
        if (e.key === "ArrowLeft") movePlayer(-1, 0);
        if (e.key === "ArrowRight") movePlayer(1, 0);
    });

    // REGISTER et LOGIN 

    const registerForm = document.getElementById("registerForm");
    if (registerForm) {
        registerForm.addEventListener("submit", async (event) => {
            event.preventDefault();
            const email = document.getElementById("email").value;
            const username = document.getElementById("username").value;
            const password = document.getElementById("password").value;
            const userData = {
                email, mdp: password, pseudo: username,
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
                    showNotif("Inscription réussie ! Redirection vers la page de connexion...", "success");
                    setTimeout(() => {
                        window.location.href = "login.html";
                    }, 2000);
                } else {
                    const errorText = await response.text();
                    showNotif("Échec de l'inscription : " + errorText, "error");
                }
            } catch (error) {
                showNotif("Une erreur est survenue. Veuillez réessayer.", "error");
                console.error("Erreur durant l'inscription :", error);
            }
        });
    }

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
                    if (response.ok) {
                        showNotif("Connexion réussie !", "success");
                        localStorage.setItem("userEmail", email); // sauvegarde l'email
                        window.location.href = "../index.html";
                    }
                } else {
                    const errorText = await response.text();
                    showNotif("Échec de la connexion : " + errorText, "error");
                }
            } catch (error) {
                console.error("Error during login:", error);
                showNotif("Une erreur est survenue. Veuillez réessayer.", "error");
            }
        });
    }

    // LOGOUT /api/Users/Logout/{email}
    const logoutBtn = document.getElementById("logoutBtn");
    if (logoutBtn) {
        logoutBtn.addEventListener("click", async () => {
            const email = localStorage.getItem("userEmail");
            if (!email) {
                showNotif("Aucun utilisateur connecté.", "error");
                return;
            }
            try {
                const response = await fetch(`https://localhost:7039/api/Users/Logout/${email}`, {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" }
                });
                if (response.ok) {
                    showNotif("Déconnexion réussie ! Redirection vers la page de connexion...", "success");
                    localStorage.removeItem("userEmail");
                    setTimeout(() => {
                        window.location.href = "templates/login.html";
                    }, 2000);
                } else {
                    const errorText = await response.text();
                    showNotif("Échec de la déconnexion : " + errorText, "error");
                }
            } catch (error) {
                console.error("Error during logout:", error);
                showNotif("Une erreur est survenue. Veuillez réessayer.", "error");
            }
        });
    }

    createGrid();
    updateViewport();

    // Charger et afficher les infos du personnage

    async function loadPlayerInfo() {
        const email = localStorage.getItem("userEmail"); 
        if (!email) {
            console.warn("Aucun email trouvé dans le localStorage !");
            return;
        }

        try {
            const response = await fetch(`https://localhost:7039/api/Characters/Load/${email}`, {
                method: "GET",
                headers: { "Content-Type": "application/json" }
            });

            if (response.ok) {
                const character = await response.json();
                const persoInfoDiv = document.getElementById("perso_info");

                if (persoInfoDiv) {
                    persoInfoDiv.innerHTML = `
                        <h5 class="text-center mb-3">Infos du Personnage</h5>
                        <p><strong>Nom :</strong> ${character.nom}</p>
                        <p><strong>Niveau :</strong> ${character.niveau}</p>
                        <p><strong>Expérience :</strong> ${character.exp}</p>
                        <p><strong>PV :</strong> ${character.pv} / ${character.pvMax}</p>
                        <p><strong>Force :</strong> ${character.force}</p>
                        <p><strong>Défense :</strong> ${character.def}</p>
                        <p><strong>Position :</strong> (${character.posX}, ${character.posY})</p>
                        <p><strong>Date de création :</strong> ${new Date(character.dateCreation).toLocaleString()}</p>
                    `;
                }
            } else {
                console.error("Impossible de charger les infos du personnage");
            }
        } catch (error) {
            console.error("Erreur lors du chargement des infos du personnage :", error);
        }
    }

    loadPlayerInfo();


    // mettre a jour la position du personnage dans la BD
    async function updatePlayerPositionInDB(x, y) {
        const email = localStorage.getItem("userEmail");
        if (!email) {
            console.warn("Aucun email trouvé dans le localStorage !");
            return;
        }

        try {
            const response = await fetch(`https://localhost:7039/api/Characters/Deplacement/${x}/${y}/${email}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" }
            });
            if (!response.ok) {
                console.error("Erreur lors de la mise à jour de la position du personnage");
            }
        } catch (error) {
            console.error("Erreur lors de la mise à jour de la position du personnage :", error);
        }
    }
});
