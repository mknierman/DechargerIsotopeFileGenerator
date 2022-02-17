using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Specialized;
using Science.Chemistry;
using MassSpectrometry;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Xml.Schema;
//using System.Web.Script.Serialization;
using System.IO;
using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;


namespace DechargerIsotopeFileGenerator
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var maxMass = 60000;
            var interval = 10;
            var prlop = new ParallelOptions();
            prlop.MaxDegreeOfParallelism = Environment.ProcessorCount;

            ConcurrentDictionary<int, double[]> cache = new ConcurrentDictionary<int, double[]>();

            Parallel.For(1, prlop.MaxDegreeOfParallelism + 1, prlop, index =>
            {
                for (int i = index; i <= maxMass / interval; i += prlop.MaxDegreeOfParallelism)
                {

                    var ion = GenerateAveragine(i * interval);
                    var pattern = MassSpectrometry.IsotopeCalc.CalcIsotopePeaks(ion, 1f);
                    var isoPattern = pattern.Select(p => (double)p.Value).ToArray();
                    cache.TryAdd(i * interval, isoPattern);
                }
            });

            //Serilaize object -- Save the cache for quick load next time.
            FileStream fs;
            var cacheFileName = "d:\\temp\\isotopeCache.dat";
            using (fs = new FileStream(cacheFileName, FileMode.Create))
            {
                try
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(fs, cache);
                }
                catch (SerializationException ex)
                {
                    // failing to serialize is not fatal, just slows startup performance.  So don't throw anything.
                    Debug.Print(ex.Message);
                }
            }

        }
            private static IIon GenerateAveragine(double mw)
            {
            // for proteins and peptides

            float AveragineMW = 111.1254f;
            float AverageC = 4.9384f;
            float AverageH = 7.7583f;
            float AverageN = 1.3577f;
            float AverageO = 1.4773f;
            float AverageS = 0.0417f;

            var roundNumAveragine = (int)Math.Round(mw / AveragineMW, 0);

            // If rounded to 0 set to 1  this keeps isotopes for peaks < 0.5 * averagineMW
            if (roundNumAveragine == 0) roundNumAveragine = 1;


            // Example: C(644) H(1012) N(177) O(193) S(5)
            var formula = "C(" + Math.Round(AverageC * roundNumAveragine, 0)
                      + ") H(" + Math.Round(AverageH * roundNumAveragine, 0)
                      + ") N(" + Math.Round(AverageN * roundNumAveragine, 0)
                      + ") O(" + Math.Round(AverageO * roundNumAveragine, 0)
                      + ") S(" + Math.Round(AverageS * roundNumAveragine, 0) + ")";

            // for DNA oligos

            //float AveragineMW = 305.8335f;
            //    float AverageC = 9.75f;
            //    float AverageH = 12.30f;
            //    float AverageN = 3.75f;
            //    float AverageO = 5.90f;
            //    float AverageP = 0.95f;

            //    var roundNumAveragine = (int)Math.Round(mw / AveragineMW, 0);

            //    // If rounded to 0 set to 1  this keeps isotopes for peaks < 0.5 * averagineMW
            //    if (roundNumAveragine == 0) roundNumAveragine = 1;
            

            //    // Example: C(644) H(1012) N(177) O(193) S(5)
            //    var formula = "C(" + Math.Round(AverageC * roundNumAveragine, 0)
            //              + ") H(" + Math.Round(AverageH * roundNumAveragine, 0)
            //              + ") N(" + Math.Round(AverageN * roundNumAveragine, 0)
            //              + ") O(" + Math.Round(AverageO * roundNumAveragine, 0)
            //              + ") P(" + Math.Round(AverageP * roundNumAveragine, 0) + ")";


                return new Ion(formula, -1);


             
            }


        }

    
}
