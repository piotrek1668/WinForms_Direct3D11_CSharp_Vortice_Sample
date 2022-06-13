using Timer = System.Windows.Forms.Timer;

namespace WinFormsDirect3D11Sample
{
    public partial class MainWindow : Form
    {
        private readonly Label labelDevice;
        private readonly Label labelFeatureLevel;
        private readonly Label labelResolution;
        private readonly Direct3D11 direct3D;

        private const int LabelWidth = 300;
        private const int LabelHeight = 20;
        private const int Offset = 5;

        private const int ButtonWidth = 200;
        private const int ButtonHeight = 30;

        public MainWindow()
        {
            InitializeComponent();

            Button drawTriangleButton = new()
            {
                Text = @"Draw grid",
                Size = new Size(MainWindow.ButtonWidth, MainWindow.ButtonHeight),
                Location = new Point(10, 10),
                Name = "drawGrid"
            };

            Button drawLine = new()
            {
                Text = @"Draw line",
                Size = new Size(MainWindow.ButtonWidth, MainWindow.ButtonHeight),
                Location = new Point(drawTriangleButton.Left + drawTriangleButton.Width + MainWindow.Offset, 10),
                Name = "drawLine"
            };

            Button drawCube = new()
            {
                Text = @"Draw cube",
                Size = new Size(MainWindow.ButtonWidth, MainWindow.ButtonHeight),
                Location = new Point(drawLine.Left + drawLine.Width + MainWindow.Offset, 10),
                Name = "drawCube"
            };

            Button drawText = new()
            {
                Text = @"Draw text",
                Size = new Size(MainWindow.ButtonWidth, MainWindow.ButtonHeight),
                Location = new Point(drawCube.Left + drawCube.Width + MainWindow.Offset, 10),
                Name = "drawText"
            };

            drawTriangleButton.Click += this.Button_Click;
            drawLine.Click += this.Button_Click;
            drawCube.Click += this.Button_Click;
            drawText.Click += this.Button_Click;

            var labelInformation = new Label {
                Text = @"Device information:",
                Size = new Size(MainWindow.LabelWidth, MainWindow.LabelHeight),
                Location = new Point(10, drawTriangleButton.Top + drawTriangleButton.Height + MainWindow.Offset)
            };

            labelDevice = new Label
            {
                Text = @"Device: ",
                Size = new Size(MainWindow.LabelWidth, MainWindow.LabelHeight),
                Location = new Point(10, labelInformation.Top + labelInformation.Height + MainWindow.Offset)
            };

            labelFeatureLevel = new Label
            {
                Text = @"Feature level: ",
                Size = new Size(MainWindow.LabelWidth, MainWindow.LabelHeight),
                Location = new Point(10, labelDevice.Top + labelDevice.Height + MainWindow.Offset)
            };

            labelResolution = new Label
            {
                Text = @"Resolution: ",
                Size = new Size(MainWindow.LabelWidth, MainWindow.LabelHeight),
                Location = new Point(10, labelFeatureLevel.Top + labelFeatureLevel.Height + MainWindow.Offset)
            };

            Control leftControl = new("Direct3D11 Control", 10, this.labelResolution.Top + this.labelResolution.Height + MainWindow.Offset, 600, 500);

            Control rightControl = new("Direct2D Control")
            {
                Location = new Point(leftControl.Width + leftControl.Left + MainWindow.Offset, leftControl.Top),
                Size = new Size(leftControl.Width, leftControl.Height),
                BackColor = Color.Gray
            };

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

            var timer = new Timer();
            timer.Tick += Timer_Tick;
            timer.Interval = 20;
            timer.Start();
        }

        ~MainWindow()
        {
            direct3D.Dispose();
        }

        private void Button_Click(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                direct3D.SetDrawObject(button.Name);
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            direct3D.OnUpdate();
            direct3D.OnRender();
        }

        public void UpdateLabels(string device, string featureLevel, string resolution)
        {
            this.labelDevice.Text += device;
            this.labelFeatureLevel.Text += featureLevel;
            this.labelResolution.Text += resolution;
        }
    }
}