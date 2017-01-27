using System;
using System.Windows;
using System.Windows.Controls;
using TimeLogger.DTO;
using TimeLogger.Models;

namespace TimeLogger.Panels
{
    public partial class TagsPanel : UserControl
    {
        public TagsPanel()
        {
            InitializeComponent();
            init();
        }

        //--------------------------------------------------------------------------------
        //
        //      initialisation
        //
        //--------------------------------------------------------------------------------

        private void init()
        {
            //hook up listeners
            Model.instance.tagsChanged += tagsTableChangedHandler;
            ////initialise gui
            setDisplay();
        }

        //--------------------------------------------------------------------------------
        //
        //      crud
        //
        //--------------------------------------------------------------------------------

        private void addTag()
        {
            //check for valid input
            string newLabel = tagLabel.Text;
            bool isLabelValid = validateTagLabel(newLabel);
            if (isLabelValid == false) return;
            //add new tag to model
            Tag tag = new Tag(0, newLabel);
            Model.instance.addTag(tag);
            //show confirmation
            MessageBox.Show("Tag '" + newLabel + "' added", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void saveTag()
        {
            //check for valid input
            string newLabel = tagLabel.Text;
            bool isLabelValid = validateTagLabel(newLabel);
            if (isLabelValid == false) return;
            //update model
            Tag tag = tagsCombo.SelectedItem as Tag;
            tag.label = newLabel;
            Model.instance.updateTag(tag);
        }

        private void deleteTag()
        {
            //update model
            Tag tag = tagsCombo.SelectedItem as Tag;
            Model.instance.deleteTag(tag.id);
        }

        private bool validateTagLabel(string label)
        {
            //check for empty
            if (String.IsNullOrEmpty(label) == true)
            {
                MessageBox.Show("Empty tag label", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            //check not "None"
            if (label == App.LABEL_FOR_NO_TAG)
            {
                MessageBox.Show("'" + App.LABEL_FOR_NO_TAG + "' is reserved", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            //check not already in use
            for (int i = 0; i < Model.instance.tags.Count; i++)
            {
                if (label == Model.instance.tags[i].label)
                {
                    MessageBox.Show("Tag label already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return true;
        }

        //--------------------------------------------------------------------------------
        //
        //      gui
        //
        //--------------------------------------------------------------------------------

        private void setDisplay()
        {
            tagsCombo.ItemsSource = Model.instance.tags;
            tagsCombo.SelectedItem = null;
            tagLabel.Text = "";
        }

        //--------------------------------------------------------------------------------
        //
        //      events
        //
        //--------------------------------------------------------------------------------

        private void tagsTableChangedHandler()
        {
            setDisplay();
        }

        private void clickhandler(object sender, RoutedEventArgs e)
        {
            if (sender == addBtn)
            {
                //add data
                addTag();
            }
            else if (sender == deleteBtn)
            {
                //check for selection
                if (tagsCombo.SelectedItem == null)
                {
                    MessageBox.Show("No tag selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                //show confirmation message
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this tag?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                //if confirmed, delete item
                if (result == MessageBoxResult.Yes)
                {
                    deleteTag();
                }
            }
            else if (sender == submitBtn)
            {
                //check for selection
                if (tagsCombo.SelectedItem == null)
                {
                    MessageBox.Show("No tag selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                //save data
                saveTag();
            }
        }

        private void selectionChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            //update form
            Tag selectedTag = tagsCombo.SelectedItem as Tag;
            tagLabel.Text = (selectedTag != null) ? selectedTag.label : "";
        }

    }
}
