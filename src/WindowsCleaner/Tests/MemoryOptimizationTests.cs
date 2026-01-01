using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsCleaner.Tests
{
    /// <summary>
    /// Tests de validation des optimisations mémoire (v1.0.8)
    /// </summary>
    public class MemoryOptimizationTests
    {
        private double _initialMemory;
        private double _peakMemory;
        private Stopwatch? _stopwatch;

        /// <summary>
        /// Initialise les métriques de test
        /// </summary>
        public void StartMemoryTest(string testName)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            _initialMemory = MemoryOptimizer.GetMemoryUsageMB();
            _peakMemory = _initialMemory;
            _stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"[{testName}] Mémoire initiale: {_initialMemory:F2} MB");
        }

        /// <summary>
        /// Termine le test et affiche les résultats
        /// </summary>
        public void EndMemoryTest(string testName)
        {
            _stopwatch?.Stop();
            var finalMemory = MemoryOptimizer.GetMemoryUsageMB();
            var memoryDelta = finalMemory - _initialMemory;
            var peakDelta = _peakMemory - _initialMemory;

            Console.WriteLine($"[{testName}] Mémoire finale: {finalMemory:F2} MB");
            Console.WriteLine($"[{testName}] Delta mémoire: {memoryDelta:F2} MB");
            Console.WriteLine($"[{testName}] Pic mémoire: {_peakMemory:F2} MB (delta: {peakDelta:F2} MB)");
            Console.WriteLine($"[{testName}] Temps exécution: {_stopwatch?.ElapsedMilliseconds ?? 0} ms");
            Console.WriteLine();
        }

        /// <summary>
        /// Teste l'énumération optimisée de fichiers
        /// </summary>
        public void TestOptimizedFileEnumeration()
        {
            StartMemoryTest("OptimizedFileEnumeration");

            try
            {
                int fileCount = 0;
                long totalSize = 0;

                foreach (var (path, size) in MemoryOptimizer.EnumerateFilesOptimized(
                    System.IO.Path.GetTempPath(),
                    "*",
                    System.IO.SearchOption.AllDirectories))
                {
                    fileCount++;
                    totalSize += size;
                    
                    var currentMemory = MemoryOptimizer.GetMemoryUsageMB();
                    if (currentMemory > _peakMemory)
                    {
                        _peakMemory = currentMemory;
                    }
                }

                Console.WriteLine($"  Fichiers énumérés: {fileCount}");
                Console.WriteLine($"  Taille totale: {totalSize / (1024.0 * 1024.0):F2} MB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Erreur: {ex.Message}");
            }

            EndMemoryTest("OptimizedFileEnumeration");
        }

        /// <summary>
        /// Teste le traitement par batch
        /// </summary>
        public void TestBatchProcessing()
        {
            StartMemoryTest("BatchProcessing");

            try
            {
                int batchCount = 0;
                int totalFiles = 0;

                MemoryOptimizer.ProcessFilesInBatches(
                    System.IO.Path.GetTempPath(),
                    batch =>
                    {
                        batchCount++;
                        foreach (var file in batch)
                        {
                            totalFiles++;
                        }
                        
                        var currentMemory = MemoryOptimizer.GetMemoryUsageMB();
                        if (currentMemory > _peakMemory)
                        {
                            _peakMemory = currentMemory;
                        }
                    },
                    batchSize: 5000);

                Console.WriteLine($"  Batches traités: {batchCount}");
                Console.WriteLine($"  Fichiers au total: {totalFiles}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Erreur: {ex.Message}");
            }

            EndMemoryTest("BatchProcessing");
        }

        /// <summary>
        /// Teste l'optimisation mémoire passive
        /// </summary>
        public void TestMemoryOptimization()
        {
            StartMemoryTest("MemoryOptimization");

            try
            {
                var before = MemoryOptimizer.GetMemoryUsageMB();
                MemoryOptimizer.OptimizeMemory(aggressive: true);
                var after = MemoryOptimizer.GetMemoryUsageMB();
                
                Console.WriteLine($"  Avant: {before:F2} MB");
                Console.WriteLine($"  Après: {after:F2} MB");
                Console.WriteLine($"  Récupéré: {(before - after):F2} MB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Erreur: {ex.Message}");
            }

            EndMemoryTest("MemoryOptimization");
        }

        /// <summary>
        /// Lance tous les tests
        /// </summary>
        public void RunAllTests()
        {
            Console.WriteLine("=== TESTS D'OPTIMISATION MÉMOIRE - v1.0.8 ===\n");

            TestOptimizedFileEnumeration();
            TestBatchProcessing();
            TestMemoryOptimization();

            Console.WriteLine("=== TOUS LES TESTS TERMINÉS ===");
        }
    }
}
