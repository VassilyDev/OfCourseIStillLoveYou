using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.Visuals.Media.Imaging;
using OfCourseIStillLoveYou.Communication;

namespace OfCourseIStillLoveYou.DesktopClient;

public class MainWindow : Window
{
    private const int Delay = 16;
    private const string SettingPath = "settings.json";
    private const string Endpoint = "localhost";
    private const int Port = 5077;
    private string? _currentCamera;

    private Bitmap? _initialImage;
    private string? _previousSelectedCamera;

    private SettingsPoco? _settings;

    private bool _statusUnstable;
    private Bitmap? _texture;
    private double _desiredHeight;
    private byte[]? _previousTexture;


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
            _initialImage = (Bitmap)this.FindControl<Image>("ImgCameraTexture").Source);
    }


    private void CameraTextureWorker()
    {
        while (!IsClosing)
        {
            Task.Delay(Delay).Wait();
            
            if (string.IsNullOrEmpty(_currentCamera)) continue;

            var cameraData = GrpcClient.GetCameraDataAsync(_currentCamera).Result;

            if (cameraData.Texture == null)
            {
                _texture = _initialImage;
                _statusUnstable = true;
                continue;
            }
            
            if (ByteArrayCompare(cameraData.Texture, _previousTexture)) continue;

            if (_desiredHeight == 0) _desiredHeight = 800;

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
                _desiredHeight = this.FindControl<Image>("ImgCameraTexture").DesiredSize.Height;

                this.FindControl<Image>("ImgCameraTexture").Source = _texture;

                if (_statusUnstable) return;

                var textInfo = this.FindControl<TextBlock>("TextInfo");
                textInfo.Text = sb.ToString();

                var window = this.FindControl<Window>("MainWindow");
                window.Title = cameraData.CameraName;
            });
        }
    }

    static bool ByteArrayCompare(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
    {
        return a1.SequenceEqual(a2);
    }


    private void CameraFetchWorker()
    {
        while (!IsClosing)
        {
            Task.Delay(1000).Wait();
            try
            {
                var cameraIds = GrpcClient.GetCameraIds();

                if (_statusUnstable)
                    Dispatcher.UIThread.InvokeAsync(NotifyUnstableCameraFeed);
                else if (cameraIds == null || cameraIds.Count == 0)
                    Dispatcher.UIThread.InvokeAsync(NotifyWaitingForCameraFeed);

                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (cameraIds != null) UpdateCameraList(cameraIds);
                });

                Dispatcher.UIThread.InvokeAsync(GetSelectedCamera).ContinueWith(selectedCamera =>
                {
                    _currentCamera = selectedCamera.Result;
                });
            }
            catch (Exception)
            {
                Dispatcher.UIThread.InvokeAsync(NotifyConnectingToServer);
            }
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
    }

    private void NotifyUnstableCameraFeed()
    {
        var cbCameras = this.FindControl<ComboBox>("CbCameras");

        if (!string.IsNullOrEmpty(cbCameras.SelectedItem?.ToString()))
            _previousSelectedCamera = cbCameras.SelectedItem.ToString();

        var textInfo = this.FindControl<TextBlock>("TextInfo");
        textInfo.Text = "VIDEO CONNECTION HAS BEEN LOST. RIP";
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


        if (string.IsNullOrEmpty(_previousSelectedCamera) || !cameraIds.Contains(_previousSelectedCamera)) return;


        cbCameras.SelectedItem = _previousSelectedCamera;
        _previousSelectedCamera = "";
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