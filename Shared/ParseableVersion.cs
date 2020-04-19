using System;
using System.Collections.Generic;
using System.Reflection;

namespace RDRNetworkShared
{
    public struct ParseableVersion : IComparable<ParseableVersion>
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Revision { get; set; }
        public int Build { get; set; }

        public ParseableVersion(int major, int minor, int build, int rev)
        {
            Major = major;
            Minor = minor;
            Revision = rev;
            Build = build;
        }

        public override string ToString()
        {
            return Major + "." + Minor + "." + Build + "." + Revision;
        }

        public int CompareTo(ParseableVersion right)
        {
            return CreateComparableInteger().CompareTo(right.CreateComparableInteger());
        }

        public ulong CreateComparableInteger()
        {
            return (ulong)((Revision) + (Build * Math.Pow(10, 4)) + (Minor * Math.Pow(10, 8)) + (Major * Math.Pow(10, 12)));
        }

        public static bool operator >(ParseableVersion left, ParseableVersion right)
        {
            return left.CreateComparableInteger() > right.CreateComparableInteger();
        }

        public static bool operator <(ParseableVersion left, ParseableVersion right)
        {
            return left.CreateComparableInteger() < right.CreateComparableInteger();
        }

        public ulong ToLong()
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes((ushort)Revision));
            bytes.AddRange(BitConverter.GetBytes((ushort)Build));
            bytes.AddRange(BitConverter.GetBytes((ushort)Minor));
            bytes.AddRange(BitConverter.GetBytes((ushort)Major));

            return BitConverter.ToUInt64(bytes.ToArray(), 0);
        }

        public static ParseableVersion FromLong(ulong version)
        {
            ushort rev = (ushort)(version & 0xFFFF);
            ushort build = (ushort)((version & 0xFFFF0000) >> 16);
            ushort minor = (ushort)((version & 0xFFFF00000000) >> 32);
            ushort major = (ushort)((version & 0xFFFF000000000000) >> 48);

            return new ParseableVersion(major, minor, rev, build);
        }

        public static ParseableVersion Parse(string version)
        {
            var split = version.Split('.');
            if (split.Length < 2) throw new ArgumentException("Argument version is in wrong format");

            var output = new ParseableVersion();
            output.Major = int.Parse(split[0]);
            output.Minor = int.Parse(split[1]);
            if (split.Length >= 3) output.Build = int.Parse(split[2]);
            if (split.Length >= 4) output.Revision = int.Parse(split[3]);
            return output;
        }

        public static ParseableVersion FromAssembly()
        {
            var ourVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return new ParseableVersion()
            {
                Major = ourVersion.Major,
                Minor = ourVersion.Minor,
                Revision = ourVersion.Revision,
                Build = ourVersion.Build,
            };
        }

        public static ParseableVersion FromAssembly(Assembly assembly)
        {
            var ourVersion = assembly.GetName().Version;
            return new ParseableVersion()
            {
                Major = ourVersion.Major,
                Minor = ourVersion.Minor,
                Revision = ourVersion.Revision,
                Build = ourVersion.Build,
            };
        }
    }
}
