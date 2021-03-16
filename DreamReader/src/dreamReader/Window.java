package dreamReader;

import java.awt.*;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.KeyEvent;

import javax.swing.*;
import javax.swing.event.DocumentEvent;
import javax.swing.event.DocumentListener;
import javax.swing.text.PlainDocument;

public class Window extends JFrame {

	private static final long serialVersionUID = 1L;

	private JMenuItem loadMenuButton;
	private JMenuItem saveMenuButton;
	private JMenuItem saveAsMenuButton;

	public Window() {
    	Initialize();
    }


    private void Initialize() {
    	// set window panel
    	var mainLayout = new BorderLayout();
    	var mainPanel = (JPanel) getContentPane();
    	mainPanel.setLayout(mainLayout);

    	// set menu bar
		var bar = new JMenuBar();
		mainPanel.add(bar, BorderLayout.PAGE_START);
		var fileMenu = new JMenu("Fichier");
		bar.add(fileMenu);
		 loadMenuButton = new JMenuItem("Ouvrir");
		fileMenu.add(loadMenuButton);
		loadMenuButton.addActionListener(new ActionListener() {
			@Override
			public void actionPerformed(ActionEvent e) {
				load();
			}
		});
		 saveMenuButton = new JMenuItem("Enregistrer");
		fileMenu.add(saveMenuButton);
		 saveAsMenuButton = new JMenuItem("Enregistrer sous...");
		fileMenu.add(saveAsMenuButton);
		saveAsMenuButton.addActionListener(new ActionListener() {
			@Override
			public void actionPerformed(ActionEvent e) {
				saveAs();
			}
		});

    	//set tabs panel
    	var tabbedPane = new JTabbedPane();
		mainPanel.add(tabbedPane, BorderLayout.CENTER);
    	
    	//add content panels
    	var content1 = GenerateConfigUI();
    	content1.setPreferredSize(new Dimension(900, 600));
    	content1.getVerticalScrollBar().setUnitIncrement(20);
		tabbedPane.add("Configuration", content1);
    	var content2 = GenerateHistoryUI();
    	content2.setPreferredSize(new Dimension(900, 600));
    	content2.getVerticalScrollBar().setUnitIncrement(20);
		tabbedPane.add("Histoire", content2);
    	var content3 = GenerateEnigmasUI();
    	content3.setPreferredSize(new Dimension(900, 600));
    	content3.getVerticalScrollBar().setUnitIncrement(20);
		tabbedPane.add("Enigmes", content3);
    	var content4 = GenerateDreamFragmentsUI();
    	content4.setPreferredSize(new Dimension(900, 600));
    	content4.getVerticalScrollBar().setUnitIncrement(20);
		tabbedPane.add("Fragments virtuels", content4);
    	var content5 = GenerateUITextsUI();
    	content5.setPreferredSize(new Dimension(900, 600));
    	content5.getVerticalScrollBar().setUnitIncrement(20);
		tabbedPane.add("Textes d'UI", content5);
    	
    	//last window settings
        var icon = new ImageIcon("src/resources/DreamFragment.png");
        setIconImage(icon.getImage());
        setTitle("DreamReader");
        setPreferredSize(new Dimension(900, 600));
        setDefaultCloseOperation(EXIT_ON_CLOSE);
        
        pack();
        //center the window
        setLocationRelativeTo(null);
    }

    private void load() {
		FileDialog fd = new FileDialog(this, "Choisir un fichier \"Data_LearningScape\" à ouvrir", FileDialog.LOAD);
		fd.setFile("*.txt");
		fd.setVisible(true);
		String fileName = fd.getFile();
		if(fileName != null)
			System.out.println("nom du fichier: " + fileName);
	}

	private void saveAs() {
		FileDialog fd = new FileDialog(this, "Choisir un emplacement pour sauvegarder", FileDialog.SAVE);
		fd.setFile("*.txt");
		fd.setVisible(true);
		String fileName = fd.getFile();
		if(fileName != null)
			System.out.println("nom du fichier: " + fileName);
	}


    private JScrollPane GenerateConfigUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 0, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Theme du jeu:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateToggleLine("Traces de Laalys:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateToggleLine("Système d'aide:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateToggleLine("Activation aléatoire du système d'aide:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateToggleLine("Traces vers le LRS:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateFloatInputFieldLine("Fréquence de trace des déplacements:"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateToggleLine("Fragments de rêves virtuels:"), gbc);
    	gbc.gridy = 7;
    	content.add(CreateToggleLine("Puzzles virtuels:"), gbc);
    	gbc.gridy = 8;
    	content.add(CreateToggleLine("Salle de fin:"), gbc);
    	gbc.gridy = 9;
    	content.add(CreateToggleLine("Sauvegarde et chargement:"), gbc);
    	gbc.gridy = 10;
    	content.add(CreateToggleLine("Sauvegarde automatique:"), gbc);
    	gbc.gridy = 11;
    	content.add(CreateCategory("Chemins de fichiers", GenerateFilesPathsUI()), gbc);
    	gbc.gridy = 12;
    	content.add(CreateToggleLine("RemoveExtraGeometries:"), gbc);
    	
    	return new JScrollPane(parent, JScrollPane.VERTICAL_SCROLLBAR_AS_NEEDED, JScrollPane.HORIZONTAL_SCROLLBAR_NEVER);
    }
    
    private JPanel GenerateFilesPathsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Chemin configuration LRS:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Chemin liens fragments de rêves:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Chemin documents fragments de rêves:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Chemin indices:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Chemin indices internes au jeu:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Chemin feedback de mauvaise réponse:"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Chemin poids des énigmes:"), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Chemin poids des labels:"), gbc);
    	gbc.gridy = 8;
    	content.add(CreateTextInputFieldLine("Chemin configuration système d'aide:"), gbc);
    	gbc.gridy = 9;
    	content.add(CreateTextAreaLine("Chemins de logos additionnels:"), gbc);
    	
    	return parent;
    }
    
    private JScrollPane GenerateHistoryUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 0, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextAreaLine("Texte d'introduction:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextAreaLine("Texte de transition de la salle 1 à la salle 2:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextAreaLine("Texte de fin:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextAreaLine("Crédits additionnels:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Texte de score:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Texte d'explication final:"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Lien final:"), gbc);
    	gbc.gridy = 7;
    	content.add(CreateToggleLine("Concaténer l'id de session au lien:"), gbc);
    	
    	
    	return new JScrollPane(parent, JScrollPane.VERTICAL_SCROLLBAR_AS_NEEDED, JScrollPane.HORIZONTAL_SCROLLBAR_NEVER);
    }
    
    private JScrollPane GenerateEnigmasUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 0, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateCategory("Introduction", GenerateIntroEnigmaUI()), gbc);
    	gbc.gridy = 1;
    	content.add(CreateCategory("Boite de balles", GenerateBallBoxEnigmaUI()), gbc);
    	gbc.gridy = 2;
    	content.add(CreateCategory("Tableau et corde", GeneratePlankWireEnigmaUI()), gbc);
    	gbc.gridy = 3;
    	content.add(CreateCategory("Table et chaises", GenerateCrouchEnigmaUI()), gbc);
    	gbc.gridy = 4;
    	content.add(CreateCategory("Engrenages", GenerateGearsEnigmaUI()), gbc);
    	gbc.gridy = 5;
    	content.add(CreateCategory("Mastermind", GenerateMastermindEnigmaUI()), gbc);
    	gbc.gridy = 6;
    	content.add(CreateCategory("Lunettes", GenerateGlassesEnigmaUI()), gbc);
    	gbc.gridy = 7;
    	content.add(CreateCategory("Enigme 8", GenerateEnigma8UI()), gbc);
    	gbc.gridy = 8;
    	content.add(CreateCategory("Parchemins", GenerateScrollsEnigmaUI()), gbc);
    	gbc.gridy = 9;
    	content.add(CreateCategory("Miroir", GenerateMirrorEnigmaUI()), gbc);
    	gbc.gridy = 10;
    	content.add(CreateCategory("Enigme 11", GenerateEnigma11UI()), gbc);
    	gbc.gridy = 11;
    	content.add(CreateCategory("Enigme 12", GenerateEnigma12UI()), gbc);
    	gbc.gridy = 12;
    	content.add(CreateCategory("Verrou salle 2", GenerateLockRoom2UI()), gbc);
    	gbc.gridy = 13;
    	content.add(CreateCategory("Puzzles", GeneratePuzzlesEnigmaUI()), gbc);
    	gbc.gridy = 14;
    	content.add(CreateCategory("Lampe", GenerateLampEnigmaUI()), gbc);
    	gbc.gridy = 15;
    	content.add(CreateCategory("Tableau effaçable", GenerateWhiteBoardEnigmaUI()), gbc);
    	gbc.gridy = 16;
    	content.add(CreateCategory("Enigme 16", GenerateEnigma16UI()), gbc);
    	
    	return new JScrollPane(parent, JScrollPane.VERTICAL_SCROLLBAR_AS_NEEDED, JScrollPane.HORIZONTAL_SCROLLBAR_NEVER);
    }
    
    private JPanel GenerateIntroEnigmaUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Titre du parchemin dans l'inventaire:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Description du parchemin dans l'inventaire:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Indication pour le parchemin dans l'inventaire:"), gbc);
    	
    	return parent;
    }
    
    private JPanel GenerateBallBoxEnigmaUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateToggleLine("Position aléatoire des balles:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Réponses attendues dans l'IAR:", 3), gbc);
    	gbc.gridy = 6;
    	content.add(CreateIntInputFieldLine("Numéro des balles solutions:", 3), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Textes des balles:", 10), gbc);
    	gbc.gridy = 8;
    	content.add(CreateTextInputFieldLine("Titre de la clé dans l'inventaire:"), gbc);
    	gbc.gridy = 9;
    	content.add(CreateTextInputFieldLine("Description de la clé dans l'inventaire:"), gbc);
    	gbc.gridy = 10;
    	content.add(CreateTextInputFieldLine("Indication pour la clé dans l'inventaire:"), gbc);
    	
    	return parent;
    }

    private JPanel GeneratePlankWireEnigmaUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Enoncé affiché sur le tableau en jeu:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Réponses attendues dans l'IAR:", 3), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Mauvaises réponses affichées sur le tableau:", 6), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Mots solutions sur le tableau:", 3), gbc);
    	gbc.gridy = 8;
    	content.add(CreateTextInputFieldLine("Mauvais mots affichés sur le tableau:", 10), gbc);
    	gbc.gridy = 9;
    	content.add(CreateTextInputFieldLine("Titre de la corde dans l'inventaire:"), gbc);
    	gbc.gridy = 10;
    	content.add(CreateTextInputFieldLine("Description de la corde dans l'inventaire:"), gbc);
    	gbc.gridy = 11;
    	content.add(CreateTextInputFieldLine("Indication pour la corde dans l'inventaire:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateCrouchEnigmaUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Réponse attendue:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Textes des fragments de rêves:", 6), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Liens des fragments de rêves:", 6), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Textes des boutons vers les liens des fragments de rêves:", 6), gbc);
    	
    	return parent;
    }

    private JPanel GenerateGearsEnigmaUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Question de l'énigme:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Indication pour l'énigme:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Texte de l'engrenage du haut:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Texte de l'engranage du bas:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Textes des engrenages déplaçables:", 4), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Réponse attendue:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateMastermindEnigmaUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Question du mastermind:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Intitulé du champ de réponse:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Texte du boutton de validation:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateFloatInputFieldLine("Position Y de la question sur l'objet dans la scène:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateIntInputFieldLine("Réponse attendue:"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Chemin du fichier image du background du mastermind:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateGlassesEnigmaUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Chemins des fichiers image pour le sac:", 4), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Titre de la clé du sac dans l'inventaire:"), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Description de la clé du sac dans l'inventaire:"), gbc);
    	gbc.gridy = 8;
    	content.add(CreateTextInputFieldLine("Indication pour la clé du sac dans l'inventaire:"), gbc);
    	gbc.gridy = 9;
    	content.add(CreateTextInputFieldLine("Titre de la lunette rouge (droite) dans l'inventaire:"), gbc);
    	gbc.gridy = 10;
    	content.add(CreateTextInputFieldLine("Description de la lunette rouge (droite) dans l'inventaire:"), gbc);
    	gbc.gridy = 11;
    	content.add(CreateTextInputFieldLine("Indication pour la lunette rouge (droite) dans l'inventaire:"), gbc);
    	gbc.gridy = 12;
    	content.add(CreateTextInputFieldLine("Titre de la lunette jaune (gauche) dans l'inventaire:"), gbc);
    	gbc.gridy = 13;
    	content.add(CreateTextInputFieldLine("Description de la lunette jaune (gauche) dans l'inventaire:"), gbc);
    	gbc.gridy = 14;
    	content.add(CreateTextInputFieldLine("Indication pour la lunette jaune (gauche) dans l'inventaire:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateEnigma8UI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateScrollsEnigmaUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Textes des parchemins:", 5), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Titre de l'ensemble des parchemins dans l'inventaire:"), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Description de l'ensemble des parchemins dans l'inventaire:"), gbc);
    	gbc.gridy = 8;
    	content.add(CreateTextInputFieldLine("Indication pour l'ensemble des parchemins dans l'inventaire:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateMirrorEnigmaUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Chemin du fichier image pour la planche:"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Titre du miroir dans l'inventaire:"), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Description du miroir dans l'inventaire:"), gbc);
    	gbc.gridy = 8;
    	content.add(CreateTextInputFieldLine("Indication pour le miroir dans l'inventaire:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateEnigma11UI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateEnigma12UI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateLockRoom2UI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateIntInputFieldLine("Mot de passe:"), gbc);
    	
    	return parent;
    }

    private JPanel GeneratePuzzlesEnigmaUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Chemin de l'image pour le puzzle:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Titre du puzzle dans l'inventaire:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Description du puzzle dans l'inventaire:"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Indication pour le puzzle dans l'inventaire:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateLampEnigmaUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Chemins des images éclairables par la lampe:", 6), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Titre de la lampe dans l'inventaire:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Description de la lampe dans l'inventaire:"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Indication pour la lampe dans l'inventaire:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateWhiteBoardEnigmaUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Liste des textes du tableau:", 12), gbc);
    	
    	return parent;
    }

    private JPanel GenerateEnigma16UI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:"), gbc);
    	
    	return parent;
    }

    private JScrollPane GenerateDreamFragmentsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 0, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateCategory("Introduction", GenerateIntroductionDreamFragmentsUI()), gbc);
    	gbc.gridy = 1;
    	content.add(CreateCategory("Salle 1", GenerateRoom1DreamFragmentsUI()), gbc);
    	gbc.gridy = 2;
    	content.add(CreateCategory("Salle 2", GenerateRoom2DreamFragmentsUI()), gbc);
    	gbc.gridy = 3;
    	content.add(CreateCategory("Salle 3", GenerateRoom3DreamFragmentsUI()), gbc);
    	
    	return new JScrollPane(parent, JScrollPane.VERTICAL_SCROLLBAR_AS_NEEDED, JScrollPane.HORIZONTAL_SCROLLBAR_NEVER);
    }

    private JPanel GenerateIntroductionDreamFragmentsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateDreamFragmentCategory(0), gbc);
    	
    	return parent;
    }

    private JPanel GenerateRoom1DreamFragmentsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateDreamFragmentCategory(4, "table et chaises"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateDreamFragmentCategory(9, "tableau et corde"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateDreamFragmentCategory(17, "tableau et corde"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateRoom2DreamFragmentsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateDreamFragmentCategory(1, "énigme 8"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateDreamFragmentCategory(2, "énigme 11"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateDreamFragmentCategory(3, "énigme 12"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateDreamFragmentCategory(5, "énigme 8"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateDreamFragmentCategory(8, "énigme 8"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateDreamFragmentCategory(11, "énigme 12"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateDreamFragmentCategory(12, "énigme 12"), gbc);
    	gbc.gridy = 7;
    	content.add(CreateDreamFragmentCategory(14, "puzzles"), gbc);
    	gbc.gridy = 8;
    	content.add(CreateDreamFragmentCategory(15, "puzzles"), gbc);
    	gbc.gridy = 9;
    	content.add(CreateDreamFragmentCategory(18, "puzzles"), gbc);
    	
    	return parent;
    }
    
    private JPanel GenerateRoom3DreamFragmentsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateDreamFragmentCategory(6, "lampe"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateDreamFragmentCategory(7, "puzzles"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateDreamFragmentCategory(10, "énigme 16"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateDreamFragmentCategory(13, "énigme 16"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateDreamFragmentCategory(16, "puzzles"), gbc);
    	
    	return parent;
    }

    private JScrollPane GenerateUITextsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 0, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateCategory("Menu principal", GenerateMainMenuUITextsUI()), gbc);
    	gbc.gridy = 1;
    	content.add(CreateCategory("Options", GenerateOptionsUITextsUI()), gbc);
    	gbc.gridy = 2;
    	content.add(CreateCategory("Fenêtres de sauvegarde et de chargement", GenerateSaveAndLoadUITextsUI()), gbc);
    	gbc.gridy = 3;
    	content.add(CreateCategory("Histoire", GenerateStoryReadingUITextsUI()), gbc);
    	gbc.gridy = 4;
    	content.add(CreateCategory("HUD", GenerateHUDUITextsUI()), gbc);
    	gbc.gridy = 5;
    	content.add(CreateCategory("Fragments de rêves", GenerateDreamFragmentsUITextsUI()), gbc);
    	gbc.gridy = 6;
    	content.add(CreateCategory("Onglets IAR", GenerateIARTabsUITextsUI()), gbc);
    	gbc.gridy = 7;
    	content.add(CreateCategory("Questions", GenerateQuestionsUITextsUI()), gbc);
    	gbc.gridy = 8;
    	content.add(CreateCategory("Indices", GenerateHintsUITextsUI()), gbc);
    	gbc.gridy = 9;
    	content.add(CreateCategory("Menu en jeu", GenerateGameMenuUITextsUI()), gbc);
    	gbc.gridy = 10;
    	content.add(CreateCategory("Textes tutoriel", GenerateTutorialUITextsUI()), gbc);
    	gbc.gridy = 11;
    	content.add(CreateCategory("Commandes", GenerateInputsUITextsUI()), gbc);
    	
    	return new JScrollPane(parent, JScrollPane.VERTICAL_SCROLLBAR_AS_NEEDED, JScrollPane.HORIZONTAL_SCROLLBAR_NEVER);
    }

    private JPanel GenerateMainMenuUITextsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Chargement:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Commencer:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Charger:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Options:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Quitter:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Intitulé de l'ID de session:"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Indication pour l'ID de session:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateOptionsUITextsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Paramètres de contrôles:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Paramètres de son:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Paramètres d'affichage:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Bouton de validation des Paramètres:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Bouton de validation des sous catégories:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Bouton de réinitialisation des Paramètres:"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateCategory("Options de contrôles", GenerateControlOptionsUITextsUI()), gbc);
    	gbc.gridy = 7;
    	content.add(CreateCategory("Options de son", GenerateSoundOptionsUITextsUI()), gbc);
    	gbc.gridy = 8;
    	content.add(CreateCategory("Options d'affichage", GenerateDisplayOptionsUITextsUI()), gbc);
    	
    	return parent;
    }

    private JPanel GenerateControlOptionsUITextsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Fragments de rêves virtuels:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Textes en mouvement:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Vitesse de déplacement:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Sensibilité de la caméra:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Vitesse des roues des verrous:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Commandes:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateSoundOptionsUITextsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Volume de la musique:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Volume des effets sonores:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateDisplayOptionsUITextsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Police accessible:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Taille du viseur:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Intensité lumineuse:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Effet de transparence:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateSaveAndLoadUITextsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateCategory("Popup de chargement", GenerateLoadUITextsUI()), gbc);
    	gbc.gridy = 1;
    	content.add(CreateCategory("Popup de sauvegarde", GenerateSaveUITextsUI()), gbc);
    	
    	return parent;
    }
    
    private JPanel GenerateLoadUITextsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Bouton de chargement:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Bouton d'annulation:"), gbc);
    	
    	return parent;
    }
    
    private JPanel GenerateSaveUITextsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Bouton de sauvegarde:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Bouton d'annulation:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Placeholder pour le nom du fichier:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Indication pour un nom invalide:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Bouton de validation de l'indication pour un nom invalide:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Indication pour écraser un fichier:"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Bouton de validation pour écraser un fichier:"), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Bouton d'annulation pour écraser un fichier:"), gbc);
    	gbc.gridy = 8;
    	content.add(CreateTextInputFieldLine("Indication de sauvegarde réussie:"), gbc);
    	gbc.gridy = 9;
    	content.add(CreateTextInputFieldLine("Bouton de validation de sauvegarde réussie:"), gbc);
    	
    	return parent;
    }
    
    private JPanel GenerateStoryReadingUITextsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Indication pour continuer la lecture:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Bouton d'ouverture de lien sur la page finale:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Bouton quitter sur la page finale:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateHUDUITextsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Observation:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Déplacement:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Se baisser:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Inventaire:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Fragments de rêves:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Questions:"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Aide:"), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Menu:"), gbc);
    	
    	return parent;
    }
    
    private JPanel GenerateDreamFragmentsUITextsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Texte de la fenêtre de collection de fragment:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Bouton de validation de la fenêtre de collection de fragment:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Bouton d'ouverture de lien d'un fragment:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Bouton de réinitialisation d'un fragment dans l'IAR:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateIARTabsUITextsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Inventaire:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Fragments de rêves:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Questions salle 1:"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Questions salle 2:"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Questions salle 3:"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Aide:"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Menu:"), gbc);
    	
    	return parent;
    }
    
    private JPanel GenerateQuestionsUITextsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Bouton de validation des questions:"), gbc);
    	
    	return parent;
    }

    private JPanel GenerateHintsUITextsUI() {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Bouton d'indice:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Bouton de demande d'indice:"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Bouton d'ouverture de lien d'un indice:"), gbc);
    	
    	return parent;
    }

	private JPanel GenerateGameMenuUITextsUI() {
		var parent = new JPanel(new BorderLayout());
		var content = new JPanel(new GridBagLayout());
		content.setSize(900, 20);
		parent.add(content, BorderLayout.PAGE_START);
		GridBagConstraints gbc = new GridBagConstraints();

		gbc.fill = GridBagConstraints.HORIZONTAL;
		gbc.gridx = 0;
		gbc.gridy = 0;
		gbc.insets = new Insets(5, 25, 0, 0);
		gbc.weightx = 1;
		content.add(CreateTextInputFieldLine("Bouton reprendre:"), gbc);
		gbc.gridy = 1;
		content.add(CreateTextInputFieldLine("Bouton de sauvegarde:"), gbc);
		gbc.gridy = 2;
		content.add(CreateTextInputFieldLine("Indication pour le bouton de sauvegarde:"), gbc);
		gbc.gridy = 3;
		content.add(CreateTextInputFieldLine("Bouton d'options:"), gbc);
		gbc.gridy = 4;
		content.add(CreateTextInputFieldLine("Bouton de retour au menu:"), gbc);
		gbc.gridy = 5;
		content.add(CreateTextInputFieldLine("Bouton quitter:"), gbc);

		return parent;
	}

	private JPanel GenerateTutorialUITextsUI() {
		var parent = new JPanel(new BorderLayout());
		var content = new JPanel(new GridBagLayout());
		content.setSize(900, 20);
		parent.add(content, BorderLayout.PAGE_START);
		GridBagConstraints gbc = new GridBagConstraints();

		gbc.fill = GridBagConstraints.HORIZONTAL;
		gbc.gridx = 0;
		gbc.gridy = 0;
		gbc.insets = new Insets(5, 25, 0, 0);
		gbc.weightx = 1;
		content.add(CreateTextInputFieldLine("Texte 0 du tutoriel:"), gbc);
		gbc.gridy = 1;
		content.add(CreateTextInputFieldLine("Texte 1 du tutoriel:"), gbc);
		gbc.gridy = 2;
		content.add(CreateTextInputFieldLine("Texte 2 du tutoriel:"), gbc);
		gbc.gridy = 3;
		content.add(CreateTextInputFieldLine("Texte 3 du tutoriel:"), gbc);
		gbc.gridy = 4;
		content.add(CreateTextInputFieldLine("Texte 4 du tutoriel:"), gbc);

		return parent;
	}

	private JPanel GenerateInputsUITextsUI() {
		var parent = new JPanel(new BorderLayout());
		var content = new JPanel(new GridBagLayout());
		content.setSize(900, 20);
		parent.add(content, BorderLayout.PAGE_START);
		GridBagConstraints gbc = new GridBagConstraints();

		gbc.fill = GridBagConstraints.HORIZONTAL;
		gbc.gridx = 0;
		gbc.gridy = 0;
		gbc.insets = new Insets(5, 25, 0, 0);
		gbc.weightx = 1;
		content.add(CreateTextInputFieldLine("Titre de la colonne 1:"), gbc);
		gbc.gridy = 1;
		content.add(CreateTextInputFieldLine("Titre de la colonne 2:"), gbc);
		gbc.gridy = 2;
		content.add(CreateTextInputFieldLine("Titre de la colonne 3:"), gbc);
		gbc.gridy = 3;
		content.add(CreateInputCategory("Rotation de la caméra"), gbc);
		gbc.gridy = 4;
		content.add(CreateInputCategory("Avancer"), gbc);
		gbc.gridy = 5;
		content.add(CreateInputCategory("Reculer"), gbc);
		gbc.gridy = 6;
		content.add(CreateInputCategory("Daplacement à gauche"), gbc);
		gbc.gridy = 7;
		content.add(CreateInputCategory("Déplacement à droite"), gbc);
		gbc.gridy = 8;
		content.add(CreateInputCategory("Se baisser"), gbc);
		gbc.gridy = 9;
		content.add(CreateInputCategory("Interaction"), gbc);
		gbc.gridy = 10;
		content.add(CreateInputCategory("Inventaire"), gbc);
		gbc.gridy = 11;
		content.add(CreateInputCategory("Fragments de rêves"), gbc);
		gbc.gridy = 12;
		content.add(CreateInputCategory("Questions"), gbc);
		gbc.gridy = 13;
		content.add(CreateInputCategory("Aide"), gbc);
		gbc.gridy = 14;
		content.add(CreateInputCategory("Menu"), gbc);
		gbc.gridy = 15;
		content.add(CreateInputCategory("Changer de mode de vue"), gbc);
		gbc.gridy = 16;
		content.add(CreateInputCategory("Afficher de la cible"), gbc);
		gbc.gridy = 17;
		content.add(CreateInputCategory("Zoomer"), gbc);
		gbc.gridy = 18;
		content.add(CreateInputCategory("Dézoomer"), gbc);

		return parent;
	}


    private JPanel CreateCategory(String label, JPanel content) {
    	var panel = new JPanel(new BorderLayout());
    	
    	var button = new JToggleButton();
    	button.setText(label);
    	button.addActionListener(new ActionListener() {
			
			@Override
			public void actionPerformed(ActionEvent arg0) {
				AbstractButton b = (AbstractButton) arg0.getSource();
				content.setVisible(b.getModel().isSelected());
			}
		});
    	panel.add(button, BorderLayout.PAGE_START);
    	
    	content.setVisible(false);
    	panel.add(content, BorderLayout.CENTER);
    	
    	return panel;
    }

    private JPanel CreateDreamFragmentCategory(int dreamFragmentID) {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Lien du fragment de réve:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Texte du bouton vers le lien:"), gbc);
    	
    	return CreateCategory("Fragment de réve " + dreamFragmentID, parent);
    }

    private JPanel CreateDreamFragmentCategory(int dreamFragmentID, String dreamFragmentDescription) {
    	var parent = new JPanel(new BorderLayout());
    	var content = new JPanel(new GridBagLayout());
    	content.setSize(900, 20);
    	parent.add(content, BorderLayout.PAGE_START);
    	GridBagConstraints gbc = new GridBagConstraints();

    	gbc.fill = GridBagConstraints.HORIZONTAL;
    	gbc.gridx = 0;
    	gbc.gridy = 0;
    	gbc.insets = new Insets(5, 25, 0, 0);
    	gbc.weightx = 1;
    	content.add(CreateTextInputFieldLine("Lien du fragment de réve:"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Texte du bouton vers le lien:"), gbc);
    	
    	return CreateCategory("Fragment de réve " + dreamFragmentID + " (" + dreamFragmentDescription + ")", parent);
    }

	private JPanel CreateInputCategory(String label) {
		var parent = new JPanel(new BorderLayout());
		var content = new JPanel(new GridBagLayout());
		content.setSize(900, 20);
		parent.add(content, BorderLayout.PAGE_START);
		GridBagConstraints gbc = new GridBagConstraints();

		gbc.fill = GridBagConstraints.HORIZONTAL;
		gbc.gridx = 0;
		gbc.gridy = 0;
		gbc.insets = new Insets(5, 25, 0, 0);
		gbc.weightx = 1;
		content.add(CreateTextInputFieldLine("Titre:"), gbc);
		gbc.gridy = 1;
		content.add(CreateTextInputFieldLine("Touche 1:"), gbc);
		gbc.gridy = 2;
		content.add(CreateTextInputFieldLine("Touche 2:"), gbc);
		gbc.gridy = 3;
		content.add(CreateTextInputFieldLine("Touche 3:"), gbc);

		return CreateCategory(label, parent);
	}
    
    private JPanel CreateTextInputFieldLine(String label) {

    	var layout = new GridLayout(1, 2);
    	layout.setHgap(5);
    	var line = new JPanel();
    	line.setLayout(layout);
    	
    	var labelPane = new JTextPane();
    	labelPane.setText(label);
    	labelPane.setEditable(false);
    	labelPane.setBackground(new Color(153, 204, 255));
    	
    	var inputfield = new JTextField();
    	
    	line.add(labelPane);
    	line.add(inputfield);
    	return line;
    }
    
    private JPanel CreateTextInputFieldLine(String label, int numberOfInputfield) {

    	var layout = new GridLayout(1, 2);
    	layout.setHgap(5);
    	var line = new JPanel();
    	line.setLayout(layout);
    	
    	var labelPane = new JTextPane();
    	labelPane.setText(label);
    	labelPane.setEditable(false);
    	labelPane.setBackground(new Color(153, 204, 255));
    	
    	var inputfields = new JPanel();
    	var inputfieldsLayout = new GridLayout(numberOfInputfield, 1);
    	inputfieldsLayout.setVgap(5);
    	inputfields.setLayout(inputfieldsLayout);
    	for(int i = 0; i < numberOfInputfield; i++)
    		inputfields.add(new JTextField());
    	
    	line.add(labelPane);
    	line.add(inputfields);
    	return line;
    }
    
    private JPanel CreateFloatInputFieldLine(String label) {

    	var layout = new GridLayout(1, 2);
    	layout.setHgap(5);
    	var line = new JPanel();
    	line.setLayout(layout);
    	
    	var labelPane = new JTextPane();
    	labelPane.setText(label);
    	labelPane.setEditable(false);
    	labelPane.setBackground(new Color(153, 204, 255));
    	
    	var inputfield = new JTextField();
    	PlainDocument doc = (PlainDocument) inputfield.getDocument();
    	doc.setDocumentFilter(new FloatFilter());
    	
    	line.add(labelPane);
    	line.add(inputfield);
    	return line;
    }
    
    private JPanel CreateIntInputFieldLine(String label) {

    	var layout = new GridLayout(1, 2);
    	layout.setHgap(5);
    	var line = new JPanel();
    	line.setLayout(layout);
    	
    	var labelPane = new JTextPane();
    	labelPane.setText(label);
    	labelPane.setEditable(false);
    	labelPane.setBackground(new Color(153, 204, 255));
    	
    	var inputfield = new JTextField();
    	PlainDocument doc = (PlainDocument) inputfield.getDocument();
    	doc.setDocumentFilter(new IntFilter());
    	
    	line.add(labelPane);
    	line.add(inputfield);
    	return line;
    }
    
    private JPanel CreateIntInputFieldLine(String label, int numberOfInputfield) {

    	var layout = new GridLayout(1, 2);
    	layout.setHgap(5);
    	var line = new JPanel();
    	line.setLayout(layout);
    	
    	var labelPane = new JTextPane();
    	labelPane.setText(label);
    	labelPane.setEditable(false);
    	labelPane.setBackground(new Color(153, 204, 255));
    	
    	var inputfields = new JPanel();
    	var inputfieldsLayout = new GridLayout(numberOfInputfield, 1);
    	inputfieldsLayout.setVgap(5);
    	inputfields.setLayout(inputfieldsLayout);
    	JTextField inputfield = null;
    	for(int i = 0; i < numberOfInputfield; i++) {
    		inputfield = new JTextField();
	    	PlainDocument doc = (PlainDocument) inputfield.getDocument();
	    	doc.setDocumentFilter(new IntFilter());
    		inputfields.add(inputfield);
    	}
    	
    	line.add(labelPane);
    	line.add(inputfields);
    	return line;
    }
    
    private JPanel CreateTextAreaLine(String label) {
		// create category button
		var panel = new JPanel(new BorderLayout());
		var parent = new JPanel(new BorderLayout());

		var button = new JToggleButton();
		button.setText(label);
		button.addActionListener(new ActionListener() {

			@Override
			public void actionPerformed(ActionEvent arg0) {
				AbstractButton b = (AbstractButton) arg0.getSource();
				parent.setVisible(b.getModel().isSelected());
			}
		});
		panel.add(button, BorderLayout.PAGE_START);

		parent.setVisible(false);
		panel.add(parent, BorderLayout.CENTER);

		// create content panel
		var content = new JPanel(new GridBagLayout());
		content.setSize(900, 20);
		parent.add(content, BorderLayout.PAGE_START);
		GridBagConstraints gbc = new GridBagConstraints();

		gbc.fill = GridBagConstraints.HORIZONTAL;
		gbc.gridx = 0;
		gbc.gridy = 0;
		gbc.insets = new Insets(5, 25, 0, 0);
		gbc.weightx = 1;

		// create size inputfield line
		var layout = new GridLayout(1, 2);
		layout.setHgap(5);
		var line = new JPanel();
		line.setLayout(layout);
		content.add(line, gbc);

		var labelPane = new JTextPane();
		labelPane.setText("Nombre de champs:");
		labelPane.setEditable(false);
		labelPane.setBackground(new Color(153, 204, 255));

		var inputfield = new JTextField();
		PlainDocument doc = (PlainDocument) inputfield.getDocument();
		doc.setDocumentFilter(new IntFilter(2));

		line.add(labelPane);
		line.add(inputfield);

		// create content input field lines
		gbc.gridy = 1;
		var line2 = new JPanel();
		line2.setLayout(layout);
		content.add(line2, gbc);

		var labelPane2 = new JTextPane();
		labelPane2.setText("Contenu:");
		labelPane2.setEditable(false);
		labelPane2.setBackground(new Color(153, 204, 255));

		var inputfields = new JPanel();

		line2.add(labelPane2);
		line2.add(inputfields);

		// add listener to size inputfield
		doc.addDocumentListener(new InputFieldGenerator(inputfield, inputfields));

		return panel;
    }
    
    private JPanel CreateToggleLine(String label) {

    	var layout = new GridLayout(1, 2);
    	layout.setHgap(5);
    	var line = new JPanel();
    	line.setLayout(layout);
    	
    	var labelPane = new JTextPane();
    	labelPane.setText(label);
    	labelPane.setEditable(false);
    	labelPane.setBackground(new Color(153, 204, 255));
    	
    	var togglePane = new JPanel();
    	var toggle = new JCheckBox();
    	togglePane.add(toggle);
    	
    	line.add(labelPane);
    	line.add(togglePane);
    	return line;
    }


    public static void main(String[] args) {
    	//queue this event in the swing event queue
        EventQueue.invokeLater(() -> {
            var window = new Window();
            window.setVisible(true);
        });
    }
}
