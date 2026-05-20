using UnityEngine;
namespace SweetMatch.Systems
{
    // Oyuncunun kaçıncı level'da olduğunu, scene reload'lar arasında korur.
    public class LevelProgress
    {
        // Aktif level'ın 0-based index'i. Dizi erişimi için (levels[CurrentIndex]). Kullanıcıya gösterilirken +1 ("Level 1").
        private const string PrefKey = "SweetMatch.LevelIndex";
        private const string StartScreenSeenKey = "SweetMatch.HasSeenStartScreen";

        public static int CurrentIndex
        {
            get => Mathf.Max(0, PlayerPrefs.GetInt(PrefKey, 0));
            private set
            {
                PlayerPrefs.SetInt(PrefKey, Mathf.Max(0, value));
                PlayerPrefs.Save();
            }
        }

        // Start ekranı ömür boyu bir kez görünür; sadece Reset() onu tekrar gösterir.
        public static bool HasSeenStartScreen
        {
            get => PlayerPrefs.GetInt(StartScreenSeenKey, 0) == 1;
            private set
            {
                PlayerPrefs.SetInt(StartScreenSeenKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        //Bir sonraki level'a ilerle. maxIndex (dahil) üst sınırını aşmaz, son level'da çağrılırsa sabit kalır
        public static void Advance(int maxIndex)
        {
            if (CurrentIndex < maxIndex)
                CurrentIndex = CurrentIndex + 1;
        }
        public static void MarkStartScreenSeen()
        {
            HasSeenStartScreen = true;
        }

        // "Oyunu en baştan açmış gibi" — index + start screen flag birlikte sıfırlanır.
        public static void Reset()
        {
            CurrentIndex = 0;
            HasSeenStartScreen = false;
        }
    }
}