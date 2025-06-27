using System;
using System.Collections.Generic;
using System.Linq;
using System.Media; // Required for SoundPlayer
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms; // This line is CRUCIAL for MessageBox.Show to work

namespace CybersecurityChatbotGUI
{
    public class ChatbotLogic
    {
        // Non-static global variables for memory
        public string UserName { get; private set; } = "Friend"; // Default name
        private string userFavoriteCybersecurityTopic = "";
        private string lastDiscussedTopic = "";

        // Task and Reminder Management
        private List<CybersecurityTask> tasks; // List to store user-defined tasks
        private CybersecurityTask currentQuiz = null; // To track if a quiz is active
        private int currentQuestionIndex = 0;
        private int quizScore = 0;
        private List<QuizQuestion> quizQuestions; // List to hold quiz questions

        // Activity Log
        private List<ActivityLogEntry> activityLog; // List to store activity log entries

        private Dictionary<string, List<string>> keywordResponses;
        private delegate string KeywordResponseDelegate(string keyword);
        private Dictionary<string, KeywordResponseDelegate> keywordActionDelegates;

        private Random random = new Random();

        // OnBotResponseReady event was removed previously as Form1 directly processes the returned string.

        public ChatbotLogic()
        {
            InitializeChatbotData();
            tasks = new List<CybersecurityTask>();
            activityLog = new List<ActivityLogEntry>();
            InitializeQuizQuestions(); // Initialize quiz questions
        }

        // Method to set the user's name from the GUI
        public void SetUserName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                UserName = name.Trim(); // Assign to public property
                LogActivity("Name set", $"User's name set to {UserName}"); // Log name change
            }
        }

        // --- Core Chatbot Data Initialization ---
        private void InitializeChatbotData()
        {
            keywordResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                // Basic Greetings & Farewell
                { "hello", new List<string> { "Hi there!", "Hello!", "Greetings!" } },
                { "hi", new List<string> { "Hey!", "Hello!", "What's up?" } },
                { "how are you", new List<string> { "I'm just a program, but I'm ready to help you with cybersecurity!", "I'm functioning perfectly, thanks for asking!" } },
                { "goodbye", new List<string> { $"Goodbye, {UserName}! Have an amazing day. Stay vigilant online!" } },
                { "bye", new List<string> { $"Farewell, {UserName}! Stay safe on the internet." } },
                { "exit", new List<string> { $"Closing down, {UserName}. Remember to always be cyber-aware!" } },

                // Cybersecurity Topics
                { "password", new List<string> {
                    "A strong password is at least 12 characters long, uses a mix of uppercase and lowercase letters, numbers, and symbols. Avoid common words or personal information.",
                    "Always use unique passwords for different accounts. If one account is compromised, others remain safe.",
                    "Consider using a password manager to securely store and generate complex passwords. It makes managing many unique passwords much easier!"
                }},
                { "phishing", new List<string> {
                    "Phishing is a type of online fraud where attackers try to trick you into revealing sensitive information, often through fake emails or websites.",
                    "Be suspicious of unsolicited emails or messages asking for personal details, especially if they contain urgent or threatening language.",
                    "Always check the sender's email address and hover over links (without clicking!) to see the actual URL before visiting."
                }},
                { "malware", new List<string> {
                    "Malware is a broad term for malicious software designed to harm or gain unauthorized access to your computer.",
                    "Examples include viruses, worms, trojans, ransomware, and spyware.",
                    "Keep your operating system and software updated, use reputable antivirus software, and be careful about opening suspicious attachments or downloading files from untrusted sources."
                }},
                { "2fa", new List<string> {
                    "Two-factor authentication (2FA) adds an extra layer of security beyond just a password. It usually involves a second piece of information only you would have, like a code from your phone.",
                    "Enabling 2FA significantly reduces the risk of unauthorized access to your accounts, even if your password is stolen.",
                    "Most major online services offer 2FA. Look for it in your account's security settings."
                }},
                 { "encryption", new List<string> {
                    "Encryption is the process of converting information or data into a code, especially to prevent unauthorized access.",
                    "It's like scrambling a message so only someone with the right key can unscramble and read it.",
                    "Look for 'HTTPS' in website addresses, which indicates a secure, encrypted connection, especially for online shopping or banking."
                }},
                { "privacy", new List<string> {
                    "Online privacy is about controlling who can see your personal information and how it's used.",
                    "Regularly review privacy settings on social media and other online accounts. Be mindful of what you share publicly.",
                    "Using a VPN (Virtual Private Network) can help enhance your privacy by encrypting your internet connection and masking your IP address."
                }},
                { "backup", new List<string> {
                    "Regularly backing up your data is crucial! If your device fails or is infected with ransomware, a backup can save your files.",
                    "You can back up to an external hard drive, cloud storage (like Google Drive, OneDrive, Dropbox), or a network-attached storage (NAS) device.",
                    "Test your backups periodically to ensure they are working correctly and that you can restore your data."
                }},
                { "update", new List<string> {
                    "Keeping your software and operating system updated is vital for cybersecurity. Updates often include security patches for newly discovered vulnerabilities.",
                    "Enable automatic updates whenever possible to ensure you're always protected.",
                    "Outdated software is a common entry point for cyber attackers."
                }},
                { "cybersecurity", new List<string> {
                    "Cybersecurity is the practice of protecting systems, networks, and programs from digital attacks.",
                    "It's about safeguarding your information and devices in the digital world, encompassing everything from strong passwords to understanding phishing scams.",
                    "Staying informed and practicing good digital habits are key to strong cybersecurity."
                }},
                // Task Assistant & Reminders (Basic NLP Simulation)
                { "add task", new List<string> {
                    "Sure, I can help you add a task. What is the task you'd like to add?",
                    "Okay, what is the task you want to remember?"
                }},
                 { "set reminder", new List<string> {
                    "I can set a reminder for you. What is the reminder for?",
                    "Alright, tell me what you need to be reminded about."
                }},
                 { "quiz", new List<string> {
                    "Ready for a cybersecurity quiz? I'll ask you a question, and you pick the best answer!",
                    "Let's test your knowledge! Here's your first quiz question:"
                 }},
                 { "game", new List<string> {
                    "Want to play a game? I have a cybersecurity quiz ready!",
                    "Great! Let's start the cybersecurity quiz. Are you ready?"
                 }},
                 { "activity log", new List<string> {
                    "Here's a summary of recent actions: [No activity logged yet, implement this feature!]",
                    "I can show you your recent activities. (Feature coming soon!)"
                 }},
                 { "what have you done", new List<string> {
                    "I've responded to your queries and helped you learn about cybersecurity! What else can I do?",
                    "So far, I've answered your questions. I'm ready for more!"
                 }},
                // Sentiment Analysis (Simple example)
                { "happy", new List<string> { "That's wonderful to hear!", "I'm glad you're feeling good!" } },
                { "sad", new List<string> { "I'm sorry to hear that. Is there anything cybersecurity-related I can help with to cheer you up?", "I hope things get better soon." } },
                { "frustrated", new List<string> { "I understand. Cybersecurity can sometimes be complex. How can I assist you?", "Let's break down what's frustrating you." } },
                { "thank you", new List<string> { "You're welcome!", "Glad I could help!", "Anytime!" } },
                { "thanks", new List<string> { "No problem!", "My pleasure!", "You got it!" } }
            };

            keywordActionDelegates = new Dictionary<string, KeywordResponseDelegate>(StringComparer.OrdinalIgnoreCase)
            {
                {"password", (k) => GetRandomResponse(k) },
                {"scam", (k) => GetRandomResponse(k) },
                {"privacy", (k) => GetRandomResponse(k) },
                {"phishing", (k) => GetRandomResponse(k) },
                {"malware", (k) => GetRandomResponse(k) },
                {"2fa", (k) => GetRandomResponse(k) },
                {"how are you", (k) => GetRandomResponse(k) },
                {"purpose", (k) => GetRandomResponse(k) },
                {"what can i ask", (k) => GetRandomResponse(k) },
                {"add task", (k) => "Okay, what task would you like to add? E.g., 'Add task: Update antivirus'." },
                {"set reminder", (k) => "For which task or topic would you like to set a reminder? E.g., 'Remind me to change password in 3 days'." },
                {"show tasks", (k) => ShowTasks() },
                {"complete task", (k) => "Which task would you like to mark as complete? Please provide the task number or title." },
                {"delete task", (k) => "Which task would you like to delete? Please provide the task number or title." },
                {"start quiz", (k) => StartQuiz() },
                {"show activity log", (k) => ShowActivityLog() },
                {"what have you done for me", (k) => ShowActivityLog() }
            };
        }

        // --- Activity Log Management ---
        private void LogActivity(string actionType, string description)
        {
            activityLog.Add(new ActivityLogEntry(actionType, description));
            // Keep only the last 10 entries for clarity
            if (activityLog.Count > 10)
            {
                activityLog.RemoveRange(0, activityLog.Count - 10);
            }
        }

        private string ShowActivityLog()
        {
            if (activityLog.Count == 0)
            {
                LogActivity("View Log", "User attempted to view an empty activity log");
                return $"Hello {UserName}! My activity log is currently empty. I haven't done much yet.";
            }

            StringBuilder logSummary = new StringBuilder();
            logSummary.AppendLine($"Hello {UserName}! Here's a summary of my recent actions:");

            // Display in reverse chronological order (most recent first)
            var recentActivities = activityLog.OrderByDescending(entry => entry.Timestamp)
                                              .Take(10); // Display only last 5-10 actions

            int counter = 1;
            foreach (var entry in recentActivities)
            {
                logSummary.AppendLine($"{counter}. [{entry.Timestamp.ToShortTimeString()}] {entry.Description}");
                counter++;
            }
            LogActivity("View Log", "User viewed activity log");
            return logSummary.ToString();
        }

        // --- Task Assistant Management ---
        private string AddTask(string input)
        {
            string taskTitle = "";
            string reminderDate = "";
            string description = "";

            // Extract task title
            if (input.Contains("add task:") && input.Split(new[] { "add task:" }, StringSplitOptions.RemoveEmptyEntries).Length > 0)
            {
                taskTitle = input.Split(new[] { "add task:" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            }
            else if (input.Contains("add a task to") && input.Split(new[] { "add a task to" }, StringSplitOptions.RemoveEmptyEntries).Length > 0)
            {
                taskTitle = input.Split(new[] { "add a task to" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            }
            else
            {
                // Fallback for more general "add task" command without specific phrasing
                taskTitle = input.Replace("add task", "").Replace("add a task", "").Trim();
            }

            // Basic extraction of reminder details (very simplified)
            if (input.Contains("remind me in"))
            {
                var parts = input.Split(new[] { "remind me in" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    string reminderPart = parts[1].Trim();
                    // Attempt to parse for "X days", "tomorrow", etc.
                    if (reminderPart.Contains("day"))
                    {
                        if (int.TryParse(new string(reminderPart.Where(char.IsDigit).ToArray()), out int days))
                        {
                            reminderDate = DateTime.Now.AddDays(days).ToShortDateString();
                        }
                    }
                    else if (reminderPart.Contains("tomorrow"))
                    {
                        reminderDate = DateTime.Now.AddDays(1).ToShortDateString();
                    }
                    // More complex date parsing would be needed for robustness
                }
            }
            else if (input.Contains("on") && (input.Contains("tomorrow") || input.Contains("date")))
            {
                if (input.Contains("tomorrow"))
                {
                    reminderDate = DateTime.Now.AddDays(1).ToShortDateString();
                }
                // Add more sophisticated date parsing if needed
            }

            // Simple description from task title for now
            description = taskTitle; // For simplicity, task title is the description

            CybersecurityTask newTask = new CybersecurityTask(taskTitle, description, reminderDate);
            tasks.Add(newTask);
            LogActivity("Task Added", $"Task added: '{taskTitle}'{(string.IsNullOrEmpty(reminderDate) ? "" : " with reminder for " + reminderDate)}");

            string response = $"Task added: '{taskTitle}'.";
            if (!string.IsNullOrEmpty(reminderDate))
            {
                response += $" I'll remind you on {reminderDate}.";
            }
            else
            {
                response += " Would you like to set a reminder for this task?";
            }
            return response;
        }

        private string ShowTasks()
        {
            if (tasks.Count == 0)
            {
                LogActivity("View Tasks", "User attempted to view an empty task list");
                return "You currently have no cybersecurity tasks. Would you like to add one?";
            }

            StringBuilder taskList = new StringBuilder();
            taskList.AppendLine("Here are your current cybersecurity tasks:");
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                string reminderInfo = string.IsNullOrEmpty(task.ReminderDate) ? "(No reminder set)" : $"(Reminder for {task.ReminderDate})";
                string status = task.IsCompleted ? "[COMPLETED]" : "[PENDING]";
                taskList.AppendLine($"{i + 1}. {task.Title} {status} {reminderInfo}");
            }
            LogActivity("View Tasks", "User viewed task list");
            return taskList.ToString();
        }

        private string CompleteTask(string input)
        {
            string response = "I couldn't find that task. Please specify the task number or title.";
            string searchTerm = input.Replace("complete task", "").Replace("mark as complete", "").Trim();

            // Try by number
            if (int.TryParse(searchTerm, out int taskNumber) && taskNumber > 0 && taskNumber <= tasks.Count)
            {
                tasks[taskNumber - 1].IsCompleted = true;
                response = $"Task '{tasks[taskNumber - 1].Title}' marked as complete. Great job!";
                LogActivity("Task Completed", $"Task '{tasks[taskNumber - 1].Title}' marked as complete");
            }
            else
            {
                // Try by title using IndexOf for broader .NET compatibility
                var taskToComplete = tasks.FirstOrDefault(t => t.Title.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);
                if (taskToComplete != null)
                {
                    taskToComplete.IsCompleted = true;
                    response = $"Task '{taskToComplete.Title}' marked as complete. Excellent!";
                    LogActivity("Task Completed", $"Task '{taskToComplete.Title}' marked as complete");
                }
            }
            return response;
        }

        private string DeleteTask(string input)
        {
            string response = "I couldn't find that task to delete. Please specify the task number or title.";
            string searchTerm = input.Replace("delete task", "").Replace("remove task", "").Trim();

            // Try by number
            if (int.TryParse(searchTerm, out int taskNumber) && taskNumber > 0 && taskNumber <= tasks.Count)
            {
                string deletedTitle = tasks[taskNumber - 1].Title;
                tasks.RemoveAt(taskNumber - 1);
                response = $"Task '{deletedTitle}' has been deleted.";
                LogActivity("Task Deleted", $"Task '{deletedTitle}' deleted");
            }
            else
            {
                // Try by title using IndexOf for broader .NET compatibility
                var taskToDelete = tasks.FirstOrDefault(t => t.Title.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);
                if (taskToDelete != null)
                {
                    tasks.Remove(taskToDelete);
                    response = $"Task '{taskToDelete.Title}' has been deleted.";
                    LogActivity("Task Deleted", $"Task '{taskToDelete.Title}' deleted");
                }
            }
            return response;
        }

        // --- Quiz Game Management ---
        private void InitializeQuizQuestions()
        {
            quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion(
                    "What should you do if you receive an email asking for your password?",
                    new List<string> { "A) Reply with your password", "B) Delete the email", "C) Report the email as phishing", "D) Ignore it" },
                    "C",
                    "Correct! Reporting phishing emails helps prevent scams."
                ),
                new QuizQuestion(
                    "True or False: Using the same password for all your online accounts is a good security practice.",
                    new List<string> { "True", "False" },
                    "False",
                    "False. Using unique, strong passwords for each account is crucial to prevent credential stuffing attacks."
                ),
                new QuizQuestion(
                    "Which of the following is an example of strong password?",
                    new List<string> { "A) password123", "B) YourName1!", "C) P@$$w0rd!", "D) 9jK!pS@2xT7fR" },
                    "D",
                    "Correct! Strong passwords combine uppercase, lowercase, numbers, and symbols, and are long."
                ),
                new QuizQuestion(
                    "What is Two-Factor Authentication (2FA)?",
                    new List<string> { "A) Using two different passwords", "B) Logging in from two different devices", "C) Requiring two forms of verification for login", "D) Sharing your password with two friends" },
                    "C",
                    "Correct! 2FA adds an extra layer of security beyond just a password."
                ),
                 new QuizQuestion(
                    "Malware is software designed to:",
                    new List<string> { "A) Improve computer performance", "B) Cause harm or steal data", "C) Help manage files", "D) Protect your privacy" },
                    "B",
                    "That's right! Malware, like viruses and ransomware, is designed to damage or gain unauthorized access to computer systems."
                ),
                new QuizQuestion(
                    "True or False: Public Wi-Fi networks are generally safe for sensitive transactions like online banking.",
                    new List<string> { "True", "False" },
                    "False",
                    "False. Public Wi-Fi networks are often unsecured, making your data vulnerable to interception. It's best to avoid sensitive transactions on them."
                ),
                new QuizQuestion(
                    "What is 'phishing'?",
                    new List<string> { "A) A type of fishing sport", "B) A way to catch digital fish", "C) A cyberattack to trick users into revealing sensitive information", "D) A network security protocol" },
                    "C",
                    "Exactly! Phishing attacks use deceptive emails or websites to trick individuals into divulging personal data."
                ),
                new QuizQuestion(
                    "Which of these is a good practice for protecting your online privacy?",
                    new List<string> { "A) Sharing all personal details on social media", "B) Accepting all cookies on websites", "C) Regularly reviewing app permissions and privacy settings", "D) Using your real birthdate for all online profiles" },
                    "C",
                    "Correct! Actively managing your privacy settings and permissions helps control your digital footprint."
                ),
                new QuizQuestion(
                    "True or False: Clicking on suspicious links in emails is harmless if you don't download anything.",
                    new List<string> { "True", "False" },
                    "False",
                    "False. Even without downloading, clicking a malicious link can lead to drive-by downloads or expose you to exploit kits."
                ),
                new QuizQuestion(
                    "What does VPN stand for?",
                    new List<string> { "A) Very Private Network", "B) Virtual Personal Network", "C) Virtual Private Network", "D) Verified Protection Network" },
                    "C",
                    "You got it! VPNs create a secure, encrypted connection over a less secure network, like the internet."
                )
            };
        }

        private string StartQuiz()
        {
            if (quizQuestions.Count == 0)
            {
                LogActivity("Quiz Attempt", "User tried to start quiz, but no questions available.");
                return "I don't have any quiz questions loaded right now. Please check back later!";
            }

            quizScore = 0;
            currentQuestionIndex = 0;
            currentQuiz = new CybersecurityTask("Cybersecurity Quiz", "User is currently taking a cybersecurity quiz.", ""); // Use task for quiz tracking
            LogActivity("Quiz Started", $"User started a cybersecurity quiz with {quizQuestions.Count} questions.");
            return GetNextQuizQuestion();
        }

        private string GetNextQuizQuestion()
        {
            if (currentQuestionIndex < quizQuestions.Count)
            {
                QuizQuestion q = quizQuestions[currentQuestionIndex];
                StringBuilder questionText = new StringBuilder();
                questionText.AppendLine($"Question {currentQuestionIndex + 1} of {quizQuestions.Count}:");
                questionText.AppendLine(q.QuestionText);
                foreach (var option in q.Options)
                {
                    questionText.AppendLine(option);
                }
                return questionText.ToString();
            }
            else
            {
                return EndQuiz();
            }
        }

        private string SubmitQuizAnswer(string answer)
        {
            if (currentQuiz == null)
            {
                return "You are not currently taking a quiz. Type 'start quiz' to begin!";
            }

            QuizQuestion currentQ = quizQuestions[currentQuestionIndex];
            string botFeedback;

            if (currentQ.IsCorrect(answer))
            {
                quizScore++;
                botFeedback = $"Correct! {currentQ.Feedback}";
            }
            else
            {
                botFeedback = $"Incorrect. The correct answer was {currentQ.CorrectAnswer}. {currentQ.Feedback}";
            }
            LogActivity("Quiz Answered", $"Answered question {currentQuestionIndex + 1} with '{answer}'. {(currentQ.IsCorrect(answer) ? "Correct" : "Incorrect")}.");

            currentQuestionIndex++;
            string nextQuestionOrEnd = GetNextQuizQuestion();

            // Return feedback immediately, followed by the next question or quiz summary
            return $"{botFeedback}\n\n{nextQuestionOrEnd}";
        }

        private string EndQuiz()
        {
            string finalMessage = $"Quiz completed, {UserName}! You scored {quizScore} out of {quizQuestions.Count}.";
            if (quizScore == quizQuestions.Count)
            {
                finalMessage += " Amazing! You're a cybersecurity pro! 🏆";
            }
            else if (quizScore >= quizQuestions.Count / 2)
            {
                finalMessage += " Good effort! Keep learning to stay safe online.";
            }
            else
            {
                finalMessage += " You did well! There's always more to learn in cybersecurity. Let me know if you have questions!";
            }
            LogActivity("Quiz Ended", $"User completed quiz with score: {quizScore}/{quizQuestions.Count}.");
            currentQuiz = null; // End the quiz state
            return finalMessage;
        }

        // --- Memory and Recall ---
        private string StoreUserInterest(string topic)
        {
            userFavoriteCybersecurityTopic = topic;
            LogActivity("Interest Noted", $"User's favorite topic set to: {topic}");
            return $"Great, {UserName}! I'll remember that you're interested in {topic}. It's a crucial part of staying safe online.";
        }

        private string RecallUserInterest()
        {
            if (!string.IsNullOrEmpty(userFavoriteCybersecurityTopic))
            {
                LogActivity("Recall Interest", $"User recalled interest in: {userFavoriteCybersecurityTopic}");
                return $"As someone interested in {userFavoriteCybersecurityTopic}, you might want to review the security settings on your accounts or learn more about related threats.";
            }
            else
            {
                LogActivity("Recall Interest", "User attempted to recall interest, but none set.");
                return $"I haven't noted a specific cybersecurity topic you're most interested in yet, {UserName}. Tell me what you'd like to learn!";
            }
        }

        // --- Sentiment Detection (Simplified) ---
        private string DetectSentiment(string input)
        {
            input = input.ToLower();
            if (input.Contains("worried") || input.Contains("scared") || input.Contains("concerned") || input.Contains("anxious"))
            {
                return "worried";
            }
            if (input.Contains("curious") || input.Contains("wondering") || input.Contains("tell me more") || input.Contains("learn about"))
            {
                return "curious";
            }
            if (input.Contains("frustrated") || input.Contains("confused") || input.Contains("don't understand") || input.Contains("unclear"))
            {
                return "frustrated";
            }
            return "neutral";
        }

        private string GetSentimentResponse(string sentiment)
        {
            switch (sentiment)
            {
                case "worried":
                    return "It's completely understandable to feel that way. Scammers can be very convincing, but awareness is your best defense. Let me share some tips to help you stay safe.";
                case "curious":
                    return "That's great, curiosity is key to learning and staying safe! What specific aspect are you curious about?";
                case "frustrated":
                    return "I understand this can be a complex topic. Don't worry, we can break it down. What exactly is unclear?";
                default:
                    return ""; // No response for neutral sentiment
            }
        }

        // --- Main User Input Processing (Returns the bot's full response as a string) ---
        public async Task<string> ProcessUserInput(string input)
        {
            StringBuilder botResponse = new StringBuilder();
            input = input.ToLower().Trim();

            // If a quiz is active, prioritize quiz answer processing
            if (currentQuiz != null)
            {
                // Accept 'stop' or 'exit' to end quiz early
                if (input.Contains("stop quiz") || input.Contains("exit quiz") || input.Contains("cancel quiz"))
                {
                    LogActivity("Quiz Interrupted", "User stopped the quiz early.");
                    currentQuiz = null;
                    return $"Okay {UserName}, I've ended the quiz. Your final score was {quizScore} out of {currentQuestionIndex} questions attempted. You can start a new one anytime!";
                }
                // Process quiz answers (A, B, C, D, True, False)
                if (input.Equals("a") || input.Equals("b") || input.Equals("c") || input.Equals("d") || input.Equals("true") || input.Equals("false"))
                {
                    return SubmitQuizAnswer(input);
                }
                else
                {
                    return "Please provide your answer as A, B, C, D, True, or False. Or type 'stop quiz' to end.";
                }
            }


            // 1. Handle explicit exit command
            if (input.Contains("exit") || input.Contains("bye") || input.Contains("goodbye"))
            {
                LogActivity("Exit Command", "User requested to exit the chatbot.");
                // Special signal for the GUI to close
                return $"Goodbye, {UserName}! Have an amazing day. Stay vigilant online. 👋|EXIT_COMMAND|";
            }

            // 2. Handle specific task/reminder commands
            if (input.Contains("add task") || input.Contains("set task"))
            {
                LogActivity("Task Command", "User attempted to add a task.");
                return AddTask(input);
            }
            if (input.Contains("remind me") || input.Contains("set reminder"))
            {
                LogActivity("Reminder Command", "User attempted to set a reminder.");
                // This is a simplified reminder. A full implementation would parse date/time.
                // For now, if "remind me to X in Y days" is detected, it adds as task with reminder.
                return AddTask(input);
            }
            if (input.Contains("show tasks") || input.Contains("list tasks") || input.Contains("what are my tasks"))
            {
                LogActivity("View Tasks Command", "User requested to view tasks.");
                return ShowTasks();
            }
            if (input.Contains("complete task") || input.Contains("mark task as complete"))
            {
                LogActivity("Complete Task Command", "User attempted to complete a task.");
                return CompleteTask(input);
            }
            if (input.Contains("delete task") || input.Contains("remove task"))
            {
                LogActivity("Delete Task Command", "User attempted to delete a task.");
                return DeleteTask(input);
            }

            // 3. Handle quiz start command
            if (input.Contains("start quiz") || input.Contains("play quiz") || input.Contains("quiz me"))
            {
                return StartQuiz();
            }

            // 4. Handle activity log commands
            if (input.Contains("show activity log") || input.Contains("what have you done for me"))
            {
                return ShowActivityLog();
            }

            // 5. Detect and respond to sentiment (before keyword check to allow sentiment to override generic keyword)
            string detectedSentiment = DetectSentiment(input);
            string sentimentResponse = GetSentimentResponse(detectedSentiment);
            if (!string.IsNullOrEmpty(sentimentResponse))
            {
                botResponse.AppendLine(sentimentResponse);
                LogActivity("Sentiment Detected", $"Detected '{detectedSentiment}' sentiment.");
            }

            // 6. Handle explicit memory recall trigger
            if (input.Contains("what did i say") || input.Contains("my interest") || input.Contains("remembered"))
            {
                botResponse.AppendLine(RecallUserInterest());
                lastDiscussedTopic = ""; // Clear last discussed topic as context changed
                LogActivity("Memory Recall", $"User recalled favorite topic.");
                return botResponse.ToString();
            }

            // 7. Handle "Tell me more" as a specific follow-up command
            if (input.Contains("tell me more") && !input.Contains("about"))
            {
                string topicForMoreInfo = "";
                if (!string.IsNullOrEmpty(lastDiscussedTopic) && keywordResponses.ContainsKey(lastDiscussedTopic))
                {
                    topicForMoreInfo = lastDiscussedTopic;
                }
                else if (!string.IsNullOrEmpty(userFavoriteCybersecurityTopic) && keywordResponses.ContainsKey(userFavoriteCybersecurityTopic))
                {
                    topicForMoreInfo = userFavoriteCybersecurityTopic;
                }

                if (!string.IsNullOrEmpty(topicForMoreInfo))
                {
                    botResponse.AppendLine($"Let's dive deeper into {topicForMoreInfo}. Here's another tip: {GetRandomResponse(topicForMoreInfo)}");
                    LogActivity("More Info", $"Provided more info on '{topicForMoreInfo}'.");
                }
                else
                {
                    botResponse.AppendLine("I'm not sure which topic you'd like more information on. Could you specify? (e.g., 'tell me more about password')");
                }
                lastDiscussedTopic = "";
                return botResponse.ToString();
            }

            // 8. Keyword recognition and dynamic response using delegates
            string matchedKeyword = "";

            foreach (var entry in keywordActionDelegates)
            {
                string keyword = entry.Key;
                if (System.Text.RegularExpressions.Regex.IsMatch(input, @"\b" + System.Text.RegularExpressions.Regex.Escape(keyword) + @"\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    // If the input contains "interested in" or "my favorite is" and a valid topic keyword, store it
                    if ((input.Contains("interested in") || input.Contains("my favorite is")) &&
                        (keyword.Equals("password", StringComparison.OrdinalIgnoreCase) ||
                         keyword.Equals("scam", StringComparison.OrdinalIgnoreCase) ||
                         keyword.Equals("privacy", StringComparison.OrdinalIgnoreCase) ||
                         keyword.Equals("phishing", StringComparison.OrdinalIgnoreCase) ||
                         keyword.Equals("malware", StringComparison.OrdinalIgnoreCase) ||
                         keyword.Equals("2fa", StringComparison.OrdinalIgnoreCase)))
                    {
                        botResponse.AppendLine(StoreUserInterest(keyword));
                    }
                    else if (keywordResponses.ContainsKey(keyword))
                    {
                        botResponse.AppendLine(entry.Value(keyword));
                    }
                    else
                    {
                        // This branch handles cases where a keyword has an action delegate
                        // but might not have a direct entry in keywordResponses (e.g., "add task" itself)
                        // Since these are handled by specific 'if' blocks above, this 'else' is for completeness
                        // but might not add to botResponse here if the action already did.
                    }

                    matchedKeyword = keyword;
                    break;
                }
            }

            // 9. Default response / Error Handling
            if (botResponse.Length == 0)
            {
                botResponse.AppendLine("I'm not sure I understand. Can you try rephrasing? You can ask about topics like 'password', 'phishing', 'malware', 'privacy', '2FA', or try commands like 'add task', 'show tasks', 'start quiz', or 'show activity log'.");
                lastDiscussedTopic = "";
                LogActivity("Unrecognized Input", $"User input: '{input}' was not recognized.");
            }
            else
            {
                lastDiscussedTopic = matchedKeyword;
                LogActivity("Keyword Response", $"Responded to keyword: '{matchedKeyword}'.");
            }

            // Simple conversation flow continuation prompt
            botResponse.AppendLine("\nIs there anything else you'd like to know or do?");

            return botResponse.ToString();
        }

        // Helper to get a random response from a list for a given keyword
        private string GetRandomResponse(string keyword)
        {
            if (keywordResponses.ContainsKey(keyword))
            {
                List<string> responses = keywordResponses[keyword];
                int index = random.Next(responses.Count);
                return responses[index];
            }
            return "I don't have information on that specific topic yet.";
        }
    }

    // --- Helper Classes for Tasks, Reminders, Quiz, and Activity Log ---

    public class CybersecurityTask
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ReminderDate { get; set; } // Simplified: just a string for date
        public bool IsCompleted { get; set; }
        public DateTime CreatedDate { get; private set; }

        public CybersecurityTask(string title, string description, string reminderDate = "")
        {
            Title = title;
            Description = description;
            ReminderDate = reminderDate;
            IsCompleted = false;
            CreatedDate = DateTime.Now;
        }
    }

    public class QuizQuestion
    {
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }
        public string CorrectAnswer { get; set; } // "A", "B", "C", "D", "True", "False"
        public string Feedback { get; set; }

        public QuizQuestion(string questionText, List<string> options, string correctAnswer, string feedback)
        {
            QuestionText = questionText;
            Options = options;
            CorrectAnswer = correctAnswer.Trim().ToUpper();
            Feedback = feedback;
        }

        public bool IsCorrect(string userAnswer)
        {
            return userAnswer.Trim().ToUpper() == CorrectAnswer;
        }
    }

    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; private set; }
        public string ActionType { get; private set; } // e.g., "Task Added", "Quiz Started", "Name Set"
        public string Description { get; private set; } // Detailed description of the action

        public ActivityLogEntry(string actionType, string description)
        {
            Timestamp = DateTime.Now;
            ActionType = actionType;
            Description = description;
        }
    }
}
