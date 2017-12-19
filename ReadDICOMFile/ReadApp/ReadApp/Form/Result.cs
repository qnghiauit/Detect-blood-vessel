﻿using ReadApp.Helper;
using ReadApp.Manager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReadApp
{
    public partial class Result : Form
    {
        public delegate void CallbackFunc();
        private DateTime beginTime;
        private DateTime endTime;
        private string sourceFileName;
        private string resultFileName;
        private int currentFrame;
        private bool firstLoad;
        public Result()
        {
            InitializeComponent();
            firstLoad = true;
            this.currentFrame = MainForm.currentFrame;
            int[] arr = Enumerable.Range(1, DICOMManager.shared.FrameCount).ToArray();
            comboBoxFrameNumber.DataSource = arr;
            comboBoxFrameNumber.SelectedIndex = currentFrame - 1;
            LoadData();
        }

        private void LoadData()
        {
            sourceFileName = DICOMManager.shared.FileName + "_" + currentFrame.ToString() + ".tif";
            resultFileName = "result_" + sourceFileName;
            labelFrameNumber.Text = currentFrame.ToString();
            imagePanelSource.Image = MainForm.LoadImageFromPath(Application.StartupPath + "\\matlab\\data\\" + sourceFileName);
            Detect();
        }

        private void Result_Load(object sender, EventArgs e)
        {
            
        }

        private void Detect(bool forceRun = false)
        {
            beginTime = DateTime.Now;
            if (forceRun)
            {
                System.Threading.Thread t = new System.Threading.Thread(() => {
                    MatlabHelper.shared.DetectVessel();
                    endTime = DateTime.Now;
                    this.Invoke(new CallbackFunc(SetResultImage));
                });
                t.Start();
            }
            else
            {
                if (!File.Exists(Application.StartupPath + "\\matlab\\data\\" + resultFileName))
                {
                    System.Threading.Thread t = new System.Threading.Thread(() => {
                        MatlabHelper.shared.DetectVessel();
                        endTime = DateTime.Now;
                        this.Invoke(new CallbackFunc(SetResultImage));
                    });
                    t.Start();
                }
                else
                {
                    imagePanelResult.Image = MainForm.LoadImageFromPath(Application.StartupPath + "\\matlab\\data\\" + resultFileName);
                    labelTime.Text = "Extracted output";
                }
            }
        }

        private void SetResultImage()
        {
            var sourcePath = Application.StartupPath + "\\matlab\\data\\result.tif";
            var desPath = Application.StartupPath + "\\matlab\\data\\" + resultFileName;
            File.Copy(sourcePath, desPath,true);
            imagePanelResult.Image = MainForm.LoadImageFromPath(desPath);
            TimeSpan ts = (endTime - beginTime);
            labelTime.Text = (ts.Seconds + ts.Milliseconds*0.001).ToString() + " s";
        }

        private void buttonAccurary_Click(object sender, EventArgs e)
        {
            var accuracy = new Accuracy(resultFileName);
            accuracy.ShowDialog();
        }

        private void comboBoxFrameNumber_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (firstLoad)
            {
                firstLoad = false;
                return;
            }
            currentFrame = Convert.ToInt32(comboBoxFrameNumber.SelectedValue);
            System.Threading.Thread t = new System.Threading.Thread(() => {
                var fileName = DICOMManager.shared.FileName + "_" + currentFrame.ToString() + ".tif";
                var filePath = Application.StartupPath + "\\matlab\\data\\" + fileName;
                if (!File.Exists(filePath))
                {
                    DICOMManager.shared.ExportFrame(currentFrame - 1, ImageFormat.Tiff, filePath);
                }
                File.Copy(filePath, Application.StartupPath + "\\matlab\\data\\source.tif", true);
                this.Invoke(new CallbackFunc(LoadData));
            });
            t.Start();
        }

        private void buttonForceDetect_Click(object sender, EventArgs e)
        {
            Detect(true);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "png(*.png)| *.png; |Bitmap(*.bmp)| *.bmp; | JPG(*.jpg)| *.jpg; | TIFF(*.tif)| *.tif";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var desPath = saveFileDialog1.FileName;
                var sourcePath = Application.StartupPath + "\\matlab\\data\\" + resultFileName;
                File.Copy(sourcePath, desPath);
            }
        }
    }
}
