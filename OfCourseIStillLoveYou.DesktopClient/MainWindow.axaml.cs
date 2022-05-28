using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.Visuals.Media.Imaging;
using OfCourseIStillLoveYou.Communication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OfCourseIStillLoveYou.DesktopClient;

public class MainWindow : Window
{
    private const int Delay = 10;
    private const string SettingPath = "settings.json";
    private const string Endpoint = "localhost";
    private const int Port = 5077;
    int cameraNmb = 0;
    int cameraNmbOld = 0;
    int sizeW = 1000;
    int sizeH = 1000;
    int sizeWold = 0;
    int sizeHold = 0;
    int vertsnap = 200;
    int horizsnap = 200;
    int camsize = 400;

    private Bitmap? _initialImage;

    private SettingsPoco? _settings;

    private bool _statusUnstable;
    private Bitmap? _texture;
    private double _desiredHeight;
    private byte[]? _previousTexture;

    List<string>? camIds;

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
    }

    public bool IsClosing { get; set; }


    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        StoreInitialImage();
        ReadSettings();

        if (_settings != null) GrpcClient.ConnectToServer(_settings.EndPoint, _settings.Port);

        Task.Run(CameraTextureWorker);
        Task.Run(CameraFetchWorker);
        Task.Run(Resizer);

    }

    private void ReadSettings()
    {
        try
        {
            var settingsText = File.ReadAllText(SettingPath);
            _settings = JsonSerializer.Deserialize<SettingsPoco>(settingsText);
        }
        catch (Exception)
        {
            _settings = new SettingsPoco { EndPoint = Endpoint, Port = Port };
        }
    }

    private void StoreInitialImage()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
            _initialImage = (Bitmap)this.FindControl<Image>("imgCameraTexture1").Source);
    }


    private void CameraTextureWorker(){
        while (!IsClosing){
            Task.Delay(Delay).Wait();
            // For each available cam, get the current frame
            for (int i = 0; i < cameraNmb; i++){
                if (i == 6) continue;
                string actualct = (i+1).ToString();
                string actualct1 = "imgCameraTexture" + actualct;
                if (string.IsNullOrEmpty(camIds[i])) continue;
                var cameraData = GrpcClient.GetCameraDataAsync(camIds[i]).Result;
                if (cameraData.Texture == null)
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        this.FindControl<Image>(actualct1).Source = _initialImage;
                    });
                    continue;
                }
                if (ByteArrayCompare(cameraData.Texture, _previousTexture)) continue;
                // _desiredHeight = (int) (vertsnap * 2);
                _desiredHeight = camsize;
                using MemoryStream ms = new(cameraData.Texture);
                _texture = Bitmap.DecodeToHeight(ms, (int)_desiredHeight,
                    BitmapInterpolationMode.LowQuality);
                _statusUnstable = false;
                _previousTexture = cameraData.Texture;
                StringBuilder sb = new();
                sb.AppendLine(cameraData.Altitude);
                sb.AppendLine(cameraData.Speed);
                Dispatcher.UIThread.InvokeAsync(() =>
            {
                _desiredHeight = this.FindControl<Image>(actualct1).DesiredSize.Height;
                this.FindControl<Image>(actualct1).Source = _texture;
                if (_statusUnstable) return;
                var textInfo = this.FindControl<TextBlock>("TextInfo");
                textInfo.Text = sb.ToString();
                //var window = this.FindControl<Window>("MainWindow");
                //window.Title = cameraData.CameraName;
            });
            }
        }
    }

    static bool ByteArrayCompare(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2){
        return a1.SequenceEqual(a2);
    }


    private void CameraFetchWorker()
    {
        while (!IsClosing)
        {
            Task.Delay(1000).Wait();
            // Get the current camera list
            try
            {
                List<string>? cameraIds = GrpcClient.GetCameraIds();
                 if (cameraIds == null || cameraIds.Count == 0) Dispatcher.UIThread.InvokeAsync(NotifyWaitingForCameraFeed);

                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (cameraIds != null) UpdateCameraList(cameraIds);
                });
                
                Dispatcher.UIThread.InvokeAsync(GetSelectedCamera).ContinueWith(selectedCamera =>
                {
                    //_currentCamera = selectedCamera.Result;
                });
            }
            catch (Exception)
            {
                Dispatcher.UIThread.InvokeAsync(NotifyConnectingToServer);
            }
        }
    }

    private void Resizer()
    {
        while (!IsClosing)
        {
            Task.Delay(1000).Wait();
            // Resize panels and controls, only if changed
            Dispatcher.UIThread.InvokeAsync(() =>
        {
            var wd = this.FindControl<Window>("MainWindow");
            sizeW = (int)wd.Width;
            sizeH = (int)wd.Height;
            vertsnap = sizeH / 5;
            horizsnap = sizeW / 3;
            if ((sizeWold != sizeW) || (sizeHold != sizeH) || (cameraNmbOld != cameraNmb))
            {
                sizeWold = sizeW;
                sizeHold = sizeH;
                cameraNmbOld = cameraNmb;
                var MainCanvas = this.FindControl<Canvas>("mc");
                var InfoCanvas = this.FindControl<Canvas>("ic");
                var txtCanvas = this.FindControl<Canvas>("ic1");
                var txt1 = this.FindControl<TextBlock>("TextInfo");
                var labelCanvas = this.FindControl<Canvas>("ic2");
                var cbCanvas = this.FindControl<Canvas>("ic3");
                var cc7 = this.FindControl<Canvas>("im7c");
                var cc8 = this.FindControl<Canvas>("im8c");
                var cc9 = this.FindControl<Canvas>("im9c");
                var im7 = this.FindControl<Image>("im7");
                var im8 = this.FindControl<Image>("ImgClose");
                var im9 = this.FindControl<Image>("ImgResize");
                var cc1 = this.FindControl<Canvas>("c1");
                var cc2 = this.FindControl<Canvas>("c2");
                var cc3 = this.FindControl<Canvas>("c3");
                var cc4 = this.FindControl<Canvas>("c4");
                var cc5 = this.FindControl<Canvas>("c5");
                var cc6 = this.FindControl<Canvas>("c6");
                var im1 = this.FindControl<Image>("imgCameraTexture1");
                var im2 = this.FindControl<Image>("imgCameraTexture2");
                var im3 = this.FindControl<Image>("imgCameraTexture3");
                var im4 = this.FindControl<Image>("imgCameraTexture4");
                var im5 = this.FindControl<Image>("imgCameraTexture5");
                var im6 = this.FindControl<Image>("imgCameraTexture6");

                MainCanvas.Width = sizeW;
                MainCanvas.Height = sizeH;
                Canvas.SetTop(MainCanvas, 0);
                Canvas.SetLeft(MainCanvas, 0);

                InfoCanvas.Width = sizeW;
                InfoCanvas.Height = vertsnap;
                Canvas.SetTop(InfoCanvas, vertsnap * 4);
                Canvas.SetLeft(InfoCanvas, 0);

                cc7.Width = horizsnap;
                cc7.Height = vertsnap;
                im7.Width = horizsnap;
                im7.Height = vertsnap;
                Canvas.SetTop(cc7, 0);
                Canvas.SetLeft(cc7, 0);

                txtCanvas.Width = horizsnap;
                txtCanvas.Height = vertsnap;
                Canvas.SetTop(txtCanvas, 0);
                Canvas.SetLeft(txtCanvas, horizsnap);
                txt1.Width = horizsnap;
                txt1.Height = vertsnap / 3;

                cc8.Width = 32;
                cc8.Height = 32;
                cc9.Width = 32;
                cc9.Height = 32;
                Canvas.SetTop(cc8, vertsnap - 32);
                Canvas.SetLeft(cc8, sizeW - 64);
                Canvas.SetTop(cc9, vertsnap - 32);
                Canvas.SetLeft(cc9, sizeW - 32);
                im8.Width = 32;
                im8.Height = 32;
                im9.Width = 32;
                im9.Height = 32;

                labelCanvas.IsVisible = false;
                cbCanvas.IsVisible = false;

                if (cameraNmb == 1) {
                    cc1.IsVisible = true;
                    cc2.IsVisible = false;
                    cc3.IsVisible = false;
                    cc4.IsVisible = false;
                    cc5.IsVisible = false;
                    cc6.IsVisible = false;
                    camsize = vertsnap * 4;
                    cc1.Width = vertsnap * 4;
                    cc1.Height = vertsnap * 4;
                    Canvas.SetTop(cc1, 0);
                    Canvas.SetLeft(cc1, ((sizeW - (vertsnap * 4)) / 2));
                    im1.Width = vertsnap * 4;
                    im1.Height = vertsnap * 4;
                }
                if (cameraNmb == 2)
                {
                    cc1.IsVisible = true;
                    cc2.IsVisible = true;
                    cc3.IsVisible = false;
                    cc4.IsVisible = false;
                    cc5.IsVisible = false;
                    cc6.IsVisible = false;
                    camsize = sizeW / 2;
                    cc1.Width = sizeW / 2;
                    cc1.Height = sizeW / 2;
                    cc2.Width = sizeW / 2;
                    cc2.Height = sizeW / 2;
                    Canvas.SetTop(cc1, 0);
                    Canvas.SetLeft(cc1, 0);
                    Canvas.SetTop(cc2, 0);
                    Canvas.SetLeft(cc2, sizeW / 2);
                    im1.Width = sizeW / 2;
                    im1.Height = sizeW / 2;
                    im2.Width = sizeW / 2;
                    im2.Height = sizeW / 2;
                }
                if (cameraNmb == 3)
                {
                    cc1.IsVisible = true;
                    cc2.IsVisible = true;
                    cc3.IsVisible = true;
                    cc4.IsVisible = false;
                    cc5.IsVisible = false;
                    cc6.IsVisible = false;
                    camsize = sizeW / 3;
                    cc1.Width = sizeW / 3;
                    cc1.Height = sizeW / 3;
                    cc2.Width = sizeW / 3;
                    cc2.Height = sizeW / 3;
                    cc3.Width = sizeW / 3;
                    cc3.Height = sizeW / 3;
                    Canvas.SetTop(cc1, 0);
                    Canvas.SetLeft(cc1, 0);
                    Canvas.SetTop(cc2, 0);
                    Canvas.SetLeft(cc2, sizeW / 3);
                    Canvas.SetTop(cc3, 0);
                    Canvas.SetLeft(cc3, ((sizeW / 3) *2));
                    im1.Width = sizeW / 3;
                    im1.Height = sizeW / 3;
                    im2.Width = sizeW / 3;
                    im2.Height = sizeW / 3;
                    im3.Width = sizeW / 3;
                    im3.Height = sizeW / 3;
                }
                if (cameraNmb == 4)
                {
                    cc1.IsVisible = true;
                    cc2.IsVisible = true;
                    cc3.IsVisible = true;
                    cc4.IsVisible = true;
                    cc5.IsVisible = false;
                    cc6.IsVisible = false;
                    camsize = vertsnap * 2;
                    cc1.Width = vertsnap * 2;
                    cc1.Height = vertsnap * 2;
                    cc2.Width = vertsnap * 2;
                    cc2.Height = vertsnap * 2;
                    cc3.Width = vertsnap * 2;
                    cc3.Height = vertsnap * 2;
                    cc4.Width = vertsnap * 2;
                    cc4.Height = vertsnap * 2;
                    Canvas.SetTop(cc1, 0);
                    Canvas.SetLeft(cc1, 0);
                    Canvas.SetTop(cc2, 0);
                    Canvas.SetLeft(cc2, sizeW / 3);
                    Canvas.SetTop(cc3, 0);
                    Canvas.SetLeft(cc3, ((sizeW / 3) * 2));
                    Canvas.SetTop(cc4, vertsnap * 2);
                    Canvas.SetLeft(cc4, sizeW / 3);
                    im1.Width = vertsnap * 2;
                    im1.Height = vertsnap * 2;
                    im2.Width = vertsnap * 2;
                    im2.Height = vertsnap * 2;
                    im3.Width = vertsnap * 2;
                    im3.Height = vertsnap * 2;
                    im4.Width = vertsnap * 2;
                    im4.Height = vertsnap * 2;
                }
                if (cameraNmb == 5)
                {
                    cc1.IsVisible = true;
                    cc2.IsVisible = true;
                    cc3.IsVisible = true;
                    cc4.IsVisible = true;
                    cc5.IsVisible = true;
                    cc6.IsVisible = false;
                    camsize = vertsnap * 2;
                    cc1.Width = vertsnap * 2;
                    cc1.Height = vertsnap * 2;
                    cc2.Width = vertsnap * 2;
                    cc2.Height = vertsnap * 2;
                    cc3.Width = vertsnap * 2;
                    cc3.Height = vertsnap * 2;
                    cc4.Width = vertsnap * 2;
                    cc4.Height = vertsnap * 2;
                    cc5.Width = vertsnap * 2;
                    cc5.Height = vertsnap * 2;
                    Canvas.SetTop(cc1, 0);
                    Canvas.SetLeft(cc1, 0);
                    Canvas.SetTop(cc2, 0);
                    Canvas.SetLeft(cc2, sizeW / 3);
                    Canvas.SetTop(cc3, 0);
                    Canvas.SetLeft(cc3, ((sizeW / 3) * 2));
                    Canvas.SetTop(cc4, vertsnap * 2);
                    Canvas.SetLeft(cc4, 0);
                    Canvas.SetTop(cc5, vertsnap * 2);
                    Canvas.SetLeft(cc5, sizeW / 3);
                    im1.Width = vertsnap * 2;
                    im1.Height = vertsnap * 2;
                    im2.Width = vertsnap * 2;
                    im2.Height = vertsnap * 2;
                    im3.Width = vertsnap * 2;
                    im3.Height = vertsnap * 2;
                    im4.Width = vertsnap * 2;
                    im4.Height = vertsnap * 2;
                    im5.Width = vertsnap * 2;
                    im5.Height = vertsnap * 2;
                }
                if (cameraNmb == 6)
                {
                    cc1.IsVisible = true;
                    cc2.IsVisible = true;
                    cc3.IsVisible = true;
                    cc4.IsVisible = true;
                    cc5.IsVisible = true;
                    cc6.IsVisible = true;
                    camsize = vertsnap * 2;
                    cc1.Width = vertsnap * 2;
                    cc1.Height = vertsnap * 2;
                    cc2.Width = vertsnap * 2;
                    cc2.Height = vertsnap * 2;
                    cc3.Width = vertsnap * 2;
                    cc3.Height = vertsnap * 2;
                    cc4.Width = vertsnap * 2;
                    cc4.Height = vertsnap * 2;
                    cc5.Width = vertsnap * 2;
                    cc5.Height = vertsnap * 2;
                    cc6.Width = vertsnap * 2;
                    cc6.Height = vertsnap * 2;
                    Canvas.SetTop(cc1, 0);
                    Canvas.SetLeft(cc1, 0);
                    Canvas.SetTop(cc2, 0);
                    Canvas.SetLeft(cc2, sizeW / 3);
                    Canvas.SetTop(cc3, 0);
                    Canvas.SetLeft(cc3, ((sizeW / 3) * 2));
                    Canvas.SetTop(cc4, vertsnap * 2);
                    Canvas.SetLeft(cc4, 0);
                    Canvas.SetTop(cc5, vertsnap * 2);
                    Canvas.SetLeft(cc5, sizeW / 3);
                    Canvas.SetTop(cc6, vertsnap * 2);
                    Canvas.SetLeft(cc6, ((sizeW / 3) * 2));
                    im1.Width = vertsnap * 2;
                    im1.Height = vertsnap * 2;
                    im2.Width = vertsnap * 2;
                    im2.Height = vertsnap * 2;
                    im3.Width = vertsnap * 2;
                    im3.Height = vertsnap * 2;
                    im4.Width = vertsnap * 2;
                    im4.Height = vertsnap * 2;
                    im5.Width = vertsnap * 2;
                    im5.Height = vertsnap * 2;
                    im6.Width = vertsnap * 2;
                    im6.Height = vertsnap * 2;
                }
            }
        });
        }
    }

    private string? GetSelectedCamera()
    {
        var cbCameras = this.FindControl<ComboBox>("CbCameras");
        return cbCameras.SelectedItem == null ? string.Empty : cbCameras.SelectedItem.ToString();
    }

    private void NotifyWaitingForCameraFeed()
    {
        var textInfo = this.FindControl<TextBlock>("TextInfo");
        textInfo.Text = "Waiting for camera feed...";
        this.FindControl<Image>("imgCameraTexture1").Source = _initialImage;
        this.FindControl<Image>("imgCameraTexture2").Source = _initialImage;
        this.FindControl<Image>("imgCameraTexture3").Source = _initialImage;
        this.FindControl<Image>("imgCameraTexture4").Source = _initialImage;
        this.FindControl<Image>("imgCameraTexture5").Source = _initialImage;
        this.FindControl<Image>("imgCameraTexture6").Source = _initialImage;
    }

    private void NotifyConnectingToServer()
    {
        var textInfo = this.FindControl<TextBlock>("TextInfo");
        textInfo.Text = "Connecting to server...";
    }

    private void UpdateCameraList(List<string?> cameraIds)
    {
        var cbCameras = this.FindControl<ComboBox>("CbCameras");
        cbCameras.Items = cameraIds;

        // CUSTOM CODE
        cameraNmb = cameraIds.Count;
        camIds = cameraIds;
        // END CUSTOM CODE

        /*if (string.IsNullOrEmpty(_previousSelectedCamera) || !cameraIds.Contains(_previousSelectedCamera)) return;
        cbCameras.SelectedItem = _previousSelectedCamera;
        _previousSelectedCamera = "";*/
    }

    private void ImgCameraTexture_OnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var textInfo = this.FindControl<TextBlock>("TextInfo");
            var cbCameras = this.FindControl<ComboBox>("CbCameras");
            var labelCameras = this.FindControl<Label>("LabelCameras");
            var imgResize = this.FindControl<Image>("ImgResize");
            var imgClose = this.FindControl<Image>("ImgClose");

            labelCameras.IsVisible = !labelCameras.IsVisible;
            textInfo.IsVisible = !textInfo.IsVisible;
            cbCameras.IsVisible = !cbCameras.IsVisible;
            imgResize.IsVisible = !imgResize.IsVisible;
            imgClose.IsVisible = !imgClose.IsVisible;
        });
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
    }

    private void ImgResize_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginResizeDrag(WindowEdge.SouthEast, e);
    }

    private void ImgClose_OnTapped(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_OnClosing(object? sender, CancelEventArgs e)
    {
        IsClosing = true;
    }
}