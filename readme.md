Hello!

Tames is a Unity template project that allows management and planning of associative and interactive elements (targeting architectural design) without any programming skills. The name Tames is a bit lame, I would admit. It was chosen in early stages when its main class was called TameObject, i.e. a tamed GameObject (the class is still the main class for controlling 3D elements). A manual is available in the Manual folder. Tames has been constantly tested for desktop visualisation, but there may be issues in VR. Currently the multiuser feature is disabled after an extensive change of data structures and interaction logics.

I have used the RipTide networking for the multiuser feature (from https://github.com/RiptideNetworking/Riptide).

I will try to frequently update videos on YouTube about Tames: 

To use Tames you need to first install Unity (version 2021.3.21 or later). Please see here: https://unity.com/download 

Tames's YouTube channel: https://www.youtube.com/channel/UCJYqvUKr7L0AjuitMkR70Mw

- NOT TO DO: currently Tames is available as a template (not a package). Therefore, you should NOT create a new project in Unity Hub.
- TO DO:
1. Copy Tames' root folder in your desired location.  
2. Rename the copied folder. 
3. Open it in Unity Hub.

IMPORTANT (constantly pay attention when working with Tames):  
1. You should NEVER move the camera. If you want to position the camera, move and rotate its parent (XRRig).
2. Only objects are considered as interactive or dynamic that are descnedants of a root object in the scene that has a Marker Root component (with Active field checked)
3. I recommend to always have separately created walkable surfaces so that you exactly know where the camera can move to. 
4. If you include an interaction area or pathed element, always have the Read/Write checked (see the manual)

Tames uses the networking toolkit RipTide created by Tom Weiland: https://www.youtube.com/@tomweiland 

Thanks again for your time and interest

Peiman 