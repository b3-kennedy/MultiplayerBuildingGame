using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AudioManager : NetworkBehaviour
{

    public static AudioManager Instance;

    
    public List<AudioClip> audioClips = new List<AudioClip>();

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAudio(int index, float x, float y, float z)
    {
        AudioSource.PlayClipAtPoint(audioClips[index], new Vector3(x, y, z));
        PlayAudioServerRpc(index, x, y, z, NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    void PlayAudioServerRpc(int index, float x, float y, float z, ulong clientId)
    {
        PlayAudioClientRpc(index, x, y, z, clientId);
    }

    [ClientRpc]
    void PlayAudioClientRpc(int index, float x, float y, float z, ulong clientId)
    {
        if(NetworkManager.Singleton.LocalClientId != clientId)
        {
            AudioSource.PlayClipAtPoint(audioClips[index], new Vector3(x, y, z));
        }
        
    }
}
