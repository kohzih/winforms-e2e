using System;
using System.Diagnostics;
using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: DoNotParallelize]

namespace WinFormsSample.E2E
{
    [TestClass]
    public class GreetingTests
    {
        private const string Name = "FlaUI";
        private const string Greeting = "Hello, FlaUI!";
        private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan UiTimeout = TimeSpan.FromSeconds(5);

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void UiaPatterns_UpdateResult()
        {
            VerifyGreeting((_, input, button) =>
            {
                TestContext.WriteLine("Setting the input with UI Automation ValuePattern.");
                input.Patterns.Value.Pattern.SetValue(Name);
                WaitForText(input, Name, "ValuePattern.SetValue");

                TestContext.WriteLine("Activating Greet with UI Automation InvokePattern.");
                button.Invoke();
            });
        }

        [TestMethod]
        public void MouseAndKeyboard_UpdateResult()
        {
            VerifyGreeting((window, input, button) =>
            {
                TestContext.WriteLine("Bringing the window forward and clicking the input.");
                window.Focus();
                input.Click();
                var focus = Retry.WhileFalse(() => input.Properties.HasKeyboardFocus.Value, UiTimeout);
                Assert.IsTrue(focus.Result, "The input did not receive keyboard focus after the click.");

                TestContext.WriteLine("Typing into the input with FlaUI Keyboard.Type.");
                Keyboard.Type(Name);
                WaitForText(input, Name, "Keyboard.Type");

                TestContext.WriteLine("Clicking Greet with FlaUI mouse input.");
                button.Click();
            });
        }

        private void VerifyGreeting(Action<Window, TextBox, Button> interact)
        {
            using (var testProcess = Process.GetCurrentProcess())
            {
                TestContext.WriteLine($"Test session ID: {testProcess.SessionId}, interactive: {Environment.UserInteractive}");
            }

            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WinFormsSample.exe");
            Assert.IsTrue(File.Exists(samplePath), $"Sample executable is missing: {samplePath}");
            TestContext.WriteLine("Launching WinFormsSample.exe.");
            using (var app = Application.Launch(samplePath))
            using (var automation = new UIA3Automation())
            {
                TestContext.WriteLine($"Waiting for the main window of PID {app.ProcessId}.");
                var window = app.GetMainWindow(automation, StartupTimeout);
                Assert.IsNotNull(window, "The sample app's main window did not appear.");

                var input = FindControl(window, "Name input").AsTextBox();
                var button = FindControl(window, "Greet").AsButton();
                var result = FindControl(window, "Greeting result").AsTextBox();
                Assert.AreEqual(string.Empty, input.Text, "The name input must start empty.");
                Assert.AreEqual(string.Empty, result.Text, "The greeting must start empty.");

                interact(window, input, button);
                WaitForText(result, Greeting, "Greeting display after activating the button");
            }
        }

        private static AutomationElement FindControl(Window window, string name)
        {
            var control = Retry.WhileNull(() => window.FindFirstDescendant(cf => cf.ByName(name)), UiTimeout).Result;
            Assert.IsNotNull(control, $"UI Automation could not find the '{name}' control.");
            return control;
        }

        private static void WaitForText(TextBox textBox, string expected, string operation)
        {
            var actual = Retry.While(() => textBox.Text, value => value != expected, UiTimeout).Result;
            Assert.AreEqual(expected, actual, $"{operation} did not produce the expected text.");
        }
    }
}
