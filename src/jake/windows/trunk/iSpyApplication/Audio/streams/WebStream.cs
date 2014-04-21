﻿using System;
using System.Net.Sockets;
using System.Threading;
using AForge.Video;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace iSpyApplication.Audio.streams
{
    class WebStream: IAudioSource
    {
        private Socket _socket;
        private float _gain;
        private bool _listening;
        private ManualResetEvent _stopEvent;

        private Thread _thread;

        private WaveFormat _recordingFormat;
        private BufferedWaveProvider _waveProvider;
        private SampleChannel _sampleChannel;

        public BufferedWaveProvider WaveOutProvider { get; set; }

        /// <summary>
        /// New frame event.
        /// </summary>
        /// 
        /// <remarks><para>Notifies clients about new available frame from audio source.</para>
        /// 
        /// <para><note>Since audio source may have multiple clients, each client is responsible for
        /// making a copy (cloning) of the passed audio frame, because the audio source disposes its
        /// own original copy after notifying of clients.</note></para>
        /// </remarks>
        /// 
        public event DataAvailableEventHandler DataAvailable;

        /// <summary>
        /// New frame event.
        /// </summary>
        /// 
        /// <remarks><para>Notifies clients about new available frame from audio source.</para>
        /// 
        /// <para><note>Since audio source may have multiple clients, each client is responsible for
        /// making a copy (cloning) of the passed audio frame, because the audio source disposes its
        /// own original copy after notifying of clients.</note></para>
        /// </remarks>
        /// 
        public event LevelChangedEventHandler LevelChanged;

        /// <summary>
        /// audio source error event.
        /// </summary>
        /// 
        /// <remarks>This event is used to notify clients about any type of errors occurred in
        /// audio source object, for example internal exceptions.</remarks>
        /// 
        //public event AudioSourceErrorEventHandler AudioSourceError;

        /// <summary>
        /// audio playing finished event.
        /// </summary>
        /// 
        /// <remarks><para>This event is used to notify clients that the audio playing has finished.</para>
        /// </remarks>
        /// 
        public event AudioFinishedEventHandler AudioFinished;

        /// <summary>
        /// audio source.
        /// </summary>
        /// 
        /// <remarks></remarks>
        /// 
        public virtual string Source
        {
            get { return _socket.ToString(); }
        }

        public float Gain
        {
            get { return _gain; }
            set
            {
                _gain = value;
                if (_sampleChannel != null)
                {
                    _sampleChannel.Volume = value;
                }
            }
        }

        public bool Listening
        {
            get
            {
                if (IsRunning && _listening)
                    return true;
                return false;

            }
            set
            {
                if (RecordingFormat == null)
                {
                    _listening = false;
                    return;
                }

                if (WaveOutProvider != null)
                {
                    if (WaveOutProvider.BufferedBytes>0) WaveOutProvider.ClearBuffer();
                    WaveOutProvider = null;
                }

                if (value)
                {
                    WaveOutProvider = new BufferedWaveProvider(RecordingFormat) {DiscardOnBufferOverflow = true};
                }
                
                _listening = value;
            }
        }


        /// <summary>
        /// State of the audio source.
        /// </summary>
        /// 
        /// <remarks>Current state of audio source object - running or not.</remarks>
        /// 
        public bool IsRunning
        {
            get
            {
                if (_thread != null)
                {
                    // check thread status
                    if (!_thread.Join(TimeSpan.Zero))
                        return true;

                    // the thread is not running, free resources
                    Free();
                }
                return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDeviceStream"/> class.
        /// </summary>
        /// 
        public WebStream()
        {
            

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDeviceStream"/> class.
        /// </summary>
        /// 
        /// <param name="skt">socket, which provides audio data.</param>
        /// 
        public WebStream(Socket skt)
        {
            _socket = skt;
        }

        /// <summary>
        /// Start audio source.
        /// </summary>
        /// 
        /// <remarks>Starts audio source and return execution to caller. audio source
        /// object creates background thread and notifies about new frames with the
        /// help of <see cref="DataAvailable"/> event.</remarks>
        /// 
        /// <exception cref="ArgumentException">audio source is not specified.</exception>
        /// 
        public void Start()
        {
            if (!IsRunning)
            {
                // check source
                if (_socket == null)
                    throw new ArgumentException("Audio source is not specified.");

                _waveProvider = new BufferedWaveProvider(RecordingFormat);
                _sampleChannel = new SampleChannel(_waveProvider);
                _sampleChannel.PreVolumeMeter += new EventHandler<StreamVolumeEventArgs>(_sampleChannel_PreVolumeMeter);

                _stopEvent = new ManualResetEvent(false);
                _thread = new Thread(WebStreamListener)
                                          {
                                              Name = "WebStream Audio Receiver"
                                          };
                _thread.Start();

            }
        }

        void _sampleChannel_PreVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
            if (LevelChanged != null)
                LevelChanged(this, new LevelChangedEventArgs(e.MaxSampleValues));
        }


        private void WebStreamListener()
        {
            try
            {
                var data = new byte[6400];
                if (_socket != null)
                {
                    while (!_stopEvent.WaitOne(0, false) && !MainForm.Reallyclose)
                    {
                        if (DataAvailable != null)
                        {
                            int recbytesize = _socket.Receive(data, 0, 6400,SocketFlags.None);

                            if (_sampleChannel != null)
                            {
                                _waveProvider.AddSamples(data, 0, recbytesize);

                                var sampleBuffer = new float[recbytesize];
                                _sampleChannel.Read(sampleBuffer, 0, recbytesize);

                                if (Listening && WaveOutProvider != null)
                                {
                                    WaveOutProvider.AddSamples(data, 0, recbytesize);
                                }
                                var da = new DataAvailableEventArgs((byte[])data.Clone());
                                DataAvailable(this, da);
                            }
                        }
                        else
                        {
                            break;
                        }
                        // need to stop ?
                        if (_stopEvent.WaitOne(0, false))
                            break;
                    }
                }

                if (AudioFinished != null)
                    AudioFinished(this, ReasonToFinishPlaying.StoppedByUser);
            }
            catch (Exception e)
            {
                //if (AudioSourceError!=null)
                //    AudioSourceError(this, new AudioSourceErrorEventArgs(e.Message));
                if (AudioFinished != null)
                    AudioFinished(this, ReasonToFinishPlaying.DeviceLost);
                MainForm.LogExceptionToFile(e);
            }
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }


        }

        public void WaitForStop()
        {
            if (_sampleChannel != null)
                _sampleChannel.PreVolumeMeter -= _sampleChannel_PreVolumeMeter;

            if (IsRunning)
            {                
                _stopEvent.Set();
                _thread.Join(MainForm.ThreadKillDelay);
                if (_thread != null && !_thread.Join(TimeSpan.Zero))
                    _thread.Abort();
                Free();



                if (_waveProvider != null && _waveProvider.BufferedBytes > 0)
                    _waveProvider.ClearBuffer();

                if (WaveOutProvider != null)
                    if (WaveOutProvider.BufferedBytes>0) WaveOutProvider.ClearBuffer();
            }
        }

        /// <summary>
        /// Stop audio source.
        /// </summary>
        /// 
        /// <remarks><para>Stops audio source.</para>
        /// </remarks>
        /// 
        public void Stop()
        {
            WaitForStop();
        }

        /// <summary>
        /// Free resource.
        /// </summary>
        /// 
        private void Free()
        {
            _thread = null;

            // release events
            if (_stopEvent != null)
            {
                _stopEvent.Close();
                _stopEvent.Dispose();
            }
            _stopEvent = null;
        }

        public WaveFormat RecordingFormat
        {
            get { return _recordingFormat; }
            set
            {
                _recordingFormat = value;
            }
        }
    }
}
