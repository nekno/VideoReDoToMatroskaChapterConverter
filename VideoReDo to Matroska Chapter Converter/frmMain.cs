using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VideoReDo_to_Matroska_Chapter_Converter
{
    public partial class frmMain : Form
    {
        ManualResetEventSlim resetEvent = null;

        public frmMain()
        {
            InitializeComponent();
            lbFiles.KeyUp += new KeyEventHandler(lbFiles_KeyUp);
            this.DragEnter += new DragEventHandler(lbFiles_DragEnter);
            this.DragDrop += new DragEventHandler(lbFiles_DragDrop);
            lbFiles.DragEnter += new DragEventHandler(lbFiles_DragEnter);
            lbFiles.DragDrop += new DragEventHandler(lbFiles_DragDrop);
            btnGo.Click += new System.EventHandler(btnGo_Click);
            resetEvent = new ManualResetEventSlim(true);
        }

		#region Click Handlers
		void btnGo_Click(object sender, EventArgs e)
        {
            if (resetEvent.IsSet)
            {
                double framesPerMillisecond = 0;

                try
                {
                    object input = new DataTable().Compute(txtFPS.Text, null);
                    framesPerMillisecond = Convert.ToDouble(input) / 1000.0d;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("FPS must be a number or calculation (e.g., 30000/1001).\r\n\r\n{0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                resetEvent.Reset();
                btnGo.Text = "&Stop";

                pbProgress.Maximum = lbFiles.Items.Count;
                pbProgress.Value = 0;

                Thread t = new Thread(go);
                t.Start(new object[] { lbFiles.Items, framesPerMillisecond });
            }
            else
            {
                changeStopToGo();
            }
        }

		void btnAdd_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Multiselect = true;
			if (DialogResult.OK == ofd.ShowDialog())
			{
				lbFiles.Items.AddRange(ofd.FileNames);
			}
		}

		void btnClear_Click(object sender, EventArgs e)
		{
			lbFiles.Items.Clear();
		}

		void btnExit_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		void btnRemove_Click(object sender, EventArgs e)
		{
			while (lbFiles.SelectedIndices.Count > 0)
			{
				lbFiles.Items.RemoveAt(lbFiles.SelectedIndices[0]);
			}
		}
		#endregion

		#region ListBox Event Handlers
		void lbFiles_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				while (lbFiles.SelectedIndices.Count > 0)
				{
					lbFiles.Items.RemoveAt(lbFiles.SelectedIndices[0]);
				}
			}
		}

		void lbFiles_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Copy;
			else
				e.Effect = DragDropEffects.None;

			this.Activate();
		}

		void lbFiles_DragDrop(object sender, DragEventArgs e)
		{
			object data = e.Data.GetData(DataFormats.FileDrop);
			string[] files = data as string[];

			if (files != null)
			{
				lbFiles.Items.AddRange(files);
			}

			this.Activate();
		}
		#endregion

		#region Custom Methods
		void go(object input)
        {
            object[] inputs = (object[])input;
            ListBox.ObjectCollection files = (ListBox.ObjectCollection)inputs[0];
            double framesPerMillisecond = (double)inputs[1];
            
            char[] delims = { ':', ';', ',', '.' };
            List<object> itemsToRemove = new List<object>();
            List<Exception> exceptions = new List<Exception>();
            int badFiles = 0;

            while ((files.Count > badFiles) && !resetEvent.IsSet)
            {
                try
                {
                    string currentPath = files[badFiles] as string;
                    string newPath = Path.ChangeExtension(currentPath, ".OGM.txt");
                    List<string> outputText = new List<string>();
                    int chapter = 1;

                    if (!File.Exists(currentPath))
                        throw new Exception(String.Format("\"{0}\" does not exist.", currentPath));

                    foreach (string line in File.ReadLines(currentPath))
                    {
                        string[] parts = line.Split(delims, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != 4) throw new Exception(String.Format("Cannot parse the file \"{0}\".\r\nEnsure the timestamps are in one of these formats: HH:MM:SS,ff HH:MM:SS.ff HH:MM:SS;ff HH:MM:SS:ff", currentPath));
                        int lastDelim = line.LastIndexOfAny(delims);
                        string hoursMinsSeconds = line.Substring(0, lastDelim);
                        string frames = line.Substring(lastDelim + 1);
                        double milliseconds = Convert.ToDouble(frames) / framesPerMillisecond;
                        string newLine1 = String.Format("CHAPTER{0:d2}={1}.{2:000}", chapter, hoursMinsSeconds, milliseconds);
                        string newLine2 = String.Format("CHAPTER{0:d2}NAME=Chapter {0}", chapter);
                        outputText.Add(newLine1);
                        outputText.Add(newLine2);
                        chapter++;
                    }

                    File.WriteAllLines(newPath, outputText);

                    removeItem(files[badFiles]);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    badFiles++;
                }
                finally
                {
                    updateProgress();
                }
            }

            showErrors(exceptions);
            changeStopToGo();
        }

        void updateProgress()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(updateProgress));
                return;
            }

            pbProgress.PerformStep();
        }

        void removeItem(object item)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => removeItem(item)));
                return;
            }

            lbFiles.Items.Remove(item);
        }

        void showErrors(List<Exception> exceptions)
        {
            if (exceptions.Count == 0)
                return;

            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => showErrors(exceptions)));
                return;
            }

            String errors = String.Empty;
            foreach (Exception ex in exceptions)
            {
                errors += String.Format("{0}{1}{0}", "\r\n",  ex.Message);
            }

            //MessageBox.Show(errors, "Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
            frmError errorDlg = new frmError();
            errorDlg.ShowDialog(errors);
        }

        void changeStopToGo()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(changeStopToGo));
                return;
            }

            resetEvent.Set();
            btnGo.Text = "&Go";
		}
		#endregion
	}
}
