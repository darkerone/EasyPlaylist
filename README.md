# EasyPlaylist

## Problématique
Avoir une playlist sous forme de dossiers Windows peut être compliqué à gérer. 
Cette playlist est souvent copiée sur le téléphone ou une clé USB pour la voiture.
Il faut ajouter régulièrement de nouvelles musiques et en retirer d'autres.
On se demande alors où mettre les musiques retirées car on ne veux pas forcement les supprimer.

Et tout se complique lorsque l'on a plusieurs playlist à gérer avec des arborescences différentes.

## Présentation
EsayPlaylist permet de créer ses propres playlists et de les exporter sous forme d'arborescence de dossiers Windows.

Sur la partie gauche de l'écran, l'arborescence des dossiers du disque dur. Cette arborescence sera eppelée "Explorer".

Sur la partie droite, une "Playlist" par onglet. Chaque playlist représente une arborescence qui pourra être exportée sous forme de dossiers Windows contenant les musiques (une copie des musiques original est donc réalisée).

Il suffit d'ajouter les dossiers de gauche à droite puis d'exporter la playlist vers un dossier au choix (mobile, clé usb,...)

## Fonctionnalités
- Général
  - Bouton de sélection d'un dossier Windows à explorer
  - Bouton de création d'une playlist
  - Bouton de suppression d'une playlist
  - Bouton de copie d'une playlist
  - Bouton de de sauvegarde des playlists
  - Bouton d'ajout de l'élément sélectionné de l'explorer dans l'élément sélectionné de la playlist
  - Au lancement, notification des fichiers présents dans les playlists mais absents du disque dur
  - A la fermeture, demande de sauvegarde si une playlist a été modifiée
  - Drag & Drop des fichiers/dossiers
    - De l'explorer vers la playlist : copie
    - De la playlist vers la playlist : déplacement
- Explorer
  - Bouton de mise à jour de l'explorer
  - Bouton de mise en avant des doublons
  - Bouton de mise en avant des fichiers récents
  - Bouton de réduction de tous les dossiers
  - Champ de recherche
  - Surlignage des fichiers récents
  - Paramétrage
    - Définition d'un fichier récent. Il est possible de choisir le nombre d'année, mois, jour, heure à compter d'aujourd'hui qui seront comptés pour définir si un fichier est récent ou non. La date analysée est la date à laquelle EasyPlaylist a eu connaissance de l'ajout du fichier.
    - Activation/désactivation de la mise à jour peut être automatique de l'explorer. Attention, cela peut réduire les performances
  - Coloration des dossiers/fichiers en fonction de leur présence dans la playlist sélectionnée
    - Rouge : le fichier n'est pas présent dans la playlist sélectionnée. Le dossier ne contient aucun élément présent dans la playlist sélectionnée
    - Jaune : le dossier contient au moins un élément présent dans la playlist sélectionnée
    - Vert : le fichier est présent dans la playlist sélectionnée. Le dossier ne contient que des éléments présents dans la playlist sélectionnée
  - Menu contextuel (clic droit sur un élément)
    - Bouton d'ajout de l'élément à la playlist
    - Bouton de recherche de l'élément dans la playlist
  - Sélection multiple
- Playlist
  - Bouton d'ajout d'un nouveau dossier à la playlist
  - Bouton de renommage de l'élément sélectionné
  - Bouton de suppression de l'élément sélectionné
  - Bouton de mise en avant des doublons
  - Bouton de mise en avant des fichiers en erreur (absents du disque dur)
  - Bouton de réduction de tous les dossiers
  - Champ de recherche
  - Bouton d'export de la playlist
  - Paramétrage
    - Nom
    - Export applati : récupère tous les fichiers de la playlist et les place dans le même dossier, sans arborescence
  - Coloration en rouge des fichiers absents du disque dur
  - Menu contextuel
    - Bouton de renommage de l'élément sélectionné
    - Bouton de recherche de l'élément dans l'explorer
    - Bouton de suppression de l'élément de la playlist
  - Sélection multiple
  
## Fonctionnalités à implémenter
- Undu/Redo
- Sauvegarder les playlists individuellement
- Export en mélangeant les fichiers (utiliser MixerAC)
