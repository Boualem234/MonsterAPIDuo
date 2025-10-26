var villeActuelleX = 10, villeActuelleY = 10, tuilesChargees = [], estConnecte = false, reponse, data, texteErreur = "";
var email, villeBody = {}, simulateur, pageActuelle, monstreId, docBody, grille, positionJoueurGlobal = { x: 10, y: 10 };
var simulateurModal, bodyModal, tilesModal, formulaireInscription, formulaireConnexion, nomUtilisateur, mdp, lienSombre = null, lienClair = null;
const notif = document.getElementById("notif"), taillePortVue = 5, moitie = Math.floor(taillePortVue / 2), zoneJoueur = 1, url = "https://localhost:7039/api";
const minX = 0, minY = 0, maxX = 50, maxY = 50;

function AfficherNotif(message, type = "info"){
    notif.textContent = message;

    notif.style.background = 
        type === "success" ? "green" :
        type === "error" ? "crimson" : "rgba(0,0,0,0.8)";

    notif.style.display = "block";

    setTimeout(() => {
        notif.style.display = "none";
    }, 3000);
}

async function MettreEstConnecteAJour(){
    try{
        reponse = await fetch(`${url}/Monsters/IsConnected`, {
            method: "GET"
        });
        if(reponse.ok) estConnecte = true;
        else estConnecte = false;
    } catch(err){
        estConnecte = false;
    }

    var statusElement = document.getElementById("status");
    if (statusElement) {
        if(!estConnecte) statusElement.innerHTML = "Status de l'API : Déconnectée";
        else statusElement.innerHTML = "Status de l'API : Connectée";
    }
}

async function ObtenirVille() {
    if (!email) return;
    reponse = await fetch(`${url}/Characters/Ville/${email}`, { method: "GET" });
    data = await reponse.json();

    if (data.villeX != 0 && data.villeY != 0) {
        villeActuelleX = data.villeX;
        villeActuelleY = data.villeY;
    } else {
        villeActuelleX = 10;
        villeActuelleY = 10;
    }
}

async function PosterVille(){
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

function RedirigerSiNonConnecte(){
    if (!email && pageActuelle === "index.html") {
        window.location.href = "templates/login.html";
    }
}

function ModalSiSimulateur(){
    if(simulateur){
        simulateur.addEventListener('click', async (e) => {
            e.preventDefault();
            monstreId = document.getElementById('simulateurID').value;
            simulateurModal = new bootstrap.Modal(document.getElementById('simulateurModal'));
            bodyModal = document.querySelector("#simulateurModal .modal-body");

            reponse = await fetch(`${url}/Characters/Simulate/${monstreId}/${email}`, { method: "GET" });
            data = await reponse.json();

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
            simulateurModal.show();
        });
    }
}

function CreerGrille() {
    if (!grille) return;
    grille.innerHTML = "";
    for (let row = 0; row < taillePortVue; row++) {
        for (let col = 0; col < taillePortVue; col++) {
            let tileBtn = document.createElement("button"), tileImg = document.createElement("img");
            let tileMonstreSprite, monstreSprite = document.createElement("img");
            tileBtn.classList.add("tuile-boutton");
            tileBtn.style.border = "none";
            tileBtn.style.background = "transparent";
            tileBtn.style.padding = "0";
            tileBtn.style.cursor = "pointer";
            tileBtn.style.position = "relative";
            tileImg.src = "images/rien.png";
            tileImg.classList.add("grille-tuile");
            tileBtn.appendChild(tileImg);
            monstreSprite.classList.add("monstre-sprite");
            tileBtn.dataset.row = row;
            tileBtn.dataset.col = col;
            tileBtn.addEventListener("click", async () => {
                var mondeX = positionJoueurGlobal.x + col - moitie;
                var mondeY = positionJoueurGlobal.y + row - moitie;
                var exists = tuilesChargees.some(t => t.x == mondeX && t.y == mondeY);
                try {
                    reponse = await fetch(`${url}/Tuiles/${mondeX}/${mondeY}`);
                    data = await reponse.json();
                    tileImg.src = data.imageURL || "images/rien.png";
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

                    if(exists){
                        tilesModal = new bootstrap.Modal(document.getElementById('pokeModal'));
                        bodyModal = document.querySelector("#pokeModal .modal-body");
                        if(data.monstres){
                            bodyModal.innerHTML = `
                                <h5>Info tuile</h5>
                                <p>Position X: ${mondeX}</p>
                                <p>Position Y: ${mondeY}</p>
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
                            bodyModal.innerHTML = `
                                <h5>Info tuile</h5>
                                <p>Position X: ${mondeX}</p>
                                <p>Position Y: ${mondeY}</p>
                                <p>Est traversable: ${data.estTraversable ? "Oui": "Non"}</p>
                                <img src="${data.imageURL}">`;
                        }
                        tilesModal.show();
                    }
                    else{
                        tuilesChargees.push({
                            x: mondeX,
                            y: mondeY,
                            data: data
                        });
                    }
                } catch (err) {
                    console.error(`Erreur tuile (${mondeX},${mondeY})`, err);
                    tileImg.src = "images/rien.png";
                }
            });
            grille.appendChild(tileBtn);
        }
    }
}

async function MettreVuePortAJour() {
    var buttons = grille.querySelectorAll("button");

    for (let row = 0; row < taillePortVue; row++) {
        for (let col = 0; col < taillePortVue; col++) {
            var index = row * taillePortVue + col;
            var indexBtn = buttons[index], indexImg = indexBtn.querySelector(".grille-tuile"), indexMonstreSprite = null;
            var oldPlayer = indexBtn.querySelector(".player-dot");
            var distance = Math.max(Math.abs(row - moitie), Math.abs(col - moitie));
            var mondeX = positionJoueurGlobal.x + col - moitie, mondeY = positionJoueurGlobal.y + row - moitie;

            if (oldPlayer) oldPlayer.remove();

            if (row === moitie && col === moitie) {
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

            if (distance <= zoneJoueur) {
                var exists = tuilesChargees.some(t => t.x == mondeX && t.y == mondeY);

                try {
                    indexMonstreSprite = indexBtn.querySelector(".monstre-sprite");
                    if(!exists){
                        reponse = await fetch(`${url}/Tuiles/${mondeX}/${mondeY}`);
                        data = await reponse.json();
                        tuilesChargees.push({
                            x: mondeX,
                            y: mondeY,
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
                                indexMonstreSprite.remove();
                            }
                        }
                    }
                    else if(exists){
                        var tileData = tuilesChargees.find(t => t.x == mondeX && t.y == mondeY).data;
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
                    if(indexMonstreSprite !== null && indexMonstreSprite !== undefined){
                        indexMonstreSprite.remove();
                    }
                }
            } else {
                var tileExists = tuilesChargees.some(t => t.x == mondeX && t.y == mondeY);
                if (tileExists) {
                    // Garder l'affichage de la tuile chargée
                    var tileData = tuilesChargees.find(t => t.x == mondeX && t.y == mondeY).data;
                    indexImg.src = tileData.imageURL || "images/rien.png";

                    indexMonstreSprite = indexBtn.querySelector(".monstre-sprite");
                    if (tileData.monstres) {
                        if (!indexMonstreSprite) {
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
                } else {
                    // Remettre l'image par défaut ssi pas chargée
                    indexImg.src = "images/rien.png";
                    var monstreASupprimer = indexBtn.querySelector(".monstre-sprite");
                    if (monstreASupprimer) {
                        monstreASupprimer.remove();
                    }
                }
            }
        }
    }
}

async function BougerJoueur(dx, dy) {
    var nouveauX = positionJoueurGlobal.x + dx, nouveauY = positionJoueurGlobal.y + dy, tuile;

    if(nouveauX < minX || nouveauX > maxX || nouveauY < minY || nouveauY > maxY){
        AfficherNotif("Vous ne pouvez pas sortir des limites du monde");
        return;
    }

    try{
        await fetch(`${url}/Tuiles/${nouveauX}/${nouveauY}`, {
            method: "GET"
        }).then(async res => {
            if(!res.ok) throw new Error("Erreur lors du chargement");
            else{
                tuile = await res.json();
                if(!tuile.estTraversable){
                    AfficherNotif("Impossible de traverser cette tuile");
                    return;
                }
                else{
                    if(tuile.type == 4){
                        villeActuelleX = nouveauX;
                        villeActuelleY = nouveauY;
                        PosterVille();
                    }
                    positionJoueurGlobal.x = nouveauX;
                    positionJoueurGlobal.y = nouveauY;

                    MettreVuePortAJour();
                    await MettrePositionAJourDB(nouveauX, nouveauY);
                    await ChargerInfoJoueur();
                }
            }
        });
    } catch(err){
        console.log(err);
        AfficherNotif("Erreur inattendue", "error");
        return;
    }
}

async function MettrePositionAJourDB(x, y) {
    email = localStorage.getItem("utilisateurEmail");

    if (!email) {
        console.warn("Aucun email trouvé dans le localStorage !");
        return;
    }

    try {
        reponse = await fetch(`${url}/Characters/Deplacement/${x}/${y}/${email}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" }
        });
        data = await reponse.json();
        if(data.combat){
            AfficherNotif(data.message);
            if(!(data.resultat) && data.message == "Défaite ! Vous êtes téléporté à la ville et vos HP sont restaurés."){
                await TeleporterReaparition();
            }
            else if(data.resultat){
                tuilesChargees = tuilesChargees.filter(item => !(item.x == data.character.posX && item.y == data.character.posY));
                MettreVuePortAJour();
            }
        }
        if (!reponse.ok) {
            console.error("Erreur lors de la mise à jour de la position du personnage");
        }
    } catch (error) {
        console.error("Erreur lors de la mise à jour de la position du personnage :", error);
    }
}

async function TeleporterReaparition(){
    await ObtenirVille();

    positionJoueurGlobal.x = villeActuelleX;
    positionJoueurGlobal.y = villeActuelleY;

    MettreVuePortAJour();
    await MettrePositionAJourDB(villeActuelleX, villeActuelleY);
    ChargerInfoJoueur();
}

function ChargerFormulaireInscription(){
    formulaireInscription = document.getElementById("formulaireInscription");

    if (formulaireInscription) {
        formulaireInscription.addEventListener("submit", async (event) => {
            event.preventDefault();
            email = document.getElementById("email").value;
            nomUtilisateur = document.getElementById("nomUtilisateur").value;
            mdp = document.getElementById("mdp").value;
            var userData = {
                email, 
                mdp: mdp, 
                pseudo: nomUtilisateur,
                dateInscription: new Date().toISOString(),
                isConnected: true
            };

            try {
                reponse = await fetch(`${url}/Users/Register`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(userData)
                });

                if (reponse.ok) {
                    AfficherNotif("Inscription réussie ! Redirection vers la page de connexion...", "success");
                    setTimeout(() => {
                        window.location.href = "login.html";
                    }, 2000);
                }
                else {
                    texteErreur = await reponse.text();
                    AfficherNotif("Échec de l'inscription : " + texteErreur, "error");
                }
            } catch (error) {
                AfficherNotif("Une erreur est survenue. Veuillez réessayer.", "error");
                console.error("Erreur durant l'inscription :", error);
            }
        });
    }
}

function ChargerFormulaireAuth(){
    formulaireConnexion = document.getElementById("formulaireConnexion");
    var connexionBtn = document.getElementById("connexionBtn");

    if (formulaireConnexion) {
        formulaireConnexion.addEventListener("submit", async (event) => {
            event.preventDefault();
            email = document.getElementById("email").value;
            mdp = document.getElementById("mdp").value;

            try {
                reponse = await fetch(`${url}/Users/Login/${email}/${mdp}`, {
                    method: "GET",
                    headers: { "Content-Type": "application/json" }
                });

                if (reponse.ok) {
                    AfficherNotif("Connexion réussie !", "success");
                    localStorage.setItem("utilisateurEmail", email);
                    window.location.href = "../index.html";
                } 
                else {
                    texteErreur = await reponse.text();
                    AfficherNotif("Échec de la connexion : " + texteErreur, "error");
                }
            } catch (error) {
                console.error("Erreur durant la connexion:", error);
                AfficherNotif("Une erreur est survenue. Veuillez réessayer.", "error");
            }
        });
    }

    if(connexionBtn){
        connexionBtn.addEventListener("click", () => {
            window.location.href = "templates/login.html"
        })
    }
}

function ChargerBoutonDeconnexion(){
    var deconnexionBtn = document.getElementById("deconnexionBtn");

    if (deconnexionBtn) {
        deconnexionBtn.addEventListener("click", async () => {
            email = localStorage.getItem("utilisateurEmail");

            if (!email) {
                AfficherNotif("Aucun utilisateur connecté.", "error");
                return;
            }

            try {
                reponse = await fetch(`${url}/Users/Logout/${email}`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" }
                });

                if (reponse.ok) {
                    AfficherNotif("Déconnexion réussie ! Redirection vers la page de connexion...", "success");
                    localStorage.removeItem("utilisateurEmail");
                    setTimeout(() => {
                        window.location.href = "templates/login.html";
                    }, 2000);
                } 
                else {
                    texteErreur = await reponse.text();
                    AfficherNotif("Échec de la déconnexion : " + texteErreur, "error");
                }
            } catch (error) {
                console.error("Erreur durant la déconnexion:", error);
                AfficherNotif("Une erreur est survenue. Veuillez réessayer.", "error");
            }
        })
    }
}

async function ChargerInfoJoueur() {
    email = localStorage.getItem("utilisateurEmail"); 
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
        reponse = await fetch(`${url}/Characters/Load/${email}`, {
            method: "GET",
            headers: { "Content-Type": "application/json" }
        });

        if (reponse.ok) {
            character = await reponse.json();
            positionJoueurGlobal.x = character.posX;
            positionJoueurGlobal.y = character.posY;
            MettreVuePortAJour();

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
    MettreEstConnecteAJour();
    email = localStorage.getItem("utilisateurEmail"); 
    simulateur = document.getElementById('simulateur');
    pageActuelle = window.location.pathname.split("/").pop();
    ObtenirVille();
    RedirigerSiNonConnecte();
    ModalSiSimulateur();
    docBody = document.body;
    grille = document.getElementById("grilleTuiles");
    lienSombre = document.getElementById('modeSombre');
    lienClair = document.getElementById('modeClair');
    CreerGrille();

    if(lienSombre){
        lienSombre.addEventListener('click', (e) => {
            e.preventDefault();
            docBody.classList.add('sombre');
            localStorage.setItem('theme', 'sombre');
        });
    }

    if(lienClair){
        lienClair.addEventListener('click', (e) => {
            e.preventDefault();
            docBody.classList.remove('sombre');
            localStorage.setItem('theme', 'clair');
        });
    }

    if (localStorage.getItem('theme') === 'sombre') {
        docBody.classList.add('sombre');
    }
    
    document.getElementById("btn-up")?.addEventListener("click", () => BougerJoueur(0, -1));
    document.getElementById("btn-down")?.addEventListener("click", () => BougerJoueur(0, 1));
    document.getElementById("btn-left")?.addEventListener("click", () => BougerJoueur(-1, 0));
    document.getElementById("btn-right")?.addEventListener("click", () => BougerJoueur(1, 0));

    document.addEventListener("keydown", (e) => {

        if (["ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight"].includes(e.key)) {
            e.preventDefault();
        }

        if (e.key === "ArrowUp") BougerJoueur(0, -1);
        if (e.key === "ArrowDown") BougerJoueur(0, 1);
        if (e.key === "ArrowLeft") BougerJoueur(-1, 0);
        if (e.key === "ArrowRight") BougerJoueur(1, 0);
    });

    ChargerFormulaireInscription();
    ChargerFormulaireAuth();
    ChargerBoutonDeconnexion();
    if(pageActuelle === "index.html"){
        MettreVuePortAJour();
        ChargerInfoJoueur();
    }
});
