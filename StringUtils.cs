/*
 *  Copyright 2012 Tamme Schichler <tammeschichler@googlemail.com>
 * 
 *  This file is part of TQ.ArcUtils.
 *
 *  TQ.ArcUtils is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  TQ.ArcUtils is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with TQ.ArcUtils.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TQ.ArcLib;

namespace TQ.ArcUtils
{
    public static class StringUtils
    {
        public static void MergeTextArcs(string mainArc, string fillerArc, string arcOut, out bool clean,
                                         bool verbose = false)
        {
            clean = true;

            using (FileStream arcFile1 = File.OpenRead(mainArc))
            using (FileStream arcFile2 = File.OpenRead(fillerArc))
            using (FileStream arcFileO = File.Create(arcOut))
            {
                var r1 = new ArcReader(arcFile1);
                var r2 = new ArcReader(arcFile2);
                var w = new ArcWriter(arcFileO, r1.Header, r1.Unknown1, r1.Unknown2);

                Dictionary<string, AssetInfo> fi1 =
                    r1.GetFileInfo().ToDictionary(x => r1.GetString(x.NameOffset, x.NameLength));
                Dictionary<string, AssetInfo> fi2 =
                    r2.GetFileInfo().ToDictionary(x => r2.GetString(x.NameOffset, x.NameLength));

                foreach (var f1 in fi1)
                {
                    if (string.IsNullOrEmpty(f1.Key))
                    {
                        if (verbose) Console.Write("0");
                        continue;
                    }
                    AssetInfo f2;
                    if (fi2.TryGetValue(f1.Key, out f2))
                    {
                        bool c;
                        MergeTextData(r1.GetBytes(f1.Value), r2.GetBytes(f1.Value), out c, verbose);
                        if (!c) clean = false;
                        fi2.Remove(f1.Key);
                    }

                    if (verbose) Console.Write("1");

                    w.WriteFile(f1.Key, r1.GetBytes(f1.Value), AssetInfo.StorageType.Uncompressed, f1.Value.Unknown1,
                                f1.Value.Unknown2, f1.Value.Unknown3);
                }
            }
        }

        private static void MergeTextData(byte[] td1, byte[] td2, out bool clean, bool verbose)
        {
            clean = true;
            Encoding enc = Encoding.GetEncoding(1252); //ANSI

            string t1 = enc.GetString(td1);
            string t2 = enc.GetString(td2);

            string[] tl1 = t1.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] tl2 = t2.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (string[] kv in tl1.Concat(tl2).Select(line => line.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)).Where(kv => kv.Length != 2))
            {
                if (verbose)
                {
                    Console.Write("d" + kv.Length);
                }
                clean = false;
            }

            throw new NotImplementedException();
        }
    }
}