using CefSharp.WinForms;
using COMWrapperSampleApp.BrowserLogic;
using System.Windows.Forms;

namespace COMWrapperSampleApp
{
    public partial class ComWrapper : Form, IBrowserOperations
    {
        EmbeddedBrowser IBrowserOperations.WebBrowser => webBrowser == null ? webBrowser = new EmbeddedBrowser(chromiumWebBrowser, "http://127.0.0.1:55555") : webBrowser;

        private EmbeddedBrowser webBrowser;
        private ChromiumWebBrowser chromiumWebBrowser;
        private string accessToken;
        private string idToken;

        public ComWrapper()
        {
            InitializeComponent();
            DoubleBuffered = true;

            chromiumWebBrowser = new ChromiumWebBrowser("")
            {
                Dock = DockStyle.Fill
            };

            pnlControls.Controls.Add(chromiumWebBrowser);

            // Fix minimum size problem 
            int heightDelta = Height - ClientRectangle.Height;
            int widthDelta = Width - ClientRectangle.Width;
            MinimumSize = new System.Drawing.Size(1024 + widthDelta, 680 + heightDelta);

            Visible = true;
        }

        public void Navigate(string url)
        {
            chromiumWebBrowser.Load(url);
        }

        public void ShowMessageDialog(string message)
        {
            MessageBox.Show(message);
        }
    }
}
