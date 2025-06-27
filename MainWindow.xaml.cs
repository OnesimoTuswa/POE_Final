using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//Import the ML
using Microsoft.ML;
using Microsoft.ML.Data;

namespace POE_Draft
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //global declaration for all instances and variables
        private string user_name = string.Empty;
        private string user_asking = string.Empty;
        Dictionary<string, List<string>> replies = new Dictionary<string, List<string>>();
        HashSet<string> ignore = new HashSet<string>();
        int delayTime = 0;
        string username;
        Boolean interest_in_phishing = false;
        Boolean interest_in_browsing = false;
        Boolean interest_in_password = false;

        //ArrayList declaration for activities done
        List<string> activities = new List<string>();

        //List [ generic ]
        private List<QuizQuestion> quizData;

        //variables
        private int questionIndex = 0;
        private int currentScore = 0;

        //buttons
        private Button selectedChoice = null;
        private Button correctChoiceButton = null;

        //NLP Declarations
        // ML.NET context
        private readonly MLContext mlContext;

        // List to store training data dynamically
        private List<SentimentData> trainingData;

        // Prediction engine
        private PredictionEngine<SentimentData, SentimentPrediction> predEngine;

        // Sentiment input data class
        private class SentimentData
        {
            public string Text { get; set; }
            public bool Label { get; set; }
        }

        // Sentiment prediction result class
        private class SentimentPrediction
        {
            [ColumnName("PredictedLabel")]
            public bool Prediction { get; set; }
            public float Probability { get; set; }
            public float Score { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();

            //Call the load quiz method 
            LoadQuizData();
            showQuiz();

            //Call the replies and ignore methods
            store_replies();
            store_ignores();

            InitializeComponent();

            // Initialize ML context
            mlContext = new MLContext();

            // Initialize with base training data
            trainingData = new List<SentimentData>
            {
                new SentimentData { Text = "I am happy", Label = true },
                new SentimentData { Text = "I hate this", Label = false },
                new SentimentData { Text = "I am sad", Label = false },
                new SentimentData { Text = "I am good", Label = true }
            };

            TrainModel();
        }

        //Sidebar content
        private void chats(object sender, RoutedEventArgs e)
        {
            reminder_page.Visibility = Visibility.Hidden;
            quiz_page.Visibility = Visibility.Hidden;
            activity_page.Visibility = Visibility.Hidden;

            //current
            chats_page.Visibility = Visibility.Visible;
        }

        private void reminder(object sender, RoutedEventArgs e)
        {
            reminder_page.Visibility = Visibility.Visible;

            chats_page.Visibility = Visibility.Hidden;
            quiz_page.Visibility = Visibility.Hidden;
            activity_page.Visibility = Visibility.Hidden;
        }

        private void quiz(object sender, RoutedEventArgs e)
        {
            quiz_page.Visibility = Visibility.Visible;

            chats_page.Visibility = Visibility.Hidden;
            reminder_page.Visibility = Visibility.Hidden;
            activity_page.Visibility = Visibility.Hidden;
        }

        private void activity(object sender, RoutedEventArgs e)
        {
            activity_page.Visibility = Visibility.Visible;

            chats_page.Visibility = Visibility.Hidden;
            reminder_page.Visibility = Visibility.Hidden;
            quiz_page.Visibility = Visibility.Hidden;
        }

        private void nlp(object sender, RoutedEventArgs e)
        {
            //Set the NLP page to visible
            nlp_page.Visibility = Visibility.Visible;

            activity_page.Visibility = Visibility.Hidden;
            chats_page.Visibility = Visibility.Hidden;
            reminder_page.Visibility = Visibility.Hidden;
            quiz_page.Visibility = Visibility.Hidden;
        }

        private void exit(object sender, RoutedEventArgs e)
        {
            //Close the application
            System.Environment.Exit(0);
        }//End of sidebar content

        //Chatbot Content
        private void chatbot(object sender, RoutedEventArgs e)
        {
            //Voice greeeting to welcome the user
            Greeting voiceGreeting = new Greeting();

            //Interaction
            //Set interaction to visible
            ShowInteraction.Visibility = Visibility.Visible;
            user_enquiry.Visibility = Visibility.Visible;
            enquireButton.Visibility = Visibility.Visible;

            //Introduce the Chatbot
            ShowInteraction.Items.Add("Chatbot: Hello! How are you? I'm ChatBot and I am here to help you with any queries you might have regarding password safety, phishing and safe browsing. " +
                "How can I assist you today?");
            
        }//End of Chatbot Content

        //Chatbot Interaction Content
        private void enquire(object sender, RoutedEventArgs e)
        {   
            String collectEnquiry = user_enquiry.Text.ToString().ToLower();

            //Check if string is not empty
            if (!collectEnquiry.Equals(""))
                {
                    ShowInteraction.Items.Add("User: " + collectEnquiry);
                    
                    if (collectEnquiry.Contains("interested") || collectEnquiry.Contains("interest") || collectEnquiry.Contains("curious"))
                    {
                        if (collectEnquiry.Contains("phishing"))
                        {
                            interest_in_phishing = true;
                            ShowInteraction.Items.Add(interest_phishing());
                        }
                        else if (collectEnquiry.Contains("browsing"))
                        {
                            interest_in_browsing = true;
                            ShowInteraction.Items.Add(interest_browsing());
                        }
                        else if (collectEnquiry.Contains("password"))
                        {
                            interest_in_password = true;
                            ShowInteraction.Items.Add(interest_password());
                        }
                    }
                    else if (collectEnquiry.Contains("tips"))
                    {
                        if (collectEnquiry.Contains("phishing"))
                        {
                            string phishingTip = tips_phishing();
                            ShowInteraction.Items.Add("Chatbot: " + phishingTip);
                        }
                        else if (collectEnquiry.Contains("browsing"))
                        {
                            string browsingTip = tips_browsing();
                            ShowInteraction.Items.Add("Chatbot: " + browsingTip);
                        }
                        else if (collectEnquiry.Contains("password"))
                        {
                            string passwordTip = tips_password();
                            ShowInteraction.Items.Add("Chatbot: " + passwordTip);
                        }
                    }
                    else if (collectEnquiry.Contains("worried") || collectEnquiry.Contains("unsure") || collectEnquiry.Contains("concerned"))
                    {
                        if (collectEnquiry.Contains("phishing"))
                        {
                            string phishingWorry = worried_phishing();
                            ShowInteraction.Items.Add("Chatbot: " + phishingWorry);
                        }
                        else if (collectEnquiry.Contains("browsing"))
                        {
                            string browsingWorry = worried_browsing();
                            ShowInteraction.Items.Add("Chatbot: " + browsingWorry);
                        }
                        else if (collectEnquiry.Contains("password"))
                        {
                            string passwordWorry = worried_password();
                            ShowInteraction.Items.Add("Chatbot: " + passwordWorry);
                        }
                    }
                    else
                    {
                        //making use of split function to store each word
                        string[] store_word = collectEnquiry.Split(' ');
                        ArrayList store_final_words = new ArrayList();

                        //for loop to check the words to store
                        foreach (string word in store_word)
                        {
                            //if statement to check if words store in 1D array are not ignored
                            if (!ignore.Contains(word.ToLower()))
                            {
                                //store the not ignored words
                                store_final_words.Add(word);
                            }
                        }

                        //temp variables
                        Boolean found = false;
                        string messages = string.Empty;

                        //for loop to get final answer
                        foreach (string word in store_final_words)
                        {
                            string keyword = word.ToLower();
                            if (replies.ContainsKey(keyword))
                            {
                                foreach (var message in replies[keyword])
                                {
                                    messages += message + "\n";
                                    found = true;
                                }
                            }
                        }

                        if (collectEnquiry != "exit")
                        {
                            //display error message or answers
                            if (found == true)
                            {
                                if (interest_in_phishing == true || interest_in_browsing == true || interest_in_password == true)
                                {
                                    if (interest_in_phishing == true && messages.Contains("Phishing"))
                                    {
                                        ShowInteraction.Items.Add("Chatbot:You previously spoke about an interest in phishing.");
                                        ShowInteraction.Items.Add(messages);
                                    }
                                    else if (interest_in_browsing == true && messages.Contains("Browsing"))
                                    {
                                        ShowInteraction.Items.Add("Chatbot: You previously spoke about an interest in browsing. ");
                                        ShowInteraction.Items.Add(messages);
                                    }
                                    else if (interest_in_password == true && messages.Contains("Password"))
                                    {
                                        ShowInteraction.Items.Add("Chatbot: You previously spoke about an interest in password.");
                                        ShowInteraction.Items.Add(messages);
                                    }
                                }
                                else
                                {
                                    //display
                                    ShowInteraction.Items.Add("Chatbot: " + messages);
                                }
                            }
                            else
                            {
                                //This will display if the question is not understood
                                ShowInteraction.Items.Add("Chatbot: I'm sorry, I am not familiar with that subject. Hopefully, I will next time.");
                            }
                        }
                        else
                        {
                            //exit application
                            ShowInteraction.Items.Add("Chatbot: Thank you for using AI Chatbot, goodbye.");
                            System.Environment.Exit(0);
                        }
                    }
                activities.Add("The user initiated an enquiry with the chatbot on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ".");
            }
                else
                {
                    MessageBox.Show("The enquiry field cannot be empty. Please enter your enquiry.");
                }

                //Set listview to auto-scroll
                ShowInteraction.ScrollIntoView(ShowInteraction.Items[ShowInteraction.Items.Count - 1]);
        }
        //End of Chatbot Interaction Content

        //Task Reminder Content
        private void ShowChats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // This is where you would handle the double-click event on the ShowChats element
            //Get the selected item from the listview
            String selected_task = ShowChats.SelectedItem.ToString();

            MessageBox.Show(selected_task);

            //Check if the not marked 'done'
            if (!selected_task.Contains("status done"))
            {
                //Get the index of the selected item
                int getIndex = ShowChats.Items.IndexOf(selected_task);

                //Inform the user that the task is done
                MessageBox.Show("The selected task has been marked as done");
                //Edit the selected item to mark it as done
                ShowChats.Items[getIndex] = selected_task + " status done";

                //Add to activities list
                activities.Add("The task '" + selected_task + "' has been marked as done on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {
                //Inform the user that the selected task has been deleted
                MessageBox.Show("The selected task has been deleted from the list");
                //Then remove if marked done
                ShowChats.Items.Remove(selected_task);

                //Add to activities list
                activities.Add("The task '" + selected_task + "' was deleted on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        private void add_tasks(object sender, RoutedEventArgs e)
        {
            // This is where you would handle the button click event
            //Collect what the user enters
            String collectTask = user_question.Text.ToString();

            //Validate if the user entered something
            if (!collectTask.Equals(""))
            {
                //Add the task to the list view and get the date&time
                DateTime date = DateTime.Now.Date;
                DateTime time = DateTime.Now.ToLocalTime();

                //Set the format for the date
                String date_format = date.ToString("yyyy-MM-dd");

                //Then add to the list
                ShowChats.Items.Add("User: " + collectTask + "\n" + date_format + " Time: " + time);
                ShowChats.Items.Add("Chatbot: The task '" + collectTask + "' has been added.");

                //Add to activities list
                activities.Add("The task '" + collectTask + "' was added on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                //Set listview to auto-scroll
                ShowChats.ScrollIntoView(ShowChats.Items[ShowChats.Items.Count - 1]);
            }
            else
            {
                //Error message
                MessageBox.Show("Task field cannot be empty. Please enter a task you would like to add.");
            }
        }//End of Task Reminder Content

        //Quiz Content
        //method to show the  quiz on the buttons
        private void showQuiz()
        {
            //check if the user is not done playing
            if (questionIndex >= quizData.Count)
            {
                //show complete message
                MessageBox.Show("You have completed the game with a total score of " + currentScore + " out of 5 questions.");
                //Add to activities list
                activities.Add("The user completed the quiz with a score of " + currentScore + " on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //then reset the buttons
                //then reset the game
                currentScore = 0;
                currentScore = 0;
                questionIndex = 0;

                DisplayScore.Text = "";

                showQuiz();
                //stop the execute
                return;
            }

            //get the current index quiz
            correctChoiceButton = null;
            selectedChoice = null;

            //then get all the questions values
            var currentQuiz = quizData[questionIndex];

            //displays the question to the user
            DisplayedQuestion.Text = currentQuiz.Question;

            //add the choices to the buttons
            var shuffled = currentQuiz.Choices.OrderBy(_ => Guid.NewGuid()).ToList();

            //then add by index
            FirstChoiceButton.Content = shuffled[0];
            SecondChoiceButton.Content = shuffled[1];
            ThirdChoiceButton.Content = shuffled[2];

            //correct one
            FourthChoiceButton.Content = currentQuiz.CorrectChoice;

            clearStyle();
        }

        //method to rest the buttons
        private void clearStyle()
        {
            //use for each to reset
            foreach (Button choice in new[] { FirstChoiceButton, SecondChoiceButton, ThirdChoiceButton, FourthChoiceButton })
            {
                choice.Background = Brushes.LightGray;
            }
        }//end of the clear style method 

        //method to load the quiz data
        private void LoadQuizData()
        {
            //store info
            quizData = new List<QuizQuestion> {

                new QuizQuestion
                {
                    Question="The practice of protecting digital entities is called what security?",
                    CorrectChoice ="Cyber",
                    Choices = new List<string>
                    {
                        "Technological", "Technical", "Digital", "Cyber"
                    }
                }   ,

                new QuizQuestion
                {
                    Question="Phishing is a form of...",
                    CorrectChoice ="scamming people",
                    Choices = new List<string>
                    {
                        "hunting for fish", "dressing up", "surfing the web", "scamming people"
                    }
                }   ,

                new QuizQuestion
                {
                    Question="What should you do when creating a password?",
                    CorrectChoice ="Enable 2 Factor Authentication",
                    Choices = new List<string>
                    {
                        "Use personal information","Use simple patterns","Share your password","Enable 2 Factor Authentication"
                    }
                }   ,

                new QuizQuestion
                {
                    Question="What is not a safe browsing practice?",
                    CorrectChoice ="Ignore software updates",
                    Choices = new List<string>
                    {
                        "Use secure networks", "Backup your data", "Usa a VPN", "Ignore software updates"
                    }
                }   ,

                new QuizQuestion{

                    Question="What is the strongest password here?",
                    CorrectChoice ="merCEDE$5241",
                    Choices = new List<string>
                    {
                    "pa$$word", "easyCOMEeasyGO", "JOHNSMITH1995", "merCEDE$5241"
                    }
                }
            };
        }//end of the method load quiz data or info

        private void HandleAnswerSelection(object sender, RoutedEventArgs e)
        {
            //use sender object name to get the selected button
            selectedChoice = sender as Button;

            string chosen = selectedChoice.Content.ToString();

            //then check with correct on the current quiz
            string correct = quizData[questionIndex].CorrectChoice;

            //then check if correct or not by if statement
            if (chosen == correct)
            {
                //then set the button background color
                selectedChoice.Background = Brushes.Green;
                //assing to hold
                correctChoiceButton = selectedChoice;
            }
            else
            {
                //if incorrect
                selectedChoice.Background = Brushes.DarkRed;
                correctChoiceButton = selectedChoice;
            }

        }//end of handle answer selection event handler

        //event handler for the next button
        private void HandleNextQuestion(object sender, RoutedEventArgs e)
        {
            //check if the user selected one of the choices
            if (selectedChoice == null)
            {
                //then show error message
                MessageBox.Show("Choose one of the 4 choices");
            }
            else
            {
                //then add points , and only if correct
                string chosen = selectedChoice.Content.ToString();
                string correct = quizData[questionIndex].CorrectChoice;

                //check if correct 
                if (chosen == correct)
                {
                    //then add point
                    currentScore++;
                    //then show the score
                    DisplayScore.Text = "Score : " + currentScore;

                    //then move to the next index question
                    questionIndex++;
                    //show the question again for the next one
                    showQuiz();
                }
                else
                {
                    //move to the next question 
                    questionIndex++;
                    showQuiz();
                }
            }
        }//end of the handle next question event handler
        //End of Quiz Content

        //Activity Log Content
        private void activityLog(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < activities.Count; i++)
            {
                MessageBox.Show("Activity " + (i + 1) + ": " + activities[i] + "\n");
            }
        }//End of Activity Log Content

        //NLP Content
        private void TrainModel()
        {
            var trainDataView = mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text))
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));

            var model = pipeline.Fit(trainDataView);

            predEngine = mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
        }

        private void emotions(object sender, RoutedEventArgs e)
        {
            string input = emotion.Text;

            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Please enter how you are feeling");
                return;
            }

            var prediction = predEngine.Predict(new SentimentData { Text = input });

            // Confidence percentages
            float positiveScore = prediction.Probability * 100;
            float negativeScore = 100 - positiveScore;

            string emotionType = prediction.Prediction ? "Positive" : "Negative";

            // Construct feedback message
            string feedback = $"{emotionType} Emotion\n" +
                              $"Positive: {positiveScore:F1}%\n" +
                              $"Negative: {negativeScore:F1}%\n";

            string reply;
            if (positiveScore > 75)
            {
                reply = "You seem really upbeat! Keep shining ";
            }
            else if (positiveScore > 50)
            {
                reply = "You’re doing alright — keep your chin up!";
            }
            else if (positiveScore > 30)
            {
                reply = "I sense some heaviness — it’s okay to feel down sometimes.";
            }
            else
            {
                reply = "You seem quite low. Be kind to yourself — brighter days will come.";
            }
            show_emotion_detected.Text = feedback + reply;

            // Ask user to confirm if prediction was correct
            var result = MessageBox.Show("Was this prediction correct?", "Feedback", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
            {
                // Ask what the correct label was
                var correct = MessageBox.Show("Was it actually Positive?", "Correct Label", MessageBoxButton.YesNo);
                bool correctLabel = (correct == MessageBoxResult.Yes);

                // Add to training data and retrain model
                trainingData.Add(new SentimentData { Text = input, Label = correctLabel });
                TrainModel();

                MessageBox.Show("Thanks! I've learned from that.");
            }
        }//End of NLP Content
        private void store_replies()
        {
            //store values of replies
            replies["browsing"] = new List<string> { "Safe Browsing Practices: Keep Your Browser and Plugins Up-to-Date. Updates often include security patches that address vulnerabilities that hackers could exploit." };
            replies["browsing"] = new List<string> { "Safe Browsing Practices: Use a VPN(Virtual Private Network). A VPN encrypts your internet traffic, making it harder for hackers to intercept your data." };
            replies["browsing"] = new List<string> { "Safe Browsing Practices: Install and Use Antivirus Software. Antivirus software helps detect and remove malware and viruses that can compromise your device and data." };

            replies["password"] = new List<string> { "Password: Never reuse passwords across different accounts. If one is compromised, others could be at risk." };
            replies["password"] = new List<string> { "Password: Enable Multi-Factor Authentication(MFA) for an additional layer of security beyond just passwords." };
            replies["password"] = new List<string> { "Password: Avoid common passwords like '12345' or 'abcde'. Hackers use automated tools to crack them easily." };

            replies["phishing"] = new List<string> { "Phishing: Phishing is used to deceive victims into clicking on malicious links, downloading harmful attachments, or entering sensitive information on fake websites." };
            replies["phishing"] = new List<string> { "Phishing: A good way to protect yourself is never download attachments or click on links in unsolicited emails. Cybercriminals often disguise malware in these files." };
            replies["phishing"] = new List<string> { "Phishing: You can protect yourself by being cautious of emails that claim your account is at risk and demand urgent action. Verify directly with the company instead." };
        }

        private void store_ignores()
        {
            //store values of ignore
            string[] ignoredWords = {
                "tell", "me", "about", "recognize", "what", "is", "safe", "can", "i", "ask", "questions", "other",
                "how", "to", "do", "does", "the", "explain", "define", "between", "mean", "why", "when", "where",
                "who", "which", "give", "more", "example", "detail"
            };

            foreach (var word in ignoredWords)
            {
                ignore.Add(word);
            }
        }

        public string tips_phishing()
        {
            List<string> tips = new List<string>();

            tips.Add("Be cautious of unexpected emaols with urgent requests for personal information.");
            tips.Add("Always check email addresses and domain names carefully.");
            tips.Add("Never click links or open attachments in suspicious messages");

            Random get_Index = new Random();
            int found_Index = get_Index.Next(0, tips.Count);

            return tips[found_Index];
        }

        public string tips_password()
        {
            List<string> tips = new List<string>();

            tips.Add("Make sure your password is atleast 12 characters long, 14 or more is usually better");
            tips.Add("Combine uppercase and lowercase letters, numbers, and symbols.");
            tips.Add("Do not use dictionary words or common phrases.");


            Random get_Index = new Random();
            int found_Index = get_Index.Next(0, tips.Count);

            return tips[found_Index];
        }

        public string tips_browsing()
        {
            List<string> tips = new List<string>();

            tips.Add("Always keep your operating system up-to-date to patch security vulnerabilities.");
            tips.Add("Make use of VPN to encrypt your internet traffic, especially on public Wi-Fi");
            tips.Add("Regularly clear your browser's cache and cookies to prevent data collection.");

            Random get_Index = new Random();
            int found_Index = get_Index.Next(0, tips.Count);

            return tips[found_Index];
        }

        public string worried_phishing()
        {
            List<string> worried = new List<string>();

            worried.Add("It's okay to feel that way. The emails can be very deceiving.");
            worried.Add("Understandably so. It tends to be hard to recognise dodgy emails if you do not have the proper awareness to recognise them.");

            Random get_Index = new Random();
            int found_Index = get_Index.Next(0, worried.Count);

            return worried[found_Index];
        }

        public string worried_browsing()
        {
            List<string> worried = new List<string>();

            worried.Add("It's okay to feel that way. The internet is filled with bad people with malicious tendencies.");
            worried.Add("Understandably so. The internet is a broad entity with many vulnerabilities.");

            Random get_Index = new Random();
            int found_Index = get_Index.Next(0, worried.Count);

            return worried[found_Index];
        }

        public string worried_password()
        {
            List<string> worried = new List<string>();

            worried.Add("It's okay to feel that way. Many people tend to feel the same way about this topic.");
            worried.Add("Understandably so. Passwords are usually our only security from people trying access our personal information.");

            Random get_Index = new Random();
            int found_Index = get_Index.Next(0, worried.Count);

            return worried[found_Index];
        }

        public string interest_phishing()
        {
            List<string> interest = new List<string>();

            interest.Add("That's great to hear! I will remember that next time.");
            interest.Add("Awesome! It is very wise to teach yourself about such topics.");

            Random get_Index = new Random();
            int found_Index = get_Index.Next(0, interest.Count);

            return interest[found_Index];
        }
        public string interest_browsing()
        {
            List<string> interest = new List<string>();

            interest.Add("That's great to hear! I will remember that next time.");
            interest.Add("Awesome! It is very wise to teach yourself about such topics.");

            Random get_Index = new Random();
            int found_Index = get_Index.Next(0, interest.Count);

            return interest[found_Index];
        }
        public string interest_password()
        {
            List<string> interest = new List<string>();

            interest.Add("That's great to hear! I will remember that next time.");
            interest.Add("Awesome! It is very wise to teach yourself about such topics.");

            Random get_Index = new Random();
            int found_Index = get_Index.Next(0, interest.Count);

            return interest[found_Index];
        }
    }
}