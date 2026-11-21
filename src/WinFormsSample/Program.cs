using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsSample
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GreetingForm());
        }
    }

    internal sealed class GreetingForm : Form
    {
        public GreetingForm()
        {
            Text = "WinForms E2E Sample";
            ClientSize = new Size(320, 144);
            StartPosition = FormStartPosition.CenterScreen;

            var nameInput = new TextBox
            {
                AccessibleName = "Name input",
                Location = new Point(16, 34),
                Width = 184
            };
            var greetButton = new Button
            {
                AccessibleName = "Greet",
                Text = "Greet",
                Location = new Point(216, 32),
                Width = 88
            };
            var result = new TextBox
            {
                AccessibleName = "Greeting result",
                Location = new Point(16, 92),
                Width = 288,
                ReadOnly = true,
                TabStop = false
            };

            greetButton.Click += (_, __) => result.Text = $"Hello, {nameInput.Text}!";
            Controls.AddRange(new Control[]
            {
                new Label { Text = "Name", AutoSize = true, Location = new Point(16, 14) },
                nameInput,
                greetButton,
                new Label { Text = "Result", AutoSize = true, Location = new Point(16, 72) },
                result
            });
            ActiveControl = greetButton;
        }
    }
}
