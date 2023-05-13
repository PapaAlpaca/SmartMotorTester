using Microsoft.SPOT;
using System.Collections;
using System;
using CTRE.Gadgeteer.Module;

namespace DisplayModule_Example
{
    public class ChoiceMenu
    {
        public class ChoiceItem
        {
            public string text { get; set; }
            ChoiceMenu child;

            public ChoiceItem(string text, ChoiceMenu child = null)
            {
                this.text = text;
                this.child = child;
            }

            public ChoiceMenu getChild()
            {
                return child;
            }
        }

        // Scrolling????
        string title;
        ArrayList choices = new ArrayList();
        ChoiceMenu parent;
        public int selected { get; set; }

        // Assuming choices <= displayLines
        public ChoiceMenu(string title, ChoiceMenu parent)
        {
            this.title = title;
            this.parent = parent;
            selected = 0;
        }

        public ChoiceItem add(string text, ChoiceMenu child = null)
        {
            ChoiceItem item = new ChoiceItem(text, child);
            choices.Add(item);
            return item;
        }

        public void up()
        {
            selected = selected > 0 ? selected - 1 : choices.Count - 1;
        }

        public void down()
        {
            selected = selected < choices.Count - 1 ? selected + 1 : 0;
        }

        public ChoiceMenu getChildMenu() { return ((ChoiceItem)choices[selected]).getChild(); }
        public ChoiceMenu getParentMenu() { return parent; }
        public String getChoiceText() { return ((ChoiceItem)choices[selected]).text; }
        public ChoiceItem getChoice() { return (ChoiceItem)choices[selected]; }

        public void displayChoices(DisplayModule.LabelSprite title, DisplayModule.LabelSprite[] labels)
        {
            title.SetText(this.title);
            for (int i = 0; i < (System.Math.Min(choices.Count, labels.Length)); i++)
            {
                labels[i].SetText((i == selected ? "> " : "") + ((ChoiceItem)choices[i]).text);
            }
        }
    }
}
