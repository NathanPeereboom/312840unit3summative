/* Nathan Peereboom
 * May 20, 2020
 * Unit 3 Summative. Scrabble word generator. Uses unedited ScrabbleGame and ScrabbleLetter Classes by Mr McTavish.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace _312840unit3summative
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Global variables
        ScrabbleGame sg;//The Scrabble Game. Contains all tiles.
        char[] tiles;//The tiles drawn
        string dictionary = "";//The entire English dictionary. Pulled from the web to be written into text file.
        int blanks;//Number of blank tiles
        List<char> featuredTiles;//List of tiles, not including repeats
        List<string> textFiles;//List of text files that need to be read to find possible words
        List<string> possibleWords;//List of words that can be created with tiles
        int highestValue;//Highest value of any word that can be made with the given tiles
        List<Label> labelTiles;//List of labels displaying tiles
        List<Label> labelValues;//List of labels displaying value of tiles
        int page;//The page to be shown. Each page shows 10 possible words.
        List<string> scoredWords;//List of words which have been scored

        public MainWindow()
        {
            InitializeComponent();

            //Pull dictionary from web
            System.Net.WebClient webClient = new System.Net.WebClient();
            System.IO.StreamReader sr = new System.IO.StreamReader(webClient.OpenRead("http://darcy.rsgc.on.ca/ACES/ICS4U/SourceCode/Words.txt"));
            dictionary = sr.ReadToEnd();
            sr.Close();

            //Write dictionary to file
            using (StreamWriter dictionaryWriter = File.CreateText("dictionary.txt"))
            {
                dictionaryWriter.Write(dictionary);
                dictionaryWriter.Flush();
                dictionaryWriter.Close();
            }

            //Organize dictionary alphabetically
            System.IO.StreamReader dictionaryReader = new System.IO.StreamReader("dictionary.txt");
            for (int i = 0; i < 26; i++)
            {
                char letter = (char)(65 + i);
                string fileName = letter + ".txt";
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    while (!dictionaryReader.EndOfStream)
                    {
                        string word = dictionaryReader.ReadLine().ToUpper();
                        char firstLetter = word[0];
                        if (firstLetter == letter)
                        {
                            if (word.Length >= 2 && word.Length <= 7)
                            {
                                sw.WriteLine(word);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            dictionaryReader.Close();

            //Labels lists
            labelTiles = new List<Label>();
            labelTiles.Add(lblTile0);
            labelTiles.Add(lblTile1);
            labelTiles.Add(lblTile2);
            labelTiles.Add(lblTile3);
            labelTiles.Add(lblTile4);
            labelTiles.Add(lblTile5);
            labelTiles.Add(lblTile6);
            labelValues = new List<Label>();
            labelValues.Add(lblValue0);
            labelValues.Add(lblValue1);
            labelValues.Add(lblValue2);
            labelValues.Add(lblValue3);
            labelValues.Add(lblValue4);
            labelValues.Add(lblValue5);
            labelValues.Add(lblValue6);

            //Generate tiles upon opening program
            btnGenerate_Click(new object(), new RoutedEventArgs());
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            //Delete old sorted files
            for (int i = 30; i >= 0; i--)
            {
                string fileName = i + ".txt";
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }

            //Draw tiles
            sg = new ScrabbleGame();
            tiles = new char[7];
            string tilesDraw = sg.drawInitialTiles();
            for (int i = 0; i < 7; i++)
            {
                tiles[i] = tilesDraw[i];
            }

            //Check for blanks
            blanks = 0;
            for (int i = 0; i < 7; i++)
            {
                if (tiles[i] == ' ')
                {
                    blanks++;
                }
            }

            //Setup featured tiles list
            featuredTiles = new List<char>();
            for (int i = 0; i < 7; i++)
            {
                char tile = tiles[i];
                bool repeatTile = false;
                foreach (char letter in featuredTiles)
                {
                    if (tile == letter)
                    {
                        repeatTile = true;
                    }
                }
                if (!repeatTile)
                {
                    featuredTiles.Add(tile);
                }
            }

            //Find applicable text files
            textFiles = new List<string>();
            if (blanks == 0)//If no blanks
            {
                foreach (char tile in featuredTiles)
                {
                    string file = tile + ".txt";
                    textFiles.Add(file);
                }
            }
            else//If blanks
            {
                for (int i = 0; i < 26; i++)
                {
                    char letter = (char)(65 + i);
                    string file = letter + ".txt";
                    textFiles.Add(file);
                }
            }

            //Find possible words
            possibleWords = new List<string>();
            highestValue = 0;
            foreach (string file in textFiles)
            {
                System.IO.StreamReader fileReader = new System.IO.StreamReader(file);
                while (!fileReader.EndOfStream)
                {
                    int value = 0;
                    int availableBlanks = blanks;
                    bool canMakeWord = true;
                    string word = fileReader.ReadLine();
                    List<char> availableTiles = new List<char>();
                    for (int i = 0; i < 7; i++)
                    {
                        availableTiles.Add(tiles[i]);
                    }
                    for (int i = 0; i < word.Length; i++)
                    {
                        char iLetter = word[i];
                        bool hasTile = false;
                        foreach (char tile in availableTiles)
                        {
                            if (tile == iLetter)
                            {
                                hasTile = true;
                                availableTiles.Remove(tile);
                                ScrabbleLetter scrabbleLetter = new ScrabbleLetter(tile);
                                value += scrabbleLetter.Points;
                                break;
                            }
                        }
                        if (!hasTile)
                        {
                            if (availableBlanks > 0)
                            {
                                availableBlanks--;
                            }
                            else
                            {
                                canMakeWord = false;
                                break;
                            }
                        }
                    }
                    if (canMakeWord)
                    {
                        possibleWords.Add(word);

                        if (value > highestValue)
                        {
                            highestValue = value;
                        }

                        //Sort based on value
                        string fileName = value.ToString() + ".txt";
                        if (File.Exists(fileName))
                        {
                            System.IO.StreamWriter sortWriter = new System.IO.StreamWriter(fileName, true);
                            sortWriter.WriteLine(word);
                            sortWriter.Close();
                        }
                        else
                        {
                            using (StreamWriter sortWriter = File.CreateText(fileName))
                            {
                                sortWriter.WriteLine(word);
                            }
                        }
                    }
                }
            }

            //Ouput
            scoredWords = new List<string>();
            for (int i = highestValue; i >= 0; i--)
            {
                string fileName = i.ToString() + ".txt";
                if (File.Exists(fileName))
                {
                    System.IO.StreamReader reader = new System.IO.StreamReader(fileName);
                    while (!reader.EndOfStream)
                    {
                        scoredWords.Add(reader.ReadLine() + " (" + i + ")");
                    }
                    reader.Close();
                }
            }
            int counter = 0;
            foreach (Label label in labelTiles)
            {
                label.Content = tiles[counter];
                counter++;
            }
            counter = 0;
            foreach (Label label in labelValues)
            {
                ScrabbleLetter letter = new ScrabbleLetter(tiles[counter]);
                label.Content = letter.Points;
                counter++;
            }
            page = 1;
            displayPage();
        }

        private void btnBackPage_Click(object sender, RoutedEventArgs e)
        {
            if (page > 1)
            {
                page--;
                displayPage();
            }
            else
            {
                MessageBox.Show("Can't go back to previous page as this is the first page.");
            }
        }

        private void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (page < (scoredWords.Count/10) + 1)
            {
                page++;
                displayPage();
            }
            else
            {
                MessageBox.Show("Can't go to the next page as this is the last page.");
            }
        }

        public void displayPage()
        {
            string output = "";
            for (int i = (page-1) * 10; i < ((page-1) * 10) + 10; i++)
            {
                if (i < scoredWords.Count)
                {
                    output += scoredWords[i] + Environment.NewLine;
                }
            }
            lblWords.Content = output;
            lblPageNumber.Content = "Page " + page.ToString();
        }
    }
}
