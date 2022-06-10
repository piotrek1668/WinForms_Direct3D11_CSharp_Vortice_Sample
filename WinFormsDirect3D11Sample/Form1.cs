namespace WinFormsDirect3D11Sample
{
    public partial class Form1 : Form
    {
        private Control control = new Control("Direct3D11 Control", 10, 40, 600, 500);
        private Control control2D = new Control("Direct2D Control");
        private Label label;
        private Label labelFeatureLevel;
        private Direct3D11 direct3D;
        private System.Windows.Forms.Timer timer;
        private bool rendering = true;

        public Form1()
        {
            InitializeComponent();

            Button button = new Button();
            button.Text = "Toggle rendering";
            button.Size = new Size(300, 24);
            button.Location = new Point(10, 10);
            button.Click += this.Button_Click;
            this.Controls.Add(button);

            label = new Label();
            label.Text = "Direct3D11 Mode";
            label.Size = new Size(150, 15);
            label.Location = new Point(button.Left + button.Size.Width + 10, 15);
            this.Controls.Add(label);

            labelFeatureLevel = new Label();
            labelFeatureLevel.Text = "Highest supported feature level: NONE";
            labelFeatureLevel.Size = new Size(400, 15);
            labelFeatureLevel.Location = new Point(label.Left + label.Width + 10, 15);
            this.Controls.Add(labelFeatureLevel);

            control2D.Location = new Point(control.Width + control.Left + 10, control.Top);
            control2D.Size = new Size(control.Width, control.Height);
            control2D.BackColor = Color.Gray;

            control.BackColor = Color.Gray;

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