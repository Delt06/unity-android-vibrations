using System;
using System.Collections.Generic;
using UnityEngine;

namespace Effects
{
    public static class Vibration
    {
        private static readonly int Sdk;

        private static readonly AndroidJavaObject Vibrator;
        private static readonly IntPtr VibratorPtr;
        private static readonly IntPtr VibrateWithEffectMethodPtr;
        private static readonly IntPtr VibrateMethodPtr;
        private static readonly AndroidJavaClass VibrationEffectClass;

        static Vibration()
        {
            // Trick Unity into giving the App vibration permission when it builds.
            // This check will always be false, but the compiler doesn't know that.
            if (Application.isEditor) Handheld.Vibrate();

            if (!IsAndroid()) return;     
            
            using (var version = new AndroidJavaClass("android.os.Build$VERSION")) 
            {
                Sdk = version.GetStatic<int>("SDK_INT");
            }
            
            Vibrator = new AndroidJavaClass("com.unity3d.player.UnityPlayer")// Get the Unity Player.
                .GetStatic<AndroidJavaObject>("currentActivity")// Get the Current Activity from the Unity Player.
                ?.Call<AndroidJavaObject>("getSystemService", "vibrator");// Then get the Vibration Service from the Current Activity.

            if (Vibrator != null)
            {
                VibratorPtr = Vibrator.GetRawObject();
                
                VibrateWithEffectMethodPtr =
                    AndroidJNI.GetMethodID(Vibrator.GetRawClass(), "vibrate", "(Landroid/os/VibrationEffect;)V");

                VibrateMethodPtr = AndroidJNI.GetMethodID(Vibrator.GetRawClass(), "vibrate", "(J)V");
            }

            if (Sdk >= MinSdkForVibrationEffect)
            {
                VibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");    
            }
        }

        public static void Vibrate(long durationMs, int amplitude)
        {
            if (!IsAndroid()) return;

            if (Sdk >= MinSdkForVibrationEffect)
            { 
                VibrateWithEffect(durationMs, amplitude);
            }
            else
            {
                VibrateLegacy(durationMs);
            }
        }

        private static void VibrateWithEffect(long durationMs, int amplitude)
        {
            var effect = CreateVibrationEffect(durationMs, amplitude);
            AndroidJNI.CallVoidMethod(VibratorPtr, VibrateWithEffectMethodPtr, effect);
        }

        private static jvalue[] CreateVibrationEffect(long durationMs, int amplitude)
        {
            if (!IsAndroid()) return null;

            if (!VibrateEffectArgsCache.TryGetValue((durationMs, amplitude), out var args))
            {
                var effect = VibrationEffectClass?.CallStatic<AndroidJavaObject>("createOneShot", durationMs, amplitude);
                
                if (effect != null)
                    GlobalReferences.Add(effect);
                
                args = new[] {new jvalue {l = effect?.GetRawObject() ?? IntPtr.Zero}};
                VibrateEffectArgsCache[(durationMs, amplitude)] = args;
            }

            return args;
        }

        private static readonly IDictionary<(long duration, int amplitude), jvalue[]> VibrateEffectArgsCache = new Dictionary<(long duration, int amplitude), jvalue[]>();
        private static readonly IDictionary<long, jvalue[]> VibrateArgsCache = new Dictionary<long, jvalue[]>();
        private static readonly ISet<AndroidJavaObject> GlobalReferences = new HashSet<AndroidJavaObject>();

        private static void VibrateLegacy(long duration)
        {
            if (!IsAndroid()) return;
            
            if (!VibrateArgsCache.TryGetValue(duration, out var args))
            {
                args = new [] {new jvalue{j = duration}};
                VibrateArgsCache[duration] = args;
            }
            
            AndroidJNI.CallVoidMethod(VibratorPtr, VibrateMethodPtr, args);
        }


        private static bool IsAndroid() 
        {
#if UNITY_ANDROID && !UNITY_EDITOR
	        return true;
#else
            return false;
#endif
        }

        private const int MinSdkForVibrationEffect = 26;
    }
}