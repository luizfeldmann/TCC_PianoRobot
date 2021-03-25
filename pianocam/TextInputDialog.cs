using System;
namespace pianocam
{
    public partial class TextInputDialog : Gtk.Dialog
    {
        public TextInputDialog()
        {
            this.Build();
        }

        public static string ShowDialog(string title, string question, string suggestion)
        {
            TextInputDialog diag = new TextInputDialog();
            diag.labelContent.Text = question;
            diag.titleLabel.Text = title;
            diag.entry1.Text = suggestion;

            while (diag.Run() != (int)Gtk.ResponseType.Ok || diag.entry1.Text.Length <= 0)
                continue;

            string response = diag.entry1.Text;
            diag.Destroy();
            return response;
        }
    }
}
