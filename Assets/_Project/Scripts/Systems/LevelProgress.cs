using UnityEngine;
namespace SweetMatch.Systems
{
    // Oyuncunun kaçıncı level'da olduğunu, scene reload'lar arasında korur.
    public class LevelProgress
    {
        private const string PrefKey = "SweetMatch.LevelIndex";

        /// <summary>
        /// Aktif level'ın 0-based index'i. Dizi erişimi için (levels[CurrentIndex]).
        /// Kullanıcıya gösterilirken +1 ("Level 1").
        /// </summary>
        public static int CurrentIndex
        {
            get => Mathf.Max(0, PlayerPrefs.GetInt(PrefKey, 0));
            private set
            {
                PlayerPrefs.SetInt(PrefKey, Mathf.Max(0, value));
                PlayerPrefs.Save();
            }
        }

        //Bir sonraki level'a ilerle. maxIndex (dahil) üst sınırını aşmaz, son level'da çağrılırsa sabit kalır

        public static void Advance(int maxIndex)
        {
            if (CurrentIndex < maxIndex)
                CurrentIndex = CurrentIndex + 1;
        }
        public static void Reset()
        {
            CurrentIndex = 0;
        }
    }
}