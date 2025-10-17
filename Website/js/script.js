var villeActuelleX, villeActuelleY, loadedTile = [], isConnected = false, response, data, errorText = "";
var email, villeBody = {}, simulator, currentPage, monstreId, docBody, grid, playerPosGlobal = { x: 10, y: 10 };
var simulatorModal, bodyModal, tilesModal, registerForm, loginForm, username, password, darkLink = null, lightLink = null;
const notif = document.getElementById("notif"), viewportSize = 5, half = Math.floor(viewportSize / 2), playerRadius = 1, url = "https://localhost:7039/api";
const minX = 0, minY = 0, maxX = 50, maxY = 50;

function ShowNotif(message, type = "info"){
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
    try{
        response = await fetch(`${url}/Monsters/IsConnected`, {
            method: "GET"
        });
        if(response.ok) isConnected = true;
        else isConnected = false;
    } catch(err){
        isConnected = false;
    }

    if(!isConnected) document.getElementById("status").innerHTML = "Status API : Disconnected";
    else document.getElementById("status").innerHTML = "Status API : Connected";
}

async function GetVille(){
    response = await fetch(`${url}/Characters/Ville/${email}`, {method: "GET"});
    data = await response.json();
    if(data.villeX != 0 && data.villeY != 0){
        villeActuelleX = data.villeX;
        villeActuelleY = data.villeY;
    }
    else{
        villeActuelleX = 10;
        villeActuelleY = 10;
    }
}

async function PostVille(){
    villeBody = {
        villeX: villeActuelleX,
        villeY: villeActuelleY
    }
    await fetch(`${url}/Characters/Ville/${email}`, {
        method: "POST",
        headers:
        {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(villeBody)
    });
}

function RedirectIfNotLogged(){
    if (!email && currentPage === "index.html") {
        window.location.href = "templates/login.html";
    }
}

function ModalIfSimulator(){
    if(simulator){
        simulator.addEventListener('click', async (e) => {
            e.preventDefault();
            monstreId = document.getElementById('simulatorID').value;
            simulatorModal = new bootstrap.Modal(document.getElementById('simulatorModal'));
            bodyModal = document.querySelector("#simulatorModal .modal-body");

            response = await fetch(`${url}/Characters/Simulate/${monstreId}/${email}`, {method: "GET"});
            data = await response.json();

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
}

function CreateGrid() {
    grid.innerHTML = "";
    for (let row = 0; row < viewportSize; row++) {
        for (let col = 0; col < viewportSize; col++) {
            var tileBtn = document.createElement("button"), tileImg = document.createElement("img");
            var tileMonstreSprite, monstreSprite = document.createElement("img");
            tileBtn.classList.add("tile-button");
            tileBtn.style.border = "none";
            tileBtn.style.background = "transparent";
            tileBtn.style.padding = "0";
            tileBtn.style.cursor = "pointer";
            tileBtn.style.position = "relative";
            tileImg.src = "images/rien.png";
            tileImg.classList.add("grid-tile");
            tileBtn.appendChild(tileImg);
            monstreSprite.classList.add("monstre-sprite");
            tileBtn.dataset.row = row;
            tileBtn.dataset.col = col;
            tileBtn.addEventListener("click", async () => {
                var worldX = playerPosGlobal.x + col - half;
                var worldY = playerPosGlobal.y + row - half;
                var exists = loadedTile.some(t => t.x == worldX && t.y == worldY);
                try {
                    response = await fetch(`${url}/Tuiles/${worldX}/${worldY}`);
                    data = await response.json();
                    tileImg.src = data.imageURL || "images/rien.png";

                    if(exists){
                        tilesModal = new bootstrap.Modal(document.getElementById('pokeModal'));
                        bodyModal = document.querySelector("#pokeModal .modal-body");
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
                            tileMonstreSprite = tileBtn.querySelector(".monstre-sprite");
                            if (!tileMonstreSprite) {
                                tileMonstreSprite = document.createElement("img");
                                tileMonstreSprite.classList.add("monstre-sprite");
                                tileBtn.appendChild(tileMonstreSprite);
                            }
                            tileMonstreSprite.src = data.monstres.spriteUrl;
                            tileMonstreSprite.style.display = "block";
                        } else {
                            tileMonstreSprite = tileBtn.querySelector(".monstre-sprite");
                            if (tileMonstreSprite) {
                                tileMonstreSprite.remove();
                            }
                        }
                    }
                } catch (err) {
                    console.error(`Erreur tuile (${worldX},${worldY})`, err);
                    tileImg.src = "images/rien.png";
                }
            });
            grid.appendChild(tileBtn);
        }
    }
}

async function UpdateViewPort() {
    var buttons = grid.querySelectorAll("button");

    for (let row = 0; row < viewportSize; row++) {
        for (let col = 0; col < viewportSize; col++) {
            var index = row * viewportSize + col;
            var indexBtn = buttons[index], indexImg = indexBtn.querySelector(".grid-tile"), indexMonstreSprite = null;
            var oldPlayer = indexBtn.querySelector(".player-dot");
            var distance = Math.max(Math.abs(row - half), Math.abs(col - half));
            var worldX = playerPosGlobal.x + col - half, worldY = playerPosGlobal.y + row - half;

            if (oldPlayer) oldPlayer.remove();

            if (row === half && col === half) {
                var dot = document.createElement("img");
                dot.classList.add("player-dot");
                dot.style.width = "100px";
                dot.style.height = "100px";
                dot.style.position = "absolute";
                dot.style.top = "50%";
                dot.style.left = "50%";
                dot.style.transform = "translate(-50%, -50%)";
                dot.style.pointerEvents = "none";
                dot.src = "./images/genji.png"
                indexBtn.appendChild(dot);
            }

            if (distance <= playerRadius) {
                var exists = loadedTile.some(t => t.x == worldX && t.y == worldY);

                try {
                    indexMonstreSprite = indexBtn.querySelector(".monstre-sprite");
                    if(!exists){
                        response = await fetch(`${url}/Tuiles/${worldX}/${worldY}`);
                        data = await response.json();
                        loadedTile.push({
                            x: worldX,
                            y: worldY,
                            data: data
                        });
                        indexImg.src = data.imageURL || "images/rien.png";
                        if (data.monstres) {
                            if (!indexMonstreSprite) {
                                indexMonstreSprite = document.createElement("img");
                                indexMonstreSprite.classList.add("monstre-sprite");
                                indexBtn.appendChild(indexMonstreSprite);
                            }
                            indexMonstreSprite.src = data.monstres.spriteUrl;
                            indexMonstreSprite.style.display = "block";
                        } else {
                            if (indexMonstreSprite) {
                                monstreSprite.remove();
                            }
                        }
                    }
                    else if(exists){
                        var tileData = loadedTile.find(t => t.x == worldX && t.y == worldY).data;
                        indexImg.src = tileData.imageURL || "images/rien.png";

                        if(tileData.monstres){
                            if(!indexMonstreSprite){
                                indexMonstreSprite = document.createElement("img");
                                indexMonstreSprite.classList.add("monstre-sprite");
                                indexBtn.appendChild(indexMonstreSprite);
                            }
                            indexMonstreSprite.src = tileData.monstres.spriteUrl;
                            indexMonstreSprite.style.display = "block";
                        } else {
                            if (indexMonstreSprite) {
                                indexMonstreSprite.remove();
                            }
                        }
                    }
                } catch (err) {
                    indexImg.src = "images/rien.png";
                }
            } else {
                indexImg.src = "images/rien.png";
            }
        }
    }
}

async function MovePlayer(dx, dy) {
    var newX = playerPosGlobal.x + dx, newY = playerPosGlobal.y + dy, tuile;

    if(newX < minX || newX > maxX || newY < minY || newY > maxY){
        ShowNotif("Vous ne pouvez pas sortir des limites du monde");
        return;
    }

    try{
        await fetch(`${url}/Tuiles/${newX}/${newY}`, {
            method: "GET"
        }).then(async res => {
            if(!res.ok) throw new Error("Erreur lors du chargement");
            else{
                tuile = await res.json();
                if(!tuile.estTraversable){
                    ShowNotif("Impossible de traverser cette tuile");
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

                    UpdateViewPort();
                    await UpdatePlayerPositionInDB(newX, newY);
                    await LoadPlayerInfo();
                }
            }
        });
    } catch(err){
        console.log(err);
        ShowNotif("Erreur inattendue", "error");
        return;
    }
}

async function UpdatePlayerPositionInDB(x, y) {
    email = localStorage.getItem("userEmail");

    if (!email) {
        console.warn("Aucun email trouvé dans le localStorage !");
        return;
    }

    try {
        response = await fetch(`${url}/Characters/Deplacement/${x}/${y}/${email}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" }
        });
        data = await response.json();
        if(data.combat){
            ShowNotif(data.message);
            if(!(data.resultat) && data.message == "Défaite ! Vous êtes téléporté à la ville et vos HP sont restaurés."){
                TeleportToRespawn();
            }
            else if(data.resultat){
                loadedTile = loadedTile.filter(item => !(item.x == data.character.posX && item.y == data.character.posY));
                UpdateViewPort();
            }
        }
        if (!response.ok) {
            console.error("Erreur lors de la mise à jour de la position du personnage");
        }
    } catch (error) {
        console.error("Erreur lors de la mise à jour de la position du personnage :", error);
    }
}

function TeleportToRespawn(){
    playerPosGlobal.x = villeActuelleX;
    playerPosGlobal.y = villeActuelleY;

    UpdateViewPort();
    UpdatePlayerPositionInDB(villeActuelleX, villeActuelleY);
    LoadPlayerInfo();
}

function LoadRegisterForm(){
    registerForm = document.getElementById("registerForm");

    if (registerForm) {
        registerForm.addEventListener("submit", async (event) => {
            event.preventDefault();
            email = document.getElementById("email").value;
            username = document.getElementById("username").value;
            password = document.getElementById("password").value;
            var userData = {
                email, 
                mdp: password, 
                pseudo: username,
                dateInscription: new Date().toISOString(),
                isConnected: true
            };

            try {
                response = await fetch(`${url}/Users/Register`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(userData)
                });

                if (response.ok) {
                    ShowNotif("Inscription réussie ! Redirection vers la page de connexion...", "success");
                    setTimeout(() => {
                        window.location.href = "login.html";
                    }, 2000);
                }
                else {
                    errorText = await response.text();
                    ShowNotif("Échec de l'inscription : " + errorText, "error");
                }
            } catch (error) {
                ShowNotif("Une erreur est survenue. Veuillez réessayer.", "error");
                console.error("Erreur durant l'inscription :", error);
            }
        });
    }
}

function LoadLoginForm(){
    loginForm = document.getElementById("loginForm");
    var loginBtn = document.getElementById("loginBtn");

    if (loginForm) {
        loginForm.addEventListener("submit", async (event) => {
            event.preventDefault();
            email = document.getElementById("email").value;
            password = document.getElementById("password").value;

            try {
                response = await fetch(`${url}/Users/Login/${email}/${password}`, {
                    method: "GET",
                    headers: { "Content-Type": "application/json" }
                });

                if (response.ok) {
                    ShowNotif("Connexion réussie !", "success");
                    localStorage.setItem("userEmail", email);
                    window.location.href = "../index.html";
                } 
                else {
                    errorText = await response.text();
                    ShowNotif("Échec de la connexion : " + errorText, "error");
                }
            } catch (error) {
                console.error("Error during login:", error);
                ShowNotif("Une erreur est survenue. Veuillez réessayer.", "error");
            }
        });
    }

    if(loginBtn){
        loginBtn.addEventListener("click", () => {
            window.location.href = "templates/login.html"
        })
    }
}

function LoadLogout(){
    var logoutBtn = document.getElementById("logoutBtn");

    if (logoutBtn) {
        logoutBtn.addEventListener("click", async () => {
            email = localStorage.getItem("userEmail");

            if (!email) {
                ShowNotif("Aucun utilisateur connecté.", "error");
                return;
            }

            try {
                response = await fetch(`${url}/Users/Logout/${email}`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" }
                });

                if (response.ok) {
                    ShowNotif("Déconnexion réussie ! Redirection vers la page de connexion...", "success");
                    localStorage.removeItem("userEmail");
                    setTimeout(() => {
                        window.location.href = "templates/login.html";
                    }, 2000);
                } 
                else {
                    errorText = await response.text();
                    ShowNotif("Échec de la déconnexion : " + errorText, "error");
                }
            } catch (error) {
                console.error("Error during logout:", error);
                ShowNotif("Une erreur est survenue. Veuillez réessayer.", "error");
            }
        })
    }
}

async function LoadPlayerInfo() {
    email = localStorage.getItem("userEmail"); 
    var persoInfoDiv = document.getElementById("perso_info"), character;

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
        response = await fetch(`${url}/Characters/Load/${email}`, {
            method: "GET",
            headers: { "Content-Type": "application/json" }
        });

        if (response.ok) {
            character = await response.json();
            playerPosGlobal.x = character.posX;
            playerPosGlobal.y = character.posY;
            UpdateViewPort();

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

document.addEventListener("DOMContentLoaded", () => {
    UpdateIsConnected();
    email = localStorage.getItem("userEmail"); 
    simulator = document.getElementById('simulator');
    currentPage = window.location.pathname.split("/").pop();
    GetVille();
    RedirectIfNotLogged();
    ModalIfSimulator();
    docBody = document.body;
    grid = document.getElementById("gridtuiles");
    darkLink = document.getElementById('darkMode');
    lightLink = document.getElementById('lightMode');
    CreateGrid();

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
    
    document.getElementById("btn-up")?.addEventListener("click", () => MovePlayer(0, -1));
    document.getElementById("btn-down")?.addEventListener("click", () => MovePlayer(0, 1));
    document.getElementById("btn-left")?.addEventListener("click", () => MovePlayer(-1, 0));
    document.getElementById("btn-right")?.addEventListener("click", () => MovePlayer(1, 0));

    document.addEventListener("keydown", (e) => {
        if (e.key === "ArrowUp") MovePlayer(0, -1);
        if (e.key === "ArrowDown") MovePlayer(0, 1);
        if (e.key === "ArrowLeft") MovePlayer(-1, 0);
        if (e.key === "ArrowRight") MovePlayer(1, 0);
    });

    LoadRegisterForm();
    LoadLoginForm();
    LoadLogout();
    UpdateViewPort();
    LoadPlayerInfo();
});
