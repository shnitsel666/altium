using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using AltiumTestConsole.Const;

namespace AltiumTestConsole.Seeder
{
    public class Seeder3
    {
        private static ThreadLocal<Random> _random = new(() => new Random(Guid.NewGuid().GetHashCode()));
        private readonly int _bufferSize;
        private readonly long _maxFileSize;
        private readonly long _consoleLogInterval;
        private readonly bool _disableProcessingLogs;
        private readonly byte[][] _wordBytesDictionary;
        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly byte[] _newLineBytes;
        private readonly int _maxStringLength;

        #region .ctor
        public Seeder3()
        {
            _bufferSize = APPLICATION_CONST.SEEDER_PARAMS.BUFFER_SIZE;
            _maxFileSize = APPLICATION_CONST.SEEDER_PARAMS.MAX_FILE_LIMIT_SIZE_BYTES;
            _consoleLogInterval = APPLICATION_CONST.SEEDER_PARAMS.CONSOLE_LOG_INTERVAL;
            _disableProcessingLogs = APPLICATION_CONST.SEEDER_PARAMS.DISABLE_PROCESSING_LOGS;

            // Преобразование слов в байты
            _wordBytesDictionary = APPLICATION_CONST.SEEDER_WORDS_DICTIONARY.WORDS_DICTIONARY
                .Select(word => _encoding.GetBytes(word))
                .ToArray();

            _newLineBytes = _encoding.GetBytes(Environment.NewLine);

            // Максимальная длина строки
            _maxStringLength = APPLICATION_CONST.SEEDER_PARAMS.MAX_STRING_LENGTH;
        }
        #endregion

        #region File generators
        // Оптимизированный метод с использованием параллелизма и улучшенного алгоритма генерации случайных строк
        public void MakeFileSync_Optimized()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string filePath = CreateAndReturnFilePath(APPLICATION_CONST.SEEDER_PARAMS.FILE_NAME_7);
            long currentSize = 0;
            int lineCount = 0;
            int numCores = Environment.ProcessorCount;
            var finishedThreads = new bool[numCores];

            var arrayPool = ArrayPool<byte>.Shared;
            var queue = new ConcurrentQueue<(byte[], int)>();

            var tasks = new Task[numCores + 1];
            
            // Writer task
            tasks[0] = Task.Run(() =>
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, _bufferSize))
                {
                    while (currentSize < _maxFileSize || !queue.IsEmpty || !AllThreadsFinished(finishedThreads))
                    {
                        if (queue.TryDequeue(out var item))
                        {
                            fs.Write(item.Item1, 0, item.Item2);
                            arrayPool.Return(item.Item1, true);
                        }
                    }
                }
            });

            // Generator tasks
            for (int i = 1; i <= numCores; i++)
            {
                int localIndex = i - 1;
                tasks[i] = Task.Run(() =>
                {
                    while (true)
                    {
                        byte[] buffer = arrayPool.Rent(_bufferSize);
                        int bufferPos = 0;
                        long localSize = 0;

                        while (bufferPos < _bufferSize && currentSize + localSize < _maxFileSize)
                        {
                            byte[] randomLineBytes = GenerateRandomBytesFast();
                            if (bufferPos + randomLineBytes.Length + _newLineBytes.Length > _bufferSize)
                            {
                                break;
                            }

                            Buffer.BlockCopy(randomLineBytes, 0, buffer, bufferPos, randomLineBytes.Length);
                            bufferPos += randomLineBytes.Length;

                            Buffer.BlockCopy(_newLineBytes, 0, buffer, bufferPos, _newLineBytes.Length);
                            bufferPos += _newLineBytes.Length;

                            localSize += randomLineBytes.Length + _newLineBytes.Length;
                            lineCount++;
                        }

                        if (bufferPos > 0)
                        {
                            queue.Enqueue((buffer, bufferPos));
                        }
                        else
                        {
                            arrayPool.Return(buffer, true);
                        }

                        if (currentSize + localSize >= _maxFileSize)
                        {
                            Interlocked.Add(ref currentSize, localSize);
                            break;
                        }

                        Interlocked.Add(ref currentSize, localSize);

                        if (lineCount % _consoleLogInterval == 0 && !_disableProcessingLogs)
                        {
                            Console.WriteLine($"Line number: {lineCount}, time elapsed: {stopwatch.Elapsed}");
                        }
                    }

                    finishedThreads[localIndex] = true;
                });
            }

            Task.WaitAll(tasks);

            stopwatch.Stop();
            Console.WriteLine($"MakeFileSync_Optimized() File was generated. Total size: {currentSize}, time elapsed: {stopwatch.Elapsed}");
            Console.WriteLine($"File path: {filePath}");
        }

        private bool AllThreadsFinished(bool[] finishedThreads)
        {
            foreach (var finished in finishedThreads)
            {
                if (!finished) return false;
            }
            return true;
        }
        #endregion

        #region File generators helpers
        private string CreateAndReturnFilePath(string fileName)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetParent(currentDirectory).Parent.FullName;
            string filePath = Path.Combine(projectDirectory, APPLICATION_CONST.SEEDER_PARAMS.FILE_FOLDER_NAME, fileName);
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return filePath;
        }

        private int GenerateRandomNumber(int min, int max)
        {
            return _random.Value.Next(min, max + 1);
        }

        private byte[] GenerateRandomBytesFast()
        {
            int randomId = GenerateRandomNumber(APPLICATION_CONST.SEEDER_PARAMS.MIN_STRINGS_ID, APPLICATION_CONST.SEEDER_PARAMS.MAX_STRINGS_ID);
            string randomIdStr = randomId.ToString();
            byte[] randomIdBytes = _encoding.GetBytes(randomIdStr); // Используем UTF-8 для преобразования чисел
            byte[] dotBytes = _encoding.GetBytes(".");
            byte[] randomWordBytes = GetRandomSeederWordBytes();

            int totalLength = randomIdBytes.Length + dotBytes.Length + randomWordBytes.Length;
            byte[] randomLineBytes = new byte[totalLength];

            Buffer.BlockCopy(randomIdBytes, 0, randomLineBytes, 0, randomIdBytes.Length);
            Buffer.BlockCopy(dotBytes, 0, randomLineBytes, randomIdBytes.Length, dotBytes.Length);
            Buffer.BlockCopy(randomWordBytes, 0, randomLineBytes, randomIdBytes.Length + dotBytes.Length, randomWordBytes.Length);

            return randomLineBytes;
        }

        private byte[] GetRandomSeederWordBytes()
        {
            int randomWordId = GenerateRandomNumber(0, _wordBytesDictionary.Length - 1);
            return _wordBytesDictionary[randomWordId];
        }
        #endregion
    }
}
