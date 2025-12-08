using System.Windows;

namespace CommerceBoost.Views
{
    public partial class InputWindow : Window
    {
        public string ResultText { get; private set; }

        public InputWindow(string title, string prompt)
        {
            InitializeComponent();
            Title = title;
            PromptText.Text = prompt;
            InputBox.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            ResultText = InputBox.Text;
            DialogResult = true;
            Close();
        }
    }
}
