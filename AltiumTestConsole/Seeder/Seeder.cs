using System.Buffers;
using System.Diagnostics;
using System.Text;
using AltiumTestConsole.Const;

namespace AltiumTestConsole.Seeder
{
    public class Seeder
    {
        private readonly Random _random = new();
        private readonly int _bufferSize;
        private readonly long _maxFileSize;
        private readonly long _consoleLogInterval;
        private readonly bool _disableProcessingLogs;
        private readonly byte[][] _wordBytesDictionary;
        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly byte[] _newLineBytes;

        #region .ctor
        public Seeder()
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
        }
        #endregion

        #region File generators
        // Total size: 2147483658, time elapsed: 00:00:15.2553845
        public void MakeFileSync_BytesDict()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string filePath = CreateAndReturnFilePath(APPLICATION_CONST.SEEDER_PARAMS.FILE_NAME_2);
            long currentSize = 0;
            int lineCount = 0;

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, _bufferSize))
            {
                while (currentSize < _maxFileSize)
                {
                    byte[] randomLineBytes = GenerateRandomBytes();
                    fs.Write(randomLineBytes, 0, randomLineBytes.Length);
                    fs.Write(_newLineBytes, 0, _newLineBytes.Length);
                    currentSize += randomLineBytes.Length + _newLineBytes.Length;
                    lineCount++;

                    if (lineCount % _consoleLogInterval == 0 && !_disableProcessingLogs)
                    {
                        Console.WriteLine($"Line number: {lineCount}, time elapsed: {stopwatch.Elapsed}");
                    }
                }

                fs.Flush();
            }

            stopwatch.Stop();
            Console.WriteLine($"MakeFileSync_BytesDict() File was generated. Total size: {currentSize}, time elapsed: {stopwatch.Elapsed}");
            Console.WriteLine($"File path: {filePath}");
        }

        // Total size: 2147483659, time elapsed: 00:00:18.9042647
        public void MakeFileSync_SB()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string filePath = CreateAndReturnFilePath(APPLICATION_CONST.SEEDER_PARAMS.FILE_NAME_1);
            long currentSize = 0;
            int lineCount = 0;

            StringBuilder sb = new StringBuilder(_bufferSize * 2);

            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8, _bufferSize))
            {
                while (currentSize < _maxFileSize)
                {
                    string randomLine = GenerateRandomString();
                    sb.AppendLine(randomLine);
                    currentSize += randomLine.Length + Environment.NewLine.Length;
                    lineCount++;

                    if (sb.Length >= _bufferSize)
                    {
                        sw.Write(sb.ToString());
                        sb.Clear();
                    }

                    if (lineCount % _consoleLogInterval == 0 && !_disableProcessingLogs)
                    {
                        Console.WriteLine($"Line number: {lineCount}, time elapsed: {stopwatch.Elapsed}");
                    }
                }
                if (sb.Length > 0)
                {
                    sw.Write(sb.ToString());
                }
                sw.Flush();
            }

            stopwatch.Stop();
            Console.WriteLine($"MakeFileSync_SB() File was generated. Total size: {currentSize}, time elapsed: {stopwatch.Elapsed}");
            Console.WriteLine($"File path: {filePath}");
        }

        // Новый метод без использования Span<byte>, с минимизацией операций
        public void MakeFileSync_Optimized()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string filePath = CreateAndReturnFilePath(APPLICATION_CONST.SEEDER_PARAMS.FILE_NAME_3);
            long currentSize = 0;
            int lineCount = 0;

            var arrayPool = ArrayPool<byte>.Shared;
            byte[] buffer = arrayPool.Rent(_bufferSize);

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, _bufferSize))
                {
                    while (currentSize < _maxFileSize)
                    {
                        int bufferPos = 0;
                        while (bufferPos < _bufferSize && currentSize < _maxFileSize)
                        {
                            byte[] randomLineBytes = GenerateRandomBytes();
                            if (bufferPos + randomLineBytes.Length + _newLineBytes.Length > _bufferSize)
                            {
                                break;
                            }

                            Buffer.BlockCopy(randomLineBytes, 0, buffer, bufferPos, randomLineBytes.Length);
                            bufferPos += randomLineBytes.Length;

                            Buffer.BlockCopy(_newLineBytes, 0, buffer, bufferPos, _newLineBytes.Length);
                            bufferPos += _newLineBytes.Length;

                            currentSize += randomLineBytes.Length + _newLineBytes.Length;
                            lineCount++;
                        }

                        fs.Write(buffer, 0, bufferPos);

                        if (lineCount % _consoleLogInterval == 0 && !_disableProcessingLogs)
                        {
                            Console.WriteLine($"Line number: {lineCount}, time elapsed: {stopwatch.Elapsed}");
                        }
                    }

                    fs.Flush();
                }
            }
            finally
            {
                arrayPool.Return(buffer);
            }

            stopwatch.Stop();
            Console.WriteLine($"MakeFileSync_Optimized() File was generated. Total size: {currentSize}, time elapsed: {stopwatch.Elapsed}");
            Console.WriteLine($"File path: {filePath}");
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

        public string GenerateRandomString()
        {
            int randomId = GenerateRandomNumber(APPLICATION_CONST.SEEDER_PARAMS.MIN_STRINGS_ID, APPLICATION_CONST.SEEDER_PARAMS.MAX_STRINGS_ID);
            return $"{randomId}.{GetRandomSeederWord()}";
        }

        private string GetRandomSeederWord()
        {
            int randomWordId = GenerateRandomNumber(0, _wordBytesDictionary.Length - 1);
            return Encoding.UTF8.GetString(_wordBytesDictionary[randomWordId]);
        }

        private int GenerateRandomNumber(int min, int max)
        {
            return _random.Next(min, max + 1);
        }

        private byte[] GenerateRandomBytes()
        {
            int randomId = GenerateRandomNumber(APPLICATION_CONST.SEEDER_PARAMS.MIN_STRINGS_ID, APPLICATION_CONST.SEEDER_PARAMS.MAX_STRINGS_ID);
            byte[] randomIdBytes = _encoding.GetBytes(randomId.ToString());
            byte[] dotBytes = _encoding.GetBytes(".");
            byte[] randomWordBytes = GetRandomSeederWordBytes();

            byte[] randomLineBytes = new byte[randomIdBytes.Length + dotBytes.Length + randomWordBytes.Length];
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
