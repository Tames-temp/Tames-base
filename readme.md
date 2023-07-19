Hello!

Tames is a Unity template project that allows management and planning of associative and interactive elements (targeting architectural design) without any programming skills. The name Tames is a bit lame, I would admit. It was chosen in early stages when its main class was called TameObject, i.e. a tamed GameObject (the class is still the main class for controlling 3D elements). A manual is available in the Manual folder. Tames has been constantly tested for desktop visualisation, but there may be issues in VR. Currently the multiuser feature is disabled after an extensive change of data structures and interaction logics.

After a presentation test at RIS eCAADe in Tallinn, I found adding slide-style presentation worthwhile in the last version. For some reason, I added a basic game-like scoring system (with this in mind that this system can be used for wayfinding research).  

To use Tames you need to first install Unity (version 2021.3.20f or later). Please see here: https://unity.com/download 

Tames's YouTube channel: https://www.youtube.com/channel/UCJYqvUKr7L0AjuitMkR70Mw

- NOT TO DO: currently Tames is available as a template (not a package). Therefore, you should NOT create a new project in Unity Hub.
- TO DO:
1. Copy Tames' root folder in your desired location.  
2. Rename the copied folder. 
3. Open it in Unity Hub.

IMPORTANT (constantly pay attention when working with Tames):  
1. You should NEVER move the camera. If you want to position the camera, move and rotate its ultimate parent (XRRig).
2. Only objects are considered as interactive or dynamic that are descnedants of a root object in the scene that has a Marker Root component (with Active field checked)
3. I recommend to always separately create walkable surfaces so that you exactly know where the camera can move to. 
4. If you include an interaction area or pathed element, always have the Read/Write checked for its prefab (see the manual).

Acknowledgment:

- Tames is a project created as part of my work at Department of Modelling, Faculty of Architecture, Czech Technical University in Prague, Czechia.
- Tames uses the networking toolkit RipTide created by Tom Weiland (from https://github.com/RiptideNetworking/Riptide, see https://www.youtube.com/@tomweiland)
- This toolkit was developed independent of Toggle Toolkit (developed at Masaryk University: see https://link.springer.com/article/10.3758/s13428-020-01510-4), but nevertheless shared a similar initial concern of an research-orientated interactive toolkit for non-programmer researchers. 


Thanks again for your time and interest,

Peiman 