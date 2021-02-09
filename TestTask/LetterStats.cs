using System;

namespace TestTask
{
    /// <summary>
    /// Статистика вхождения буквы/пары букв
    /// </summary>
    public class LetterStats : IEquatable<LetterStats>
    {
        /// <summary>
        /// Буква/Пара букв для учёта статистики.
        /// </summary>
        public string Letter;

        /// <summary>
        /// Кол-во вхождений буквы/пары.
        /// </summary>
        public int Count;

        public bool Equals(LetterStats other)
        {
            if (other == null)
                return false;
            return (this.Letter.Equals(other.Letter)); ;
        }

        public override bool Equals(object other)
        {
            return this.Equals(other as LetterStats);
        }

        public override int GetHashCode()
        {
            return Convert.ToInt32(Letter);
        }

        public static bool operator ==(LetterStats a, LetterStats b)
        {
            return a.Letter.Equals(b?.Letter);
        }
        public static bool operator !=(LetterStats a, LetterStats b)
        {
            return !(a==b);
        }
    }
}
