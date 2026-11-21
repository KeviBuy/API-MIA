using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MongoDB.Driver;

namespace ЛР3_МіА._АПІшка.Tests.Task4_IntegrationTests;

public class EphemeralMongoRunner : IDisposable
{
    private Process? _process;
    private readonly string _dataDir;
    public int Port { get; }
    public string ConnectionString => $"mongodb://127.0.0.1:{Port}";

    private EphemeralMongoRunner(int port, string dataDir, Process process)
    {
        Port = port;
        _dataDir = dataDir;
        _process = process;
    }

    public static EphemeralMongoRunner Start()
    {
        int port = GetFreePort();
        string tempDir = Path.Combine(Path.GetTempPath(), "mongo_integration_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        string mongodPath = ResolveMongodPath();

        var startInfo = new ProcessStartInfo
        {
            FileName = mongodPath,
            Arguments = $"--dbpath \"{tempDir}\" --port {port} --bind_ip 127.0.0.1 --nojournal --quiet",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false
        };

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Не вдалося запустити mongod.exe");

        var runner = new EphemeralMongoRunner(port, tempDir, process);
        runner.WaitForReady();
        return runner;
    }

    private static int GetFreePort()
    {
        using var tcp = new TcpListener(IPAddress.Loopback, 0);
        tcp.Start();
        int port = ((IPEndPoint)tcp.LocalEndpoint).Port;
        tcp.Stop();
        return port;
    }

    private static string ResolveMongodPath()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var possiblePath = Path.Combine(userProfile, ".nuget", "packages", "mongo2go", "3.1.3",
            "tools", "mongodb-windows-4.4.4-database-tools-100.3.1", "bin", "mongod.exe");

        if (File.Exists(possiblePath))
            return possiblePath;

        var packagesDir = Path.Combine(userProfile, ".nuget", "packages", "mongo2go");
        if (Directory.Exists(packagesDir))
        {
            var files = Directory.GetFiles(packagesDir, "mongod.exe", SearchOption.AllDirectories);
            if (files.Length > 0)
                return files[0];
        }

        throw new FileNotFoundException("Бінарник mongod.exe не знайдено у пакеті Mongo2Go");
    }

    private void WaitForReady()
    {
        var client = new MongoClient(ConnectionString);
        var timeout = TimeSpan.FromSeconds(15);
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            try
            {
                if (_process != null && _process.HasExited)
                {
                    throw new InvalidOperationException("Процес mongod.exe неочікувано завершився.");
                }

                client.GetDatabase("admin").RunCommand((Command<MongoDB.Bson.BsonDocument>)"{ping:1}");
                return;
            }
            catch
            {
                Thread.Sleep(150);
            }
        }

        throw new TimeoutException($"Тимчасовий екземпляр MongoDB не відповів протягом 15 секунд на порті {Port}");
    }

    public void Dispose()
    {
        try
        {
            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
                _process.WaitForExit(3000);
                _process.Dispose();
                _process = null;
            }

            if (Directory.Exists(_dataDir))
            {
                Directory.Delete(_dataDir, true);
            }
        }
        catch
        {
            // Ігноруємо заблоковані файли
        }
    }
}
