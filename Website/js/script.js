// (8,8)  (9,8)  (10,8)  (11,8)  (12,8)
// (8,9)  (9,9)  (10,9)  (11,9)  (12,9)
// (8,10) (9,10) (10,10) (11,10) (12,10)
// (8,11) (9,11) (10,11) (11,11) (12,11)
// (8,12) (9,12) (10,12) (11,12) (12,12)

var villeActuelleX, villeActuelleY, loadedTile = [];

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

async function UpdateIsConnected(){
    var isConnected;
    try{
        var res = await fetch("https://localhost:7039/api/Monsters/IsConnected", {
            method: "GET"
        });
        if(res.ok) isConnected = true;
        else isConnected = false;
    } catch(err){
        isConnected = false;
    }

    if(!isConnected) document.getElementById("status").innerHTML = "Status API : Disconnected";
    else document.getElementById("status").innerHTML = "Status API : Connected";
}

async function TestBattle(idMonster){

}


document.addEventListener("DOMContentLoaded", () => {
    const email = localStorage.getItem("userEmail"); 
    async function GetVille(){
        var response = await fetch(`https://localhost:7039/api/Characters/Ville/${email}`, {method: "GET"});
        var data = await response.json();
        if(data.villeX != 0 && data.villeY != 0){
            villeActuelleX = data.villeX;
            villeActuelleY = data.villeY;
        }
        else{
            villeActuelleX = 10;
            villeActuelleY = 10;
        }
    }

    GetVille();

    async function PostVille(){
        const villeBody = {
            villeX: villeActuelleX,
            villeY: villeActuelleY
        }
        var response = await fetch(`https://localhost:7039/api/Characters/Ville/${email}`, {
            method: "POST",
            headers:
            {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(villeBody)
        });
    }

    const simulator = document.getElementById('simulator');
    const currentPage = window.location.pathname.split("/").pop();
    if (!email && currentPage === "index.html") {
        window.location.href = "templates/login.html";
    }
    if(simulator){
    simulator.addEventListener('click', async (e) => {
        e.preventDefault();
        const monstreId = document.getElementById('simulatorID').value;
        const simulatorModal = new bootstrap.Modal(document.getElementById('simulatorModal'));
        const bodyModal = document.querySelector("#simulatorModal .modal-body");
        var response = await fetch(`https://localhost:7039/api/Characters/Simulate/${monstreId}/${email}`, {method: "GET"});
        var data = await response.json();
        if(data.monstre){
            bodyModal.innerHTML = `
            <h3>Informations de la simulation</h3>
            <hr>
            <h5>Informations du monstre</h5>
            <p>Id: ${monstreId}</p>
            <p>Nom: ${data.monstre.nom}</p>
            <p>Niveau: ${data.monstre.niveau}</p>
            <p>HP: ${data.monstre.pv}</p>
            <img src="${data.monstre.spriteUrl}">
            <hr>
            <h5>Vos informations</h5>
            <p>Nom: ${data.character.nom}</p>
            <p>HP: ${data.character.pv}</p>
            <p>Force: ${data.character.force}</p>
            <p>Niveau: ${data.character.niveau}</p>
            <p>Defense: ${data.character.def}</p>
            <p>HP: ${data.character.pv}</p>
            <hr>
            <h5>Informations du combat</h5>
            <p>Résultat: ${data.resultat ? "Gagné" : "Perdu"}</p>
            <p>Message: ${data.message}</p>`;
        }
        else{
            bodyModal.innerHTML = `
            <h5>Monstre inexistant</h5>
            <hr>
            <p>Ce monstre n'existe pas</p>`;
        }
        simulatorModal.show();
    });
}
    const docBody = document.body;
    const darkLink = document.getElementById('darkMode');
    const lightLink = document.getElementById('lightMode');
    if(darkLink){
    darkLink.addEventListener('click', (e) => {
        e.preventDefault();
        docBody.classList.add('dark');
        localStorage.setItem('theme', 'dark');
    });
}

    if(lightLink){
    lightLink.addEventListener('click', (e) => {
        e.preventDefault();
        docBody.classList.remove('dark');
        localStorage.setItem('theme', 'light');
    });
}

    if (localStorage.getItem('theme') === 'dark') {
        docBody.classList.add('dark');
    }

    UpdateIsConnected();
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

                var monstreSprite = document.createElement("img");
                monstreSprite.classList.add("monstre-sprite");

                btn.dataset.row = row;
                btn.dataset.col = col;

                btn.addEventListener("click", async () => {
                    const worldX = playerPosGlobal.x + col - half;
                    const worldY = playerPosGlobal.y + row - half;
                    var exists = loadedTile.some(t => t.x == worldX && t.y == worldY);
                    try {
                        const response = await fetch(`${urlTuiles}/${worldX}/${worldY}`);
                        const data = await response.json();
                        img.src = data.imageURL || "images/rien.png";
                        if(exists){
                            const myModal = new bootstrap.Modal(document.getElementById('pokeModal'));
                            const body = document.querySelector("#pokeModal .modal-body");

                            if(data.monstres){
                                body.innerHTML = `
                                <h5>Info tuile</h5>
                                <p>Position X: ${worldX}</p>
                                <p>Position Y: ${worldY}</p>
                                <p>Est traversable: ${data.estTraversable ? "Oui" : "Non"}</p>
                                <img src="${data.imageURL}">
                                <hr>
                                <h5>Info monstre</h5>
                                <p>Id: ${data.monstres.id}</p>
                                <p>Niveau: ${data.monstres.niveau}</p>
                                <p>Force: ${data.monstres.force}</p>
                                <p>Defense: ${data.monstres.defense}</p>
                                <p>HP: ${data.monstres.hp}</p>
                                <img src="${data.monstres.spriteUrl}">`;
                            }
                            else{
                                body.innerHTML = `
                                <h5>Info tuile</h5>
                                <p>Position X: ${worldX}</p>
                                <p>Position Y: ${worldY}</p>
                                <p>Est traversable: ${data.estTraversable ? "Oui": "Non"}</p>
                                <img src="${data.imageURL}">`;
                            }
                            myModal.show();
                        }
                        else{
                            loadedTile.push({
                            x: worldX,
                            y: worldY,
                            data: data
                            });
                        if (data.monstres) {
                            let monstreSprite = btn.querySelector(".monstre-sprite");
                            if (!monstreSprite) {
                                monstreSprite = document.createElement("img");
                                monstreSprite.classList.add("monstre-sprite");
                                btn.appendChild(monstreSprite);
                            }
                            monstreSprite.src = data.monstres.spriteUrl;
                            monstreSprite.style.display = "block";
                        } else {
                            const monstreSprite = btn.querySelector(".monstre-sprite");
                            if (monstreSprite) {
                                monstreSprite.remove();
                            }
                        }
                    }
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
                const img = btn.querySelector(".grid-tile");

                const oldPlayer = btn.querySelector(".player-dot");
                if (oldPlayer) oldPlayer.remove();

                if (row === half && col === half) {
                    const dot = document.createElement("img");
                    dot.classList.add("player-dot");
                    dot.style.width = "100px";
                    dot.style.height = "100px";
                    dot.style.position = "absolute";
                    dot.style.top = "50%";
                    dot.style.left = "50%";
                    dot.style.transform = "translate(-50%, -50%)";
                    dot.style.pointerEvents = "none";
                    dot.src = "./images/genji.png"
                    btn.appendChild(dot);
                }

                const distance = Math.max(Math.abs(row - half), Math.abs(col - half));
                if (distance <= playerRadius) {
                    const worldX = playerPosGlobal.x + col - half;
                    const worldY = playerPosGlobal.y + row - half;
                    var exists = loadedTile.some(t => t.x == worldX && t.y == worldY);
                    try {
                        if(!exists){
                            const response = await fetch(`${urlTuiles}/${worldX}/${worldY}`);
                            const data = await response.json();
                            loadedTile.push({
                                x: worldX,
                                y: worldY,
                                data: data
                            });
                            img.src = data.imageURL || "images/rien.png";
                            if (data.monstres) {
                                let monstreSprite = btn.querySelector(".monstre-sprite");
                                if (!monstreSprite) {
                                    monstreSprite = document.createElement("img");
                                    monstreSprite.classList.add("monstre-sprite");
                                    btn.appendChild(monstreSprite);
                                }
                                monstreSprite.src = data.monstres.spriteUrl;
                                monstreSprite.style.display = "block";
                            } else {
                                const monstreSprite = btn.querySelector(".monstre-sprite");
                                if (monstreSprite) {
                                    monstreSprite.remove();
                                }
                            }
                        }
                        else if(exists){
                            var tileData = loadedTile.find(t => t.x == worldX && t.y == worldY).data;
                            img.src = tileData.imageURL || "images/rien.png";
                            if(tileData.monstres){
                                let monstreSprite = btn.querySelector(".monstre-sprite");
                                if(!monstreSprite){
                                    monstreSprite = document.createElement("img");
                                    monstreSprite.classList.add("monstre-sprite");
                                    btn.appendChild(monstreSprite);
                                }
                                monstreSprite.src = tileData.monstres.spriteUrl;
                                monstreSprite.style.display = "block";
                            } else {
                                let monstreSprite = btn.querySelector(".monstre-sprite");
                                if (monstreSprite) {
                                    monstreSprite.remove();
                                }
                            }
                        }
                    } catch (err) {
                        img.src = "images/rien.png";
                    }
                } else {
                    img.src = "images/rien.png";
                }
            }
        }
    }

    async function movePlayer(dx, dy) {
        const newX = playerPosGlobal.x + dx;
        const newY = playerPosGlobal.y + dy;

        if(newX < minX || newX > maxX || newY < minY || newY > maxY){
            showNotif("Vous ne pouvez pas sortir des limites du monde");
            return;
        }

        try{
            await fetch(`${urlTuiles}/${newX}/${newY}`, {
                method: "GET"
            }).then(async res => {
                if(!res.ok) throw new Error("Erreur lors du chargement");
                else{
                    var tuile = await res.json();
                    if(!tuile.estTraversable){
                        showNotif("Impossible de traverser cette tuile");
                        return;
                    }
                    else{
                        if(tuile.type == 4){
                            villeActuelleX = newX;
                            villeActuelleY = newY;
                            PostVille();
                        }
                        playerPosGlobal.x = newX;
                        playerPosGlobal.y = newY;

                        updateViewport();
                        await updatePlayerPositionInDB(newX, newY);
                        await loadPlayerInfo();
                    }
                }
            });
        } catch(err){
            console.log(err);
            showNotif("Erreur inattendue", "error");
            return;
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

    function TeleportToRespawn(){
        playerPosGlobal.x = villeActuelleX;
        playerPosGlobal.y = villeActuelleY;

        updateViewport();
        updatePlayerPositionInDB(villeActuelleX, villeActuelleY);
        loadPlayerInfo();
    }

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
                        localStorage.setItem("userEmail", email);
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

    const loginBtn = document.getElementById("loginBtn");
    if(loginBtn){
        loginBtn.addEventListener("click", async => {
            window.location.href = "templates/login.html"
        })
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
                    method: "POST",
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
        const persoInfoDiv = document.getElementById("perso_info");
        if (!email) {
            console.warn("Aucun email trouvé dans le localStorage !");
            persoInfoDiv.innerHTML = `
                <p class="text-center fw-bold"><i class="fa-solid fa-user-secret"></i> Infos du Personnage</p>
                <hr>
                <p><strong>Aucun utilisateur connecté</strong></p>
            `;
            return;
        }

        try {
            const response = await fetch(`https://localhost:7039/api/Characters/Load/${email}`, {
                method: "GET",
                headers: { "Content-Type": "application/json" }
            });

            if (response.ok) {
                const character = await response.json();
                playerPosGlobal.x = character.posX;
                playerPosGlobal.y = character.posY;
                updateViewport();
                if (persoInfoDiv) {
                    persoInfoDiv.innerHTML = `
                        <p class="text-center fw-bold"><i class="fa-solid fa-user-secret"></i> Infos du Personnage</p>
                        <hr>
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
            var data = await response.json();
            if(data.combat){
                showNotif(data.message);
                if(!(data.resultat) && data.message == "Défaite ! Vous êtes téléporté à la ville et vos HP sont restaurés."){
                    TeleportToRespawn();
                }
                else if(data.resultat){
                    loadedTile = loadedTile.filter(item => !(item.x == data.character.posX && item.y == data.character.posY));
                    updateViewport();
                }
            }
            if (!response.ok) {
                console.error("Erreur lors de la mise à jour de la position du personnage");
            }
        } catch (error) {
            console.error("Erreur lors de la mise à jour de la position du personnage :", error);
        }
    }
});
