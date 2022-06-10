namespace WinFormsDirect3D11Sample
{
    public partial class Form1 : Form
    {
        private readonly Control control = new("Direct3D11 Control", 10, 50, 600, 500);
        private readonly Control control2D = new("Direct2D Control");
        private readonly Label label;
        private readonly Label labelFeatureLevel;
        private readonly Direct3D11 direct3D;
        private readonly System.Windows.Forms.Timer timer;
        private bool rendering = true;

        public Form1()
        {
            InitializeComponent();

            Button button = new()
            {
                Text = "Toggle rendering",
                Size = new Size(300, 30),
                Location = new Point(10, 10)
            };

            button.Click += this.Button_Click;
            
            label = new Label
            {
                Text = "Direct3D11 Mode",
                Size = new Size(150, 30),
                Location = new Point(button.Left + button.Size.Width + 10, 15)
            };

            labelFeatureLevel = new Label
            {
                Text = "Highest supported feature level: NONE",
                Size = new Size(400, 30),
                Location = new Point(label.Left + label.Width + 10, 15)
            };

            control2D.Location = new Point(control.Width + control.Left + 10, control.Top);
            control2D.Size = new Size(control.Width, control.Height);
            control2D.BackColor = Color.Gray;

            control.BackColor = Color.Gray;

            this.Controls.Add(button);
            this.Controls.Add(label);
            this.Controls.Add(labelFeatureLevel);
            this.Controls.Add(control);
            this.Controls.Add(control2D);

            direct3D = new Direct3D11(this, control, control2D);
            direct3D.OnInit();

            timer = new System.Windows.Forms.Timer();
            timer.Tick += Timer_Tick;
            timer.Interval = 20;
            timer.Start();
        }

        ~Form1()
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
            this.label.Text = text1;
            this.labelFeatureLevel.Text = text2;
        }
    }
}