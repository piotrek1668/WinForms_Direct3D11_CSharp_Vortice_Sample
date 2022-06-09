namespace WinFormsDirect3D11Sample
{
    public partial class Form1 : Form
    {
        private Control control = new Control("D311 Control", 10, 40, 750, 600);
        private Label label;
        private Label labelFeatureLevel;
        private Direct3D11 direct3D;
        private System.Windows.Forms.Timer timer;

        public Form1()
        {
            InitializeComponent();

            Button button = new Button();
            button.Text = "Stop rendering";
            button.Size = new Size(300, 24);
            button.Location = new Point(10, 10);
            button.Click += this.Button_Click;
            this.Controls.Add(button);

            label = new Label();
            label.Text = "Direct3D11 Mode";
            label.Size = new Size(200, 24);
            label.Location = new Point(320, 10);
            this.Controls.Add(label);

            labelFeatureLevel = new Label();
            labelFeatureLevel.Text = "Highest supported feature level: NONE";
            labelFeatureLevel.Size = new Size(400, 24);
            labelFeatureLevel.Location = new Point(label.Left + label.Width + 10, 10);
            this.Controls.Add(labelFeatureLevel);

            this.Controls.Add(control);

            direct3D = new Direct3D11(this, control);
            direct3D.OnInit();

            timer = new System.Windows.Forms.Timer();
            timer.Tick += Timer_Tick;
            timer.Interval = 50;
            timer.Start();
        }

        private void Button_Click(object? sender, EventArgs e)
        {
            timer?.Stop();
            direct3D?.Dispose();
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