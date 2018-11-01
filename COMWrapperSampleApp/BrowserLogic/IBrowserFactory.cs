using System;
using System.Windows.Forms;

namespace COMWrapperSampleApp.BrowserLogic
{
    public interface IBrowserFactory
    {
        IBrowserOperations Browser { get; }

        void UpdateMainFormTitle(string title);
    }

    public class BrowserFactory : IBrowserFactory
    {
        private IBrowserOperations browser;
        public IBrowserOperations Browser
        {
            get
            {
                if (browser != null)
                {
                    return browser;
                }
                ComWrapper authForm = null;
                foreach (var openForm in Application.OpenForms)
                {
                    if (openForm is ComWrapper)
                    {
                        authForm = (ComWrapper)openForm;
                        break;
                    }
                }

                return browser = authForm;
            }
        }

        public void UpdateMainFormTitle(string title)
        {
            foreach (Form openForm in Application.OpenForms)
            {
                if (openForm is ComWrapper)
                {
                    openForm.BeginInvoke(new Action(() => { openForm.Text = title; }));
                    break;
                }
            }

            Application.DoEvents();
        }
    }
}