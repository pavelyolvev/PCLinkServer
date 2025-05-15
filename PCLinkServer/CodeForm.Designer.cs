using System.ComponentModel;

namespace PCLinkServer;

partial class CodeForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CodeForm));
        label1 = new System.Windows.Forms.Label();
        button1 = new System.Windows.Forms.Button();
        codeLbl = new System.Windows.Forms.Label();
        SuspendLayout();
        // 
        // label1
        // 
        label1.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)204));
        label1.Location = new System.Drawing.Point(39, 8);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(200, 82);
        label1.TabIndex = 0;
        label1.Text = "Введите код на телефоне:";
        label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // button1
        // 
        button1.Location = new System.Drawing.Point(103, 150);
        button1.Name = "button1";
        button1.Size = new System.Drawing.Size(75, 23);
        button1.TabIndex = 1;
        button1.Text = "Ок";
        button1.UseVisualStyleBackColor = true;
        // 
        // codeLbl
        // 
        codeLbl.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)204));
        codeLbl.Location = new System.Drawing.Point(65, 90);
        codeLbl.Name = "codeLbl";
        codeLbl.Size = new System.Drawing.Size(153, 43);
        codeLbl.TabIndex = 2;
        codeLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // CodeForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(284, 184);
        Controls.Add(codeLbl);
        Controls.Add(button1);
        Controls.Add(label1);
        Icon = ((System.Drawing.Icon)resources.GetObject("$this.Icon"));
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "Уведомление";
        ResumeLayout(false);
    }

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Label codeLbl;

    private System.Windows.Forms.Label label1;

    #endregion
}