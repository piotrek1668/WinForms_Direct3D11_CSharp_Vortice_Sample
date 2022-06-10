namespace WinFormsDirect3D11Sample
{
    public partial class MainWindow : Form
    {
        private readonly Control leftControl = new("Direct3D11 Control", 10, 50, 600, 500);
        private readonly Control rightControl = new("Direct2D Control");
        private readonly Label labelDevice;
        private readonly Label labelFeatureLevel;
        private readonly Direct3D11 direct3D;
        private readonly System.Windows.Forms.Timer timer;
        private bool rendering = true;

        public MainWindow()
        {
            InitializeComponent();

            Button button = new()
            {
                Text = "Toggle rendering",
                Size = new Size(300, 30),
                Location = new Point(10, 10)
            };

            button.Click += this.Button_Click;
            
            labelDevice = new Label
            {
                Text = "Device:",
                Size = new Size(300, 30),
                Location = new Point(button.Left + button.Size.Width + 10, 15)
            };

            labelFeatureLevel = new Label
            {
                Text = "Feature level:",
                Size = new Size(250, 30),
                Location = new Point(labelDevice.Left + labelDevice.Width + 10, 15)
            };

            rightControl.Location = new Point(leftControl.Width + leftControl.Left + 10, leftControl.Top);
            rightControl.Size = new Size(leftControl.Width, leftControl.Height);
            rightControl.BackColor = Color.Gray;

            leftControl.BackColor = Color.Gray;

            this.Controls.Add(button);
            this.Controls.Add(labelDevice);
            this.Controls.Add(labelFeatureLevel);
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
            if (rendering)
            {
                rendering = false;
                timer?.Stop();
            }
            else
            {
                rendering = true;
                timer?.Start();
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            direct3D?.OnUpdate();
            direct3D?.OnRender();
        }

        public void UpdateLabels(string text1, string text2)
        {
            this.labelDevice.Text = text1;
            this.labelFeatureLevel.Text = text2;
        }
    }
}