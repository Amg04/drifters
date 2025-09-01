using DAL.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using PL.Services.RtspUrlBuilder;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace PL.Services.RtspPumpService
{
    public class RtspPumpService : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly IRtspUrlBuilder _urlBuilder;
        private readonly ConcurrentDictionary<int, Process> _procs = new();

        public RtspPumpService(IServiceProvider sp, 
            IRtspUrlBuilder urlBuilder)
        {
            _sp = sp;
            _urlBuilder = urlBuilder;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DriftersDBContext>();

                var cams = await db.Cameras.Where(c => c.Enabled).ToListAsync(stoppingToken);

                foreach (var cam in cams)
                {
                    if (_procs.ContainsKey(cam.Id)) continue;
                    _ = RunCameraLoopAsync(cam.Id, stoppingToken); // fire-and-forget
                }

                var enabledIds = cams.Select(c => c.Id).ToHashSet();
                foreach (var kv in _procs)
                {
                    if (!enabledIds.Contains(kv.Key))
                    {
                        TryKill(kv.Value);
                        _procs.TryRemove(kv.Key, out _);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task RunCameraLoopAsync(int cameraId, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DriftersDBContext>();
                var protector = scope.ServiceProvider.GetRequiredService<IDataProtector>();

                var cam = await db.Cameras.FindAsync(new object?[] { cameraId }, token);
                if (cam == null || !cam.Enabled) return;

                var outputDir = Path.Combine("wwwroot", "streams", cam.Id.ToString());
                Directory.CreateDirectory(outputDir);
                var hlsFile = Path.Combine(outputDir, "index.m3u8");
                cam.HlsLocalPath = hlsFile;
                cam.HlsPublicUrl = $"/streams/{cam.Id}/index.m3u8"; 
                cam.Status = "Starting";
                cam.LastHeartbeatUtc = DateTime.UtcNow;
                await db.SaveChangesAsync(token);

                var pwd = protector.Unprotect(cam.PasswordEnc);
                var rtsp = _urlBuilder.Build(cam, pwd);

                var args =
                    $"-rtsp_transport tcp -i \"{rtsp}\" " +
                    "-an -c:v copy " +
                    "-f hls -hls_time 2 -hls_list_size 6 -hls_flags delete_segments+append_list " +
                    "-master_pl_name master.m3u8 " +
                    $"\"{hlsFile}\"";

                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = args,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };

                if (!_procs.TryAdd(cam.Id, proc)) return;

                try
                {
                    proc.Start();

                    _ = Task.Run(async () =>
                    {
                        while (!proc.StandardError.EndOfStream && !token.IsCancellationRequested)
                        {
                            var line = await proc.StandardError.ReadLineAsync();
                            if (string.IsNullOrWhiteSpace(line)) continue;
                        }
                    }, token);

                    await WaitForFile(hlsFile, TimeSpan.FromSeconds(8), token);
                    cam.Status = File.Exists(hlsFile) ? "Online" : "Starting";
                    cam.LastHeartbeatUtc = DateTime.UtcNow;
                    await db.SaveChangesAsync(token);

                    while (!proc.HasExited && !token.IsCancellationRequested)
                    {
                        await Task.Delay(2000, token);
                        cam.LastHeartbeatUtc = DateTime.UtcNow;
                        await db.SaveChangesAsync(token);
                    }
                }
                catch
                {
                    cam.Status = "Offline";
                    cam.LastHeartbeatUtc = DateTime.UtcNow;
                    await db.SaveChangesAsync(token);
                }
                finally
                {
                    _procs.TryRemove(cam.Id, out _);
                    TryKill(proc);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
        }

        private static async Task WaitForFile(string path, TimeSpan timeout, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeout && !ct.IsCancellationRequested)
            {
                if (File.Exists(path)) return;
                await Task.Delay(500, ct);
            }
        }

        private static void TryKill(Process p)
        {
            try { if (!p.HasExited) p.Kill(true); } catch {  }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var kv in _procs) TryKill(kv.Value);
            _procs.Clear();
            return base.StopAsync(cancellationToken);
        }

    }
}
