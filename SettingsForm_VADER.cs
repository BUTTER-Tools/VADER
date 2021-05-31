using System.Text;
using System.Windows.Forms;
using System.IO;






namespace VADER
{
    internal partial class SettingsForm_VADER : Form
    {


        #region Get and Set Options

        public bool includeSentenceText { get; set; }
        public bool useBuiltInSentenceSplitter { get; set; }

        #endregion



        public SettingsForm_VADER(bool builtInSplitter, bool textInOutput)
        {
            InitializeComponent();
            IncludeTextCheckbox.Checked = textInOutput;
            UseBuiltInSentenceSplitterCheckbox.Checked = builtInSplitter;
        }




                                   
        private void OKButton_Click(object sender, System.EventArgs e)
        {

            this.useBuiltInSentenceSplitter = UseBuiltInSentenceSplitterCheckbox.Checked;
            this.includeSentenceText = IncludeTextCheckbox.Checked;
           
            this.DialogResult = DialogResult.OK;

        }
    }
}
