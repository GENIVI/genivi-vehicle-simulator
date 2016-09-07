using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class DirectPlayer : MonoBehaviour
{

    List<SampleContainer> _samplesToPlay = new List<SampleContainer>(7);
    List<SampleContainer> _samplesToRemove = new List<SampleContainer>(7);

    public void PlayData(float[] data, int channel)
    {
        _samplesToPlay.Add(new SampleContainer(data, channel));
    }

    void OnAudioFilterRead(float[] data, int numChannels)
    {
        for (int i = 0; i < _samplesToPlay.Count; i++)
        {
            bool noMoreData;
            _samplesToPlay[i].FillBuffer(data, numChannels, out noMoreData);
            if (noMoreData)// all data has been fed in the buffer, remove
            {
                _samplesToRemove.Add(_samplesToPlay[i]); //remove carefully! cannot remove in list directly, messes up indexes.
            }
        }

        if (_samplesToRemove != null)
        {
            foreach (SampleContainer sample in _samplesToRemove)
            {
                _samplesToPlay.Remove(sample);
            }
            _samplesToRemove.Clear();
        }
    }

    class SampleContainer
    {
        float[] _data;
        int _channelNb;
        int _fromIndex;

        public SampleContainer(float[] data, int channelNb)
        {
            _data = data;
            _channelNb = channelNb;
            _fromIndex = 0;
        }

        public void FillBuffer(float[] buffer, int numChannels, out bool noMoreData)
        {
            noMoreData = false;
            if (_channelNb >= numChannels)
            {
                Debug.LogWarning("no such channel ");
                noMoreData = true;
                return;
            }
            int nbOfSamplesToCopy = buffer.Length / numChannels;

            if (nbOfSamplesToCopy > _data.Length - _fromIndex)
            {
                nbOfSamplesToCopy = _data.Length - _fromIndex;
                noMoreData = true;
            }

            for (int i = 0; i < nbOfSamplesToCopy; i++) //safer to not use System.Array.Copy because OnAudioFilterRead is on a seperate thread
            {
                buffer[i * numChannels + _channelNb] += _data[i + _fromIndex]; //+= to mix, can have two samples playing at once on the same channel, or add data to normal AudioSource audio.
            }

            _fromIndex += nbOfSamplesToCopy;
        }
    }
}
