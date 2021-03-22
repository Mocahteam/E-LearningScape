package dreamReader;

import javax.swing.text.AttributeSet;
import javax.swing.text.BadLocationException;
import javax.swing.text.Document;
import javax.swing.text.DocumentFilter;

public class FloatFilter extends DocumentFilter {

	private int maxLength = -1;

	public FloatFilter(){
	}

	public FloatFilter(int length){
		maxLength = length;
	}

	private boolean isFloat(String text) {
		if(text == null || text.length() == 0)
			return true;
		else if(maxLength != -1 && text.length() > maxLength)
			return false;
		try {
			Float.parseFloat(text);
			return true;
		}
		catch (Exception e) {
			return false;
		}
	}

	@Override
	public void insertString(FilterBypass fb, int offset, String string, AttributeSet attr) throws BadLocationException {
		Document doc = fb.getDocument();
		StringBuilder sb = new StringBuilder();
		sb.append(doc.getText(0, doc.getLength()));
		sb.insert(offset, string);

		if(isFloat(sb.toString()))
			super.insertString(fb, offset, string, attr);
	}

	@Override
	public void replace(FilterBypass fb, int offset, int length, String text, AttributeSet attrs) throws BadLocationException {
		Document doc = fb.getDocument();
		StringBuilder sb = new StringBuilder();
		sb.append(doc.getText(0, doc.getLength()));
		sb.replace(offset, offset + length, text);

		if (isFloat(sb.toString()))
			super.replace(fb, offset, length, text, attrs);
	}

	@Override
	public void remove(FilterBypass fb, int offset, int length) throws BadLocationException {
		Document doc = fb.getDocument();
		StringBuilder sb = new StringBuilder();
		sb.append(doc.getText(0, doc.getLength()));
		sb.delete(offset, offset + length);

		if (isFloat(sb.toString()))
			super.remove(fb, offset, length);
	}
}
