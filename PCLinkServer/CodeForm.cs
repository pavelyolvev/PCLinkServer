namespace PCLinkServer;

public partial class CodeForm : Form
{
    public CodeForm(string message)
    {
        InitializeComponent();
        codeLbl.Text = message;
    }
}