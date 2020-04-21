using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SmallProdGame.Utils;
using UnityEngine;

namespace SmallProdGame.Networking.Default {
    public class MyUdpClient : Debugger {
        private Thread _clientReceiveThread;
        private readonly Queue<string> _datas = new Queue<string> ();
        private Func<bool> _isReady;
        private UdpClient _socketConnection;

        private bool _stop;

        public void Start (string address, int port, Func<bool> isReady, bool debug = false) {
            try {
                this.debug = debug;
                _isReady = isReady;
                _clientReceiveThread = new Thread (ListenForData);
                _clientReceiveThread.IsBackground = true;
                var parameters = new ThreadParameter (address, port);
                _clientReceiveThread.Start (parameters);
            } catch (Exception e) {
                LogError (e);
            }
        }

        public IEnumerator GetDatas (Action<string> callback) {
            while (!_stop) {
                yield return new WaitForEndOfFrame ();
                if (_datas.Count > 0 && _isReady ()) callback (_datas.Dequeue ());
            }
        }

        public void Stop () {
            _stop = true;
            _socketConnection.Close ();
            Log ("Stop UDP client");
            if (_clientReceiveThread.ThreadState == ThreadState.Running) _clientReceiveThread.Abort ();
        }

        private void ListenForData (object parameter) {
            try {
                var par = (ThreadParameter) parameter;
                _socketConnection = new UdpClient ();
                var ep = new IPEndPoint (IPAddress.Parse (par.address), par.port);
                Log ("Connected to " + par.address + " on port " + par.port + " in udp");
                _socketConnection.Connect (ep);
                while (true)
                    try {
                        var bytes = _socketConnection.Receive (ref ep);
                        _datas.Enqueue (Encoding.ASCII.GetString (bytes));
                    }
                catch (Exception e) {
                    LogError (e);
                }
            } catch (ThreadAbortException e) {
                LogError (e);
            } catch (Exception e) {
                LogError (e);
            }
        }

        public void Send (object datas) {
            try {
                var txt = JsonUtility.ToJson (datas);
                if (_socketConnection == null) return;
                var clientMessageAsByteArray = Encoding.ASCII.GetBytes (txt);

                _socketConnection.Send (clientMessageAsByteArray, clientMessageAsByteArray.Length);
            } catch (Exception e) {
                LogError (e);
            }
        }

        public struct ThreadParameter {
            public string address;
            public int port;

            public ThreadParameter (string address, int port) {
                this.address = address;
                this.port = port;
            }
        }
    }
}