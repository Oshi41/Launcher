using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.Devices;
using Newtonsoft.Json;

namespace Launcher.Config.Interfaces
{
    public interface ISystemSettings : IConfig
    {
        string JavaPath { get; }
        int MaxMemory { get; }
        int Memory { get; set; }
        bool UseSpecialJavaArgs { get; set; }
    }

    /// <summary>
    /// Системные настройки
    /// </summary>
    class SystemSettings : ISystemSettings
    {
        #region Serializable property

        /// <summary>
        /// Выделяемая память
        /// </summary>
        public int Memory { get; set; }

        /// <summary>
        /// Использовать аргументы Java
        /// </summary>
        public bool UseSpecialJavaArgs { get; set; }

        #endregion

        #region Non serialize property

        [JsonIgnore]
        public string JavaPath { get; private set; }

        [JsonIgnore]
        public int MaxMemory { get; private set; }

        #endregion

        public SystemSettings()
        {
            if (!FindJava64Path())
                FindJavaPath();

            var info = new ComputerInfo();
            // Даю 70% от возможной памяти
            MaxMemory = (int)(info.TotalPhysicalMemory / Math.Pow(2, 20) * 0.70);

            UseSpecialJavaArgs = false;
            Memory = 1512;
        }

        private void FindJavaPath()
        {
            var javaFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Java");

            var files = Directory.GetFiles(javaFolder, "java.exe", SearchOption.AllDirectories);

        }

        /// <summary>
        ///     Ищем путь к 64 bit Java
        /// </summary>
        private bool FindJava64Path()
        {
            var base64 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Java").Replace(" (x86)", "");

            if (!Directory.Exists(base64))
                return false;

            var allExe = Directory.GetFiles(base64, "javaw.exe", SearchOption.AllDirectories);
            if (allExe.Length == 0)
                return false;

            var allJavaw = allExe.Where(x => Regex.IsMatch(x, "jre.*bin")).ToList();
            if (!allJavaw.Any())
                return false;

            allJavaw.Sort();

            var last = allJavaw.LastOrDefault();

            JavaPath = last;
            return true;
        }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj is SystemSettings settings)
                return Equals(settings);

            return false;
        }

        protected bool Equals(SystemSettings other)
        {
            return Memory == other.Memory && UseSpecialJavaArgs == other.UseSpecialJavaArgs;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Memory * 397) ^ UseSpecialJavaArgs.GetHashCode();
            }
        }

        public static bool operator ==(SystemSettings left, SystemSettings right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SystemSettings left, SystemSettings right)
        {
            return !Equals(left, right);
        }


        #endregion
    }
}