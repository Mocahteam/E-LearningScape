package dreamReader;

import javax.swing.*;
import javax.swing.event.DocumentEvent;
import javax.swing.event.DocumentListener;
import java.awt.*;
import java.util.*;
import java.util.List;

public class InputFieldGenerator implements DocumentListener {

    private JTextField sizeField;
    private JPanel container;
    private java.util.List<JTextField> list;

    public InputFieldGenerator(JTextField sizeInputField, JPanel inputFieldsContainer){
        if(inputFieldsContainer == null)
            container = new JPanel();
        else
            container = inputFieldsContainer;
        if(sizeInputField == null)
            sizeField = new JTextField();
        else
            sizeField = sizeInputField;

        var inputfieldsLayout = new GridLayout(0, 1);
        inputfieldsLayout.setVgap(5);
        container.setLayout(inputfieldsLayout);

        list = new ArrayList<JTextField>();
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
            catch (Exception e) { }
        }

        GridLayout layout = (GridLayout) container.getLayout();
        layout.setRows(size);

        if(size > list.size()){
            for(int i = list.size(); i < size; i++) {
                JTextField tf = new JTextField();
                container.add(tf);
                list.add(tf);
            }
        }
        else if(size < list.size()){
            for(int i = list.size() - 1; i > size - 1; i--){
                JTextField tf = list.get(i);
                list.remove(tf);
                container.remove(tf);
                tf = null;
            }
        }
    }
}
