using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace dotNetCrypt {
    partial class MainForm {
        private System.ComponentModel.IContainer components = null;

        private Guna2TextBox txtPath;
        private Guna2Button btnBrowse;
        private Guna2Button btnProtect;

        private Guna2CheckBox cbStringEnc;
        private Guna2CheckBox cbControlFlow;
        private Guna2CheckBox cbImportProtection;
        private Guna2CheckBox cbIntMutation;

        private OpenFileDialog openFileDialog1;

        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.txtPath = new Guna.UI2.WinForms.Guna2TextBox();
            this.btnBrowse = new Guna.UI2.WinForms.Guna2Button();
            this.btnProtect = new Guna.UI2.WinForms.Guna2Button();
            this.cbStringEnc = new Guna.UI2.WinForms.Guna2CheckBox();
            this.cbControlFlow = new Guna.UI2.WinForms.Guna2CheckBox();
            this.cbImportProtection = new Guna.UI2.WinForms.Guna2CheckBox();
            this.cbIntMutation = new Guna.UI2.WinForms.Guna2CheckBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.guna2AnimateWindow1 = new Guna.UI2.WinForms.Guna2AnimateWindow(this.components);
            this.guna2Elipse1 = new Guna.UI2.WinForms.Guna2Elipse(this.components);
            this.guna2DragControl1 = new Guna.UI2.WinForms.Guna2DragControl(this.components);
            this.guna2Elipse2 = new Guna.UI2.WinForms.Guna2Elipse(this.components);
            this.guna2Elipse3 = new Guna.UI2.WinForms.Guna2Elipse(this.components);
            this.guna2Elipse4 = new Guna.UI2.WinForms.Guna2Elipse(this.components);
            this.guna2Elipse5 = new Guna.UI2.WinForms.Guna2Elipse(this.components);
            this.guna2HtmlLabel1 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.SuspendLayout();
            // 
            // txtPath
            // 
            this.txtPath.Animated = true;
            this.txtPath.BackColor = System.Drawing.Color.Transparent;
            this.txtPath.BorderRadius = 8;
            this.txtPath.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtPath.DefaultText = "";
            this.txtPath.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.txtPath.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPath.ForeColor = System.Drawing.Color.White;
            this.txtPath.Location = new System.Drawing.Point(15, 15);
            this.txtPath.Name = "txtPath";
            this.txtPath.PlaceholderForeColor = System.Drawing.Color.White;
            this.txtPath.PlaceholderText = "Select a .NET assembly...";
            this.txtPath.SelectedText = "";
            this.txtPath.Size = new System.Drawing.Size(270, 30);
            this.txtPath.Style = Guna.UI2.WinForms.Enums.TextBoxStyle.Material;
            this.txtPath.TabIndex = 0;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Animated = true;
            this.btnBrowse.AutoRoundedCorners = true;
            this.btnBrowse.BorderRadius = 14;
            this.btnBrowse.FillColor = System.Drawing.Color.Purple;
            this.btnBrowse.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnBrowse.ForeColor = System.Drawing.Color.White;
            this.btnBrowse.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(0)))), ((int)(((byte)(150)))));
            this.btnBrowse.Location = new System.Drawing.Point(295, 15);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 30);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnProtect
            // 
            this.btnProtect.Animated = true;
            this.btnProtect.AutoRoundedCorners = true;
            this.btnProtect.BorderRadius = 16;
            this.btnProtect.FillColor = System.Drawing.Color.Purple;
            this.btnProtect.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnProtect.ForeColor = System.Drawing.Color.White;
            this.btnProtect.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(0)))), ((int)(((byte)(150)))));
            this.btnProtect.Location = new System.Drawing.Point(15, 190);
            this.btnProtect.Name = "btnProtect";
            this.btnProtect.Size = new System.Drawing.Size(355, 35);
            this.btnProtect.TabIndex = 6;
            this.btnProtect.Text = "Protect";
            this.btnProtect.Click += new System.EventHandler(this.btnProtect_Click);
            // 
            // cbStringEnc
            // 
            this.cbStringEnc.Animated = true;
            this.cbStringEnc.AutoEllipsis = true;
            this.cbStringEnc.Checked = true;
            this.cbStringEnc.CheckedState.BorderColor = System.Drawing.Color.Purple;
            this.cbStringEnc.CheckedState.BorderRadius = 0;
            this.cbStringEnc.CheckedState.BorderThickness = 0;
            this.cbStringEnc.CheckedState.FillColor = System.Drawing.Color.Purple;
            this.cbStringEnc.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbStringEnc.ForeColor = System.Drawing.Color.White;
            this.cbStringEnc.Location = new System.Drawing.Point(15, 60);
            this.cbStringEnc.Name = "cbStringEnc";
            this.cbStringEnc.Size = new System.Drawing.Size(180, 25);
            this.cbStringEnc.TabIndex = 2;
            this.cbStringEnc.Text = "String Encryption";
            this.cbStringEnc.UncheckedState.BorderRadius = 0;
            this.cbStringEnc.UncheckedState.BorderThickness = 0;
            // 
            // cbControlFlow
            // 
            this.cbControlFlow.Animated = true;
            this.cbControlFlow.AutoEllipsis = true;
            this.cbControlFlow.Checked = true;
            this.cbControlFlow.CheckedState.BorderColor = System.Drawing.Color.Purple;
            this.cbControlFlow.CheckedState.BorderRadius = 0;
            this.cbControlFlow.CheckedState.BorderThickness = 0;
            this.cbControlFlow.CheckedState.FillColor = System.Drawing.Color.Purple;
            this.cbControlFlow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbControlFlow.ForeColor = System.Drawing.Color.White;
            this.cbControlFlow.Location = new System.Drawing.Point(15, 90);
            this.cbControlFlow.Name = "cbControlFlow";
            this.cbControlFlow.Size = new System.Drawing.Size(180, 25);
            this.cbControlFlow.TabIndex = 3;
            this.cbControlFlow.Text = "Control Flow";
            this.cbControlFlow.UncheckedState.BorderRadius = 0;
            this.cbControlFlow.UncheckedState.BorderThickness = 0;
            // 
            // cbImportProtection
            // 
            this.cbImportProtection.Animated = true;
            this.cbImportProtection.AutoEllipsis = true;
            this.cbImportProtection.Checked = true;
            this.cbImportProtection.CheckedState.BorderColor = System.Drawing.Color.Purple;
            this.cbImportProtection.CheckedState.BorderRadius = 0;
            this.cbImportProtection.CheckedState.BorderThickness = 0;
            this.cbImportProtection.CheckedState.FillColor = System.Drawing.Color.Purple;
            this.cbImportProtection.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbImportProtection.ForeColor = System.Drawing.Color.White;
            this.cbImportProtection.Location = new System.Drawing.Point(15, 120);
            this.cbImportProtection.Name = "cbImportProtection";
            this.cbImportProtection.Size = new System.Drawing.Size(180, 25);
            this.cbImportProtection.TabIndex = 4;
            this.cbImportProtection.Text = "Import Protection";
            this.cbImportProtection.UncheckedState.BorderRadius = 0;
            this.cbImportProtection.UncheckedState.BorderThickness = 0;
            // 
            // cbIntMutation
            // 
            this.cbIntMutation.Animated = true;
            this.cbIntMutation.AutoEllipsis = true;
            this.cbIntMutation.Checked = true;
            this.cbIntMutation.CheckedState.BorderColor = System.Drawing.Color.Purple;
            this.cbIntMutation.CheckedState.BorderRadius = 0;
            this.cbIntMutation.CheckedState.BorderThickness = 0;
            this.cbIntMutation.CheckedState.FillColor = System.Drawing.Color.Purple;
            this.cbIntMutation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIntMutation.ForeColor = System.Drawing.Color.White;
            this.cbIntMutation.Location = new System.Drawing.Point(15, 150);
            this.cbIntMutation.Name = "cbIntMutation";
            this.cbIntMutation.Size = new System.Drawing.Size(180, 25);
            this.cbIntMutation.TabIndex = 5;
            this.cbIntMutation.Text = "Int Mutation";
            this.cbIntMutation.UncheckedState.BorderRadius = 0;
            this.cbIntMutation.UncheckedState.BorderThickness = 0;
            // 
            // guna2AnimateWindow1
            // 
            this.guna2AnimateWindow1.TargetForm = this;
            // 
            // guna2Elipse1
            // 
            this.guna2Elipse1.BorderRadius = 25;
            this.guna2Elipse1.TargetControl = this;
            // 
            // guna2DragControl1
            // 
            this.guna2DragControl1.DockIndicatorTransparencyValue = 0.6D;
            this.guna2DragControl1.TargetControl = this;
            this.guna2DragControl1.UseTransparentDrag = true;
            // 
            // guna2Elipse2
            // 
            this.guna2Elipse2.BorderRadius = 10;
            this.guna2Elipse2.TargetControl = this.cbControlFlow;
            // 
            // guna2Elipse3
            // 
            this.guna2Elipse3.BorderRadius = 10;
            this.guna2Elipse3.TargetControl = this.cbImportProtection;
            // 
            // guna2Elipse4
            // 
            this.guna2Elipse4.BorderRadius = 10;
            this.guna2Elipse4.TargetControl = this.cbIntMutation;
            // 
            // guna2Elipse5
            // 
            this.guna2Elipse5.BorderRadius = 10;
            this.guna2Elipse5.TargetControl = this.cbStringEnc;
            // 
            // guna2HtmlLabel1
            // 
            this.guna2HtmlLabel1.BackColor = System.Drawing.Color.Transparent;
            this.guna2HtmlLabel1.Font = new System.Drawing.Font("HP Simplified", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.guna2HtmlLabel1.ForeColor = System.Drawing.Color.Purple;
            this.guna2HtmlLabel1.Location = new System.Drawing.Point(201, 51);
            this.guna2HtmlLabel1.Name = "guna2HtmlLabel1";
            this.guna2HtmlLabel1.Size = new System.Drawing.Size(150, 30);
            this.guna2HtmlLabel1.TabIndex = 7;
            this.guna2HtmlLabel1.Text = "dotNet Crypter";
            // 
            // MainForm
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(390, 240);
            this.Controls.Add(this.guna2HtmlLabel1);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.cbStringEnc);
            this.Controls.Add(this.cbControlFlow);
            this.Controls.Add(this.cbImportProtection);
            this.Controls.Add(this.cbIntMutation);
            this.Controls.Add(this.btnProtect);
            this.Font = new System.Drawing.Font("HP Simplified", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = ".NET Crypter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private Guna2AnimateWindow guna2AnimateWindow1;
        private Guna2Elipse guna2Elipse1;
        private Guna2DragControl guna2DragControl1;
        private Guna2Elipse guna2Elipse2;
        private Guna2Elipse guna2Elipse3;
        private Guna2Elipse guna2Elipse4;
        private Guna2Elipse guna2Elipse5;
        private Guna2HtmlLabel guna2HtmlLabel1;
    }
}
