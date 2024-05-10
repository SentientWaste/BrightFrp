using Downloader;
using System.Diagnostics;

namespace BrightFrp;

public sealed class Frp {
    private readonly string _frpUrl = "https://github.com/fatedier/frp/releases/download/v0.48.0/frp_0.48.0_windows_386.zip";

    public string Token { get; init; }
    public string LocalIp { get; init; }
    public string ServerIp { get; init; }
    public string FilePath { get; init; }
    public string LocalPort { get; init; }
    public string ServerPort { get; init; }
    public string ClientName { get; init; }
    public string RemotePort { get; init; }

    public Process Launch() {
        var process = new Process();
        var iniFilePath = $"{FilePath}\\BrightFrp\\frp_0.48.0_windows_386\\frpc.ini";

        try {
            List<string> args = [
                $"[common]",
                $"server_addr = {ServerIp}",
                $"server_port = {ServerPort}",
                $"{(string.IsNullOrEmpty(Token) ? default : $"token = {Token}\n")}",

                $"[{ClientName}]",
                $"type = tcp",
                $"local_ip = {LocalIp}",
                $"local_port = {LocalPort}",
                $"remote_port = {RemotePort}"
            ];

            File.WriteAllLines(iniFilePath, args);

            var startInfo = new ProcessStartInfo {
                FileName = "cmd.exe",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            process.StartInfo = startInfo;
            process.Start();
            var outputStream = process.StandardOutput;

            process.StandardInput.WriteLine($"cd {Path.Combine(FilePath, "BrightFrp", "frp_0.48.0_windows_386")}");
            process.StandardInput.WriteLine("frpc.exe -c frpc.ini");
            Console.WriteLine("已启动进程");

            string output = outputStream.ReadToEnd();

            Console.WriteLine(output);
            return process;
        } catch (Exception ex) {
            Console.WriteLine($"启动服务时出现错误：{ex.Message}");
        }

        return default;
    }

    public async void Init() {
        var directoryPath = Path.Combine(FilePath, "BrightFrp");
        if (!Directory.Exists(directoryPath)) {
            Directory.CreateDirectory(directoryPath);
        }

        if (!File.Exists(Path.Combine(FilePath, "frpc.exe"))) {
            var downloadOpt = new DownloadConfiguration() {
                ChunkCount = 8,
                ParallelDownload = true
            };

            var downloader = new DownloadService(downloadOpt);
            await downloader.DownloadFileTaskAsync(_frpUrl, Path.Combine(FilePath, "BrightFrp"));
        }
    }
}