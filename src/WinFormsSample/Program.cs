using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinFormsSample
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        private static void Main(string[] args)
        {
            var fullHd = Array.IndexOf(args, "--full-hd") >= 0;
            if (fullHd)
            {
                SetProcessDPIAware();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GreetingForm(fullHd));
        }
    }

    internal sealed class GreetingForm : Form
    {
        public GreetingForm(bool fullHd)
        {
            Text = "WinForms E2E Sample";
            ClientSize = new Size(320, 144);
            StartPosition = FormStartPosition.CenterScreen;
            var origin = Point.Empty;

            if (fullHd)
            {
                Text += " - Full HD";
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                Location = Screen.PrimaryScreen.Bounds.Location;
                ClientSize = new Size(1920, 1080);
                origin = new Point(1536, 896);
                Controls.Add(new Label
                {
                    Text = Text + " (1920 x 1080)",
                    AutoSize = true,
                    Location = new Point(32, 32)
                });
            }

            Point At(int x, int y) => new Point(origin.X + x, origin.Y + y);

            var nameInput = new TextBox
            {
                AccessibleName = "Name input",
                Location = At(16, 34),
                Width = 184
            };
            var greetButton = new Button
            {
                AccessibleName = "Greet",
                Text = "Greet",
                Location = At(216, 32),
                Width = 88
            };
            var result = new TextBox
            {
                AccessibleName = "Greeting result",
                Location = At(16, 92),
                Width = 288,
                ReadOnly = true,
                TabStop = false
            };

            greetButton.Click += (_, __) => result.Text = $"Hello, {nameInput.Text}!";
            Controls.AddRange(new Control[]
            {
                new Label { Text = "Name", AutoSize = true, Location = At(16, 14) },
                nameInput,
                greetButton,
                new Label { Text = "Result", AutoSize = true, Location = At(16, 72) },
                result
            });
            ActiveControl = greetButton;
        }
    }
}
