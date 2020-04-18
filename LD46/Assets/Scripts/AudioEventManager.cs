using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEventManager : MonoBehaviour
{
    [SerializeField]
    AudioEventPosition m_audioEventPrefab;

    public AudioEventPosition MakeAudioEvent(Vector2 position, float lifespan, AK.Wwise.Event wwiseEvent)
    {
        var audioEventObject = Instantiate<AudioEventPosition>(m_audioEventPrefab);
        audioEventObject.SetLifespan(lifespan);
        wwiseEvent.Post(audioEventObject.gameObject);
        return audioEventObject;
    }
}
