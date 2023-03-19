namespace WinFormsDirect3D11Sample
{
    partial class MainWindow
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            propertyGrid = new PropertyGrid();
            SuspendLayout();
            // 
            // propertyGrid
            // 
            propertyGrid.Dock = DockStyle.Right;
            propertyGrid.Location = new Point(1223, 0);
            propertyGrid.Margin = new Padding(3, 2, 3, 2);
            propertyGrid.Name = "propertyGrid";
            propertyGrid.Size = new Size(211, 661);
            propertyGrid.TabIndex = 0;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1434, 661);
            Controls.Add(propertyGrid);
            Margin = new Padding(3, 2, 3, 2);
            Name = "MainWindow";
            Text = "Direct3D11 Examples";
            ResumeLayout(false);
        }

        #endregion

        private PropertyGrid propertyGrid;
    }
}