namespace PCLinkServer;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        button1 = new System.Windows.Forms.Button();
        LogList = new System.Windows.Forms.ListBox();
        button2 = new System.Windows.Forms.Button();
        SuspendLayout();
        // 
        // button1
        // 
        button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        button1.Location = new System.Drawing.Point(6, 31);
        button1.Name = "button1";
        button1.Size = new System.Drawing.Size(164, 30);
        button1.TabIndex = 0;
        button1.Text = "Connect";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // LogList
        // 
        LogList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        LogList.FormattingEnabled = true;
        LogList.Location = new System.Drawing.Point(12, 92);
        LogList.Name = "LogList";
        LogList.Size = new System.Drawing.Size(618, 304);
        LogList.TabIndex = 1;
        // 
        // button2
        // 
        button2.Location = new System.Drawing.Point(189, 31);
        button2.Name = "button2";
        button2.Size = new System.Drawing.Size(164, 30);
        button2.TabIndex = 2;
        button2.Text = "GetCode";
        button2.UseVisualStyleBackColor = true;
        button2.Click += OnGetCodeClick;
        // 
        // Form1
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(800, 450);
        Controls.Add(button2);
        Controls.Add(LogList);
        Controls.Add(button1);
        Text = "Form1";
        ResumeLayout(false);
    }

    private System.Windows.Forms.Button button2;

    private System.Windows.Forms.ListBox LogList;

    private System.Windows.Forms.Button button1;

    #endregion
}