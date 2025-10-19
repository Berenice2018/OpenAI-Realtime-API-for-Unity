using System;
using System.Collections;
using System.Threading.Tasks;
using CrazyMinnow.SALSA;
using UnityEngine;

namespace BC.Scripts.Postilion
{
    public class PostilionCtrl : MonoBehaviour
    {
        [SerializeField] private GameObject _talkingPostilion;
        
        private void OnDestroy()
        {
            var salsa = _talkingPostilion.transform.GetChild(0).GetComponent<Salsa>();
            salsa.audioSrc.Stop();
        }

        
        public void SetAudioFileForTalkingAndPlay(AudioClip clip)
        { 
            AssignAndPlayAudioClip(clip); // todo
        }
        
        public void StopPostillionAudio()
        {
            var salsa = _talkingPostilion.transform.GetChild(0).GetComponent<Salsa>();
            if (salsa)
            { 
                salsa.audioSrc.Stop();
            }
        }
        
        
        private void AssignAndPlayAudioClip(AudioClip audioClip)
        {
            var salsa = _talkingPostilion.transform.GetChild(0).GetComponent<Salsa>();
            if (salsa)
            {
                salsa.audioSrc.clip = audioClip;

                if (_talkingPostilion.activeInHierarchy)
                {
                    salsa.audioSrc.Stop();
                    salsa.audioSrc.Play();
                }
            }
        }
    }
}