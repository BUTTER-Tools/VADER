using OutputHelperLib;
using PluginContracts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using VaderSharp;


namespace VADER
{
    public class VADER : Plugin
    {


        public string[] InputType { get; } = { "String" };
        public string OutputType { get; } = "OutputArray";

        public Dictionary<int, string> OutputHeaderData { get; set; } = new Dictionary<int, string>() { {0, "WC"},
                                                                                                        {1, "SentenceNumber"},
                                                                                                        {2, "Classification"},
                                                                                                        {3, "Compound_M"},
                                                                                                        {4, "Positive_M"},
                                                                                                        {5, "Negative_M"},
                                                                                                        {6, "Neutral_M"},
                                                                                                        {7, "SentenceText"} };
        public bool InheritHeader { get; } = false;
        

        #region Plugin Details and Info

        public string PluginName { get; } = "VADER";
        public string PluginType { get; } = "Sentiment Analysis";
        public string PluginVersion { get; } = "1.1.0";
        public string PluginAuthor { get; } = "Ryan L. Boyd (ryan@ryanboyd.io)";
        public string PluginDescription { get; } = "Sentiment analysis for Twitter data. Outputs sentence-level sentiment scores." + Environment.NewLine + Environment.NewLine +
            "https://github.com/cjhutto/vaderSentiment" + Environment.NewLine + Environment.NewLine +
            "https://github.com/codingupastorm/vadersharp" + Environment.NewLine + Environment.NewLine +
            "Hutto, C. J., & Gilbert, E. (2014). VADER: A Parsimonious Rule-Based Model for Sentiment Analysis of Social Media Text. In Eighth International AAAI Conference on Weblogs and Social Media. Retrieved from https://www.aaai.org/ocs/index.php/ICWSM/ICWSM14/paper/view/8109";
        public bool TopLevel { get; } = false;
        public string PluginTutorial { get; } = "Coming Soon";


        public Icon GetPluginIcon
        {
            get
            {
                return Properties.Resources.icon;
            }
        }

        #endregion



        //settings for this plugin
        private Regex SentenceSplitter = new Regex(@"(?<!\w\.\w.)(?<![A-Z][a-z]\.)(?<=\.|\?|\!)\s", RegexOptions.Compiled);
        private bool includeSentenceText { get; set; } = true;
        private bool useBuiltInSentenceSplitter { get; set; } = true;


        public void ChangeSettings()
        {

            using (var form = new SettingsForm_VADER(builtInSplitter: useBuiltInSentenceSplitter, textInOutput: includeSentenceText))
            {


                form.Icon = Properties.Resources.icon;

                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    useBuiltInSentenceSplitter = form.useBuiltInSentenceSplitter;
                    includeSentenceText = form.includeSentenceText;
                }
            }

        }





        public Payload RunPlugin(Payload Input)
        {

            Payload pData = new Payload();
            pData.FileID = Input.FileID;
            bool trackSegmentID = false;
            if (Input.SegmentID.Count > 0)
            {
                trackSegmentID = true;
            }
            else
            {
                pData.SegmentID = Input.SegmentID;
            }



            for (int counter = 0; counter < Input.StringList.Count; counter++)
            {

                string[] Sentences;
                if (useBuiltInSentenceSplitter)
                {
                    Sentences = SentenceSplitter.Split(Input.StringList[counter]).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                }
                else
                {
                    Sentences = new string[] { Input.StringList[counter] };
                }

                int TotalStringLength = Input.StringList[counter].Split().Where(x => !string.IsNullOrWhiteSpace(x)).ToArray().Length;


                SentimentIntensityAnalyzer VADERAnalyzer = new SentimentIntensityAnalyzer();
                VaderSharp.SentimentAnalysisResults results = new VaderSharp.SentimentAnalysisResults();


                for (int i = 0; i < Sentences.Length; i++)
                {

                    string[] ResultsArray = new string[8] { "", "", "", "", "", "", "", ""};
                    results = VADERAnalyzer.PolarityScores(Sentences[i]);
                    int Sentence_WC = Sentences[i].Split().Where(x => !string.IsNullOrWhiteSpace(x)).ToArray().Length;

                    //Pull together our results
                    ResultsArray[0] = Sentence_WC.ToString();

                    if (useBuiltInSentenceSplitter)
                    {
                        ResultsArray[1] = (i + 1).ToString();
                    }
                    else
                    {
                        //if we're using an external sentence tokenizer, then every segment is
                        //going to be treated as its own sentence.
                        ResultsArray[1] = (counter + 1).ToString();
                    }


                    ResultsArray[1] = (i + 1).ToString();
                    ResultsArray[2] = "";

                    if (results.Compound > 0.05)
                    {
                        ResultsArray[2] = "pos";
                    }
                    else if (results.Compound > -0.05)
                    {
                        ResultsArray[2] = "neut";
                    }
                    else
                    {
                        ResultsArray[2] = "neg";
                    }

                    ResultsArray[3] = results.Compound.ToString();
                    ResultsArray[4] = results.Positive.ToString();
                    ResultsArray[5] = results.Negative.ToString();
                    ResultsArray[6] = results.Neutral.ToString();
                    if (includeSentenceText) ResultsArray[7] = Sentences[i].Trim();

                    pData.StringArrayList.Add(ResultsArray);
                    pData.SegmentNumber.Add(Input.SegmentNumber[counter]);
                    if (trackSegmentID)
                    {
                        pData.SegmentID.Add(Input.SegmentID[counter]);
                    }


                }

                


            }
            

            return (pData);

        }



        public void Initialize()
        {
            
        }


        public bool InspectSettings()
        {
            return true;
        }

        public Payload FinishUp(Payload Input)
        {
            return (Input);
        }


        #region Import/Export Settings
        public void ImportSettings(Dictionary<string, string> SettingsDict)
        {
            includeSentenceText = Boolean.Parse(SettingsDict["IncludeSentenceText"]);
            useBuiltInSentenceSplitter = Boolean.Parse(SettingsDict["useBuiltInSentenceSplitter"]);
        }

        public Dictionary<string, string> ExportSettings(bool suppressWarnings)
        {
            Dictionary<string, string> SettingsDict = new Dictionary<string, string>();
            SettingsDict.Add("IncludeSentenceText", includeSentenceText.ToString());
            SettingsDict.Add("useBuiltInSentenceSplitter", includeSentenceText.ToString());
            return (SettingsDict);
        }
        #endregion


    }
}
