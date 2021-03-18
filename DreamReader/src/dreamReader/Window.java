package dreamReader;

import java.awt.BorderLayout;
import java.awt.Color;
import java.awt.Dimension;
import java.awt.EventQueue;
import java.awt.FileDialog;
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.GridLayout;
import java.awt.Insets;

import javax.swing.*;
import javax.swing.text.PlainDocument;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.io.FileReader;

import java.io.FileWriter;
import java.io.Serial;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;

public class Window extends JFrame {

	@Serial
	private static final long serialVersionUID = 1L;

	private GameContent gameContent;
	private HashMap<String, List<String>> dreamFragmentsLinks;
	private final HashMap<String, List<JComponent>> jsonKeyToComponents;

	private String loadedFilePath;

	public Window() {
		jsonKeyToComponents = new HashMap<>();
		dreamFragmentsLinks = new HashMap<>();
    	Initialize();
    }


    private void Initialize() {
		jsonKeyToComponents.clear();
		dreamFragmentsLinks.clear();

    	// set window panel
    	var mainLayout = new BorderLayout();
    	var mainPanel = (JPanel) getContentPane();
    	mainPanel.setLayout(mainLayout);

    	// set menu bar
		var bar = new JMenuBar();
		mainPanel.add(bar, BorderLayout.PAGE_START);
		var fileMenu = new JMenu("Fichier");
		bar.add(fileMenu);
		var loadMenuButton = new JMenuItem("Ouvrir");
		fileMenu.add(loadMenuButton);
		loadMenuButton.addActionListener(e -> load());
		var saveMenuButton = new JMenuItem("Enregistrer");
		fileMenu.add(saveMenuButton);
		saveMenuButton.addActionListener(e -> save());
		var saveAsMenuButton = new JMenuItem("Enregistrer sous...");
		fileMenu.add(saveAsMenuButton);
		saveAsMenuButton.addActionListener(e -> saveAs());

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
		if(fileName != null) {
            // load json into gameContent
			GameContent loadedGameContent = null;
            Gson gson = new Gson();
            try {
				loadedGameContent = gson.fromJson(new FileReader(fd.getDirectory()+fd.getFile()), GameContent.class);
			}
            catch(Exception e){
            	e.printStackTrace();
			}

            if(loadedGameContent != null){
            	gameContent = loadedGameContent;
            	loadedFilePath = fd.getDirectory()+fd.getFile();
            	setTitle("DreamReader - "+ loadedFilePath);

				//	load dream fragments links from json
				if(gameContent.dreamFragmentLinksPath != null && gameContent.dreamFragmentLinksPath.length() != 0){
					try{
						dreamFragmentsLinks = null;
						dreamFragmentsLinks = gson.fromJson(new FileReader(gameContent.dreamFragmentLinksPath), HashMap.class);
					}
					catch (Exception e){
						e.printStackTrace();
					}
				}

				// load gameContent into the window
				loadConfigContent();
				loadHistoryContent();
				loadEnigmasContent();
				loadInventoryContent();
				if(dreamFragmentsLinks != null)
					loadDreamFragmentsContent();
				loadUIContent();
				loadCommandsContent();
			}
        }
	}

	private void loadConfigContent(){
		JTextField text;
		JCheckBox toggle;
		int length;

		text = (JTextField) jsonKeyToComponents.get("theme").get(0);
		text.setText(gameContent.theme);
		text.setColumns(1);
		toggle = (JCheckBox) jsonKeyToComponents.get("randomHelpSystemActivation").get(0);
		toggle.setSelected(gameContent.randomHelpSystemActivation);
		toggle = (JCheckBox) jsonKeyToComponents.get("helpSystem").get(0);
		toggle.setSelected(gameContent.helpSystem);
		toggle = (JCheckBox) jsonKeyToComponents.get("trace").get(0);
		toggle.setSelected(gameContent.trace);
		toggle = (JCheckBox) jsonKeyToComponents.get("traceToLRS").get(0);
		toggle.setSelected(gameContent.traceToLRS);
		text = (JTextField) jsonKeyToComponents.get("traceMovementFrequency").get(0);
		text.setText(Float.toString(gameContent.traceMovementFrequency));
		text.setColumns(1);
		toggle = (JCheckBox) jsonKeyToComponents.get("virtualPuzzle").get(0);
		toggle.setSelected(gameContent.virtualPuzzle);
		toggle = (JCheckBox) jsonKeyToComponents.get("virtualDreamFragment").get(0);
		toggle.setSelected(gameContent.virtualDreamFragment);
		toggle = (JCheckBox) jsonKeyToComponents.get("useEndRoom").get(0);
		toggle.setSelected(gameContent.useEndRoom);
		toggle = (JCheckBox) jsonKeyToComponents.get("autoSaveProgression").get(0);
		toggle.setSelected(gameContent.autoSaveProgression);
		toggle = (JCheckBox) jsonKeyToComponents.get("saveAndLoadProgression").get(0);
		toggle.setSelected(gameContent.saveAndLoadProgression);
		text = (JTextField) jsonKeyToComponents.get("lrsConfigPath").get(0);
		text.setText(gameContent.lrsConfigPath);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("dreamFragmentLinksPath").get(0);
		text.setText(gameContent.dreamFragmentLinksPath);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("dreamFragmentDocumentsPathFile").get(0);
		text.setText(gameContent.dreamFragmentDocumentsPathFile);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("hintsPath").get(0);
		text.setText(gameContent.hintsPath);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("internalHintsPath").get(0);
		text.setText(gameContent.internalHintsPath);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("wrongAnswerFeedbacksPath").get(0);
		text.setText(gameContent.wrongAnswerFeedbacksPath);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("enigmasWeightPath").get(0);
		text.setText(gameContent.enigmasWeightPath);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("labelWeightsPath").get(0);
		text.setText(gameContent.labelWeightsPath);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("helpSystemConfigPath").get(0);
		text.setText(gameContent.helpSystemConfigPath);
		text.setColumns(1);
		toggle = (JCheckBox) jsonKeyToComponents.get("removeExtraGeometries").get(0);
		toggle.setSelected(gameContent.removeExtraGeometries);
		text = (JTextField) jsonKeyToComponents.get("additionalLogosPathCount").get(0);
		length = gameContent.additionalLogosPath.length;
		text.setText(Integer.toString(length));
		text.setColumns(1);
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("additionalLogosPath").get(i);
			text.setText(gameContent.additionalLogosPath[i]);
			text.setColumns(1);
		}
		text = (JTextField) jsonKeyToComponents.get("additionalCreditCount").get(0);
		length = gameContent.additionalCredit.length;
		text.setText(Integer.toString(length));
		text.setColumns(1);
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("additionalCredit").get(i);
			text.setText(gameContent.additionalCredit[i]);
			text.setColumns(1);
		}
	}

	private void loadHistoryContent(){
		JTextField text;
		JCheckBox toggle;
		int length;

		text = (JTextField) jsonKeyToComponents.get("storyTextIntroCount").get(0);
		length = gameContent.storyTextIntro.length;
		text.setText(Integer.toString(length));
		text.setColumns(1);
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("storyTextIntro").get(i);
			text.setText(gameContent.storyTextIntro[i]);
			text.setColumns(1);
		}
		text = (JTextField) jsonKeyToComponents.get("storyTextransitionCount").get(0);
		length = gameContent.storyTextransition.length;
		text.setText(Integer.toString(length));
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("storyTextransition").get(i);
			text.setText(gameContent.storyTextransition[i]);
			text.setColumns(1);
		}
		text = (JTextField) jsonKeyToComponents.get("storyTextEndCount").get(0);
		length = gameContent.storyTextEnd.length;
		text.setText(Integer.toString(length));
		text.setColumns(1);
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("storyTextEnd").get(i);
			text.setText(gameContent.storyTextEnd[i]);
			text.setColumns(1);
		}
		text = (JTextField) jsonKeyToComponents.get("scoreText").get(0);
		text.setText(gameContent.scoreText);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("endExplainationText").get(0);
		text.setText(gameContent.endExplainationText);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("endLink").get(0);
		text.setText(gameContent.endLink);
		text.setColumns(1);
		toggle = (JCheckBox) jsonKeyToComponents.get("concatIdToLink").get(0);
		toggle.setSelected(gameContent.concatIdToLink);
	}

	private void loadEnigmasContent(){
		JTextField text;
		JCheckBox toggle;
		String[] stringArray;
		int length;

		// enigmas content
		// ballbox content
		toggle = (JCheckBox) jsonKeyToComponents.get("ballRandomPositioning").get(0);
		toggle.setSelected(gameContent.ballRandomPositioning);
		text = (JTextField) jsonKeyToComponents.get("ballBoxQuestion").get(0);
		text.setText(gameContent.ballBoxQuestion);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("ballBoxPlaceHolder").get(0);
		text.setText(gameContent.ballBoxPlaceHolder);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("ballBoxAnswerFeedback").get(0);
		text.setText(gameContent.ballBoxAnswerFeedback);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("ballBoxAnswerFeedbackDesc").get(0);
		text.setText(gameContent.ballBoxAnswerFeedbackDesc);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("ballBoxAnswerCount").get(0);
		length = gameContent.ballBoxAnswer.size();
		text.setText(Integer.toString(length));
		text.setColumns(1);
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("ballBoxAnswer").get(i);
			text.setText(gameContent.ballBoxAnswer.get(i));
			text.setColumns(1);
		}
		length = Integer.min(gameContent.ballBoxThreeUsefulBalls.length, jsonKeyToComponents.get("ballBoxThreeUsefulBalls").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("ballBoxThreeUsefulBalls").get(i);
			text.setText(Integer.toString(gameContent.ballBoxThreeUsefulBalls[i]));
			text.setColumns(1);
		}
		length = Integer.min(gameContent.ballTexts.length, jsonKeyToComponents.get("ballTexts").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("ballTexts").get(i);
			text.setText(gameContent.ballTexts[i]);
			text.setColumns(1);
		}

		// plank and wire content
		text = (JTextField) jsonKeyToComponents.get("plankAndWireQuestionIAR").get(0);
		text.setText(gameContent.plankAndWireQuestionIAR);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("plankAndWirePlaceHolder").get(0);
		text.setText(gameContent.plankAndWirePlaceHolder);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("plankAndWireQuestion").get(0);
		text.setText(gameContent.plankAndWireQuestion);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("plankAndWireAnswerFeedback").get(0);
		text.setText(gameContent.plankAndWireAnswerFeedback);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("plankAndWireAnswerFeedbackDesc").get(0);
		text.setText(gameContent.plankAndWireAnswerFeedbackDesc);
		text.setColumns(1);
		length = Integer.min(gameContent.plankAndWireCorrectWords.length, jsonKeyToComponents.get("plankAndWireCorrectWords").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("plankAndWireCorrectWords").get(i);
			text.setText(gameContent.plankAndWireCorrectWords[i]);
			text.setColumns(1);
		}
		length = Integer.min(gameContent.plankAndWireCorrectNumbers.size(), jsonKeyToComponents.get("plankAndWireCorrectNumbers").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("plankAndWireCorrectNumbers").get(i);
			text.setText(gameContent.plankAndWireCorrectNumbers.get(i));
			text.setColumns(1);
		}
		length = Integer.min(gameContent.plankOtherWords.length, jsonKeyToComponents.get("plankOtherWords").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("plankOtherWords").get(i);
			text.setText(gameContent.plankOtherWords[i]);
			text.setColumns(1);
		}
		length = Integer.min(gameContent.plankAndWireOtherNumbers.size(), jsonKeyToComponents.get("plankAndWireOtherNumbers").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("plankAndWireOtherNumbers").get(i);
			text.setText(gameContent.plankAndWireOtherNumbers.get(i));
			text.setColumns(1);
		}

		// crouch content
		text = (JTextField) jsonKeyToComponents.get("crouchQuestion").get(0);
		text.setText(gameContent.crouchQuestion);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("crouchPlaceHolder").get(0);
		text.setText(gameContent.crouchPlaceHolder);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("crouchAnswerCount").get(0);
		length = gameContent.crouchAnswer.size();
		text.setText(Integer.toString(length));
		text.setColumns(1);
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("crouchAnswer").get(i);
			text.setText(gameContent.crouchAnswer.get(i));
			text.setColumns(1);
		}
		text = (JTextField) jsonKeyToComponents.get("crouchAnswerFeedback").get(0);
		text.setText(gameContent.crouchAnswerFeedback);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("crouchAnswerFeedbackDesc").get(0);
		text.setText(gameContent.crouchAnswerFeedbackDesc);
		text.setColumns(1);
		length = Integer.min(gameContent.crouchWords.length, jsonKeyToComponents.get("crouchWords").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("crouchWords").get(i);
			text.setText(gameContent.crouchWords[i]);
			text.setColumns(1);
		}
		if(dreamFragmentsLinks != null){
			text = (JTextField) jsonKeyToComponents.get("crouchLinks").get(0);
			text.setText(dreamFragmentsLinks.get("Fragment_souvenir_chair").get(0));
			text.setColumns(1);
			text = (JTextField) jsonKeyToComponents.get("crouchButtons").get(0);
			text.setText(dreamFragmentsLinks.get("Fragment_souvenir_chair").get(1));
			text.setColumns(1);
			text = (JTextField) jsonKeyToComponents.get("crouchLinks").get(1);
			text.setText(dreamFragmentsLinks.get("Fragment_souvenir_furnitureDown").get(0));
			text.setColumns(1);
			text = (JTextField) jsonKeyToComponents.get("crouchButtons").get(1);
			text.setText(dreamFragmentsLinks.get("Fragment_souvenir_furnitureDown").get(1));
			text.setColumns(1);
			text = (JTextField) jsonKeyToComponents.get("crouchLinks").get(2);
			text.setText(dreamFragmentsLinks.get("Fragment_souvenir_furnitureUp").get(0));
			text.setColumns(1);
			text = (JTextField) jsonKeyToComponents.get("crouchButtons").get(2);
			text.setText(dreamFragmentsLinks.get("Fragment_souvenir_furnitureUp").get(1));
			text.setColumns(1);
			text = (JTextField) jsonKeyToComponents.get("crouchLinks").get(3);
			text.setText(dreamFragmentsLinks.get("Fragment_souvenir_table").get(0));
			text.setColumns(1);
			text = (JTextField) jsonKeyToComponents.get("crouchButtons").get(3);
			text.setText(dreamFragmentsLinks.get("Fragment_souvenir_table").get(1));
			text.setColumns(1);
			text = (JTextField) jsonKeyToComponents.get("crouchLinks").get(4);
			text.setText(dreamFragmentsLinks.get("Fragment_souvenir_tirroir").get(0));
			text.setColumns(1);
			text = (JTextField) jsonKeyToComponents.get("crouchButtons").get(4);
			text.setText(dreamFragmentsLinks.get("Fragment_souvenir_tirroir").get(1));
			text.setColumns(1);
			text = (JTextField) jsonKeyToComponents.get("crouchLinks").get(5);
			text.setText(dreamFragmentsLinks.get("Fragment_souvenir_wall").get(0));
			text.setColumns(1);
			text = (JTextField) jsonKeyToComponents.get("crouchButtons").get(5);
			text.setText(dreamFragmentsLinks.get("Fragment_souvenir_wall").get(1));
			text.setColumns(1);
		}

		// gears content
		text = (JTextField) jsonKeyToComponents.get("gearsQuestion").get(0);
		text.setText(gameContent.gearsQuestion);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("iarHelpGears").get(0);
		text.setText(gameContent.iarHelpGears);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("gearTextUp").get(0);
		text.setText(gameContent.gearTextUp);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("gearTextDown").get(0);
		text.setText(gameContent.gearTextDown);
		text.setColumns(1);
		length = Integer.min(gameContent.gearMovableTexts.length, jsonKeyToComponents.get("gearMovableTexts").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("gearMovableTexts").get(i);
			text.setText(gameContent.gearMovableTexts[i]);
			text.setColumns(1);
		}
		text = (JTextField) jsonKeyToComponents.get("gearAnswer").get(0);
		text.setText(gameContent.gearAnswer);
		text.setColumns(1);

		// mastermind content
		text = (JTextField) jsonKeyToComponents.get("mastermindQuestion").get(0);
		text.setText(gameContent.mastermindQuestion);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("masterMindPlaceholder").get(0);
		text.setText(gameContent.masterMindPlaceholder);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("masterMindPasswordText").get(0);
		text.setText(gameContent.masterMindPasswordText);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("masterMindValidation").get(0);
		text.setText(gameContent.masterMindValidation);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("mastermindQuestionYPos").get(0);
		text.setText(Integer.toString(gameContent.mastermindQuestionYPos));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("mastermindAnswer").get(0);
		text.setText(Integer.toString(gameContent.mastermindAnswer));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("mastermindBackgroundPicturePath").get(0);
		text.setText(gameContent.mastermindBackgroundPicturePath);
		text.setColumns(1);

		// glasses content
		text = (JTextField) jsonKeyToComponents.get("glassesQuestion").get(0);
		text.setText(gameContent.glassesQuestion);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("glassesPlaceHolder").get(0);
		text.setText(gameContent.glassesPlaceHolder);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("glassesAnswerCount").get(0);
		length = gameContent.glassesAnswer.size();
		text.setText(Integer.toString(length));
		text.setColumns(1);
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("glassesAnswer").get(i);
			text.setText(gameContent.glassesAnswer.get(i));
			text.setColumns(1);
		}
		text = (JTextField) jsonKeyToComponents.get("glassesAnswerFeedback").get(0);
		text.setText(gameContent.glassesAnswerFeedback);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("glassesAnswerFeedbackDesc").get(0);
		text.setText(gameContent.glassesAnswerFeedbackDesc);
		text.setColumns(1);
		length = Integer.min(gameContent.glassesPicturesPath.length, jsonKeyToComponents.get("glassesPicturesPath").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("glassesPicturesPath").get(i);
			text.setText(gameContent.glassesPicturesPath[i]);
			text.setColumns(1);
		}

		// enigma 8 content
		text = (JTextField) jsonKeyToComponents.get("enigma08Question").get(0);
		text.setText(gameContent.enigma08Question);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("enigma08PlaceHolder").get(0);
		text.setText(gameContent.enigma08PlaceHolder);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("enigma08AnswerCount").get(0);
		length = gameContent.enigma08Answer.size();
		text.setText(Integer.toString(length));
		text.setColumns(1);
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("enigma08Answer").get(i);
			text.setText(gameContent.enigma08Answer.get(i));
			text.setColumns(1);
		}
		text = (JTextField) jsonKeyToComponents.get("enigma08AnswerFeedback").get(0);
		text.setText(gameContent.enigma08AnswerFeedback);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("enigma08AnswerFeedbackDesc").get(0);
		text.setText(gameContent.enigma08AnswerFeedbackDesc);
		text.setColumns(1);

		// scrolls content
		text = (JTextField) jsonKeyToComponents.get("scrollsQuestion").get(0);
		text.setText(gameContent.scrollsQuestion);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("scrollsPlaceHolder").get(0);
		text.setText(gameContent.scrollsPlaceHolder);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("scrollsAnswerCount").get(0);
		length = gameContent.scrollsAnswer.size();
		text.setText(Integer.toString(length));
		text.setColumns(1);
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("scrollsAnswer").get(i);
			text.setText(gameContent.scrollsAnswer.get(i));
			text.setColumns(1);
		}
		text = (JTextField) jsonKeyToComponents.get("scrollsAnswerFeedback").get(0);
		text.setText(gameContent.scrollsAnswerFeedback);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("scrollsAnswerFeedbackDesc").get(0);
		text.setText(gameContent.scrollsAnswerFeedbackDesc);
		text.setColumns(1);
		length = Integer.min(gameContent.scrollsWords.length, jsonKeyToComponents.get("scrollsWords").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("scrollsWords").get(i);
			text.setText(gameContent.scrollsWords[i]);
			text.setColumns(1);
		}

		// mirror content
		text = (JTextField) jsonKeyToComponents.get("mirrorQuestion").get(0);
		text.setText(gameContent.mirrorQuestion);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("mirrorPlaceHolder").get(0);
		text.setText(gameContent.mirrorPlaceHolder);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("mirrorAnswerCount").get(0);
		length = gameContent.mirrorAnswer.size();
		text.setText(Integer.toString(length));
		text.setColumns(1);
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("mirrorAnswer").get(i);
			text.setText(gameContent.mirrorAnswer.get(i));
			text.setColumns(1);
		}
		text = (JTextField) jsonKeyToComponents.get("mirrorAnswerFeedback").get(0);
		text.setText(gameContent.mirrorAnswerFeedback);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("mirrorAnswerFeedbackDesc").get(0);
		text.setText(gameContent.mirrorAnswerFeedbackDesc);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("mirrorPicturePath").get(0);
		text.setText(gameContent.mirrorPicturePath);
		text.setColumns(1);

		// enigma 11 content
		text = (JTextField) jsonKeyToComponents.get("enigma11Question").get(0);
		text.setText(gameContent.enigma11Question);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("enigma11PlaceHolder").get(0);
		text.setText(gameContent.enigma11PlaceHolder);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("enigma11AnswerCount").get(0);
		length = gameContent.enigma11Answer.size();
		text.setText(Integer.toString(length));
		text.setColumns(1);
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("enigma11Answer").get(i);
			text.setText(gameContent.enigma11Answer.get(i));
			text.setColumns(1);
		}
		text = (JTextField) jsonKeyToComponents.get("enigma11AnswerFeedback").get(0);
		text.setText(gameContent.enigma11AnswerFeedback);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("enigma11AnswerFeedbackDesc").get(0);
		text.setText(gameContent.enigma11AnswerFeedbackDesc);
		text.setColumns(1);

		// enigma 12 content
		text = (JTextField) jsonKeyToComponents.get("enigma12Question").get(0);
		text.setText(gameContent.enigma12Question);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("enigma12PlaceHolder").get(0);
		text.setText(gameContent.enigma12PlaceHolder);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("enigma12AnswerCount").get(0);
		length = gameContent.enigma12Answer.size();
		text.setText(Integer.toString(length));
		text.setColumns(1);
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("enigma12Answer").get(i);
			text.setText(gameContent.enigma12Answer.get(i));
			text.setColumns(1);
		}
		text = (JTextField) jsonKeyToComponents.get("enigma12AnswerFeedback").get(0);
		text.setText(gameContent.enigma12AnswerFeedback);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("enigma12AnswerFeedbackDesc").get(0);
		text.setText(gameContent.enigma12AnswerFeedbackDesc);
		text.setColumns(1);

		// lock room 2 content
		text = (JTextField) jsonKeyToComponents.get("lockRoom2Password").get(0);
		text.setText(Integer.toString(gameContent.lockRoom2Password));
		text.setColumns(1);

		// puzzle content
		stringArray = gameContent.puzzleAnswer.split("##");
		text = (JTextField) jsonKeyToComponents.get("puzzleAnswerCount").get(0);
		length = stringArray.length;
		text.setText(Integer.toString(length));
		text.setColumns(1);
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("puzzleAnswer").get(i);
			text.setText(stringArray[i]);
			text.setColumns(1);
		}
		text = (JTextField) jsonKeyToComponents.get("puzzleAnswerFeedback").get(0);
		text.setText(gameContent.puzzleAnswerFeedback);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("puzzleAnswerFeedbackDesc").get(0);
		text.setText(gameContent.puzzleAnswerFeedbackDesc);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("puzzlePicturePath").get(0);
		text.setText(gameContent.puzzlePicturePath);
		text.setColumns(1);

		// lamp content
		stringArray = gameContent.lampAnswer.split("##");
		text = (JTextField) jsonKeyToComponents.get("lampAnswerCount").get(0);
		length = stringArray.length;
		text.setText(Integer.toString(length));
		text.setColumns(1);
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("lampAnswer").get(i);
			text.setText(stringArray[i]);
			text.setColumns(1);
		}
		text = (JTextField) jsonKeyToComponents.get("lampAnswerFeedback").get(0);
		text.setText(gameContent.lampAnswerFeedback);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("lampAnswerFeedbackDesc").get(0);
		text.setText(gameContent.lampAnswerFeedbackDesc);
		text.setColumns(1);
		length = Integer.min(gameContent.lampPicturesPath.length, jsonKeyToComponents.get("lampPicturesPath").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("lampPicturesPath").get(i);
			text.setText(gameContent.lampPicturesPath[i]);
			text.setColumns(1);
		}

		// white board content
		stringArray = gameContent.whiteBoardAnswer.split("##");
		text = (JTextField) jsonKeyToComponents.get("whiteBoardAnswerCount").get(0);
		length = stringArray.length;
		text.setText(Integer.toString(length));
		text.setColumns(1);
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("whiteBoardAnswer").get(i);
			text.setText(stringArray[i]);
			text.setColumns(1);
		}
		text = (JTextField) jsonKeyToComponents.get("whiteBoardAnswerFeedback").get(0);
		text.setText(gameContent.whiteBoardAnswerFeedback);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("whiteBoardAnswerFeedbackDesc").get(0);
		text.setText(gameContent.whiteBoardAnswerFeedbackDesc);
		text.setColumns(1);
		length = Integer.min(gameContent.whiteBoardWords.length, jsonKeyToComponents.get("whiteBoardWords").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("whiteBoardWords").get(i);
			text.setText(gameContent.whiteBoardWords[i]);
			text.setColumns(1);
		}

		// enigma 16 content
		stringArray = gameContent.enigma16Answer.split("##");
		text = (JTextField) jsonKeyToComponents.get("enigma16AnswerCount").get(0);
		length = stringArray.length;
		text.setText(Integer.toString(length));
		text.setColumns(1);
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("enigma16Answer").get(i);
			text.setText(stringArray[i]);
			text.setColumns(1);
		}
		text = (JTextField) jsonKeyToComponents.get("enigma16AnswerFeedback").get(0);
		text.setText(gameContent.puzzleAnswerFeedback);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("enigma16AnswerFeedbackDesc").get(0);
		text.setText(gameContent.puzzleAnswerFeedbackDesc);
		text.setColumns(1);
	}

	private void loadInventoryContent(){
		JTextField text;

		text = (JTextField) jsonKeyToComponents.get("inventoryScrollIntroTitle").get(0);
		text.setText(gameContent.inventoryScrollIntro.get(0));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryScrollIntroDescription").get(0);
		text.setText(gameContent.inventoryScrollIntro.get(1));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryScrollIntroNotice").get(0);
		text.setText(gameContent.inventoryScrollIntro.get(2));
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inventoryKeyBallBoxTitle").get(0);
		text.setText(gameContent.inventoryKeyBallBox.get(0));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryKeyBallBoxDescription").get(0);
		text.setText(gameContent.inventoryKeyBallBox.get(1));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryKeyBallBoxNotice").get(0);
		text.setText(gameContent.inventoryKeyBallBox.get(2));
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inventoryWireTitle").get(0);
		text.setText(gameContent.inventoryWire.get(0));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryWireDescription").get(0);
		text.setText(gameContent.inventoryWire.get(1));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryWireNotice").get(0);
		text.setText(gameContent.inventoryWire.get(2));
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inventoryKeySatchelTitle").get(0);
		text.setText(gameContent.inventoryKeySatchel.get(0));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryKeySatchelDescription").get(0);
		text.setText(gameContent.inventoryKeySatchel.get(1));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryKeySatchelNotice").get(0);
		text.setText(gameContent.inventoryKeySatchel.get(2));
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inventoryScrollsTitle").get(0);
		text.setText(gameContent.inventoryScrolls.get(0));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryScrollsDescription").get(0);
		text.setText(gameContent.inventoryScrolls.get(1));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryScrollsNotice").get(0);
		text.setText(gameContent.inventoryScrolls.get(2));
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inventoryGlasses1Title").get(0);
		text.setText(gameContent.inventoryGlasses1.get(0));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryGlasses1Description").get(0);
		text.setText(gameContent.inventoryGlasses1.get(1));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryGlasses1Notice").get(0);
		text.setText(gameContent.inventoryGlasses1.get(2));
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inventoryGlasses2Title").get(0);
		text.setText(gameContent.inventoryGlasses2.get(0));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryGlasses2Description").get(0);
		text.setText(gameContent.inventoryGlasses2.get(1));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryGlasses2Notice").get(0);
		text.setText(gameContent.inventoryGlasses2.get(2));
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inventoryMirrorTitle").get(0);
		text.setText(gameContent.inventoryMirror.get(0));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryMirrorDescription").get(0);
		text.setText(gameContent.inventoryMirror.get(1));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryMirrorNotice").get(0);
		text.setText(gameContent.inventoryMirror.get(2));
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inventoryLampTitle").get(0);
		text.setText(gameContent.inventoryLamp.get(0));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryLampDescription").get(0);
		text.setText(gameContent.inventoryLamp.get(1));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryLampNotice").get(0);
		text.setText(gameContent.inventoryLamp.get(2));
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inventoryPuzzleTitle").get(0);
		text.setText(gameContent.inventoryPuzzle.get(0));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryPuzzleDescription").get(0);
		text.setText(gameContent.inventoryPuzzle.get(1));
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inventoryPuzzleNotice").get(0);
		text.setText(gameContent.inventoryPuzzle.get(2));
		text.setColumns(1);
	}

	private void loadDreamFragmentsContent(){
		JTextField text;
		String name;

		for(int i = 0; i < 19; i++){
			name = "Fragment_souvenir_" + i;
			text = (JTextField) jsonKeyToComponents.get(name + "Link").get(0);
			text.setText(dreamFragmentsLinks.get(name).get(0));
			text.setColumns(1);
			text = (JTextField) jsonKeyToComponents.get(name + "Button").get(0);
			text.setText(dreamFragmentsLinks.get(name).get(1));
			text.setColumns(1);
		}
	}

	private void loadUIContent(){
		JTextField text;

		text = (JTextField) jsonKeyToComponents.get("loadingText").get(0);
		text.setText(gameContent.loadingText);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("mainMenuStart").get(0);
		text.setText(gameContent.mainMenuStart);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("mainMenuLoad").get(0);
		text.setText(gameContent.mainMenuLoad);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("mainMenuOption").get(0);
		text.setText(gameContent.mainMenuOption);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("mainMenuLeave").get(0);
		text.setText(gameContent.mainMenuLeave);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("sessionIDText").get(0);
		text.setText(gameContent.sessionIDText);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("sessionIDPopup").get(0);
		text.setText(gameContent.sessionIDPopup);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("endLinkButtonText").get(0);
		text.setText(gameContent.endLinkButtonText);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("endLeaveButtonText").get(0);
		text.setText(gameContent.endLeaveButtonText);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionControlsMenu").get(0);
		text.setText(gameContent.optionControlsMenu);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionVirtualFragments").get(0);
		text.setText(gameContent.optionVirtualFragments);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionMovingTexts").get(0);
		text.setText(gameContent.optionMovingTexts);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionMoveSpeed").get(0);
		text.setText(gameContent.optionMoveSpeed);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionCameraSensitivity").get(0);
		text.setText(gameContent.optionCameraSensitivity);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionLockWheelSpeed").get(0);
		text.setText(gameContent.optionLockWheelSpeed);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionInputs").get(0);
		text.setText(gameContent.optionInputs);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionSoundMenu").get(0);
		text.setText(gameContent.optionSoundMenu);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionGameMusicVolume").get(0);
		text.setText(gameContent.optionGameMusicVolume);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionSoundEffectsVolume").get(0);
		text.setText(gameContent.optionSoundEffectsVolume);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionDisplayMenu").get(0);
		text.setText(gameContent.optionDisplayMenu);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionFont").get(0);
		text.setText(gameContent.optionFont);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionCursorSize").get(0);
		text.setText(gameContent.optionCursorSize);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionLightIntensity").get(0);
		text.setText(gameContent.optionLightIntensity);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionTransparency").get(0);
		text.setText(gameContent.optionTransparency);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionValidateAndReturn").get(0);
		text.setText(gameContent.optionValidateAndReturn);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("optionDefault").get(0);
		text.setText(gameContent.optionDefault);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("loadPopupLoadButton").get(0);
		text.setText(gameContent.loadPopupLoadButton);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("loadPopupCancelButton").get(0);
		text.setText(gameContent.loadPopupCancelButton);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("validateOptions").get(0);
		text.setText(gameContent.validateOptions);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("storyClickToContinue").get(0);
		text.setText(gameContent.storyClickToContinue);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("hudObserve").get(0);
		text.setText(gameContent.hudObserve);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("hudMove").get(0);
		text.setText(gameContent.hudMove);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("hudCrouch").get(0);
		text.setText(gameContent.hudCrouch);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("hudInventory").get(0);
		text.setText(gameContent.hudInventory);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("hudDreamFragments").get(0);
		text.setText(gameContent.hudDreamFragments);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("hudQuestions").get(0);
		text.setText(gameContent.hudQuestions);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("hudHelp").get(0);
		text.setText(gameContent.hudHelp);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("hudMenu").get(0);
		text.setText(gameContent.hudMenu);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("dreamFragmentText").get(0);
		text.setText(gameContent.dreamFragmentText);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("dreamFragmentValidation").get(0);
		text.setText(gameContent.dreamFragmentValidation);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("dreamFragmentVirtualReset").get(0);
		text.setText(gameContent.dreamFragmentVirtualReset);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("iarTabInventory").get(0);
		text.setText(gameContent.iarTabInventory);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("iarTabDreamFragments").get(0);
		text.setText(gameContent.iarTabDreamFragments);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("iarTabHelp").get(0);
		text.setText(gameContent.iarTabHelp);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("iarTabMenu").get(0);
		text.setText(gameContent.iarTabMenu);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("iarTabQuestions1").get(0);
		text.setText(gameContent.iarTabQuestions1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("iarTabQuestions2").get(0);
		text.setText(gameContent.iarTabQuestions2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("iarTabQuestions3").get(0);
		text.setText(gameContent.iarTabQuestions3);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("questionValidationButton").get(0);
		text.setText(gameContent.questionValidationButton);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("hintButtonText").get(0);
		text.setText(gameContent.hintButtonText);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("getHintButton").get(0);
		text.setText(gameContent.getHintButton);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("hintOpenURL").get(0);
		text.setText(gameContent.hintOpenURL);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("gameMenuResumeButton").get(0);
		text.setText(gameContent.gameMenuResumeButton);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("gameMenuSaveButton").get(0);
		text.setText(gameContent.gameMenuSaveButton);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("gameMenuSaveNotice").get(0);
		text.setText(gameContent.gameMenuSaveNotice);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("gameMenuOptionButton").get(0);
		text.setText(gameContent.gameMenuOptionButton);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("gameMenuRestartButton").get(0);
		text.setText(gameContent.gameMenuRestartButton);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("gameMenuLeaveButton").get(0);
		text.setText(gameContent.gameMenuLeaveButton);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("savePopupSaveButton").get(0);
		text.setText(gameContent.savePopupSaveButton);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("savePopupCancelButton").get(0);
		text.setText(gameContent.savePopupCancelButton);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("savePopupPlaceholder").get(0);
		text.setText(gameContent.savePopupPlaceholder);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("savePopupInvalidText").get(0);
		text.setText(gameContent.savePopupInvalidText);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("savePopupInvalidButton").get(0);
		text.setText(gameContent.savePopupInvalidButton);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("savePopupOverrideText").get(0);
		text.setText(gameContent.savePopupOverrideText);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("savePopupOverrideYesButton").get(0);
		text.setText(gameContent.savePopupOverrideYesButton);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("savePopupOverrideNoButton").get(0);
		text.setText(gameContent.savePopupOverrideNoButton);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("savePopupDoneText").get(0);
		text.setText(gameContent.savePopupDoneText);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("savePopupDoneButton").get(0);
		text.setText(gameContent.savePopupDoneButton);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("tutoText0").get(0);
		text.setText(gameContent.tutoText0);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("tutoText1").get(0);
		text.setText(gameContent.tutoText1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("tutoText2").get(0);
		text.setText(gameContent.tutoText2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("tutoText3").get(0);
		text.setText(gameContent.tutoText3);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("tutoText4").get(0);
		text.setText(gameContent.tutoText4);
		text.setColumns(1);
	}

	private void loadCommandsContent(){
		JTextField text;

		text = (JTextField) jsonKeyToComponents.get("inputsSetTitle1").get(0);
		text.setText(gameContent.inputsSetTitle1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputsSetTitle2").get(0);
		text.setText(gameContent.inputsSetTitle2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputsSetTitle3").get(0);
		text.setText(gameContent.inputsSetTitle3);
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inputObserve").get(0);
		text.setText(gameContent.inputObserve);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputObserveKey1").get(0);
		text.setText(gameContent.inputObserveKey1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputObserveKey2").get(0);
		text.setText(gameContent.inputObserveKey2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputObserveKey3").get(0);
		text.setText(gameContent.inputObserveKey3);
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inputForward").get(0);
		text.setText(gameContent.inputForward);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputForwardKey1").get(0);
		text.setText(gameContent.inputForwardKey1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputForwardKey2").get(0);
		text.setText(gameContent.inputForwardKey2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputForwardKey3").get(0);
		text.setText(gameContent.inputForwardKey3);
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inputBack").get(0);
		text.setText(gameContent.inputBack);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputBackKey1").get(0);
		text.setText(gameContent.inputBackKey1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputBackKey2").get(0);
		text.setText(gameContent.inputBackKey2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputBackKey3").get(0);
		text.setText(gameContent.inputBackKey3);
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inputLeft").get(0);
		text.setText(gameContent.inputLeft);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputLeftKey1").get(0);
		text.setText(gameContent.inputLeftKey1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputLeftKey2").get(0);
		text.setText(gameContent.inputLeftKey2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputLeftKey3").get(0);
		text.setText(gameContent.inputLeftKey3);
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inputRight").get(0);
		text.setText(gameContent.inputRight);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputRightKey1").get(0);
		text.setText(gameContent.inputRightKey1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputRightKey2").get(0);
		text.setText(gameContent.inputRightKey2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputRightKey3").get(0);
		text.setText(gameContent.inputRightKey3);
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inputCrouch").get(0);
		text.setText(gameContent.inputCrouch);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputCrouchKey1").get(0);
		text.setText(gameContent.inputCrouchKey1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputCrouchKey2").get(0);
		text.setText(gameContent.inputCrouchKey2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputCrouchKey3").get(0);
		text.setText(gameContent.inputCrouchKey3);
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inputInteract").get(0);
		text.setText(gameContent.inputInteract);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputInteractKey1").get(0);
		text.setText(gameContent.inputInteractKey1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputInteractKey2").get(0);
		text.setText(gameContent.inputInteractKey2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputInteractKey3").get(0);
		text.setText(gameContent.inputInteractKey3);
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inputInventory").get(0);
		text.setText(gameContent.inputInventory);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputInventoryKey1").get(0);
		text.setText(gameContent.inputInventoryKey1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputInventoryKey2").get(0);
		text.setText(gameContent.inputInventoryKey2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputInventoryKey3").get(0);
		text.setText(gameContent.inputInventoryKey3);
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inputDreamFragments").get(0);
		text.setText(gameContent.inputDreamFragments);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputDreamFragmentsKey1").get(0);
		text.setText(gameContent.inputDreamFragmentsKey1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputDreamFragmentsKey2").get(0);
		text.setText(gameContent.inputDreamFragmentsKey2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputDreamFragmentsKey3").get(0);
		text.setText(gameContent.inputDreamFragmentsKey3);
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inputQuestions").get(0);
		text.setText(gameContent.inputQuestions);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputQuestionsKey1").get(0);
		text.setText(gameContent.inputQuestionsKey1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputQuestionsKey2").get(0);
		text.setText(gameContent.inputQuestionsKey2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputQuestionsKey3").get(0);
		text.setText(gameContent.inputQuestionsKey3);
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inputHelp").get(0);
		text.setText(gameContent.inputHelp);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputHelpKey1").get(0);
		text.setText(gameContent.inputHelpKey1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputHelpKey2").get(0);
		text.setText(gameContent.inputHelpKey2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputHelpKey3").get(0);
		text.setText(gameContent.inputHelpKey3);
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inputMenu").get(0);
		text.setText(gameContent.inputMenu);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputMenuKey1").get(0);
		text.setText(gameContent.inputMenuKey1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputMenuKey2").get(0);
		text.setText(gameContent.inputMenuKey2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputMenuKey3").get(0);
		text.setText(gameContent.inputMenuKey3);
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inputView").get(0);
		text.setText(gameContent.inputView);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputViewKey1").get(0);
		text.setText(gameContent.inputViewKey1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputViewKey2").get(0);
		text.setText(gameContent.inputViewKey2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputViewKey3").get(0);
		text.setText(gameContent.inputViewKey3);
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inputTarget").get(0);
		text.setText(gameContent.inputTarget);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputTargetKey1").get(0);
		text.setText(gameContent.inputTargetKey1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputTargetKey2").get(0);
		text.setText(gameContent.inputTargetKey2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputTargetKey3").get(0);
		text.setText(gameContent.inputTargetKey3);
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inputZoomIn").get(0);
		text.setText(gameContent.inputZoomIn);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputZoomInKey1").get(0);
		text.setText(gameContent.inputZoomInKey1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputZoomInKey2").get(0);
		text.setText(gameContent.inputZoomInKey2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputZoomInKey3").get(0);
		text.setText(gameContent.inputZoomInKey3);
		text.setColumns(1);

		text = (JTextField) jsonKeyToComponents.get("inputZoomOut").get(0);
		text.setText(gameContent.inputZoomOut);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputZoomOutKey1").get(0);
		text.setText(gameContent.inputZoomOutKey1);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputZoomOutKey2").get(0);
		text.setText(gameContent.inputZoomOutKey2);
		text.setColumns(1);
		text = (JTextField) jsonKeyToComponents.get("inputZoomOutKey3").get(0);
		text.setText(gameContent.inputZoomOutKey3);
		text.setColumns(1);
	}

	private void save(){
		if(loadedFilePath != null && loadedFilePath.length() != 0)
			saveAs(loadedFilePath);
	}

	private void saveAs() {
		FileDialog fd = new FileDialog(this, "Choisir un emplacement pour sauvegarder", FileDialog.SAVE);
		fd.setFile("*.txt");
		fd.setVisible(true);
		String fileName = fd.getFile();
		if(fileName != null)
			saveAs(fd.getDirectory() + fd.getFile());
	}

	private void saveAs(String filePath){
		// put all the content in gameContent and in dreamFragmentsLinks
		getConfigContentFromWindow();
		getHistoryContentFromWindow();
		getEnigmasContentFromWindow();
		getInventoryContentFromWindow();
		getDreamFragmentsContentFromWindow();
		getUIContentFromWindow();
		getCommandsContentFromWindow();

		// save gameContent and dreamFragmentsLinks in a file
		Gson gson = new GsonBuilder().setPrettyPrinting().create();
		FileWriter fw;
		try{
			fw = new FileWriter(filePath);
			fw.write(gson.toJson(gameContent));
			loadedFilePath = filePath;
			setTitle("DreamReader - "+ loadedFilePath);
			fw.close();
		}
		catch(Exception e){
			e.printStackTrace();
		}
		if(gameContent.dreamFragmentLinksPath != null && gameContent.dreamFragmentLinksPath.length() != 0){
			try{
				fw = new FileWriter(gameContent.dreamFragmentLinksPath);
				fw.write(gson.toJson(dreamFragmentsLinks));
				fw.close();
			}
			catch(Exception e){
				e.printStackTrace();
			}
		}
	}

	private void getConfigContentFromWindow(){
		JTextField text;
		JCheckBox toggle;
		int length;

		text = (JTextField) jsonKeyToComponents.get("theme").get(0);
		gameContent.theme = text.getText();
		toggle = (JCheckBox) jsonKeyToComponents.get("trace").get(0);
		gameContent.trace = toggle.isSelected();
		toggle = (JCheckBox) jsonKeyToComponents.get("helpSystem").get(0);
		gameContent.helpSystem = toggle.isSelected();
		toggle = (JCheckBox) jsonKeyToComponents.get("randomHelpSystemActivation").get(0);
		gameContent.randomHelpSystemActivation = toggle.isSelected();
		toggle = (JCheckBox) jsonKeyToComponents.get("traceToLRS").get(0);
		gameContent.traceToLRS = toggle.isSelected();
		text = (JTextField) jsonKeyToComponents.get("traceMovementFrequency").get(0);
		try{
			gameContent.traceMovementFrequency = Float.parseFloat(text.getText());
		} catch (Exception e) {
			e.printStackTrace();
		}
		toggle = (JCheckBox) jsonKeyToComponents.get("virtualDreamFragment").get(0);
		gameContent.virtualDreamFragment = toggle.isSelected();
		toggle = (JCheckBox) jsonKeyToComponents.get("virtualPuzzle").get(0);
		gameContent.virtualPuzzle = toggle.isSelected();
		toggle = (JCheckBox) jsonKeyToComponents.get("useEndRoom").get(0);
		gameContent.useEndRoom = toggle.isSelected();
		toggle = (JCheckBox) jsonKeyToComponents.get("saveAndLoadProgression").get(0);
		gameContent.saveAndLoadProgression = toggle.isSelected();
		toggle = (JCheckBox) jsonKeyToComponents.get("autoSaveProgression").get(0);
		gameContent.autoSaveProgression = toggle.isSelected();
		text = (JTextField) jsonKeyToComponents.get("lrsConfigPath").get(0);
		gameContent.lrsConfigPath = text.getText();
		text = (JTextField) jsonKeyToComponents.get("dreamFragmentLinksPath").get(0);
		gameContent.dreamFragmentLinksPath = text.getText();
		text = (JTextField) jsonKeyToComponents.get("dreamFragmentDocumentsPathFile").get(0);
		gameContent.dreamFragmentDocumentsPathFile = text.getText();
		text = (JTextField) jsonKeyToComponents.get("hintsPath").get(0);
		gameContent.hintsPath = text.getText();
		text = (JTextField) jsonKeyToComponents.get("internalHintsPath").get(0);
		gameContent.internalHintsPath = text.getText();
		text = (JTextField) jsonKeyToComponents.get("wrongAnswerFeedbacksPath").get(0);
		gameContent.wrongAnswerFeedbacksPath = text.getText();
		text = (JTextField) jsonKeyToComponents.get("enigmasWeightPath").get(0);
		gameContent.enigmasWeightPath = text.getText();
		text = (JTextField) jsonKeyToComponents.get("labelWeightsPath").get(0);
		gameContent.labelWeightsPath = text.getText();
		text = (JTextField) jsonKeyToComponents.get("helpSystemConfigPath").get(0);
		gameContent.helpSystemConfigPath = text.getText();
		toggle = (JCheckBox) jsonKeyToComponents.get("removeExtraGeometries").get(0);
		gameContent.removeExtraGeometries = toggle.isSelected();
		length = jsonKeyToComponents.get("additionalLogosPath").size();
		gameContent.additionalLogosPath = new String[length];
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("additionalLogosPath").get(i);
			gameContent.additionalLogosPath[i] = text.getText();
		}
		length = jsonKeyToComponents.get("additionalCredit").size();
		gameContent.additionalCredit = new String[length];
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("additionalCredit").get(i);
			gameContent.additionalCredit[i] = text.getText();
		}
	}

	private void getHistoryContentFromWindow(){
		JTextField text;
		JCheckBox toggle;
		int length;

		length = jsonKeyToComponents.get("storyTextIntro").size();
		gameContent.storyTextIntro = new String[length];
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("storyTextIntro").get(i);
			gameContent.storyTextIntro[i] = text.getText();
		}
		length = jsonKeyToComponents.get("storyTextransition").size();
		gameContent.storyTextransition = new String[length];
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("storyTextransition").get(i);
			gameContent.storyTextransition[i] = text.getText();
		}
		length = jsonKeyToComponents.get("storyTextEnd").size();
		gameContent.storyTextEnd = new String[length];
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("storyTextEnd").get(i);
			gameContent.storyTextEnd[i] = text.getText();
		}
		text = (JTextField) jsonKeyToComponents.get("scoreText").get(0);
		gameContent.scoreText = text.getText();
		text = (JTextField) jsonKeyToComponents.get("endExplainationText").get(0);
		gameContent.endExplainationText = text.getText();
		text = (JTextField) jsonKeyToComponents.get("endLink").get(0);
		gameContent.endLink = text.getText();
		toggle = (JCheckBox) jsonKeyToComponents.get("concatIdToLink").get(0);
		gameContent.concatIdToLink = toggle.isSelected();
	}

	private void getEnigmasContentFromWindow(){
		JTextField text;
		JCheckBox toggle;
		StringBuilder concat;
		int length;

		// enigmas content
		// ballbox content
		toggle = (JCheckBox) jsonKeyToComponents.get("ballRandomPositioning").get(0);
		gameContent.ballRandomPositioning = toggle.isSelected();
		text = (JTextField) jsonKeyToComponents.get("ballBoxQuestion").get(0);
		gameContent.ballBoxQuestion = text.getText();
		text = (JTextField) jsonKeyToComponents.get("ballBoxPlaceHolder").get(0);
		gameContent.ballBoxPlaceHolder = text.getText();
		text = (JTextField) jsonKeyToComponents.get("ballBoxAnswerFeedback").get(0);
		gameContent.ballBoxAnswerFeedback = text.getText();
		text = (JTextField) jsonKeyToComponents.get("ballBoxAnswerFeedbackDesc").get(0);
		gameContent.ballBoxAnswerFeedbackDesc = text.getText();
		length = jsonKeyToComponents.get("ballBoxAnswer").size();
		gameContent.ballBoxAnswer.clear();
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("ballBoxAnswer").get(i);
			gameContent.ballBoxAnswer.add(text.getText());
		}
		length = Integer.min(gameContent.ballBoxThreeUsefulBalls.length, jsonKeyToComponents.get("ballBoxThreeUsefulBalls").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("ballBoxThreeUsefulBalls").get(i);
			try{
				gameContent.ballBoxThreeUsefulBalls[i] = Integer.parseInt(text.getText());
			}
			catch (Exception e){
				e.printStackTrace();
			}
		}
		length = Integer.min(gameContent.ballTexts.length, jsonKeyToComponents.get("ballTexts").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("ballTexts").get(i);
			gameContent.ballTexts[i] = text.getText();
		}

		// plank and wire content
		text = (JTextField) jsonKeyToComponents.get("plankAndWireQuestionIAR").get(0);
		gameContent.plankAndWireQuestionIAR = text.getText();
		text = (JTextField) jsonKeyToComponents.get("plankAndWirePlaceHolder").get(0);
		gameContent.plankAndWirePlaceHolder = text.getText();
		text = (JTextField) jsonKeyToComponents.get("plankAndWireQuestion").get(0);
		gameContent.plankAndWireQuestion = text.getText();
		text = (JTextField) jsonKeyToComponents.get("plankAndWireAnswerFeedback").get(0);
		gameContent.plankAndWireAnswerFeedback = text.getText();
		text = (JTextField) jsonKeyToComponents.get("plankAndWireAnswerFeedbackDesc").get(0);
		gameContent.plankAndWireAnswerFeedbackDesc = text.getText();
		length = Integer.min(gameContent.plankAndWireCorrectWords.length, jsonKeyToComponents.get("plankAndWireCorrectWords").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("plankAndWireCorrectWords").get(i);
			gameContent.plankAndWireCorrectWords[i] = text.getText();
		}
		length = jsonKeyToComponents.get("plankAndWireCorrectNumbers").size();
		gameContent.plankAndWireCorrectNumbers.clear();
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("plankAndWireCorrectNumbers").get(i);
			gameContent.plankAndWireCorrectNumbers.add(text.getText());
		}
		length = Integer.min(gameContent.plankOtherWords.length, jsonKeyToComponents.get("plankOtherWords").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("plankOtherWords").get(i);
			gameContent.plankOtherWords[i] = text.getText();
		}
		length = jsonKeyToComponents.get("plankAndWireOtherNumbers").size();
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("plankAndWireOtherNumbers").get(i);
			gameContent.plankAndWireOtherNumbers.add(text.getText());
		}

		// crouch content
		text = (JTextField) jsonKeyToComponents.get("crouchQuestion").get(0);
		gameContent.crouchQuestion = text.getText();
		text = (JTextField) jsonKeyToComponents.get("crouchPlaceHolder").get(0);
		gameContent.crouchPlaceHolder = text.getText();
		length = jsonKeyToComponents.get("crouchAnswer").size();
		gameContent.crouchAnswer.clear();
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("crouchAnswer").get(i);
			gameContent.crouchAnswer.add(text.getText());
		}
		text = (JTextField) jsonKeyToComponents.get("crouchAnswerFeedback").get(0);
		gameContent.crouchAnswerFeedback = text.getText();
		text = (JTextField) jsonKeyToComponents.get("crouchAnswerFeedbackDesc").get(0);
		gameContent.crouchAnswerFeedbackDesc = text.getText();
		length = Integer.min(gameContent.crouchWords.length, jsonKeyToComponents.get("crouchWords").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("crouchWords").get(i);
			gameContent.crouchWords[i] = text.getText();
		}
		if(dreamFragmentsLinks != null){
			dreamFragmentsLinks.get("Fragment_souvenir_chair").clear();
			text = (JTextField) jsonKeyToComponents.get("crouchLinks").get(0);
			dreamFragmentsLinks.get("Fragment_souvenir_chair").add(text.getText());
			text = (JTextField) jsonKeyToComponents.get("crouchButtons").get(0);
			dreamFragmentsLinks.get("Fragment_souvenir_chair").add(text.getText());
			dreamFragmentsLinks.get("Fragment_souvenir_furnitureDown").clear();
			text = (JTextField) jsonKeyToComponents.get("crouchLinks").get(1);
			dreamFragmentsLinks.get("Fragment_souvenir_furnitureDown").add(text.getText());
			text = (JTextField) jsonKeyToComponents.get("crouchButtons").get(1);
			dreamFragmentsLinks.get("Fragment_souvenir_furnitureDown").add(text.getText());
			dreamFragmentsLinks.get("Fragment_souvenir_furnitureUp").clear();
			text = (JTextField) jsonKeyToComponents.get("crouchLinks").get(2);
			dreamFragmentsLinks.get("Fragment_souvenir_furnitureUp").add(text.getText());
			text = (JTextField) jsonKeyToComponents.get("crouchButtons").get(2);
			dreamFragmentsLinks.get("Fragment_souvenir_furnitureUp").add(text.getText());
			dreamFragmentsLinks.get("Fragment_souvenir_table").clear();
			text = (JTextField) jsonKeyToComponents.get("crouchLinks").get(3);
			dreamFragmentsLinks.get("Fragment_souvenir_table").add(text.getText());
			text = (JTextField) jsonKeyToComponents.get("crouchButtons").get(3);
			dreamFragmentsLinks.get("Fragment_souvenir_table").add(text.getText());
			dreamFragmentsLinks.get("Fragment_souvenir_tirroir").clear();
			text = (JTextField) jsonKeyToComponents.get("crouchLinks").get(4);
			dreamFragmentsLinks.get("Fragment_souvenir_tirroir").add(text.getText());
			text = (JTextField) jsonKeyToComponents.get("crouchButtons").get(4);
			dreamFragmentsLinks.get("Fragment_souvenir_tirroir").add(text.getText());
			dreamFragmentsLinks.get("Fragment_souvenir_wall").clear();
			text = (JTextField) jsonKeyToComponents.get("crouchLinks").get(5);
			dreamFragmentsLinks.get("Fragment_souvenir_wall").add(text.getText());
			text = (JTextField) jsonKeyToComponents.get("crouchButtons").get(5);
			dreamFragmentsLinks.get("Fragment_souvenir_wall").add(text.getText());
		}

		// gears content
		text = (JTextField) jsonKeyToComponents.get("gearsQuestion").get(0);
		gameContent.gearsQuestion = text.getText();
		text = (JTextField) jsonKeyToComponents.get("iarHelpGears").get(0);
		gameContent.iarHelpGears = text.getText();
		text = (JTextField) jsonKeyToComponents.get("gearTextUp").get(0);
		gameContent.gearTextUp = text.getText();
		text = (JTextField) jsonKeyToComponents.get("gearTextDown").get(0);
		gameContent.gearTextDown = text.getText();
		length = Integer.min(gameContent.gearMovableTexts.length, jsonKeyToComponents.get("gearMovableTexts").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("gearMovableTexts").get(i);
			gameContent.gearMovableTexts[i] = text.getText();
		}
		text = (JTextField) jsonKeyToComponents.get("gearAnswer").get(0);
		gameContent.gearAnswer = text.getText();

		// mastermind content
		text = (JTextField) jsonKeyToComponents.get("mastermindQuestion").get(0);
		gameContent.mastermindQuestion = text.getText();
		text = (JTextField) jsonKeyToComponents.get("masterMindPlaceholder").get(0);
		gameContent.masterMindPlaceholder = text.getText();
		text = (JTextField) jsonKeyToComponents.get("masterMindPasswordText").get(0);
		gameContent.masterMindPasswordText = text.getText();
		text = (JTextField) jsonKeyToComponents.get("masterMindValidation").get(0);
		gameContent.masterMindValidation = text.getText();
		text = (JTextField) jsonKeyToComponents.get("mastermindQuestionYPos").get(0);
		try{
			gameContent.mastermindQuestionYPos = Integer.parseInt(text.getText());
		}
		catch (Exception e){
			e.printStackTrace();
		}
		text = (JTextField) jsonKeyToComponents.get("mastermindAnswer").get(0);
		try{
			gameContent.mastermindAnswer = Integer.parseInt(text.getText());
		}
		catch (Exception e){
			e.printStackTrace();
		}
		text = (JTextField) jsonKeyToComponents.get("mastermindBackgroundPicturePath").get(0);
		gameContent.mastermindBackgroundPicturePath = text.getText();

		// glasses content
		text = (JTextField) jsonKeyToComponents.get("glassesQuestion").get(0);
		gameContent.glassesQuestion = text.getText();
		text = (JTextField) jsonKeyToComponents.get("glassesPlaceHolder").get(0);
		gameContent.glassesPlaceHolder = text.getText();
		length = jsonKeyToComponents.get("glassesAnswer").size();
		gameContent.glassesAnswer.clear();
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("glassesAnswer").get(i);
			gameContent.glassesAnswer.add(text.getText());
		}
		text = (JTextField) jsonKeyToComponents.get("glassesAnswerFeedback").get(0);
		gameContent.glassesAnswerFeedback = text.getText();
		text = (JTextField) jsonKeyToComponents.get("glassesAnswerFeedbackDesc").get(0);
		gameContent.glassesAnswerFeedbackDesc = text.getText();
		length = Integer.min(gameContent.glassesPicturesPath.length, jsonKeyToComponents.get("glassesPicturesPath").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("glassesPicturesPath").get(i);
			gameContent.glassesPicturesPath[i] = text.getText();
		}

		// enigma 8 content
		text = (JTextField) jsonKeyToComponents.get("enigma08Question").get(0);
		gameContent.enigma08Question = text.getText();
		text = (JTextField) jsonKeyToComponents.get("enigma08PlaceHolder").get(0);
		gameContent.enigma08PlaceHolder = text.getText();
		length = jsonKeyToComponents.get("enigma08Answer").size();
		gameContent.enigma08Answer.clear();
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("enigma08Answer").get(i);
			gameContent.enigma08Answer.add(text.getText());
		}
		text = (JTextField) jsonKeyToComponents.get("enigma08AnswerFeedback").get(0);
		gameContent.enigma08AnswerFeedback = text.getText();
		text = (JTextField) jsonKeyToComponents.get("enigma08AnswerFeedbackDesc").get(0);
		gameContent.enigma08AnswerFeedbackDesc = text.getText();

		// scrolls content
		text = (JTextField) jsonKeyToComponents.get("scrollsQuestion").get(0);
		gameContent.scrollsQuestion = text.getText();
		text = (JTextField) jsonKeyToComponents.get("scrollsPlaceHolder").get(0);
		gameContent.scrollsPlaceHolder = text.getText();
		length = jsonKeyToComponents.get("scrollsAnswer").size();
		gameContent.scrollsAnswer.clear();
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("scrollsAnswer").get(i);
			gameContent.scrollsAnswer.add(text.getText());
		}
		text = (JTextField) jsonKeyToComponents.get("scrollsAnswerFeedback").get(0);
		gameContent.scrollsAnswerFeedback = text.getText();
		text = (JTextField) jsonKeyToComponents.get("scrollsAnswerFeedbackDesc").get(0);
		gameContent.scrollsAnswerFeedbackDesc = text.getText();
		length = Integer.min(gameContent.scrollsWords.length, jsonKeyToComponents.get("scrollsWords").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("scrollsWords").get(i);
			gameContent.scrollsWords[i] = text.getText();
		}

		// mirror content
		text = (JTextField) jsonKeyToComponents.get("mirrorQuestion").get(0);
		gameContent.mirrorQuestion = text.getText();
		text = (JTextField) jsonKeyToComponents.get("mirrorPlaceHolder").get(0);
		gameContent.mirrorPlaceHolder = text.getText();
		length = jsonKeyToComponents.get("mirrorAnswer").size();
		gameContent.mirrorAnswer.clear();
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("mirrorAnswer").get(i);
			gameContent.mirrorAnswer.add(text.getText());
		}
		text = (JTextField) jsonKeyToComponents.get("mirrorAnswerFeedback").get(0);
		gameContent.mirrorAnswerFeedback = text.getText();
		text = (JTextField) jsonKeyToComponents.get("mirrorAnswerFeedbackDesc").get(0);
		gameContent.mirrorAnswerFeedbackDesc = text.getText();
		text = (JTextField) jsonKeyToComponents.get("mirrorPicturePath").get(0);
		gameContent.mirrorPicturePath = text.getText();

		// enigma 11 content
		text = (JTextField) jsonKeyToComponents.get("enigma11Question").get(0);
		gameContent.enigma11Question = text.getText();
		text = (JTextField) jsonKeyToComponents.get("enigma11PlaceHolder").get(0);
		gameContent.enigma11PlaceHolder = text.getText();
		length = jsonKeyToComponents.get("enigma11Answer").size();
		gameContent.enigma11Answer.clear();
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("enigma11Answer").get(i);
			gameContent.enigma11Answer.add(text.getText());
		}
		text = (JTextField) jsonKeyToComponents.get("enigma11AnswerFeedback").get(0);
		gameContent.enigma11AnswerFeedback = text.getText();
		text = (JTextField) jsonKeyToComponents.get("enigma11AnswerFeedbackDesc").get(0);
		gameContent.enigma11AnswerFeedbackDesc = text.getText();

		// enigma 12 content
		text = (JTextField) jsonKeyToComponents.get("enigma12Question").get(0);
		gameContent.enigma12Question = text.getText();
		text = (JTextField) jsonKeyToComponents.get("enigma12PlaceHolder").get(0);
		gameContent.enigma12PlaceHolder = text.getText();
		length = jsonKeyToComponents.get("enigma12Answer").size();
		gameContent.enigma12Answer.clear();
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("enigma12Answer").get(i);
			gameContent.enigma12Answer.add(text.getText());
		}
		text = (JTextField) jsonKeyToComponents.get("enigma11AnswerFeedback").get(0);
		gameContent.enigma11AnswerFeedback = text.getText();
		text = (JTextField) jsonKeyToComponents.get("enigma11AnswerFeedbackDesc").get(0);
		gameContent.enigma11AnswerFeedbackDesc = text.getText();

		// lock room 2 content
		text = (JTextField) jsonKeyToComponents.get("lockRoom2Password").get(0);
		try{
			gameContent.lockRoom2Password = Integer.parseInt(text.getText());
		}
		catch (Exception e){
			e.printStackTrace();
		}

		// puzzle content
		length = jsonKeyToComponents.get("puzzleAnswer").size();
		concat = new StringBuilder();
		if(length > 0) {
			text = (JTextField) jsonKeyToComponents.get("puzzleAnswer").get(0);
			concat = new StringBuilder(text.getText());
		}
		for(int i = 1; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("puzzleAnswer").get(i);
			concat.append("##").append(text.getText());
		}
		gameContent.puzzleAnswer = concat.toString();
		text = (JTextField) jsonKeyToComponents.get("puzzleAnswerFeedback").get(0);
		gameContent.puzzleAnswerFeedback = text.getText();
		text = (JTextField) jsonKeyToComponents.get("puzzleAnswerFeedbackDesc").get(0);
		gameContent.puzzleAnswerFeedbackDesc = text.getText();
		text = (JTextField) jsonKeyToComponents.get("puzzlePicturePath").get(0);
		gameContent.puzzlePicturePath = text.getText();

		// lamp content
		length = jsonKeyToComponents.get("lampAnswer").size();
		concat = new StringBuilder();
		if(length > 0) {
			text = (JTextField) jsonKeyToComponents.get("lampAnswer").get(0);
			concat = new StringBuilder(text.getText());
		}
		for(int i = 1; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("lampAnswer").get(i);
			concat.append("##").append(text.getText());
		}
		gameContent.lampAnswer = concat.toString();
		text = (JTextField) jsonKeyToComponents.get("lampAnswerFeedback").get(0);
		gameContent.lampAnswerFeedback = text.getText();
		text = (JTextField) jsonKeyToComponents.get("lampAnswerFeedbackDesc").get(0);
		gameContent.lampAnswerFeedbackDesc = text.getText();
		length = Integer.min(gameContent.lampPicturesPath.length, jsonKeyToComponents.get("lampPicturesPath").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("lampPicturesPath").get(i);
			gameContent.lampPicturesPath[i] = text.getText();
		}

		// white board content
		length = jsonKeyToComponents.get("whiteBoardAnswer").size();
		concat = new StringBuilder();
		if(length > 0) {
			text = (JTextField) jsonKeyToComponents.get("whiteBoardAnswer").get(0);
			concat = new StringBuilder(text.getText());
		}
		for(int i = 1; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("whiteBoardAnswer").get(i);
			concat.append("##").append(text.getText());
		}
		gameContent.whiteBoardAnswer = concat.toString();
		text = (JTextField) jsonKeyToComponents.get("whiteBoardAnswerFeedback").get(0);
		gameContent.whiteBoardAnswerFeedback = text.getText();
		text = (JTextField) jsonKeyToComponents.get("whiteBoardAnswerFeedbackDesc").get(0);
		gameContent.whiteBoardAnswerFeedbackDesc = text.getText();
		length = Integer.min(gameContent.whiteBoardWords.length, jsonKeyToComponents.get("whiteBoardWords").size());
		for(int i = 0; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("whiteBoardWords").get(i);
			gameContent.whiteBoardWords[i] = text.getText();
		}

		// enigma 16 content
		length = jsonKeyToComponents.get("enigma16Answer").size();
		concat = new StringBuilder();
		if(length > 0) {
			text = (JTextField) jsonKeyToComponents.get("enigma16Answer").get(0);
			concat = new StringBuilder(text.getText());
		}
		for(int i = 1; i < length; i++){
			text = (JTextField) jsonKeyToComponents.get("enigma16Answer").get(i);
			concat.append("##").append(text.getText());
		}
		gameContent.enigma16Answer = concat.toString();
		text = (JTextField) jsonKeyToComponents.get("enigma16AnswerFeedback").get(0);
		gameContent.enigma16AnswerFeedback = text.getText();
		text = (JTextField) jsonKeyToComponents.get("enigma16AnswerFeedbackDesc").get(0);
		gameContent.enigma16AnswerFeedbackDesc = text.getText();
	}

	private void getInventoryContentFromWindow(){
		JTextField text;

		gameContent.inventoryScrollIntro.clear();
		text = (JTextField) jsonKeyToComponents.get("inventoryScrollIntroTitle").get(0);
		gameContent.inventoryScrollIntro.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryScrollIntroDescription").get(0);
		gameContent.inventoryScrollIntro.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryScrollIntroNotice").get(0);
		gameContent.inventoryScrollIntro.add(text.getText());

		gameContent.inventoryKeyBallBox.clear();
		text = (JTextField) jsonKeyToComponents.get("inventoryKeyBallBoxTitle").get(0);
		gameContent.inventoryKeyBallBox.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryKeyBallBoxDescription").get(0);
		gameContent.inventoryKeyBallBox.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryKeyBallBoxNotice").get(0);
		gameContent.inventoryKeyBallBox.add(text.getText());

		gameContent.inventoryWire.clear();
		text = (JTextField) jsonKeyToComponents.get("inventoryWireTitle").get(0);
		gameContent.inventoryWire.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryWireDescription").get(0);
		gameContent.inventoryWire.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryWireNotice").get(0);
		gameContent.inventoryWire.add(text.getText());

		gameContent.inventoryKeySatchel.clear();
		text = (JTextField) jsonKeyToComponents.get("inventoryKeySatchelTitle").get(0);
		gameContent.inventoryKeySatchel.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryKeySatchelDescription").get(0);
		gameContent.inventoryKeySatchel.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryKeySatchelNotice").get(0);
		gameContent.inventoryKeySatchel.add(text.getText());

		gameContent.inventoryScrolls.clear();
		text = (JTextField) jsonKeyToComponents.get("inventoryScrollsTitle").get(0);
		gameContent.inventoryScrolls.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryScrollsDescription").get(0);
		gameContent.inventoryScrolls.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryScrollsNotice").get(0);
		gameContent.inventoryScrolls.add(text.getText());

		gameContent.inventoryGlasses1.clear();
		text = (JTextField) jsonKeyToComponents.get("inventoryGlasses1Title").get(0);
		gameContent.inventoryGlasses1.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryGlasses1Description").get(0);
		gameContent.inventoryGlasses1.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryGlasses1Notice").get(0);
		gameContent.inventoryGlasses1.add(text.getText());

		gameContent.inventoryGlasses2.clear();
		text = (JTextField) jsonKeyToComponents.get("inventoryGlasses2Title").get(0);
		gameContent.inventoryGlasses2.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryGlasses2Description").get(0);
		gameContent.inventoryGlasses2.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryGlasses2Notice").get(0);
		gameContent.inventoryGlasses2.add(text.getText());

		gameContent.inventoryMirror.clear();
		text = (JTextField) jsonKeyToComponents.get("inventoryMirrorTitle").get(0);
		gameContent.inventoryMirror.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryMirrorDescription").get(0);
		gameContent.inventoryMirror.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryMirrorNotice").get(0);
		gameContent.inventoryMirror.add(text.getText());

		gameContent.inventoryLamp.clear();
		text = (JTextField) jsonKeyToComponents.get("inventoryLampTitle").get(0);
		gameContent.inventoryLamp.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryLampDescription").get(0);
		gameContent.inventoryLamp.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryLampNotice").get(0);
		gameContent.inventoryLamp.add(text.getText());

		gameContent.inventoryPuzzle.clear();
		text = (JTextField) jsonKeyToComponents.get("inventoryPuzzleTitle").get(0);
		gameContent.inventoryPuzzle.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryPuzzleDescription").get(0);
		gameContent.inventoryPuzzle.add(text.getText());
		text = (JTextField) jsonKeyToComponents.get("inventoryPuzzleNotice").get(0);
		gameContent.inventoryPuzzle.add(text.getText());
	}

	private void getDreamFragmentsContentFromWindow(){
		JTextField text;
		String name;

		for(int i = 0; i < 19; i++){
			name = "Fragment_souvenir_" + i;
			dreamFragmentsLinks.get(name).clear();
			text = (JTextField) jsonKeyToComponents.get(name + "Link").get(0);
			dreamFragmentsLinks.get(name).add(text.getText());
			text = (JTextField) jsonKeyToComponents.get(name + "Button").get(0);
			dreamFragmentsLinks.get(name).add(text.getText());
		}
	}

	private void getUIContentFromWindow(){
		JTextField text;

		text = (JTextField) jsonKeyToComponents.get("loadingText").get(0);
		gameContent.loadingText = text.getText();
		text = (JTextField) jsonKeyToComponents.get("mainMenuStart").get(0);
		gameContent.mainMenuStart = text.getText();
		text = (JTextField) jsonKeyToComponents.get("mainMenuLoad").get(0);
		gameContent.mainMenuLoad = text.getText();
		text = (JTextField) jsonKeyToComponents.get("mainMenuOption").get(0);
		gameContent.mainMenuOption = text.getText();
		text = (JTextField) jsonKeyToComponents.get("mainMenuLeave").get(0);
		gameContent.mainMenuLeave = text.getText();
		text = (JTextField) jsonKeyToComponents.get("sessionIDText").get(0);
		gameContent.sessionIDText = text.getText();
		text = (JTextField) jsonKeyToComponents.get("sessionIDPopup").get(0);
		gameContent.sessionIDPopup = text.getText();
		text = (JTextField) jsonKeyToComponents.get("endLinkButtonText").get(0);
		gameContent.endLinkButtonText = text.getText();
		text = (JTextField) jsonKeyToComponents.get("endLeaveButtonText").get(0);
		gameContent.endLeaveButtonText = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionControlsMenu").get(0);
		gameContent.optionControlsMenu = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionVirtualFragments").get(0);
		gameContent.optionVirtualFragments = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionMovingTexts").get(0);
		gameContent.optionMovingTexts = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionMoveSpeed").get(0);
		gameContent.optionMoveSpeed = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionCameraSensitivity").get(0);
		gameContent.optionCameraSensitivity = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionLockWheelSpeed").get(0);
		gameContent.optionLockWheelSpeed = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionInputs").get(0);
		gameContent.optionInputs = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionSoundMenu").get(0);
		gameContent.optionSoundMenu = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionGameMusicVolume").get(0);
		gameContent.optionGameMusicVolume = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionSoundEffectsVolume").get(0);
		gameContent.optionSoundEffectsVolume = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionDisplayMenu").get(0);
		gameContent.optionDisplayMenu = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionFont").get(0);
		gameContent.optionFont = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionCursorSize").get(0);
		gameContent.optionCursorSize = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionLightIntensity").get(0);
		gameContent.optionLightIntensity = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionTransparency").get(0);
		gameContent.optionTransparency = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionValidateAndReturn").get(0);
		gameContent.optionValidateAndReturn = text.getText();
		text = (JTextField) jsonKeyToComponents.get("optionDefault").get(0);
		gameContent.optionDefault = text.getText();
		text = (JTextField) jsonKeyToComponents.get("loadPopupLoadButton").get(0);
		gameContent.loadPopupLoadButton = text.getText();
		text = (JTextField) jsonKeyToComponents.get("loadPopupCancelButton").get(0);
		gameContent.loadPopupCancelButton = text.getText();
		text = (JTextField) jsonKeyToComponents.get("validateOptions").get(0);
		gameContent.validateOptions = text.getText();
		text = (JTextField) jsonKeyToComponents.get("storyClickToContinue").get(0);
		gameContent.storyClickToContinue = text.getText();
		text = (JTextField) jsonKeyToComponents.get("hudObserve").get(0);
		gameContent.hudObserve = text.getText();
		text = (JTextField) jsonKeyToComponents.get("hudMove").get(0);
		gameContent.hudMove = text.getText();
		text = (JTextField) jsonKeyToComponents.get("hudCrouch").get(0);
		gameContent.hudCrouch = text.getText();
		text = (JTextField) jsonKeyToComponents.get("hudInventory").get(0);
		gameContent.hudInventory = text.getText();
		text = (JTextField) jsonKeyToComponents.get("hudDreamFragments").get(0);
		gameContent.hudDreamFragments = text.getText();
		text = (JTextField) jsonKeyToComponents.get("hudQuestions").get(0);
		gameContent.hudQuestions = text.getText();
		text = (JTextField) jsonKeyToComponents.get("hudHelp").get(0);
		gameContent.hudHelp = text.getText();
		text = (JTextField) jsonKeyToComponents.get("hudMenu").get(0);
		gameContent.hudMenu = text.getText();
		text = (JTextField) jsonKeyToComponents.get("dreamFragmentText").get(0);
		gameContent.dreamFragmentText = text.getText();
		text = (JTextField) jsonKeyToComponents.get("dreamFragmentValidation").get(0);
		gameContent.dreamFragmentValidation = text.getText();
		text = (JTextField) jsonKeyToComponents.get("dreamFragmentVirtualReset").get(0);
		gameContent.dreamFragmentVirtualReset = text.getText();
		text = (JTextField) jsonKeyToComponents.get("iarTabInventory").get(0);
		gameContent.iarTabInventory = text.getText();
		text = (JTextField) jsonKeyToComponents.get("iarTabDreamFragments").get(0);
		gameContent.iarTabDreamFragments = text.getText();
		text = (JTextField) jsonKeyToComponents.get("iarTabHelp").get(0);
		gameContent.iarTabHelp = text.getText();
		text = (JTextField) jsonKeyToComponents.get("iarTabMenu").get(0);
		gameContent.iarTabMenu = text.getText();
		text = (JTextField) jsonKeyToComponents.get("iarTabQuestions1").get(0);
		gameContent.iarTabQuestions1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("iarTabQuestions2").get(0);
		gameContent.iarTabQuestions2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("iarTabQuestions3").get(0);
		gameContent.iarTabQuestions3 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("questionValidationButton").get(0);
		gameContent.questionValidationButton = text.getText();
		text = (JTextField) jsonKeyToComponents.get("hintButtonText").get(0);
		gameContent.hintButtonText = text.getText();
		text = (JTextField) jsonKeyToComponents.get("getHintButton").get(0);
		gameContent.getHintButton = text.getText();
		text = (JTextField) jsonKeyToComponents.get("hintOpenURL").get(0);
		gameContent.hintOpenURL = text.getText();
		text = (JTextField) jsonKeyToComponents.get("gameMenuResumeButton").get(0);
		gameContent.gameMenuResumeButton = text.getText();
		text = (JTextField) jsonKeyToComponents.get("gameMenuSaveButton").get(0);
		gameContent.gameMenuSaveButton = text.getText();
		text = (JTextField) jsonKeyToComponents.get("gameMenuSaveNotice").get(0);
		gameContent.gameMenuSaveNotice = text.getText();
		text = (JTextField) jsonKeyToComponents.get("gameMenuOptionButton").get(0);
		gameContent.gameMenuOptionButton = text.getText();
		text = (JTextField) jsonKeyToComponents.get("gameMenuRestartButton").get(0);
		gameContent.gameMenuRestartButton = text.getText();
		text = (JTextField) jsonKeyToComponents.get("gameMenuLeaveButton").get(0);
		gameContent.gameMenuLeaveButton = text.getText();
		text = (JTextField) jsonKeyToComponents.get("savePopupSaveButton").get(0);
		gameContent.savePopupSaveButton = text.getText();
		text = (JTextField) jsonKeyToComponents.get("savePopupCancelButton").get(0);
		gameContent.savePopupCancelButton = text.getText();
		text = (JTextField) jsonKeyToComponents.get("savePopupPlaceholder").get(0);
		gameContent.savePopupPlaceholder = text.getText();
		text = (JTextField) jsonKeyToComponents.get("savePopupInvalidText").get(0);
		gameContent.savePopupInvalidText = text.getText();
		text = (JTextField) jsonKeyToComponents.get("savePopupInvalidButton").get(0);
		gameContent.savePopupInvalidButton = text.getText();
		text = (JTextField) jsonKeyToComponents.get("savePopupOverrideText").get(0);
		gameContent.savePopupOverrideText = text.getText();
		text = (JTextField) jsonKeyToComponents.get("savePopupOverrideYesButton").get(0);
		gameContent.savePopupOverrideYesButton = text.getText();
		text = (JTextField) jsonKeyToComponents.get("savePopupOverrideNoButton").get(0);
		gameContent.savePopupOverrideNoButton = text.getText();
		text = (JTextField) jsonKeyToComponents.get("savePopupDoneText").get(0);
		gameContent.savePopupDoneText = text.getText();
		text = (JTextField) jsonKeyToComponents.get("savePopupDoneButton").get(0);
		gameContent.savePopupDoneButton = text.getText();
		text = (JTextField) jsonKeyToComponents.get("tutoText0").get(0);
		gameContent.tutoText0 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("tutoText1").get(0);
		gameContent.tutoText1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("tutoText2").get(0);
		gameContent.tutoText2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("tutoText3").get(0);
		gameContent.tutoText3 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("tutoText4").get(0);
		gameContent.tutoText4 = text.getText();
	}

	private void getCommandsContentFromWindow(){
		JTextField text;

		text = (JTextField) jsonKeyToComponents.get("inputsSetTitle1").get(0);
		gameContent.inputsSetTitle1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputsSetTitle2").get(0);
		gameContent.inputsSetTitle2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputsSetTitle3").get(0);
		gameContent.inputsSetTitle3 = text.getText();

		text = (JTextField) jsonKeyToComponents.get("inputObserve").get(0);
		gameContent.inputObserve = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputObserveKey1").get(0);
		gameContent.inputObserveKey1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputObserveKey2").get(0);
		gameContent.inputObserveKey2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputObserveKey3").get(0);
		gameContent.inputObserveKey3 = text.getText();

		text = (JTextField) jsonKeyToComponents.get("inputForward").get(0);
		gameContent.inputForward = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputForwardKey1").get(0);
		gameContent.inputForwardKey1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputForwardKey2").get(0);
		gameContent.inputForwardKey2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputForwardKey3").get(0);
		gameContent.inputForwardKey3 = text.getText();

		text = (JTextField) jsonKeyToComponents.get("inputBack").get(0);
		gameContent.inputBack = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputBackKey1").get(0);
		gameContent.inputBackKey1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputBackKey2").get(0);
		gameContent.inputBackKey2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputBackKey3").get(0);
		gameContent.inputBackKey3 = text.getText();

		text = (JTextField) jsonKeyToComponents.get("inputLeft").get(0);
		gameContent.inputLeft = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputLeftKey1").get(0);
		gameContent.inputLeftKey1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputLeftKey2").get(0);
		gameContent.inputLeftKey2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputLeftKey3").get(0);
		gameContent.inputLeftKey3 = text.getText();

		text = (JTextField) jsonKeyToComponents.get("inputRight").get(0);
		gameContent.inputRight = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputRightKey1").get(0);
		gameContent.inputRightKey1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputRightKey2").get(0);
		gameContent.inputRightKey2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputRightKey3").get(0);
		gameContent.inputRightKey3 = text.getText();

		text = (JTextField) jsonKeyToComponents.get("inputCrouch").get(0);
		gameContent.inputCrouch = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputCrouchKey1").get(0);
		gameContent.inputCrouchKey1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputCrouchKey2").get(0);
		gameContent.inputCrouchKey2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputCrouchKey3").get(0);
		gameContent.inputCrouchKey3 = text.getText();

		text = (JTextField) jsonKeyToComponents.get("inputInteract").get(0);
		gameContent.inputInteract = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputInteractKey1").get(0);
		gameContent.inputInteractKey1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputInteractKey2").get(0);
		gameContent.inputInteractKey2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputInteractKey3").get(0);
		gameContent.inputInteractKey3 = text.getText();

		text = (JTextField) jsonKeyToComponents.get("inputInventory").get(0);
		gameContent.inputInventory = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputInventoryKey1").get(0);
		gameContent.inputInventoryKey1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputInventoryKey2").get(0);
		gameContent.inputInventoryKey2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputInventoryKey3").get(0);
		gameContent.inputInventoryKey3 = text.getText();

		text = (JTextField) jsonKeyToComponents.get("inputDreamFragments").get(0);
		gameContent.inputDreamFragments = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputDreamFragmentsKey1").get(0);
		gameContent.inputDreamFragmentsKey1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputDreamFragmentsKey2").get(0);
		gameContent.inputDreamFragmentsKey2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputDreamFragmentsKey3").get(0);
		gameContent.inputDreamFragmentsKey3 = text.getText();

		text = (JTextField) jsonKeyToComponents.get("inputQuestions").get(0);
		gameContent.inputQuestions = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputQuestionsKey1").get(0);
		gameContent.inputQuestionsKey1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputQuestionsKey2").get(0);
		gameContent.inputQuestionsKey2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputQuestionsKey3").get(0);
		gameContent.inputQuestionsKey3 = text.getText();

		text = (JTextField) jsonKeyToComponents.get("inputHelp").get(0);
		gameContent.inputHelp = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputHelpKey1").get(0);
		gameContent.inputHelpKey1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputHelpKey2").get(0);
		gameContent.inputHelpKey2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputHelpKey3").get(0);
		gameContent.inputHelpKey3 = text.getText();

		text = (JTextField) jsonKeyToComponents.get("inputMenu").get(0);
		gameContent.inputMenu = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputMenuKey1").get(0);
		gameContent.inputMenuKey1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputMenuKey2").get(0);
		gameContent.inputMenuKey2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputMenuKey3").get(0);
		gameContent.inputMenuKey3 = text.getText();

		text = (JTextField) jsonKeyToComponents.get("inputView").get(0);
		gameContent.inputView = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputViewKey1").get(0);
		gameContent.inputViewKey1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputViewKey2").get(0);
		gameContent.inputViewKey2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputViewKey3").get(0);
		gameContent.inputViewKey3 = text.getText();

		text = (JTextField) jsonKeyToComponents.get("inputTarget").get(0);
		gameContent.inputTarget = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputTargetKey1").get(0);
		gameContent.inputTargetKey1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputTargetKey2").get(0);
		gameContent.inputTargetKey2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputTargetKey3").get(0);
		gameContent.inputTargetKey3 = text.getText();

		text = (JTextField) jsonKeyToComponents.get("inputZoomIn").get(0);
		gameContent.inputZoomIn = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputZoomInKey1").get(0);
		gameContent.inputZoomInKey1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputZoomInKey2").get(0);
		gameContent.inputZoomInKey2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputZoomInKey3").get(0);
		gameContent.inputZoomInKey3 = text.getText();

		text = (JTextField) jsonKeyToComponents.get("inputZoomOut").get(0);
		gameContent.inputZoomOut = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputZoomOutKey1").get(0);
		gameContent.inputZoomOutKey1 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputZoomOutKey2").get(0);
		gameContent.inputZoomOutKey2 = text.getText();
		text = (JTextField) jsonKeyToComponents.get("inputZoomOutKey3").get(0);
		gameContent.inputZoomOutKey3 = text.getText();
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
    	content.add(CreateTextInputFieldLine("Theme du jeu:", "theme"), gbc);
		gbc.gridy = 3;
		content.add(CreateToggleLine("Activation aléatoire du système d'aide:", "randomHelpSystemActivation"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateToggleLine("Système d'aide:", "helpSystem",
				(JCheckBox) jsonKeyToComponents.get("randomHelpSystemActivation").get(0), false, false), gbc);
		gbc.gridy = 1;
		content.add(CreateToggleLine("Traces de Laalys:", "trace",
				(JCheckBox) jsonKeyToComponents.get("helpSystem").get(0), false, false), gbc);
    	gbc.gridy = 4;
    	content.add(CreateToggleLine("Traces vers le LRS:", "traceToLRS"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateFloatInputFieldLine("Fréquence de trace des déplacements:", "traceMovementFrequency"), gbc);
    	gbc.gridy = 7;
    	content.add(CreateToggleLine("Puzzles virtuels:", "virtualPuzzle"), gbc);
		gbc.gridy = 6;
		content.add(CreateToggleLine("Fragments de rêves virtuels:", "virtualDreamFragment",
				(JCheckBox) jsonKeyToComponents.get("virtualPuzzle").get(0), true, true), gbc);
    	gbc.gridy = 8;
    	content.add(CreateToggleLine("Salle de fin:", "useEndRoom"), gbc);
		gbc.gridy = 10;
		content.add(CreateToggleLine("Sauvegarde automatique:", "autoSaveProgression"), gbc);
    	gbc.gridy = 9;
    	content.add(CreateToggleLine("Sauvegarde et chargement:", "saveAndLoadProgression",
				(JCheckBox) jsonKeyToComponents.get("autoSaveProgression").get(0), false, false), gbc);
    	gbc.gridy = 11;
    	content.add(CreateCategory("Chemins de fichiers", GenerateFilesPathsUI()), gbc);
    	gbc.gridy = 12;
    	content.add(CreateToggleLine("RemoveExtraGeometries:", "removeExtraGeometries"), gbc);
		gbc.gridy = 13;
		content.add(CreateTextAreaLine("Crédits", "additionalCredit"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Chemin configuration LRS:", "lrsConfigPath"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Chemin liens fragments de rêves:", "dreamFragmentLinksPath"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Chemin documents fragments de rêves:", "dreamFragmentDocumentsPathFile"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Chemin indices:", "hintsPath"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Chemin indices internes au jeu:", "internalHintsPath"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Chemin feedback de mauvaise réponse:", "wrongAnswerFeedbacksPath"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Chemin poids des énigmes:", "enigmasWeightPath"), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Chemin poids des labels:", "labelWeightsPath"), gbc);
    	gbc.gridy = 8;
    	content.add(CreateTextInputFieldLine("Chemin configuration système d'aide:", "helpSystemConfigPath"), gbc);
    	gbc.gridy = 9;
    	content.add(CreateTextAreaLine("Chemins de logos additionnels", "additionalLogosPath"), gbc);
    	
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
    	content.add(CreateTextAreaLine("Texte d'introduction", "storyTextIntro"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextAreaLine("Texte de transition de la salle 1 à la salle 2", "storyTextransition"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextAreaLine("Texte de fin", "storyTextEnd"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Texte de score:", "scoreText"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Texte d'explication final:", "endExplainationText"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Lien final:", "endLink"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateToggleLine("Concaténer l'id de session au lien:", "concatIdToLink"), gbc);
    	
    	
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
    	content.add(CreateTextInputFieldLine("Titre du parchemin dans l'inventaire:", "inventoryScrollIntroTitle"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Description du parchemin dans l'inventaire:", "inventoryScrollIntroDescription"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Indication pour le parchemin dans l'inventaire:", "inventoryScrollIntroNotice"), gbc);
    	
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
    	content.add(CreateToggleLine("Position aléatoire des balles:", "ballRandomPositioning"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:", "ballBoxQuestion"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:", "ballBoxPlaceHolder"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:", "ballBoxAnswerFeedback"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:", "ballBoxAnswerFeedbackDesc"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR", "ballBoxAnswer"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateIntInputFieldLine("Numéro des balles solutions:", "ballBoxThreeUsefulBalls", 3), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Textes des balles:", "ballTexts", 10), gbc);
    	gbc.gridy = 8;
    	content.add(CreateTextInputFieldLine("Titre de la clé dans l'inventaire:", "inventoryKeyBallBoxTitle"), gbc);
    	gbc.gridy = 9;
    	content.add(CreateTextInputFieldLine("Description de la clé dans l'inventaire:", "inventoryKeyBallBoxDescription"), gbc);
    	gbc.gridy = 10;
    	content.add(CreateTextInputFieldLine("Indication pour la clé dans l'inventaire:", "inventoryKeyBallBoxNotice"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:", "plankAndWireQuestionIAR"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:", "plankAndWirePlaceHolder"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Enoncé affiché sur le tableau en jeu:", "plankAndWireQuestion"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:", "plankAndWireAnswerFeedback"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:", "plankAndWireAnswerFeedbackDesc"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Réponses attendues dans l'IAR:", "plankAndWireCorrectNumbers", 3), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Mauvaises réponses affichées sur le tableau:", "plankAndWireOtherNumbers", 6), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Mots solutions sur le tableau:", "plankAndWireCorrectWords", 3), gbc);
    	gbc.gridy = 8;
    	content.add(CreateTextInputFieldLine("Mauvais mots affichés sur le tableau:", "plankOtherWords", 10), gbc);
    	gbc.gridy = 9;
    	content.add(CreateTextInputFieldLine("Titre de la corde dans l'inventaire:", "inventoryWireTitle"), gbc);
    	gbc.gridy = 10;
    	content.add(CreateTextInputFieldLine("Description de la corde dans l'inventaire:", "inventoryWireDescription"), gbc);
    	gbc.gridy = 11;
    	content.add(CreateTextInputFieldLine("Indication pour la corde dans l'inventaire:", "inventoryWireNotice"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:", "crouchQuestion"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:", "crouchPlaceHolder"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextAreaLine("Réponses attendues", "crouchAnswer"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:", "crouchAnswerFeedback"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:", "crouchAnswerFeedbackDesc"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Textes des fragments de rêves:", "crouchWords", 6), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Liens des fragments de rêves:", "crouchLinks", 6), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Textes des boutons vers les liens des fragments de rêves:", "crouchButtons", 6), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Question de l'énigme:", "gearsQuestion"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Indication pour l'énigme:", "iarHelpGears"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Texte de l'engrenage du haut:", "gearTextUp"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Texte de l'engranage du bas:", "gearTextDown"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Textes des engrenages déplaçables:", "gearMovableTexts", 4), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Réponse attendue:", "gearAnswer"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Question du mastermind:", "mastermindQuestion"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse:", "masterMindPlaceholder"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Intitulé du champ de réponse:", "masterMindPasswordText"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Texte du boutton de validation:", "masterMindValidation"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateFloatInputFieldLine("Position Y de la question sur l'objet dans la scène:", "mastermindQuestionYPos"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateIntInputFieldLine("Réponse attendue:", "mastermindAnswer"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Chemin du fichier image du background du mastermind:", "mastermindBackgroundPicturePath"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:", "glassesQuestion"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:", "glassesPlaceHolder"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR", "glassesAnswer"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:", "glassesAnswerFeedback"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:", "glassesAnswerFeedbackDesc"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Chemins des fichiers image pour le sac:", "glassesPicturesPath", 4), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Titre de la clé du sac dans l'inventaire:", "inventoryKeySatchelTitle"), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Description de la clé du sac dans l'inventaire:", "inventoryKeySatchelDescription"), gbc);
    	gbc.gridy = 8;
    	content.add(CreateTextInputFieldLine("Indication pour la clé du sac dans l'inventaire:", "inventoryKeySatchelNotice"), gbc);
    	gbc.gridy = 9;
    	content.add(CreateTextInputFieldLine("Titre de la lunette rouge (droite) dans l'inventaire:", "inventoryGlasses1Title"), gbc);
    	gbc.gridy = 10;
    	content.add(CreateTextInputFieldLine("Description de la lunette rouge (droite) dans l'inventaire:", "inventoryGlasses1Description"), gbc);
    	gbc.gridy = 11;
    	content.add(CreateTextInputFieldLine("Indication pour la lunette rouge (droite) dans l'inventaire:", "inventoryGlasses1Notice"), gbc);
    	gbc.gridy = 12;
    	content.add(CreateTextInputFieldLine("Titre de la lunette jaune (gauche) dans l'inventaire:", "inventoryGlasses2Title"), gbc);
    	gbc.gridy = 13;
    	content.add(CreateTextInputFieldLine("Description de la lunette jaune (gauche) dans l'inventaire:", "inventoryGlasses2Description"), gbc);
    	gbc.gridy = 14;
    	content.add(CreateTextInputFieldLine("Indication pour la lunette jaune (gauche) dans l'inventaire:", "inventoryGlasses2Notice"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:", "enigma08Question"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:", "enigma08PlaceHolder"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR", "enigma08Answer"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:", "enigma08AnswerFeedback"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:", "enigma08AnswerFeedbackDesc"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:", "scrollsQuestion"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:", "scrollsPlaceHolder"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR", "scrollsAnswer"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:", "scrollsAnswerFeedback"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:", "scrollsAnswerFeedbackDesc"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Textes des parchemins:", "scrollsWords", 5), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Titre de l'ensemble des parchemins dans l'inventaire:", "inventoryScrollsTitle"), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Description de l'ensemble des parchemins dans l'inventaire:", "inventoryScrollsDescription"), gbc);
    	gbc.gridy = 8;
    	content.add(CreateTextInputFieldLine("Indication pour l'ensemble des parchemins dans l'inventaire:", "inventoryScrollsNotice"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:", "mirrorQuestion"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:", "mirrorPlaceHolder"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR", "mirrorAnswer"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:", "mirrorAnswerFeedback"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:", "mirrorAnswerFeedbackDesc"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Chemin du fichier image pour la planche:", "mirrorPicturePath"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Titre du miroir dans l'inventaire:", "inventoryMirrorTitle"), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Description du miroir dans l'inventaire:", "inventoryMirrorDescription"), gbc);
    	gbc.gridy = 8;
    	content.add(CreateTextInputFieldLine("Indication pour le miroir dans l'inventaire:", "inventoryMirrorNotice"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:", "enigma11Question"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:", "enigma11PlaceHolder"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR", "enigma11Answer"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:", "enigma11AnswerFeedback"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:", "enigma11AnswerFeedbackDesc"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Question dans l'IAR:", "enigma12Question"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Placeholder pour le champ de réponse dans l'IAR:", "enigma12PlaceHolder"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR", "enigma12Answer"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:", "enigma12AnswerFeedback"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:", "enigma12AnswerFeedbackDesc"), gbc);
    	
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
    	content.add(CreateIntInputFieldLine("Mot de passe:", "lockRoom2Password"), gbc);
    	
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
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR", "puzzleAnswer"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:", "puzzleAnswerFeedback"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:", "puzzleAnswerFeedbackDesc"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Chemin de l'image pour le puzzle:", "puzzlePicturePath"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Titre du puzzle dans l'inventaire:", "inventoryPuzzleTitle"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Description du puzzle dans l'inventaire:", "inventoryPuzzleDescription"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Indication pour le puzzle dans l'inventaire:", "inventoryPuzzleNotice"), gbc);
    	
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
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR", "lampAnswer"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:", "lampAnswerFeedback"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:", "lampAnswerFeedbackDesc"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Chemins des images éclairables par la lampe:", "lampPicturesPath", 6), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Titre de la lampe dans l'inventaire:", "inventoryLampTitle"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Description de la lampe dans l'inventaire:", "inventoryLampDescription"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Indication pour la lampe dans l'inventaire:", "inventoryLampNotice"), gbc);
    	
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
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR", "whiteBoardAnswer"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:", "whiteBoardAnswerFeedback"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:", "whiteBoardAnswerFeedbackDesc"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Liste des textes du tableau:", "whiteBoardWords", 12), gbc);
    	
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
    	content.add(CreateTextAreaLine("Réponses attendues dans l'IAR", "enigma16Answer"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Feedback de bonne réponse:", "enigma16AnswerFeedback"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Description de la bonne réponse:", "enigma16AnswerFeedbackDesc"), gbc);
    	
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
    	content.add(CreateDreamFragmentCategory(0, "introduction"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Chargement:", "loadingText"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Commencer:", "mainMenuStart"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Charger:", "mainMenuLoad"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Options:", "mainMenuOption"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Quitter:", "mainMenuLeave"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Intitulé de l'ID de session:", "sessionIDText"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Indication pour l'ID de session:", "sessionIDPopup"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Paramètres de contrôles:", "optionControlsMenu"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Paramètres de son:", "optionSoundMenu"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Paramètres d'affichage:", "optionDisplayMenu"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Bouton de validation des Paramètres:", "validateOptions"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Bouton de validation des sous catégories:", "optionValidateAndReturn"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Bouton de réinitialisation des Paramètres:", "optionDefault"), gbc);
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
    	content.add(CreateTextInputFieldLine("Fragments de rêves virtuels:", "optionVirtualFragments"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Textes en mouvement:", "optionMovingTexts"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Vitesse de déplacement:", "optionMoveSpeed"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Sensibilité de la caméra:", "optionCameraSensitivity"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Vitesse des roues des verrous:", "optionLockWheelSpeed"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Commandes:", "optionInputs"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Volume de la musique:", "optionGameMusicVolume"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Volume des effets sonores:", "optionSoundEffectsVolume"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Police accessible:", "optionFont"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Taille du viseur:", "optionCursorSize"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Intensité lumineuse:", "optionLightIntensity"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Effet de transparence:", "optionTransparency"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Bouton de chargement:", "loadPopupLoadButton"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Bouton d'annulation:", "loadPopupCancelButton"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Bouton de sauvegarde:", "savePopupSaveButton"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Bouton d'annulation:", "savePopupCancelButton"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Placeholder pour le nom du fichier:", "savePopupPlaceholder"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Indication pour un nom invalide:", "savePopupInvalidText"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Bouton de validation de l'indication pour un nom invalide:", "savePopupInvalidButton"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Indication pour écraser un fichier:", "savePopupOverrideText"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Bouton de validation pour écraser un fichier:", "savePopupOverrideYesButton"), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Bouton d'annulation pour écraser un fichier:", "savePopupOverrideNoButton"), gbc);
    	gbc.gridy = 8;
    	content.add(CreateTextInputFieldLine("Indication de sauvegarde réussie:", "savePopupDoneText"), gbc);
    	gbc.gridy = 9;
    	content.add(CreateTextInputFieldLine("Bouton de validation de sauvegarde réussie:", "savePopupDoneButton"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Indication pour continuer la lecture:", "storyClickToContinue"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Bouton d'ouverture de lien sur la page finale:", "endLinkButtonText"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Bouton quitter sur la page finale:", "endLeaveButtonText"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Observation:", "hudObserve"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Déplacement:", "hudMove"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Se baisser:", "hudCrouch"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Inventaire:", "hudInventory"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Fragments de rêves:", "hudDreamFragments"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Questions:", "hudQuestions"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Aide:", "hudHelp"), gbc);
    	gbc.gridy = 7;
    	content.add(CreateTextInputFieldLine("Menu:", "hudMenu"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Texte de la fenêtre de collection de fragment:", "dreamFragmentText"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Bouton de validation de la fenêtre de collection de fragment:", "dreamFragmentValidation"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Bouton de réinitialisation d'un fragment dans l'IAR:", "dreamFragmentVirtualReset"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Inventaire:", "iarTabInventory"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Fragments de rêves:", "iarTabDreamFragments"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Questions salle 1:", "iarTabQuestions1"), gbc);
    	gbc.gridy = 3;
    	content.add(CreateTextInputFieldLine("Questions salle 2:", "iarTabQuestions2"), gbc);
    	gbc.gridy = 4;
    	content.add(CreateTextInputFieldLine("Questions salle 3:", "iarTabQuestions3"), gbc);
    	gbc.gridy = 5;
    	content.add(CreateTextInputFieldLine("Aide:", "iarTabHelp"), gbc);
    	gbc.gridy = 6;
    	content.add(CreateTextInputFieldLine("Menu:", "iarTabMenu"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Bouton de validation des questions:", "questionValidationButton"), gbc);
    	
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
    	content.add(CreateTextInputFieldLine("Bouton d'indice:", "hintButtonText"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Bouton de demande d'indice:", "getHintButton"), gbc);
    	gbc.gridy = 2;
    	content.add(CreateTextInputFieldLine("Bouton d'ouverture de lien d'un indice:", "hintOpenURL"), gbc);
    	
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
		content.add(CreateTextInputFieldLine("Bouton reprendre:", "gameMenuResumeButton"), gbc);
		gbc.gridy = 1;
		content.add(CreateTextInputFieldLine("Bouton de sauvegarde:", "gameMenuSaveButton"), gbc);
		gbc.gridy = 2;
		content.add(CreateTextInputFieldLine("Indication pour le bouton de sauvegarde:", "gameMenuSaveNotice"), gbc);
		gbc.gridy = 3;
		content.add(CreateTextInputFieldLine("Bouton d'options:", "gameMenuOptionButton"), gbc);
		gbc.gridy = 4;
		content.add(CreateTextInputFieldLine("Bouton de retour au menu principal:", "gameMenuRestartButton"), gbc);
		gbc.gridy = 5;
		content.add(CreateTextInputFieldLine("Bouton quitter:", "gameMenuLeaveButton"), gbc);

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
		content.add(CreateTextInputFieldLine("Texte 0 du tutoriel:", "tutoText0"), gbc);
		gbc.gridy = 1;
		content.add(CreateTextInputFieldLine("Texte 1 du tutoriel:", "tutoText1"), gbc);
		gbc.gridy = 2;
		content.add(CreateTextInputFieldLine("Texte 2 du tutoriel:", "tutoText2"), gbc);
		gbc.gridy = 3;
		content.add(CreateTextInputFieldLine("Texte 3 du tutoriel:", "tutoText3"), gbc);
		gbc.gridy = 4;
		content.add(CreateTextInputFieldLine("Texte 4 du tutoriel:", "tutoText4"), gbc);

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
		content.add(CreateTextInputFieldLine("Titre de la colonne 1:", "inputsSetTitle1"), gbc);
		gbc.gridy = 1;
		content.add(CreateTextInputFieldLine("Titre de la colonne 2:", "inputsSetTitle2"), gbc);
		gbc.gridy = 2;
		content.add(CreateTextInputFieldLine("Titre de la colonne 3:", "inputsSetTitle3"), gbc);
		gbc.gridy = 3;
		content.add(CreateInputCategory("Rotation de la caméra", "inputObserve"), gbc);
		gbc.gridy = 4;
		content.add(CreateInputCategory("Avancer", "inputForward"), gbc);
		gbc.gridy = 5;
		content.add(CreateInputCategory("Reculer", "inputBack"), gbc);
		gbc.gridy = 6;
		content.add(CreateInputCategory("Daplacement à gauche", "inputLeft"), gbc);
		gbc.gridy = 7;
		content.add(CreateInputCategory("Déplacement à droite", "inputRight"), gbc);
		gbc.gridy = 8;
		content.add(CreateInputCategory("Se baisser", "inputCrouch"), gbc);
		gbc.gridy = 9;
		content.add(CreateInputCategory("Interaction", "inputInteract"), gbc);
		gbc.gridy = 10;
		content.add(CreateInputCategory("Inventaire", "inputInventory"), gbc);
		gbc.gridy = 11;
		content.add(CreateInputCategory("Fragments de rêves", "inputDreamFragments"), gbc);
		gbc.gridy = 12;
		content.add(CreateInputCategory("Questions", "inputQuestions"), gbc);
		gbc.gridy = 13;
		content.add(CreateInputCategory("Aide", "inputHelp"), gbc);
		gbc.gridy = 14;
		content.add(CreateInputCategory("Menu", "inputMenu"), gbc);
		gbc.gridy = 15;
		content.add(CreateInputCategory("Changer de mode de vue", "inputView"), gbc);
		gbc.gridy = 16;
		content.add(CreateInputCategory("Afficher de la cible", "inputTarget"), gbc);
		gbc.gridy = 17;
		content.add(CreateInputCategory("Zoomer", "inputZoomIn"), gbc);
		gbc.gridy = 18;
		content.add(CreateInputCategory("Dézoomer", "inputZoomOut"), gbc);

		return parent;
	}


    private JPanel CreateCategory(String label, JPanel content) {
    	var panel = new JPanel(new BorderLayout());
    	
    	var button = new JToggleButton();
    	button.setText(label);
    	button.addActionListener(arg0 -> {
			AbstractButton b = (AbstractButton) arg0.getSource();
			content.setVisible(b.getModel().isSelected());
		});
    	panel.add(button, BorderLayout.PAGE_START);
    	
    	content.setVisible(false);
    	panel.add(content, BorderLayout.CENTER);
    	
    	return panel;
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
    	content.add(CreateTextInputFieldLine("Lien du fragment de rêve:", "Fragment_souvenir_" + dreamFragmentID + "Link"), gbc);
    	gbc.gridy = 1;
    	content.add(CreateTextInputFieldLine("Texte du bouton vers le lien:", "Fragment_souvenir_" + dreamFragmentID + "Button"), gbc);
    	
    	return CreateCategory("Fragment de rêve " + dreamFragmentID + " (" + dreamFragmentDescription + ")", parent);
    }

	private JPanel CreateInputCategory(String label, String dictionaryKey) {
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
		content.add(CreateTextInputFieldLine("Titre:", dictionaryKey), gbc);
		gbc.gridy = 1;
		content.add(CreateTextInputFieldLine("Touche 1:", dictionaryKey + "Key1"), gbc);
		gbc.gridy = 2;
		content.add(CreateTextInputFieldLine("Touche 2:", dictionaryKey + "Key2"), gbc);
		gbc.gridy = 3;
		content.add(CreateTextInputFieldLine("Touche 3:", dictionaryKey + "Key3"), gbc);

		return CreateCategory(label, parent);
	}
    
    private JPanel CreateTextInputFieldLine(String label, String dictionaryKey) {

    	var layout = new GridLayout(1, 2);
    	layout.setHgap(5);
    	var line = new JPanel();
    	line.setLayout(layout);
    	
    	var labelPane = new JTextPane();
    	labelPane.setText(label);
    	labelPane.setEditable(false);
    	labelPane.setBackground(new Color(153, 204, 255));
    	
    	var inputfield = new JTextField();
    	jsonKeyToComponents.put(dictionaryKey, new ArrayList<>());
		jsonKeyToComponents.get(dictionaryKey).add(inputfield);
    	
    	line.add(labelPane);
    	line.add(inputfield);
    	return line;
    }
    
    private JPanel CreateTextInputFieldLine(String label, String dictionaryKey, int numberOfInputfield) {

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
    	JTextField inputfield;
		jsonKeyToComponents.put(dictionaryKey, new ArrayList<>());
    	for(int i = 0; i < numberOfInputfield; i++){
    		inputfield = new JTextField();
			inputfields.add(inputfield);
			jsonKeyToComponents.get(dictionaryKey).add(inputfield);
		}
    	
    	line.add(labelPane);
    	line.add(inputfields);
    	return line;
    }
    
    private JPanel CreateFloatInputFieldLine(String label, String dictionaryKey) {

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
		jsonKeyToComponents.put(dictionaryKey, new ArrayList<>());
		jsonKeyToComponents.get(dictionaryKey).add(inputfield);
    	
    	line.add(labelPane);
    	line.add(inputfield);
    	return line;
    }
    
    private JPanel CreateIntInputFieldLine(String label, String dictionaryKey) {

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
    	jsonKeyToComponents.put(dictionaryKey, new ArrayList<>());
    	jsonKeyToComponents.get(dictionaryKey).add(inputfield);
    	
    	line.add(labelPane);
    	line.add(inputfield);
    	return line;
    }
    
    private JPanel CreateIntInputFieldLine(String label, String dictionaryKey, int numberOfInputfield) {

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
    	JTextField inputfield;
    	jsonKeyToComponents.put(dictionaryKey, new ArrayList<>());
    	for(int i = 0; i < numberOfInputfield; i++) {
    		inputfield = new JTextField();
	    	PlainDocument doc = (PlainDocument) inputfield.getDocument();
	    	doc.setDocumentFilter(new IntFilter());
    		inputfields.add(inputfield);
    		jsonKeyToComponents.get(dictionaryKey).add(inputfield);
    	}
    	
    	line.add(labelPane);
    	line.add(inputfields);
    	return line;
    }
    
    private JPanel CreateTextAreaLine(String label, String dictionaryKey) {
		// create category button
		var panel = new JPanel(new BorderLayout());
		var parent = new JPanel(new BorderLayout());

		var button = new JToggleButton();
		button.setText(label);
		button.addActionListener(arg0 -> {
			AbstractButton b = (AbstractButton) arg0.getSource();
			parent.setVisible(b.getModel().isSelected());
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
		jsonKeyToComponents.put(dictionaryKey + "Count", new ArrayList<>());
		jsonKeyToComponents.get(dictionaryKey + "Count").add(inputfield);

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
		doc.addDocumentListener(new InputFieldGenerator(inputfield, inputfields, jsonKeyToComponents, dictionaryKey));

		return panel;
    }
    
    private JPanel CreateToggleLine(String label, String dictionaryKey) {

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
    	jsonKeyToComponents.put(dictionaryKey, new ArrayList<>());
    	jsonKeyToComponents.get(dictionaryKey).add(toggle);
    	
    	line.add(labelPane);
    	line.add(togglePane);
    	return line;
    }

	private JPanel CreateToggleLine(String label, String dictionaryKey, JCheckBox dependentToggle, boolean disableWhenOn, boolean valueWhenDisabled) {

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
		if(dependentToggle != null){
			toggle.addItemListener(e -> {
				if((disableWhenOn && toggle.isSelected()) || (!disableWhenOn && !toggle.isSelected())){
					dependentToggle.setSelected(valueWhenDisabled);
					dependentToggle.setEnabled(false);
				}
				else
					dependentToggle.setEnabled(true);
			});
			toggle.setSelected(true);
			toggle.setSelected(false);
		}
		togglePane.add(toggle);
		jsonKeyToComponents.put(dictionaryKey, new ArrayList<>());
		jsonKeyToComponents.get(dictionaryKey).add(toggle);

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
