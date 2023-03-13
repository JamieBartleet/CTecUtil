using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil.IO
{
    public class Sounds
    {
        public delegate void TonePlayingStartNotifier(bool isPlaying);
        public TonePlayingStartNotifier TonePlaying;


        public class ToneDef
        {
            public ToneDef(int frequency, int duration) { Frequency = frequency; Duration = duration; }
            public int Frequency;
            public int Duration;
        }


        public async void StartTone(List<ToneDef> tone, bool repeat)
        {
            if (_tonePlayer is not null)
                await Task.Run(new Action(() => StopTone()));
                
            TonePlaying?.Invoke(true);

            _toneList = tone;
            _repeat = repeat;

            _tonePlayer = new BackgroundWorker();
            _tonePlayer.DoWork += tonePlayer_DoWork;
            _tonePlayer.RunWorkerCompleted += tonePlayer_RunWorkerCompleted;
            _tonePlayer.WorkerReportsProgress = true;
            _tonePlayer.WorkerSupportsCancellation = true;
            _tonePlayer.RunWorkerAsync();
        }


        public async void StopTone()
        {
            await Task.Run(new Action(() => _tonePlayer?.CancelAsync()));
            _repeat = false;
            _toneList = new();
            _tonePlayer = null;
            TonePlaying?.Invoke(false);
        }


        private List<ToneDef> _toneList;
        private bool _repeat;


        private BackgroundWorker _tonePlayer;

        private void tonePlayer_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            do
            {
                foreach (var t in _toneList)
                {
                    if (t.Frequency >= 37 && t.Frequency <= 32767)
                        Console.Beep(t.Frequency, t.Duration);
                    else
                        System.Threading.Thread.Sleep(t.Duration);
                }

            } while (_repeat);
        }

        private void tonePlayer_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            TonePlaying?.Invoke(false);
        }

    }
}
