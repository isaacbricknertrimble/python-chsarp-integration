using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Python.Runtime;

namespace PythonFastApiPoc
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Python FastAPI + C# Integration POC\n");

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var venvPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".venv"));
            var pythonDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "python"));
            var pythonDll = FindPythonDll(venvPath);

            if (string.IsNullOrEmpty(pythonDll) || !Directory.Exists(venvPath))
            {
                Console.WriteLine("ERROR: Run setup.ps1 first!");
                return;
            }

            Console.WriteLine($"Using Python: {pythonDll}");
            Console.WriteLine($"Using venv: {venvPath}\n");

            // load python dlls and initialize
            try
            {
                Runtime.PythonDLL = pythonDll;
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();

                var sitePackages = Path.Combine(venvPath, "Lib", "site-packages");
                var libPath = Path.Combine(venvPath, "Lib");
                
                using (Py.GIL())
                {
                    dynamic sys = Py.Import("sys");
                    sys.path.insert(0, sitePackages);
                    sys.path.insert(0, libPath);
                    sys.path.insert(0, venvPath);
                }

                var cts = new CancellationTokenSource();
                var serverTask = Task.Run(() =>
                {
                    try
                    {
                        //run python uvicorn server
                        using (Py.GIL())
                        {
                            dynamic sys = Py.Import("sys");
                            sys.path.insert(0, pythonDir);
                            
                            dynamic uvicorn = Py.Import("uvicorn");
                            dynamic app_module = Py.Import("app");
                            
                            Console.WriteLine("Starting server...\n");
                            uvicorn.run(app_module.app, host: "127.0.0.1", port: 8000, log_level: "error");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Server error: {ex.Message}");
                    }
                }, cts.Token);

                await Task.Delay(2000);

                using var client = new HttpClient();
                
                Console.WriteLine("Testing API:");
                
                try
                {
                    var health = await client.GetStringAsync("http://127.0.0.1:8000/health");
                    Console.WriteLine($"  Health: {health}");

                    var json = JsonSerializer.Serialize(new { text = "This product is amazing!" });
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("http://127.0.0.1:8000/predict", content);
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"  Prediction: {result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"API test failed: {ex.Message}");
                    Console.WriteLine("Server may not have started. Check errors above.");
                }

                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();

                cts.Cancel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
            finally
            {
                try
                {
                    PythonEngine.Shutdown();
                }
                catch
                {
                }
            }
        }

        static string FindPythonDll(string venvPath)
        {
            var cfg = Path.Combine(venvPath, "pyvenv.cfg");
            if (File.Exists(cfg))
            {
                foreach (var line in File.ReadAllLines(cfg))
                {
                    if (line.StartsWith("home = "))
                    {
                        var home = line.Substring(7).Trim();
                        foreach (var ver in new[] { "313", "312", "311", "310", "39" })
                        {
                            var dll = Path.Combine(home, $"python{ver}.dll");
                            if (File.Exists(dll)) return dll;
                        }
                    }
                }
            }
            return "";
        }
    }
}
