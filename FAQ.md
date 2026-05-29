# DummyDroidStubGen (DDSG) FAQ!

##### Q: Can I use a rooted device?
By default, DDSG blocks connections to rooted devices. It is strongly advised not to circumvent this limitation or attempt to use a rooted device with DDSG.

DDSG relies on Android Accessibility Services to bridge Personal and Work profiles. Granting these services provides an application with elevated permissions, including screen reading and on-screen UI interaction.

On a rooted device, Android’s core security model is compromised, allowing non system applications (potentially those that are unsigned and/or malicious) the ability to exploit these elevated services, in turn, bypassing Work Profile isolation. 

Because root access explicitly permits the elevation of processes, that would be flagged on a non-rooted device, this creates an attack surface where cross-profile data may be exposed. This risk is not in any way theoretical; it is a fundamental consequence associated with the circumvention of the Android User Space.