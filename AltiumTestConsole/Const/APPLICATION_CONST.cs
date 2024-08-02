using System.Collections.Generic;

namespace AltiumTestConsole.Const;

public static class APPLICATION_CONST
{
    public static class SEEDER_PARAMS
    {
        public const string FILE_NAME_1 = "input1.txt";
        public const string FILE_NAME_2 = "input2.txt";
        public const string FILE_NAME_3 = "input3.txt";
        public const string FILE_NAME_4 = "input4.txt";
        public const string FILE_NAME_5 = "input5.txt";
        public const string FILE_NAME_6 = "input6.txt";
        public const string FILE_NAME_7 = "input7.txt";
        public const string FILE_FOLDER_NAME = "Input";
        public const int MIN_STRINGS_ID = 1;
        public const int MAX_STRINGS_ID = 10000000;
        public const bool DISABLE_PROCESSING_LOGS = false;
        // public const int CONSOLE_LOG_INTERVAL = 20;
        public const int CONSOLE_LOG_INTERVAL = 2000000;
        public const int BUFFER_SIZE = 1024 * 1024; // 1 MB
        public const int MAX_STRING_LENGTH = 50;
        //public const long MAX_FILE_LIMIT_SIZE_BYTES = 15L * 1024 * 1024 * 1024; // 15GB
        //public const long MAX_FILE_LIMIT_SIZE_BYTES = 5L * 1024 * 1024 * 1024; // 5GB
        public const long MAX_FILE_LIMIT_SIZE_BYTES = 2L * 1024 * 1024 * 1024; // 2GB
        //public const long MAX_FILE_LIMIT_SIZE_BYTES = 512 * 1024 * 1024; // 512MB
        //public const long MAX_FILE_LIMIT_SIZE_BYTES = 128 * 1024 * 1024; // 128MB
    }
    public static class SEEDER_WORDS_DICTIONARY
    {
        public static List<string> WORDS_DICTIONARY = new ()
        {
            "Apple",
            "Apricot",
            "Avocado",
            "Banana",
            "Coconut",
            "Fig",
            "Kiwi",
            "Lemon",
            "Lime",
            "Mango",
            "Nectarine",
            "Orange",
            "Papaya",
            "Passion fruit",
            "Pear",
            "Pineapple",
            "Plum",
            "Quince",
            "Grapefruit",
            "Honeydew",
            "Dragon fruit",
            "Cantaloupe",
            "Pomegranate",
            "Persimmon",
            "Tangerine",
        };
    }
}