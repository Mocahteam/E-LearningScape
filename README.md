# Téléchargements
Deux versions du jeu sont disponibles :
1. [E-LearningScape](https://github.com/Mocahteam/E-LearningScape/releases/download/v3/E-LearningScape.zip) est la version par défaut du jeu traitant de notions généralistes sur la pédagogie
2. [E-LearningScape Access](https://github.com/Mocahteam/E-LearningScape/releases/download/v3/E-LearningScapeAccess.zip) est une version du jeu traitant de notions relatives au handicap, l'accessibilité et la société inclusive 

# E-LearningScape
E-LearningScape est un jeu sérieux sous un format d'*escape game* mêlant contenus réels et virtuels. Les joueurs incarnent des marchands de sable naviguant dans le rêve d'une personne et devant l'aider à répondre à toutes les questions qu'elle se pose avant qu'elle ne se réveille. 

<p align="center"><img src="docs/CaptureLearningScape.PNG" alt="Une vue du jeu" height="200"/> <img src="docs/collaboratif.jpg" alt="Travail collaboratif" height="200"/></p>

# Principes de jeu
E-LearningScape se joue entre 2 et 4 joueurs autour d'un même ordinateur. Le maître du jeu doit préparer quelques éléments avant la conduite de la session, il doit notamment imprimer un ensemble de documents et les placer dans des enveloppes (nous appelons ces éléments des fragments de rêves). Toutes les informations relatives à la constitution des enveloppes sont indiquées dans le fichier Lisez-moi.txt contenu dans l'archive relative à la version du jeu utilisée.

Une fois les fragments de rêve préparés, les joueurs peuvent commencer leur partie. Une seule règle est à respecter : ouvrir les fragments de rêve (enveloppes) uniquement si le jeu vous y invite à le faire.

<p align="center"><img src="docs/ouvrirFragment.png" alt="Exemple d'écran invitant les joueurs à ouvrir le fragment de rêve numéro 4" height="200"/> <img src="docs/enveloppes.jpg" alt="Enveloppes contenant les fragments de rêve" height="200"/></p>

# Informations techniques
E-LearningScape a été développé sous Unity avec la bibliothèque [FYFY](https://github.com/Mocahteam/FYFY).

# Comment créer mon propre jeu à partir d'E-LearningScape
E-LearningScape propose plusieurs mécaniques d'énigmes (relier trois points sur un tableau avec une corde, effacer les mots d'un tableau, révéler des images à l'aide d'une lampe UV...). Vous n'avez pas la possibilité de modifier ces mécaniques en revanche vous pouvez modifier les contenus qui apparaissent dans le jeu ainsi que le contenu des enveloppes pour créer votre propre version d'E-LearningScape.

Pour changer les contenus dans le jeu il vous suffit de travailler sur les fichiers qui se trouvent dans le dossier **Data** :
- S'il s'agit d'éléments textuels modifiez le fichier **Data/Data_LearningScape.txt**. Vous pourrez modifier le scénario de votre jeu, l'intitulé des questions, les réponses attendues, la description des objets ramassés...
- S'il s'agit d'éléments graphiques écrasez simplement les images présentes dans le dossier **Data** avec vos nouveaux visuels pour qu'ils soient automatiquement chargés par le jeu.
- Le fichier **Data/DreamFragmentLinks.txt** vous permet d'ajouter un lien hypertexte à chaque fragment de rêve (utile si vous souhaitez dématérialiser le contenu des enveloppes).

# Crédits
E-LearningScape a été réalisé par [Sorbonne Université](http://www.sorbonne-universite.fr/) à travers l’équipe [MOCAH](https://www.lip6.fr/recherche/team.php?acronyme=MOCAH) du LIP6, [CAPSULE](http://capsule.sorbonne-universite.fr/) et le projet Play@SU. C'est une adaptation numérique du jeu [LearningScape](https://sapiens-uspc.com/learningscape-2/) développé par la cellule [SAPIENS](https://sapiens-uspc.com/) de l'[USPC](http://www.sorbonne-paris-cite.fr/) et le [CRI](https://cri-paris.org/).
