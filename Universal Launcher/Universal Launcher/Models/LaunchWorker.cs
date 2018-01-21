using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Universal_Launcher.Singleton;

namespace Universal_Launcher.Models
{
    internal class LaunchWorker : IDisposable
    {
        private readonly string _accessToken;
        private readonly bool _debugMode;

        private readonly IFolderService _folderService;
        private readonly string _id;
        private readonly string _javaHome64;
        private readonly string _login;
        private readonly int _memory;
        private readonly string _projectFolder;
        private readonly bool _useJavaArgs;
        private string _errorOutput = string.Empty;

        // процесс, в котором стартует консоль
        private Process _process;

        /// <summary>
        ///     Класс для запуска игры
        /// </summary>
        /// <param name="projectFolder">Путь к папке игры, там где лежат assets, mods и т.д.</param>
        /// <param name="javaHome64">Путь к java.exe для 64 bit систем</param>
        /// <param name="memory">Кол-во памяти на старте</param>
        /// <param name="login">Имя пользователя на Ely.by</param>
        /// <param name="id">Уникальный GUID пользователя</param>
        /// <param name="accessToken">Своеобразный пароль аккаунта из Ely.by</param>
        /// <param name="useJavaArgs">Используем ли эксп. аргументы</param>
        /// <param name="debugMode">Находимся ли в режиме отладки</param>
        public LaunchWorker(
            string projectFolder,
            string javaHome64,
            int memory,
            string login,
            string id,
            string accessToken,
            bool useJavaArgs = false,
            bool debugMode = false)
        {
            _projectFolder = projectFolder;
            _javaHome64 = javaHome64;
            _memory = memory;
            _login = login;
            _id = id;
            _accessToken = accessToken;
            _useJavaArgs = useJavaArgs;
            _debugMode = debugMode;
        }

        /// <summary>
        ///     Возвращаем
        /// </summary>
        public string GetBaseFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            App.ProjectName);

        public void Dispose()
        {
            _process.Dispose();
        }

        public async Task<string> Launch()
        {
            if (!Directory.Exists(GetBaseFolder))
            {
                var info = Directory.CreateDirectory(GetBaseFolder);
                info.Attributes = FileAttributes.Hidden;
            }

            var args = GetMainArgs(_javaHome64,
                _projectFolder,
                _memory,
                _login,
                _id,
                _accessToken,
                _useJavaArgs,
                _debugMode);


            var startInfo = new ProcessStartInfo
            {
                FileName = _javaHome64,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = _projectFolder,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = args,
                UseShellExecute = false
            };

            _process = new Process {StartInfo = startInfo};
            _process.Exited += OnExit;
            _process.Start();

            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();


            _process.ErrorDataReceived += (sender, eventArgs) => _errorOutput += "/n" + eventArgs.Data;

            if (_debugMode)
            {
                _process.OutputDataReceived += (sender, eventArgs) => Console.WriteLine(eventArgs.Data);
                AllocConsole();
                SetConsoleCtrlHandler(ConsoleCtrlCheck, true);
            }


            await Task.Run(() => _process.WaitForExit());
            return _errorOutput;
        }

        private void OnExit(object sender, EventArgs e)
        {
            if (_process.HasExited)
                return;

            _process.Close();
        }


        private bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            // Put your own handler here
            switch (ctrlType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                    //Console.WriteLine("CTRL+C received!");
                    break;

                case CtrlTypes.CTRL_BREAK_EVENT:
                    //Console.WriteLine("CTRL+BREAK received!");
                    break;

                case CtrlTypes.CTRL_CLOSE_EVENT:
                    //Console.WriteLine("Program being closed!");
                    _process.Kill();
                    _process.Dispose();
                    break;

                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    //Console.WriteLine("User is logging off!");
                    break;
            }

            return true;
        }

        #region Arguments

        #region Base java args

        private string GetBaseJavaArgs(string java64Path, int memory)
        {
            var temp = $"\"{java64Path}\" ";                    // Java executable path
            temp += $"-Xmx{memory}M "; // Set min amount of memory

            // some base parameters
            temp += "-Dfml.ignoreInvalidMinecraftCertificates=true " +
                    "-Dfml.ignorePatchDiscrepancies=true -XX:+UseConcMarkSweepGC " +
                    "-XX:+CMSIncrementalMode -XX:-UseAdaptiveSizePolicy -Xmn128M ";

            return temp;
        }

        #endregion

        #region Java Exp Args

        private readonly string JavaArgsExp =
            " -XX:+DisableExplicitGC -XX:+UseParNewGC -XX:+ScavengeBeforeFullGC " +
            "-XX:+CMSScavengeBeforeRemark -XX:+UseNUMA -XX:+CMSParallelRemarkEnabled " +
            "-XX:MaxTenuringThreshold=15 -XX:MaxGCPauseMillis=30 -XX:GCPauseIntervalMillis=150 " +
            "-XX:+UseAdaptiveGCBoundary -XX:-UseGCOverheadLimit -XX:+UseBiasedLocking -XX:SurvivorRatio=8 " +
            "-XX:TargetSurvivorRatio=90 -XX:MaxTenuringThreshold=15 " +
            "-XX:+UseFastAccessorMethods -XX:+UseCompressedOops -XX:+OptimizeStringConcat -XX:+AggressiveOpts " +
            " -XX:+UseCodeCacheFlushing -XX:ParallelGCThreads=5 " +
            "-XX:ReservedCodeCacheSize={0}m -XX:SoftRefLRUPolicyMSPerMB={1} ";

        private string GetJavaArgsExp(int memory)
        {
            var temp = string.Format(JavaArgsExp, Math.Min(memory / 2, 2048), memory * 2);
            return temp;
        }

        #endregion

        #region Minecraft Args

        private string GetMinecraftArgs(
            string launchFolder,
            string name,
            string id,
            string accessToken)
        {
            var libPath = Path.Combine(launchFolder, "versions", "1.10.2-forge1.10.2-12.18.3.2488", "natives");
            var temp = $"-Djava.library.path=\"{libPath}\" "; // set lib path       
            temp += $"-cp \"{GetLibraryPaths(launchFolder)}\" "; // add libs paths

            //set some default settings
            temp += "net.minecraft.launchwrapper.Launch --version 1.10.2-forge1.10.2-12.18.3.2488 " +
                    "--assetIndex 1.10 --tweakClass net.minecraftforge.fml.common.launcher.FMLTweaker " +
                    "--versionType Forge --userType legacy ";

            // add launch paths
            temp += $"--gameDir \"{launchFolder}\" " +
                    $"--assetsDir \"{Path.Combine(launchFolder, "assets")}\" ";

            temp += $"--username {name} --uuid {id} --accessToken {accessToken} ";

            return temp;
        }

        #region Library Paths

        private readonly string LibraryPaths =
            "{0}\\libraries\\net\\minecraftforge\\forge\\1.10.2-12.18.3.2488\\forge-1.10.2-12.18.3.2488.jar;" +
            "{0}\\libraries\\net\\minecraft\\launchwrapper\\1.12\\launchwrapper-1.12.jar;" +
            "{0}\\libraries\\org\\ow2\\asm\\asm-all\\5.0.3\\asm-all-5.0.3.jar;" +
            "{0}\\libraries\\jline\\jline\\2.13\\jline-2.13.jar;" +
            "{0}\\libraries\\com\\typesafe\\akka\\akka-actor_2.11\\2.3.3\\akka-actor_2.11-2.3.3.jar;" +
            "{0}\\libraries\\com\\typesafe\\config\\1.2.1\\config-1.2.1.jar;" +
            "{0}\\libraries\\org\\scala-lang\\scala-actors-migration_2.11\\1.1.0\\scala-actors-migration_2.11-1.1.0.jar;" +
            "{0}\\libraries\\org\\scala-lang\\scala-compiler\\2.11.1\\scala-compiler-2.11.1.jar;" +
            "{0}\\libraries\\org\\scala-lang\\plugins\\scala-continuations-library_2.11\\1.0.2\\scala-continuations-library_2.11-1.0.2.jar;" +
            "{0}\\libraries\\org\\scala-lang\\plugins\\scala-continuations-plugin_2.11.1\\1.0.2\\scala-continuations-plugin_2.11.1-1.0.2.jar;" +
            "{0}\\libraries\\org\\scala-lang\\scala-library\\2.11.1\\scala-library-2.11.1.jar;" +
            "{0}\\libraries\\org\\scala-lang\\scala-parser-combinators_2.11\\1.0.1\\scala-parser-combinators_2.11-1.0.1.jar;" +
            "{0}\\libraries\\org\\scala-lang\\scala-reflect\\2.11.1\\scala-reflect-2.11.1.jar;" +
            "{0}\\libraries\\org\\scala-lang\\scala-swing_2.11\\1.0.1\\scala-swing_2.11-1.0.1.jar;" +
            "{0}\\libraries\\org\\scala-lang\\scala-xml_2.11\\1.0.2\\scala-xml_2.11-1.0.2.jar;" +
            "{0}\\libraries\\lzma\\lzma\\0.0.1\\lzma-0.0.1.jar;" +
            "{0}\\libraries\\net\\sf\\jopt-simple\\jopt-simple\\4.6\\jopt-simple-4.6.jar;" +
            "{0}\\libraries\\java3d\\vecmath\\1.5.2\\vecmath-1.5.2.jar;" +
            "{0}\\libraries\\net\\sf\\trove4j\\trove4j\\3.0.3\\trove4j-3.0.3.jar;" +
            "{0}\\libraries\\net\\minecraftforge\\MercuriusUpdater\\1.10.2\\MercuriusUpdater-1.10.2.jar;" +
            "{0}\\libraries\\com\\mojang\\netty\\1.6\\netty-1.6.jar;" +
            "{0}\\libraries\\oshi-project\\oshi-core\\1.1\\oshi-core-1.1.jar;" +
            "{0}\\libraries\\net\\java\\dev\\jna\\jna\\3.4.0\\jna-3.4.0.jar;" +
            "{0}\\libraries\\net\\java\\dev\\jna\\platform\\3.4.0\\platform-3.4.0.jar;" +
            "{0}\\libraries\\com\\ibm\\icu\\icu4j-core-mojang\\51.2\\icu4j-core-mojang-51.2.jar;" +
            "{0}\\libraries\\net\\sf\\jopt-simple\\jopt-simple\\4.6\\jopt-simple-4.6.jar;" +
            "{0}\\libraries\\com\\paulscode\\codecjorbis\\20101023\\codecjorbis-20101023.jar;" +
            "{0}\\libraries\\com\\paulscode\\codecwav\\20101023\\codecwav-20101023.jar;" +
            "{0}\\libraries\\com\\paulscode\\libraryjavasound\\20101123\\libraryjavasound-20101123.jar;" +
            "{0}\\libraries\\com\\paulscode\\librarylwjglopenal\\20100824\\librarylwjglopenal-20100824.jar;" +
            "{0}\\libraries\\com\\paulscode\\soundsystem\\20120107\\soundsystem-20120107.jar;" +
            "{0}\\libraries\\io\\netty\\netty-all\\4.0.23.Final\\netty-all-4.0.23.Final.jar;" +
            "{0}\\libraries\\com\\google\\guava\\guava\\17.0\\guava-17.0.jar;" +
            "{0}\\libraries\\org\\apache\\commons\\commons-lang3\\3.3.2\\commons-lang3-3.3.2.jar;" +
            "{0}\\libraries\\commons-io\\commons-io\\2.4\\commons-io-2.4.jar;" +
            "{0}\\libraries\\commons-codec\\commons-codec\\1.9\\commons-codec-1.9.jar;" +
            "{0}\\libraries\\net\\java\\jinput\\jinput\\2.0.5\\jinput-2.0.5.jar;" +
            "{0}\\libraries\\net\\java\\jutils\\jutils\\1.0.0\\jutils-1.0.0.jar;" +
            "{0}\\libraries\\com\\google\\code\\gson\\gson\\2.2.4\\gson-2.2.4.jar;" +
            "{0}\\libraries\\com\\mojang\\authlib\\1.5.22\\authlib-1.5.22.jar;" +
            "{0}\\libraries\\com\\mojang\\realms\\1.9.8\\realms-1.9.8.jar;" +
            "{0}\\libraries\\org\\apache\\commons\\commons-compress\\1.8.1\\commons-compress-1.8.1.jar;" +
            "{0}\\libraries\\org\\apache\\httpcomponents\\httpclient\\4.3.3\\httpclient-4.3.3.jar;" +
            "{0}\\libraries\\commons-logging\\commons-logging\\1.1.3\\commons-logging-1.1.3.jar;" +
            "{0}\\libraries\\org\\apache\\httpcomponents\\httpcore\\4.3.2\\httpcore-4.3.2.jar;" +
            "{0}\\libraries\\it\\unimi\\dsi\\fastutil\\7.0.12_mojang\\fastutil-7.0.12_mojang.jar;" +
            "{0}\\libraries\\org\\apache\\logging\\log4j\\log4j-api\\2.0-beta9\\log4j-api-2.0-beta9.jar;" +
            "{0}\\libraries\\org\\apache\\logging\\log4j\\log4j-core\\2.0-beta9\\log4j-core-2.0-beta9.jar;" +
            "{0}\\libraries\\org\\lwjgl\\lwjgl\\lwjgl\\2.9.4-nightly-20150209\\lwjgl-2.9.4-nightly-20150209.jar;" +
            "{0}\\libraries\\org\\lwjgl\\lwjgl\\lwjgl_util\\2.9.4-nightly-20150209\\lwjgl_util-2.9.4-nightly-20150209.jar;" +
            "{0}\\versions\\1.10.2-forge1.10.2-12.18.3.2488\\1.10.2-forge1.10.2-12.18.3.2488.jar";

        private string GetLibraryPaths(string path)
        {
            var temp = string.Format(LibraryPaths, path);
            return temp;
        }

        #endregion

        #endregion

        #region Size

        private string GetSizeArgs(int width = 925, int height = 530)
        {
            var temp = $"--width {width} --height {height} ";
            return temp;
        }

        #endregion

        #region Main Args

        private string GetMainArgs(
            string java64Path,
            string launchFolder,
            int memory,
            string name,
            string id,
            string accessToken,
            bool useExpArgs = false,
            bool debug = false)
        {
            var temp = GetBaseJavaArgs(java64Path, memory);

            if (useExpArgs)
                temp += GetJavaArgsExp(memory);

            temp += GetMinecraftArgs(launchFolder, name, id, accessToken);
            temp += GetSizeArgs();

            if (debug)
                temp += "pause";

            return temp;
        }

        #endregion

        #endregion

        #region Win32

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        // Declare the SetConsoleCtrlHandler function
        // as external and receiving a delegate.

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        #endregion
    }
}