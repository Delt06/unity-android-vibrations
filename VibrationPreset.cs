using Sirenix.OdinInspector;
using UnityEngine;

namespace Effects
{
    [CreateAssetMenu]
    public class VibrationPreset : ScriptableObject
    { 
        [SerializeField] private long _durationMs = 1000;
        [SerializeField, Range(-1, 255), ValidateInput(nameof(ValidateAmplitude))] private int _amplitude = -1;

        public void Play()
        {
            if (DebugEnabled)
                Debug.Log($"Vibration {name} has been played.", this);
            
            Vibration.Vibrate(_durationMs, _amplitude);
        }

        private static bool ValidateAmplitude(int amplitude, ref string message)
        {
            if (1 <= amplitude && amplitude <= 255) return true;
            if (amplitude == -1) return true;
            
            message = "Amplitude must belong to [0; 255] or be equal to -1.";
            return false;
        }

        [ShowInInspector, LabelText("Debug")]
        public static bool DebugEnabled
        {
            get => PlayerPrefs.GetInt(DebugField, 0) == 1;
            set => PlayerPrefs.SetInt(DebugField, value ? 1 : 0);
        }

        private const string DebugField = "Inspector_Debug_Vibration";
    }
}