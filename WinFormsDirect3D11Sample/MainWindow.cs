namespace WinFormsDirect3D11Sample
{
    public partial class MainWindow : Form
    {
        private readonly Control leftControl;
        private readonly Control rightControl;
        private readonly Label labelInformation;
        private readonly Label labelDevice;
        private readonly Label labelFeatureLevel;
        private readonly Label labelResolution;
        private readonly Direct3D11 direct3D;
        private readonly System.Windows.Forms.Timer timer;

        private readonly int labelWidth = 300;
        private readonly int labelHeight = 20;
        private readonly int offset = 5;

        private readonly int buttonWidth = 200;
        private readonly int buttonHeight = 30;

        public MainWindow()
        {
            InitializeComponent();

            Button drawTriangleButton = new()
            {
                Text = "Draw grid",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(10, 10),
                Name = "drawGrid"
            };

            Button drawLine = new()
            {
                Text = "Draw line",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(drawTriangleButton.Left + drawTriangleButton.Width + offset, 10),
                Name = "drawLine"
            };

            Button drawCube = new()
            {
                Text = "Draw cube",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(drawLine.Left + drawLine.Width + offset, 10),
                Name = "drawCube"
            };

            Button drawText = new()
            {
                Text = "Draw text",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(drawCube.Left + drawCube.Width + offset, 10),
                Name = "drawText"
            };

            drawTriangleButton.Click += this.Button_Click;
            drawLine.Click += this.Button_Click;
            drawCube.Click += this.Button_Click;
            drawText.Click += this.Button_Click;

            labelInformation = new Label
            {
                Text = "Device information:",
                Size = new Size(labelWidth, labelHeight),
                Location = new Point(10, drawTriangleButton.Top + drawTriangleButton.Height + offset)
            };

            labelDevice = new Label
            {
                Text = "Device: ",
                Size = new Size(labelWidth, labelHeight),
                Location = new Point(10, labelInformation.Top + labelInformation.Height + offset)
            };

            labelFeatureLevel = new Label
            {
                Text = "Feature level: ",
                Size = new Size(labelWidth, labelHeight),
                Location = new Point(10, labelDevice.Top + labelDevice.Height + offset)
            };

            labelResolution = new Label
            {
                Text = "Resolution: ",
                Size = new Size(labelWidth, labelHeight),
                Location = new Point(10, labelFeatureLevel.Top + labelFeatureLevel.Height + offset)
            };

            leftControl = new("Direct3D11 Control", 10, labelResolution.Top + labelResolution.Height + offset, 600, 500);

            rightControl = new("Direct2D Control");
            rightControl.Location = new Point(leftControl.Width + leftControl.Left + offset, leftControl.Top);
            rightControl.Size = new Size(leftControl.Width, leftControl.Height);
            rightControl.BackColor = Color.Gray;

            leftControl.BackColor = Color.Gray;

            this.Controls.Add(drawTriangleButton);
            this.Controls.Add(drawLine);
            this.Controls.Add(drawCube);
            this.Controls.Add(drawText);
            this.Controls.Add(labelInformation);
            this.Controls.Add(labelDevice);
            this.Controls.Add(labelFeatureLevel);
            this.Controls.Add(labelResolution);
            this.Controls.Add(leftControl);
            this.Controls.Add(rightControl);

            direct3D = new Direct3D11(this, leftControl, rightControl);
            direct3D.OnInit();

            timer = new System.Windows.Forms.Timer();
            timer.Tick += Timer_Tick;
            timer.Interval = 20;
            timer.Start();
        }

        ~MainWindow()
        {
            direct3D?.Dispose();
        }

        private void Button_Click(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                direct3D?.SetDrawObject(button.Name);
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            direct3D?.OnUpdate();
            direct3D?.OnRender();
        }

        public void UpdateLabels(string device, string featureLevel, string resolution)
        {
            this.labelDevice.Text += device;
            this.labelFeatureLevel.Text += featureLevel;
            this.labelResolution.Text += resolution;
        }
    }
}