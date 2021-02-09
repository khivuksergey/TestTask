using System;
using System.IO;

namespace TestTask
{
    public class ReadOnlyStream : IReadOnlyStream
    {
        
        private readonly Stream _localStream;
        public StreamReader ReadStream;

        /// <summary>
        /// Конструктор класса. 
        /// Т.к. происходит прямая работа с файлом, необходимо 
        /// обеспечить ГАРАНТИРОВАННОЕ закрытие файла после окончания работы с таковым!
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        public ReadOnlyStream(string fileFullPath)
        {
            ReadStream = new StreamReader(fileFullPath);
            _localStream = ReadStream.BaseStream;
        }

        /// <summary>
        /// Метод для вывода содержимого потока в строковом виде
        /// </summary>
        public override string ToString()
        {
            using (var sr = new StreamReader(_localStream))
            {
                return sr.ReadToEnd();
            }
        }
                
        /// <summary>
        /// Флаг окончания файла.
        /// </summary>
        public bool IsEof
        {
            get
            {
                return ReadStream.EndOfStream;
            }
        }

        /// <summary>
        /// Ф-ция чтения следующего символа из потока.
        /// Если произведена попытка прочитать символ после достижения конца файла, метод 
        /// должен бросать соответствующее исключение
        /// </summary>
        /// <returns>Считанный символ.</returns>
        public char ReadNextChar()
        {
            if (ReadStream.Peek() != -1)
            {
                return Convert.ToChar(ReadStream.Read());
            }
            else
            {
                Console.WriteLine("End of stream");
                _localStream.Position = 0;
                ReadStream.DiscardBufferedData();
            }
            throw new EndOfStreamException();
        }


        /// <summary>
        /// Сбрасывает текущую позицию потока на начало.
        /// </summary>
        public void ResetPositionToStart()
        {
            if (_localStream == null)
            {
                return;
            }
            _localStream.Position = 0;
            ReadStream.DiscardBufferedData();
        }

        public void Dispose()
        {
            _localStream.Dispose();
            ReadStream.Dispose();
        }
    }
}
