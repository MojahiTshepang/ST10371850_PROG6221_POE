using System;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CybersecurityChatbotGUI
{
    public partial class Form1 : Form
    {
        private ChatbotLogic chatbot;
        private bool isInitialGreeting = true; // To manage the initial name input

        public Form1()
        {
            InitializeComponent();
            chatbot = new ChatbotLogic();

            btnSend.Click += btnSend_Click;
            txtUserInput.KeyDown += txtUserInput_KeyDown;

            AppendBotMessage("Hello! What's your name?");
            this.Text = "Cybersecurity Awareness Bot";
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Optional: Play voice greeting after the form loads
            // await Task.Run(() => chatbot.PlayVoiceGreeting());
        }

        private void AppendBotMessage(string message)
        {
            rtbChatHistory.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            rtbChatHistory.SelectionColor = Color.Blue;
            rtbChatHistory.AppendText("Bot: ");

            rtbChatHistory.SelectionFont = new Font("Segoe UI", 10, FontStyle.Regular);
            rtbChatHistory.SelectionColor = Color.Black;
            rtbChatHistory.AppendText(message + "\n\n");

            rtbChatHistory.ScrollToCaret();
        }

        private void AppendUserMessage(string message)
        {
            rtbChatHistory.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            rtbChatHistory.SelectionColor = Color.Green;
            rtbChatHistory.AppendText("You: ");

            rtbChatHistory.SelectionFont = new Font("Segoe UI", 10, FontStyle.Regular);
            rtbChatHistory.SelectionColor = Color.Black;
            rtbChatHistory.AppendText(message + "\n\n");

            rtbChatHistory.ScrollToCaret();
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            await ProcessUserChatInput();
        }

        private async void txtUserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await ProcessUserChatInput();
            }
        }

        private async Task ProcessUserChatInput()
        {
            string userInput = txtUserInput.Text.Trim();

            if (string.IsNullOrEmpty(userInput))
            {
                return;
            }

            if (isInitialGreeting)
            {
                AppendUserMessage(userInput);
                chatbot.SetUserName(userInput);
                AppendBotMessage($"Hello, {chatbot.UserName}! 👋 I'm here to help you stay safe online. What cybersecurity topic are you interested in today?");
                isInitialGreeting = false;
                txtUserInput.Clear();
                return;
            }

            AppendUserMessage(userInput);

            string botResponse;

            if (userInput.ToLower().StartsWith("remind me to"))
            {
                SetReminder(userInput);
                botResponse = "Got it! I’ll remind you.";
            }
            else
            {
                botResponse = await chatbot.ProcessUserInput(userInput);
            }

            if (botResponse.EndsWith("|EXIT_COMMAND|"))
            {
                botResponse = botResponse.Replace("|EXIT_COMMAND|", "").Trim();
                AppendBotMessage(botResponse);
                MessageBox.Show($"Thank you for chatting, {chatbot.UserName}! Goodbye!", "Goodbye", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
                return;
            }

            AppendBotMessage(botResponse);
            txtUserInput.Clear();
        }

        private async void SetReminder(string input)
        {
            try
            {
                // Split based on "remind me to" and "in"
                string[] parts = input.ToLower().Split(new string[] { "remind me to", "in" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    AppendBotMessage("Please use the format: 'Remind me to [task] in [time]' (e.g., remind me to take a break in 10 seconds)");
                    return;
                }

                string task = parts[0].Trim();
                string timePart = parts[1].Trim();

                int delayMs = ParseTimeToMilliseconds(timePart);
                if (delayMs <= 0)
                {
                    AppendBotMessage("I couldn't understand the time. Try something like '10 seconds', '2 minutes', or '1 hour'.");
                    return;
                }

                await Task.Delay(delayMs);
                AppendBotMessage($"⏰ Reminder: {task}");
                SystemSounds.Beep.Play(); // Optional alert sound
            }
            catch
            {
                AppendBotMessage("Sorry, I couldn't set the reminder. Please check the format.");
            }
        }

        private int ParseTimeToMilliseconds(string timeString)
        {
            if (timeString.Contains("second"))
            {
                int seconds = ExtractNumber(timeString);
                return seconds * 1000;
            }
            else if (timeString.Contains("minute"))
            {
                int minutes = ExtractNumber(timeString);
                return minutes * 60 * 1000;
            }
            else if (timeString.Contains("hour"))
            {
                int hours = ExtractNumber(timeString);
                return hours * 60 * 60 * 1000;
            }

            return 0;
        }

        private int ExtractNumber(string input)
        {
            string numStr = new string(input.Where(char.IsDigit).ToArray());
            return int.TryParse(numStr, out int num) ? num : 0;
        }
    }
}
