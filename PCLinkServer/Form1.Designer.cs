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
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
        button1 = new System.Windows.Forms.Button();
        button2 = new System.Windows.Forms.Button();
        label1 = new System.Windows.Forms.Label();
        notifyIcon1 = new System.Windows.Forms.NotifyIcon(components);
        contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
        statusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        onOffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        настройкиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        checkBox1 = new System.Windows.Forms.CheckBox();
        checkBox2 = new System.Windows.Forms.CheckBox();
        checkBox3 = new System.Windows.Forms.CheckBox();
        checkBox4 = new System.Windows.Forms.CheckBox();
        checkBox5 = new System.Windows.Forms.CheckBox();
        label2 = new System.Windows.Forms.Label();
        statusLbl = new System.Windows.Forms.Label();
        checkBox6 = new System.Windows.Forms.CheckBox();
        checkBox7 = new System.Windows.Forms.CheckBox();
        contextMenuStrip1.SuspendLayout();
        SuspendLayout();
        // 
        // button1
        // 
        button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
        button1.Location = new System.Drawing.Point(5, 234);
        button1.Name = "button1";
        button1.Size = new System.Drawing.Size(164, 30);
        button1.TabIndex = 0;
        button1.Text = "Connect";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // button2
        // 
        button2.Location = new System.Drawing.Point(188, 234);
        button2.Name = "button2";
        button2.Size = new System.Drawing.Size(164, 30);
        button2.TabIndex = 2;
        button2.Text = "FTP start";
        button2.UseVisualStyleBackColor = true;
        button2.Click += button2_Click;
        // 
        // label1
        // 
        label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        label1.Location = new System.Drawing.Point(6, 87);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(154, 23);
        label1.TabIndex = 6;
        label1.Text = "Настройки питания";
        // 
        // notifyIcon1
        // 
        notifyIcon1.ContextMenuStrip = contextMenuStrip1;
        notifyIcon1.Icon = ((System.Drawing.Icon)resources.GetObject("notifyIcon1.Icon"));
        notifyIcon1.Text = "PCLink";
        notifyIcon1.Visible = true;
        notifyIcon1.DoubleClick += OnTrayIconDoubleClick;
        // 
        // contextMenuStrip1
        // 
        contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statusToolStripMenuItem, onOffToolStripMenuItem, настройкиToolStripMenuItem, выходToolStripMenuItem });
        contextMenuStrip1.Name = "contextMenuStrip1";
        contextMenuStrip1.Size = new System.Drawing.Size(135, 92);
        // 
        // statusToolStripMenuItem
        // 
        statusToolStripMenuItem.Name = "statusToolStripMenuItem";
        statusToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
        statusToolStripMenuItem.Text = "Статус";
        // 
        // onOffToolStripMenuItem
        // 
        onOffToolStripMenuItem.Name = "onOffToolStripMenuItem";
        onOffToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
        onOffToolStripMenuItem.Text = "Включить";
        onOffToolStripMenuItem.Click += onOffToolStripMenuItem_Click;
        // 
        // настройкиToolStripMenuItem
        // 
        настройкиToolStripMenuItem.Name = "настройкиToolStripMenuItem";
        настройкиToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
        настройкиToolStripMenuItem.Text = "Настройки";
        настройкиToolStripMenuItem.Click += OnOpen;
        // 
        // выходToolStripMenuItem
        // 
        выходToolStripMenuItem.Name = "выходToolStripMenuItem";
        выходToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
        выходToolStripMenuItem.Text = "Выход";
        // 
        // checkBox1
        // 
        checkBox1.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        checkBox1.Location = new System.Drawing.Point(6, 113);
        checkBox1.Name = "checkBox1";
        checkBox1.Size = new System.Drawing.Size(217, 24);
        checkBox1.TabIndex = 8;
        checkBox1.Text = "Разрешить выключать ПК";
        checkBox1.UseVisualStyleBackColor = true;
        checkBox1.CheckedChanged += checkBox1_CheckedChanged;
        // 
        // checkBox2
        // 
        checkBox2.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        checkBox2.Location = new System.Drawing.Point(6, 143);
        checkBox2.Name = "checkBox2";
        checkBox2.Size = new System.Drawing.Size(243, 24);
        checkBox2.TabIndex = 9;
        checkBox2.Text = "Разрешить перезагружать ПК";
        checkBox2.UseVisualStyleBackColor = true;
        checkBox2.CheckedChanged += checkBox2_CheckedChanged;
        // 
        // checkBox3
        // 
        checkBox3.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        checkBox3.Location = new System.Drawing.Point(6, 173);
        checkBox3.Name = "checkBox3";
        checkBox3.Size = new System.Drawing.Size(233, 46);
        checkBox3.TabIndex = 10;
        checkBox3.Text = "Разрешить переводить ПК в режим сна";
        checkBox3.UseVisualStyleBackColor = true;
        checkBox3.CheckedChanged += checkBox3_CheckedChanged;
        // 
        // checkBox4
        // 
        checkBox4.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        checkBox4.Location = new System.Drawing.Point(271, 113);
        checkBox4.Name = "checkBox4";
        checkBox4.Size = new System.Drawing.Size(245, 24);
        checkBox4.TabIndex = 11;
        checkBox4.Text = "Запускать при старте Windows";
        checkBox4.UseVisualStyleBackColor = true;
        checkBox4.CheckedChanged += checkBox4_CheckedChanged;
        // 
        // checkBox5
        // 
        checkBox5.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        checkBox5.Location = new System.Drawing.Point(271, 143);
        checkBox5.Name = "checkBox5";
        checkBox5.RightToLeft = System.Windows.Forms.RightToLeft.No;
        checkBox5.Size = new System.Drawing.Size(245, 24);
        checkBox5.TabIndex = 12;
        checkBox5.Text = "Запускать свернутым";
        checkBox5.UseVisualStyleBackColor = true;
        checkBox5.CheckedChanged += checkBox5_CheckedChanged;
        // 
        // label2
        // 
        label2.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        label2.Location = new System.Drawing.Point(6, 7);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(83, 38);
        label2.TabIndex = 13;
        label2.Text = "Статус: ";
        // 
        // statusLbl
        // 
        statusLbl.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        statusLbl.ForeColor = System.Drawing.Color.Red;
        statusLbl.Location = new System.Drawing.Point(95, 7);
        statusLbl.Name = "statusLbl";
        statusLbl.Size = new System.Drawing.Size(282, 38);
        statusLbl.TabIndex = 14;
        statusLbl.Text = "Выключен";
        // 
        // checkBox6
        // 
        checkBox6.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        checkBox6.Location = new System.Drawing.Point(6, 46);
        checkBox6.Name = "checkBox6";
        checkBox6.Size = new System.Drawing.Size(243, 24);
        checkBox6.TabIndex = 15;
        checkBox6.Text = "Разрешить видеотрансляцию";
        checkBox6.UseVisualStyleBackColor = true;
        checkBox6.CheckedChanged += checkBox6_CheckedChanged;
        // 
        // checkBox7
        // 
        checkBox7.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)204));
        checkBox7.Location = new System.Drawing.Point(271, 173);
        checkBox7.Name = "checkBox7";
        checkBox7.RightToLeft = System.Windows.Forms.RightToLeft.No;
        checkBox7.Size = new System.Drawing.Size(245, 24);
        checkBox7.TabIndex = 16;
        checkBox7.Text = "Включать сервер при запуске";
        checkBox7.UseVisualStyleBackColor = true;
        checkBox7.CheckedChanged += checkBox7_CheckedChanged;
        // 
        // Form1
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        ClientSize = new System.Drawing.Size(800, 450);
        Controls.Add(checkBox7);
        Controls.Add(checkBox6);
        Controls.Add(statusLbl);
        Controls.Add(label2);
        Controls.Add(checkBox5);
        Controls.Add(checkBox4);
        Controls.Add(checkBox3);
        Controls.Add(checkBox2);
        Controls.Add(checkBox1);
        Controls.Add(label1);
        Controls.Add(button2);
        Controls.Add(button1);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        Icon = ((System.Drawing.Icon)resources.GetObject("$this.Icon"));
        Text = "PCLink";
        contextMenuStrip1.ResumeLayout(false);
        ResumeLayout(false);
    }

    private System.Windows.Forms.CheckBox checkBox7;

    private System.Windows.Forms.ToolStripMenuItem onOffToolStripMenuItem;

    private System.Windows.Forms.CheckBox checkBox6;

    private System.Windows.Forms.Label statusLbl;

    private System.Windows.Forms.Label label2;

    private System.Windows.Forms.CheckBox checkBox5;

    private System.Windows.Forms.ToolStripMenuItem настройкиToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem statusToolStripMenuItem;

    private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;

    private System.Windows.Forms.CheckBox checkBox2;
    private System.Windows.Forms.CheckBox checkBox3;
    private System.Windows.Forms.CheckBox checkBox4;

    private System.Windows.Forms.CheckBox checkBox1;

    private System.Windows.Forms.NotifyIcon notifyIcon1;

    private System.Windows.Forms.Label label1;

    private System.Windows.Forms.Button button2;

    private System.Windows.Forms.Button button1;

    #endregion
}