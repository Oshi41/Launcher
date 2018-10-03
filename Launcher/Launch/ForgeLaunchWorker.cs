using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Launcher.Config.Interfaces;
using Launcher.Models;

namespace Launcher.Launch
{
    public class ForgeLaunchWorker
    {
        #region Fields

        // список загружаемых библиотек
        private static List<string> _libraries = new List<string>
        {
            "launchwrapper",
            "asm-all",
            "jline",
            "akka-actor_",
            "config",
            "scala-actors-migration_",
            "scala-compiler",
            "scala-continuations-library_",
            "scala-continuations-plugin_",
            "scala-library",
            "scala-parser-combinators_",
            "scala-reflect",
            "scala-swing_",
            "scala-xml",
            "lzma",
            "jopt-simple",
            "vecmath",
            "trove4j",
            "MercuriusUpdater",
            "netty",
            "oshi-core",
            "jna",
            "platform",
            "icu4j-core-mojang",
            "codecjorbis",
            "codecwav",
            "libraryjavasound",
            "librarylwjglopenal",
            "soundsystem",
            @"netty-all.*.Final",
            "guava",
            "commons-lang3",
            "commons-io",
            "commons-codec",
            "jinput",
            "jutils",
            "gson",
            "mojang.*authlib",
            "realms",
            "commons-compress",
            "httpclient",
            "commons-logging",
            "httpcore",
            @"fastutil.*_mojang",
            "log4j-api",
            "log4j-core",
            @"lwjgl.*nightly",
            @"lwjgl_util.*nightly",
        };

        private readonly string _javaExpArgs =
            " -XX:+DisableExplicitGC -XX:+UseParNewGC -XX:+ScavengeBeforeFullGC " +
            "-XX:+CMSScavengeBeforeRemark -XX:+UseNUMA -XX:+CMSParallelRemarkEnabled " +
            "-XX:MaxTenuringThreshold=15 -XX:MaxGCPauseMillis=30 -XX:GCPauseIntervalMillis=150 " +
            "-XX:+UseAdaptiveGCBoundary -XX:-UseGCOverheadLimit -XX:+UseBiasedLocking -XX:SurvivorRatio=8 " +
            "-XX:TargetSurvivorRatio=90 -XX:MaxTenuringThreshold=15 " +
            "-XX:+UseFastAccessorMethods -XX:+UseCompressedOops -XX:+OptimizeStringConcat -XX:+AggressiveOpts " +
            "-XX:+UseCodeCacheFlushing -XX:ParallelGCThreads=5 " +
            "-XX:ReservedCodeCacheSize={0}m -XX:SoftRefLRUPolicyMSPerMB={1} ";

        private string _version;
        private readonly string _accessToken;
        private string _assetsIndex;
        private string _libArgs;
        private int _minMemory = 1024;
        private int _maxMemory = 4096;
        private string _forgeLibPath;
        private string _versionForgePath;

        private string _natives;

        private string _name;
        private string uuid;

        private Size _size = new Size(925, 530);

        #endregion

        #region Properties

        public string MinecraftFolder { get; }

        #endregion

        #region Constructors

        public ForgeLaunchWorker(string folder)
        {
            _accessToken = Singleton.Instance.AssessToken;
            MinecraftFolder = folder;

            if (!FindVersionsExperimantal())
                return;

            Init();
        }

        public ForgeLaunchWorker(string folder, string version, string assetsIndex = null, string accessToken = null)
        {
            MinecraftFolder = folder;
            _version = version;
            _accessToken = accessToken;
            _assetsIndex = assetsIndex;

            Init();
        }

        private void Init()
        {
            FillLibsArguments();
            FindNativesPath();
            FindForgePaths();

            if (_assetsIndex == null)
            {
                ParseAssetIndex();
            }
        }

        #endregion

        #region Public methods

        public string GetLauncArgs()
        {
            var result = $"-Xmn{_minMemory}M -Xmx{_maxMemory}M -Djava.library.path=\"{_natives}\"";
            result += $" -cp \"{_forgeLibPath};{_libArgs};{_versionForgePath}\"";
            result += $" net.minecraft.launchwrapper.Launch" +
                      $" --username {_name}";
            result += $" --version {_version}";
            result += $" --gameDir {MinecraftFolder}";
            result += $" --assetsDir {Path.Combine(MinecraftFolder, "assets")}";
            result += " --tweakClass net.minecraftforge.fml.common.launcher.FMLTweaker";
            result += $" --width {_size.Width} --height {_size.Height}";
            result += $" --accessToken {_accessToken ?? "null"}";
            result += " --userProperties {}";
            result += $" --assetIndex {_assetsIndex}";
            result += " -Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true";

            if (Singleton.Instance.Config.GetConfig<SystemSettings>().UseSpecialJavaArgs)
            {
                result += _javaExpArgs;
            }

            return result;
        }

        public string GetCmdArgs()
        {
            return $"\"{Singleton.Instance.Config.GetConfig<SystemSettings>().JavaPath}\" " + GetLauncArgs();
        }

        #endregion

        #region Methods

        private void ParseAssetIndex()
        {
            var version = GetVersion(_version);
            if (version.Major != 0)
            {
                _assetsIndex = $"{version.Major}.{version.Minor}";
            }
        }

        private void FindForgePaths()
        {
            var path = Path.Combine(MinecraftFolder, "libraries", "net", "minecraftforge", "forge");
            if (!Directory.Exists(path))
                return;

            var dirs = Directory.GetDirectories(path).ToList();
            var version = new string(_version.Where(x => Regex.IsMatch(x.ToString(), "[0-9,\\.]")).ToArray()).Trim();

            var find = dirs.FirstOrDefault(x => x.Contains(version));
            if (find == null)
                return;

            var files = Directory.GetFiles(find, "*.jar");

            _forgeLibPath = files.FirstOrDefault();


            path = Path.Combine(MinecraftFolder, "versions", _version);
            files = Directory.GetFiles(path, "*.jar", SearchOption.TopDirectoryOnly);

            _versionForgePath = files.FirstOrDefault();
        }

        private void FindNativesPath()
        {
            if (!Directory.Exists(MinecraftFolder))
                return;

            var dirs = Directory.GetDirectories(Path.Combine(MinecraftFolder, "versions")).ToList();
            _natives = Path.Combine(dirs.FirstOrDefault(x => x.Contains(_version) || _version.Contains(x)), "natives");
        }

        /// <summary>
        /// Добавляем список либ
        /// </summary>
        private void FillLibsArguments()
        {
            if (!Directory.Exists(MinecraftFolder))
                return;

            var dir = new DirectoryInfo(Path.Combine(MinecraftFolder, "libraries"));
            if (!dir.Exists)
                return;

            var jars = Directory.GetFiles(dir.FullName, "*.jar", SearchOption.AllDirectories).ToList();
            var args = "";

            foreach (var lib in _libraries)
            {
                var all = jars.Where(x => Regex.IsMatch(x, lib)).ToList();

                // выбираем самое короткое имя
                if (all.Count() > 1)
                {
                    var shortest = all.Min(x => x.Length);
                    all = all.Where(x => x.Length == shortest).ToList();
                }

                // либо позднюю версию
                if (all.Count() > 1)
                {
                    var dict = all
                        .ToDictionary(x => x, GetVersion)
                        .OrderBy(x => x.Value)
                        .ToDictionary(x => x.Key, x => x.Value);

                    all = new List<string>
                    {
                        dict.LastOrDefault().Key
                    };
                }

                var find = all.FirstOrDefault();
                if (find == null)
                {
                    Trace.WriteLine("Can't find lib - " + lib);
                    continue;
                }

                args += find + ";";
                jars.Remove(find);
            }

            _libArgs = args;
        }

        /// <summary>
        /// Получаем версию из имени файла (Experimantal)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private Version GetVersion(string text)
        {
            var corrected = text.Replace("-", ".").Replace("—", ".").Trim(' ', '.');
            var versionRaw = new string(corrected.Where(x => Regex.IsMatch(x.ToString(), "[0-9, \\.]")).ToArray());

            var cutted = string.Join(".", versionRaw.Split('.').Take(4));

            if (Version.TryParse(cutted, out var result))
                return result;

            return new Version(0, 0, 0, 0);
        }

        private bool FindVersionsExperimantal()
        {
            // путь к нативным либам forge
            var path = Path.Combine(MinecraftFolder, "libraries", "net", "minecraftforge", "forge");

            // по идее, располагается только одна либа.
            var dirs = Directory.GetDirectories($"{path}\\", "*", SearchOption.TopDirectoryOnly);
            var forgeDir = dirs.FirstOrDefault();
            if (string.IsNullOrEmpty(forgeDir))
                return false;

            var version = GetVersion(Path.GetFileName(forgeDir));

            if (version.Major != 0)
            {
                _assetsIndex = $"{version.Major}.{version.Minor}";
                _version = $"Forge {_assetsIndex}.{version.Build}";
            }

            return true;
        }
        #endregion
    }
}
