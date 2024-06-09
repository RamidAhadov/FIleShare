using System.Net.Sockets;
using System.Reflection;
using FileShare.App.Extensions;
using FileShare.App.Models;
using FileShare.App.Services.Abstraction;
using FileShare.Business.Constants;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Button = System.Windows.Forms.Button;

namespace FileShare.App;

public partial class Application : Form
{
    private readonly IConfigurationRoot _configuration;
    private readonly IEngineService _engineService;
    private readonly IDeviceService _deviceService;
    private readonly IFileService _fileService;
    private TcpListener _tcpListener;
    CancellationTokenSource _source = new();

    private string? _selectedItem;
    private string? _selectedIp;

    public Application(IConfigurationRoot configuration, IEngineService engineService, IDeviceService deviceService,
        IFileService fileService)
    {
        _configuration = configuration;
        _engineService = engineService;
        _deviceService = deviceService;
        _fileService = fileService;

        InitializeComponent();
    }

    private async void Application_Load(object sender, EventArgs e)
    {
        LoadDirectoryData();
    }

    private void LoadDirectoryData(string? path = default)
    {
        string rootDirectory;
        if (path == null)
        {
            var directoryModel = _configuration.GetSection("DirectoryModel").Get<DirectoryModel>();
            rootDirectory = directoryModel.Path;
        }
        else
        {
            rootDirectory = path;
        }

        TreeNode rootNode = new TreeNode(rootDirectory, 0, 0);
        tvDirectory.Nodes.Add(rootNode);
        LoadDirectories(rootDirectory, rootNode);

        tvDirectory.AfterSelect += tvDirectory_AfterSelect;
    }

    private void LoadDirectories(string dir, TreeNode node)
    {
        try
        {
            string[] directories = Directory.GetDirectories(dir);
            foreach (string directory in directories)
            {
                TreeNode dirNode = new TreeNode(Path.GetFileName(directory), 0, 0);
                node.Nodes.Add(dirNode);
                LoadDirectories(directory, dirNode);
            }

            string[] files = Directory.GetFiles(dir);
            foreach (string file in files)
            {
                TreeNode fileNode = new TreeNode(Path.GetFileName(file), GetFileIconIndex(file),
                    GetFileIconIndex(file));
                node.Nodes.Add(fileNode);
            }
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}");
        }
    }


    private Icon LoadIcon(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream != null)
            {
                return new Icon(stream);
            }

            throw new Exception($"Resource '{resourceName}' not found.");
        }
    }

    private int GetFileIconIndex(string filePath)
    {
        Icon icon = Icon.ExtractAssociatedIcon(filePath);

        return AddIconToImageList(icon);
    }

    private int AddIconToImageList(Icon icon)
    {
        icons.Images.Add(icon);

        return icons.Images.Count - 1;
    }

    private void tvDirectory_AfterSelect(object sender, TreeViewEventArgs e)
    {
        _selectedItem = e.Node?.FullPath;
    }

    private void CreateButton(int buttonNumber, string ip)
    {
        var width = 270;
        var height = 29;
        var button = new System.Windows.Forms.Button();
        button.Location = new Point(0, height * buttonNumber);
        button.Name = "button1";
        button.Size = new Size(270, height);
        button.TabIndex = 0;
        button.Text = $"{ip}";
        button.UseVisualStyleBackColor = true;
        button.SetValue(ip);
        button.Click += dynamicButton_Click;
        pnlDevices.Controls.Add(button);
    }

    private void dynamicButton_Click(object sender, EventArgs e)
    {
        if (sender is Button clickedButton)
        {
            _selectedIp = clickedButton.GetValue();
        }
    }

    private async void btnSearchDevice_Click(object sender, EventArgs e)
    {
        var progressCount = _deviceService.GetProgressCount();
        pbarSearchDevices.Maximum = progressCount;
        pbarSearchDevices.Visible = true;
        int count = 0;
        int progress = 0;
        await foreach (var ip in _deviceService.GetDevicesAsync())
        {
            if (!string.IsNullOrEmpty(ip))
            {
                CreateButton(count, ip);
                count++;
            }

            UpdateSearchDevicesProgressBar(progress);
            progress++;
        }

        pbarSearchDevices.Visible = false;
        pbarSearchDevices.Value = 0;
    }

    private async void btnStartEngine_Click(object sender, EventArgs e)
    {
        var progressCount = 4;
        pbarStartEngine.Maximum = progressCount;
        pbarStartEngine.Value = 0;
        pbarStartEngine.Visible = true;

        var progress = new Progress<int>(value => pbarStartEngine.Value = value);

        var result = await _engineService.StartEngineAsync(progress, CancellationToken.None);

        if (result.IsFailed)
        {
            MessageBox.Show(result.Errors.FirstOrDefault()?.Message, "Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        else
        {
            MessageBox.Show("Engine started successfully.", "Success", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        pbarStartEngine.Visible = false;
        
        var receiveRequest = Task.Run(GetRequestsAsync);

        await Task.WhenAll(receiveRequest);
        Console.WriteLine("The end");
    }

    private async void btnRestartEngine_Click(object sender, EventArgs e)
    {
        
    }

    private async void btnSendFile_Click(object sender, EventArgs e)
    {
        if (_selectedItem == null)
        {
            MessageBox.Show("Please choose a item to send.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

        if (_selectedIp == null)
        {
            MessageBox.Show("Please choose a receiver.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

        var requestResult = await _fileService.SendRequestAsync(_selectedIp, _source.Token);
        if (requestResult.IsFailed)
        {
            MessageBox.Show(requestResult.Errors.FirstOrDefault()?.Message, "Reject", MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        MessageBox.Show($"Request accepted by {_selectedIp}", "Success", MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        var filenameResult = await _fileService.SendFilenameAsync(_selectedIp, _selectedItem, _source.Token);
        if (filenameResult.IsFailed)
        {
            MessageBox.Show(requestResult.Errors.FirstOrDefault()?.Message, "Reject", MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        MessageBox.Show("File sent successfully.");
    }

    private void UpdateSearchDevicesProgressBar(int value)
    {
        if (value >= pbarSearchDevices.Minimum && value <= pbarSearchDevices.Maximum)
        {
            pbarSearchDevices.Value = value;
        }
    }

    private void UpdateSendFileProgressBar(int value)
    {
        if (value >= pbarSendFile.Minimum && value <= pbarSendFile.Maximum)
        {
            pbarSendFile.Value = value;
        }
    }

    private async void GetRequestsAsync()
    {
        var cts = new CancellationTokenSource();
        await foreach (var request in _fileService.GetRequestAsync(cts.Token))
        {
            var dialogResult = MessageBox.Show($"Received request: {request}\nDo you want to process this request?",
                "Request Received", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                var result = await ProcessRequest(request, cts.Token);
                if (result.IsFailed)
                {
                    MessageBox.Show("Response did not send to user.", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                MessageBox.Show("Response successfully sent. Waiting dor the file.", "Info", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                var result = await IgnoreRequest(request, cts.Token);
                if (result.IsFailed)
                {
                    MessageBox.Show("Response did not send to user.", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                MessageBox.Show("Response successfully sent. File did not accepted.", "Info", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
    }

    private async Task<Result> ProcessRequest(string destinationIp, CancellationToken token)
    {
        return await _fileService.SendResponseAsync(Responses.Accept, destinationIp, token);
    }

    private async Task<Result> IgnoreRequest(string destinationIp, CancellationToken token)
    {
        return await _fileService.SendResponseAsync(Responses.Reject, destinationIp, token);
    }
}