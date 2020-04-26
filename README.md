# Unity Android Vibrations

Simple class that may be used to create haptic effects in Unity for Android platform. 

In contrast to other possible solutions, this one utilizes AndroidJNI (Java Native Interface) to achieve much better performance.

Most solutions have poor performance due to allocations (including boxing) inside AndroidJavaObject/AndroidJavaClass helper classes.

One may have to exclude Odin Inspector related code if he/she does not own it. It was used for convenience only, thus code will work in the very same way without those attributes. 
