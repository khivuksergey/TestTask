using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestTask
{
    public class Program
    {
        /// <summary>
        /// Программа принимает на входе 2 пути до файлов.
        /// Анализирует в первом файле кол-во вхождений каждой буквы (регистрозависимо). Например А, б, Б, Г и т.д.
        /// Анализирует во втором файле кол-во вхождений парных букв (не регистрозависимо). Например АА, Оо, еЕ, тт и т.д.
        /// По окончанию работы - выводит данную статистику на экран.
        /// </summary>
        /// <param name="args">Первый параметр - путь до первого файла.
        /// Второй параметр - путь до второго файла.</param>
        static void Main(string[] args)
        {
            try
            {
                if (!File.Exists(args[0]) | !File.Exists(args[1]))
                {
                    Console.WriteLine("Одного или более файлов не существует");
                    Console.ReadKey();
                    return;
                }

                IReadOnlyStream inputStream1 = GetInputStream(args[0]);
                IReadOnlyStream inputStream2 = GetInputStream(args[1]);

                Console.WriteLine("Собираем статистику из загруженных файлов...");

                IList<LetterStats> singleLetterStats = FillSingleLetterStats(inputStream1);
                IList<LetterStats> doubleLetterStats = FillDoubleLetterStats(inputStream2);

                RemoveCharStatsByType(singleLetterStats, CharType.Consonants);
                RemoveCharStatsByType(doubleLetterStats, CharType.Vowel);

                Console.WriteLine("\nСтатистика частоты вхождения буквенных символов с учётом регистра:");
                PrintStatistic(singleLetterStats);

                Console.WriteLine("\nСтатистика частоты вхождения удвоенных букв без учёта регистра:");
                PrintStatistic(doubleLetterStats);

                inputStream1.Dispose();
                inputStream2.Dispose();

                Console.ReadKey();
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine($"Неверно введены аргументы командной строки.\n{e.Message}");
                Console.ReadKey();
            }
            
        }

        /// <summary>
        /// Ф-ция возвращает экземпляр потока с уже загруженным файлом для последующего посимвольного чтения.
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        /// <returns>Поток для последующего чтения.</returns>
        private static IReadOnlyStream GetInputStream(string fileFullPath)
        {
            return new ReadOnlyStream(fileFullPath);
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения каждой буквы.
        /// Статистика РЕГИСТРОЗАВИСИМАЯ!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillSingleLetterStats(IReadOnlyStream stream)
        {
            stream.ResetPositionToStart();
            IList<LetterStats> letterStats = new List<LetterStats>();
            while (!stream.IsEof)
            {
                char c = stream.ReadNextChar();
                if (char.IsLetter(c))
                {
                    var ls = new LetterStats { Letter = c.ToString() };
                    if (letterStats.Contains(ls))
                    {
                        IncStatistic(letterStats[letterStats.IndexOf(ls)]);
                    }
                    else
                    {
                        letterStats.Add(new LetterStats { Letter = c.ToString(), Count = 1 });
                    }
                }
            }
            return letterStats;
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения парных букв.
        /// В статистику должны попадать только пары из одинаковых букв, например АА, СС, УУ, ЕЕ и т.д.
        /// Статистика - НЕ регистрозависимая!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillDoubleLetterStats(IReadOnlyStream stream)
        {
            stream.ResetPositionToStart();
            IList<LetterStats> letterStats = new List<LetterStats>();
            char prevChar = '\0';
            while (!stream.IsEof)
            {
                char c = char.ToLower(stream.ReadNextChar());
                if (char.IsLetter(c))
                {
                    if (c == prevChar)
                    {
                        string doubleLetter = c.ToString() + c.ToString();
                        var ls = new LetterStats { Letter = doubleLetter };
                        if (letterStats.Contains(ls))
                        {
                            IncStatistic(letterStats[letterStats.IndexOf(ls)]);
                        }
                        else
                        {
                            letterStats.Add(new LetterStats { Letter = doubleLetter, Count = 1 });
                        }
                        // Один и тот же символ не будем учитывать дважды (в условии не было оговорено)
                        // Если для строки "aaa" нужно получить { аа : 2 }, то закомментировать следующую строку
                        prevChar = '\0';
                    }
                    else
                    {
                        prevChar = c;
                    }
                    
                }
            }
            return letterStats;
        }

        /// <summary>
        /// Функция проверяет, содержит ли элемент статистики гласные буквы.
        /// </summary>
        /// <param name="letterStats">Коллекция со статистиками вхождения букв/пар</param>
        private static bool IsVowel (LetterStats letterStats)
        {
            // Будем считать, что латинская "y" - гласная (в условии не было оговорено)
            // Иначе, если во входном файле осмысленный текст, то нужно анализировать расположение буквы "y" в словах
            return "aeiouyAEIOUYаеёиоуыэюяАЕЁИОУЫЭЮЯ".IndexOf(letterStats.Letter.First()) >= 0 ? true : false;
        }

        /// <summary>
        /// Функция проверяет, содержит ли элемент статистики согласные буквы.
        /// </summary>
        /// <param name="letterStats">Коллекция со статистиками вхождения букв/пар</param>
        private static bool IsConsonant(LetterStats letterStats)
        {
            // Мягкий и твёрдый знаки считаюся как буквы, но не являются гласными или согласными, так как это диакритические знаки
            return "bcdfghjklmnpqrstvwxzBCDFGHJKLMNPQRSTVWXZбвгджзйклмнпрстфхцчшщБВГДЖЗЙКЛМНПРСТФХЦЧШЩ".IndexOf(letterStats.Letter.First()) >= 0 ? true : false;
        }

        /// <summary>
        /// Ф-ция перебирает все найденные буквы/парные буквы, содержащие в себе только гласные или согласные буквы.
        /// (Тип букв для перебора определяется параметром charType)
        /// Все найденные буквы/пары соответствующие параметру поиска - удаляются из переданной коллекции статистик.
        /// </summary>
        /// <param name="letters">Коллекция со статистиками вхождения букв/пар</param>
        /// <param name="charType">Тип букв для анализа</param>
        private static void RemoveCharStatsByType(IList<LetterStats> letters, CharType charType)
        {
            switch (charType)
            {
                case CharType.Consonants:
                    for (var i = 0; i < letters.Count; i++)
                    {
                        if (IsConsonant(letters[i]))
                        {
                            letters.RemoveAt(i);
                            i--;
                        }
                    }
                    break;
                case CharType.Vowel:
                    for (var i = 0; i < letters.Count; i++)
                    {
                        if (IsVowel(letters[i]))
                        {
                            letters.RemoveAt(i);
                            i--;
                        }
                    }                    
                    break;
            }
        }

        /// <summary>
        /// Ф-ция выводит на экран полученную статистику в формате "{Буква} : {Кол-во}"
        /// Каждая буква - с новой строки.
        /// Выводить на экран необходимо предварительно отсортировав набор по алфавиту.
        /// В конце отдельная строчка с ИТОГО, содержащая в себе общее кол-во найденных букв/пар
        /// </summary>
        /// <param name="letters">Коллекция со статистикой</param>
        private static void PrintStatistic(IEnumerable<LetterStats> letters)
        {
            if (!letters.Any())
            {
                Console.WriteLine("В прочитанном файле условие не выполнилось");
                return;
            }
            var total = 0;
            letters = letters.OrderBy(s => s.Letter);
            foreach (var l in letters)
            {
                Console.WriteLine($"{l.Letter} : {l.Count}");
                total += l.Count;
            }
            Console.WriteLine($"ИТОГО НАЙДЕНО: {total} букв/пар");
        }

        /// <summary>
        /// Метод увеличивает счётчик вхождений по переданной структуре.
        /// </summary>
        /// <param name="letterStats"></param>
        private static void IncStatistic(LetterStats letterStats)
        {
            letterStats.Count++;
        }
    }
}
