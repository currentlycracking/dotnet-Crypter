using dnlib.DotNet;
using dnlib.DotNet.Writer;
using dotNetCrypt.Protections;
using dotNetCrypt.Protections.ControlFlow;
using dotNetCrypt.Protections.IntMutator;
using dotNetCrypt.Protections.StringEncrypter;
using dotnetCrypter.Engine.Protections.Proxy;
using Protector.Protections;
using System;
using System.IO;
using System.Windows.Forms;

namespace dotNetCrypt {
    public partial class MainForm : Form {

        public MainForm() {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e) {
            openFileDialog1.Filter = ".NET Assemblies (*.exe;*.dll)|*.exe;*.dll";
            openFileDialog1.Title = "Select .NET Module";
            if(openFileDialog1.ShowDialog() == DialogResult.OK) {
                txtPath.Text = openFileDialog1.FileName;
            }
        }


        private void btnProtect_Click(object sender, EventArgs e) {
            string inputPath = txtPath.Text;

            if(!File.Exists(inputPath)) {
                MessageBox.Show("File not found.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try {
                ModuleDefMD module = ModuleDefMD.Load(inputPath); 
                ExternalImports.Execute(module);
                if(cbStringEnc.Checked)
                    new StringEncryption().Execute(module);

                if(cbControlFlow.Checked)
                    ControlFlow.Execute(module);

                if(cbImportProtection.Checked)
                    for(int i = 0; i < 10; i++) {
                        ExternalImports.Execute(module);
                        ImportProtection.Execute(module);
                        ExternalImports.Execute(module);
                    }

                if(cbIntMutation.Checked)
                    IntMutation.Execute(module);

                var options = new ModuleWriterOptions(module) {
                    Logger = DummyLogger.NoThrowInstance
                };

                string dir = Path.GetDirectoryName(inputPath);
                string name = Path.GetFileNameWithoutExtension(inputPath);
                string ext = Path.GetExtension(inputPath);
                string output = Path.Combine(dir, name + "_crypted" + ext);

                module.Write(output, options);

                MessageBox.Show("Exported:\n" + output,
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch(Exception ex) {
                MessageBox.Show(ex.ToString(), "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
