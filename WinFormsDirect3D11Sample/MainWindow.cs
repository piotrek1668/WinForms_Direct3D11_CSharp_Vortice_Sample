using System.ComponentModel;
using Timer = System.Windows.Forms.Timer;

namespace WinFormsDirect3D11Sample;

public partial class MainWindow : Form
{
    #region Fields and Constants

    private Label? labelDevice;
    private Label? labelFeatureLevel;
    private Label? labelResolution;
    private Control? leftControl;
    private Control? rightControl;
    private Direct3D11? direct3D;

    private const int LabelWidth = 300;
    private const int LabelHeight = 20;
    private const int Offset = 5;

    private const int ButtonWidth = 200;
    private const int ButtonHeight = 30;

#if DEBUG
    public static DxgiInfoManager? infoManager = new DxgiInfoManager();
#endif

    #endregion

    #region Constructors and Deconstructors

    public MainWindow()
    {
        InitializeComponent();
        InitializeAdditionalControls();
        InitializeDirect3D();
        InitializeTimer();

        this.propertyGrid.SelectedObject = this.direct3D;
    }

    ~MainWindow()
    {
        direct3D?.Dispose();
    }

    #endregion

    #region Methods

    private void Button_Click(object? sender, EventArgs e)
    {
        if (sender is Button button)
        {
            direct3D?.SetDrawObject(button.Name);
        }
    }

    private void InitializeAdditionalControls()
    {
        Button drawTriangleButton = new()
        {
            Text = @"Draw grid",
            Size = new Size(ButtonWidth, ButtonHeight),
            Location = new Point(10, 10),
            Name = "drawGrid"
        };

        Button drawLine = new()
        {
            Text = @"Draw line",
            Size = new Size(ButtonWidth, ButtonHeight),
            Location = new Point(drawTriangleButton.Left + drawTriangleButton.Width + Offset, 10),
            Name = "drawLine"
        };

        Button drawCube = new()
        {
            Text = @"Draw cube",
            Size = new Size(ButtonWidth, ButtonHeight),
            Location = new Point(drawLine.Left + drawLine.Width + Offset, 10),
            Name = "drawCube"
        };

        Button drawText = new()
        {
            Text = @"Draw text",
            Size = new Size(ButtonWidth, ButtonHeight),
            Location = new Point(drawCube.Left + drawCube.Width + Offset, 10),
            Name = "drawText"
        };

        Button drawAll = new()
        {
            Text = @"Draw all",
            Size = new Size(ButtonWidth, ButtonHeight),
            Location = new Point(drawText.Left + drawText.Width + Offset, 10),
            Name = "drawAll"
        };

        drawTriangleButton.Click += Button_Click;
        drawLine.Click += Button_Click;
        drawCube.Click += Button_Click;
        drawText.Click += Button_Click;
        drawAll.Click += Button_Click;

        var labelInformation = new Label
        {
            Text = @"Device informations:",
            Size = new Size(LabelWidth, LabelHeight),
            Location = new Point(10, drawTriangleButton.Top + drawTriangleButton.Height + Offset)
        };

        labelDevice = new Label
        {
            Text = @"Device: ",
            Size = new Size(LabelWidth, LabelHeight),
            Location = new Point(10, labelInformation.Top + labelInformation.Height + Offset)
        };

        labelFeatureLevel = new Label
        {
            Text = @"Feature level: ",
            Size = new Size(LabelWidth, LabelHeight),
            Location = new Point(10, labelDevice.Top + labelDevice.Height + Offset)
        };

        labelResolution = new Label
        {
            Text = @"Resolution: ",
            Size = new Size(LabelWidth, LabelHeight),
            Location = new Point(10, labelFeatureLevel.Top + labelFeatureLevel.Height + Offset)
        };

        leftControl = new Control("Left Control", 10, labelResolution.Top + labelResolution.Height + Offset, 600, 500);

        rightControl = new Control("Right Control")
        {
            Location = new Point(leftControl.Width + leftControl.Left + Offset, leftControl.Top),
            Size = new Size(leftControl.Width, leftControl.Height),
            BackColor = Color.Gray
        };

        leftControl.BackColor = Color.Gray;

        Controls.Add(drawTriangleButton);
        Controls.Add(drawLine);
        Controls.Add(drawCube);
        Controls.Add(drawText);
        Controls.Add(drawAll);
        Controls.Add(labelInformation);
        Controls.Add(labelDevice);
        Controls.Add(labelFeatureLevel);
        Controls.Add(labelResolution);
        Controls.Add(leftControl);
        Controls.Add(rightControl);
    }

    private void InitializeDirect3D()
    {
        try
        {
            direct3D = new Direct3D11(this, leftControl, rightControl);
            direct3D.OnInit();
        }
        finally
        {
#if DEBUG
            infoManager?.PrintMessages();
            infoManager?.Set();
#endif
        }
        
    }

    private void InitializeTimer()
    {
        var timer = new Timer();
        timer.Tick += Timer_Tick;
        timer.Interval = 20;
        timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        direct3D?.OnUpdate();
        direct3D?.OnRender();

#if DEBUG
        infoManager?.PrintMessages();
        infoManager?.Set();
#endif
    }

    public void UpdateLabels(string device, string featureLevel, string resolution)
    {
        if (labelDevice != null) labelDevice.Text += device;
        if (labelFeatureLevel != null) labelFeatureLevel.Text += featureLevel;
        if (labelResolution != null) labelResolution.Text += resolution;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        direct3D?.Dispose();
    }

    #endregion
}