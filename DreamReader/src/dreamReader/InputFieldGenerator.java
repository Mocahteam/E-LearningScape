package dreamReader;

import javax.swing.*;
import javax.swing.event.DocumentEvent;
import javax.swing.event.DocumentListener;
import java.awt.*;
import java.util.*;
import java.util.List;

public class InputFieldGenerator implements DocumentListener {

    private final JTextField sizeField;
    private final JPanel container;
    private final java.util.List<JTextField> list;

    private final HashMap<String, List<JComponent>> componentsDictionary;
    private final String key;

    public InputFieldGenerator(JTextField sizeInputField, JPanel inputFieldsContainer, HashMap<String, List<JComponent>> jsonKeyToComponents, String dictionaryKey){
        container = Objects.requireNonNullElseGet(inputFieldsContainer, JPanel::new);
        sizeField = Objects.requireNonNullElseGet(sizeInputField, JTextField::new);

        var inputfieldsLayout = new GridLayout(0, 1);
        inputfieldsLayout.setVgap(5);
        container.setLayout(inputfieldsLayout);

        list = new ArrayList<>();

        componentsDictionary = jsonKeyToComponents;
        key = dictionaryKey;
        componentsDictionary.put(key, new ArrayList<>());
    }

    @Override
    public void insertUpdate(DocumentEvent e) {
        updateNumberOfInputFields();
    }

    @Override
    public void removeUpdate(DocumentEvent e) {
        updateNumberOfInputFields();
    }

    @Override
    public void changedUpdate(DocumentEvent e) {
        updateNumberOfInputFields();
    }

    private void updateNumberOfInputFields(){

        String sizeText = sizeField.getText();
        int size = 0;
        if(sizeText != null && sizeText.length() != 0) {
            try {
                size = Integer.parseInt(sizeText);
            }
            catch (Exception e) {
                e.printStackTrace();
            }
        }

        GridLayout layout = (GridLayout) container.getLayout();
        layout.setRows(size);

        if(size > list.size()){
            for(int i = list.size(); i < size; i++) {
                JTextField tf = new JTextField();
                container.add(tf);
                list.add(tf);
                componentsDictionary.get(key).add(tf);
            }
        }
        else if(size < list.size()){
            for(int i = list.size() - 1; i > size - 1; i--){
                JTextField tf = list.get(i);
                list.remove(tf);
                container.remove(tf);
                componentsDictionary.get(key).remove(tf);
            }
        }
    }
}
