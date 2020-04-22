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
    public class MyUdpServer : Debugger {
        private Thread _clientReceiveThread;
        private Thread _clientSendThread;
        private readonly Queue<QueueEntry> _datas = new Queue<QueueEntry> ();
        private readonly Queue<QueueEntry> _datasToSend = new Queue<QueueEntry> ();
        private UdpClient _socketConnection;

        private bool _stop;

        public void Start (string address, int port, bool debug = false) {
            try {
                this.debug = debug;
                _clientReceiveThread = new Thread (ListenForData);
                _clientReceiveThread.IsBackground = true;
                var parameters = new ThreadParameter (address, port);
                _clientReceiveThread.Start (parameters);

                var threadStart = new ThreadStart (SendDatas);
                _clientSendThread = new Thread (threadStart);
                _clientSendThread.IsBackground = true;
                _clientSendThread.Start ();
            } catch (Exception e) {
                LogError (e);
            }
        }

        public IEnumerator GetDatas (Action<string, IPEndPoint> callback) {
            while (!_stop) {
                yield return new WaitForEndOfFrame ();
                if (_datas.Count > 0) {
                    var d = _datas.Dequeue ();
                    callback (d.data, d.ep);
                }
            }
        }

        public void Stop () {
            Log ("Stop udp server");
            _stop = true;
        }

        private void ListenForData (object parameter) {
            try {
                var par = (ThreadParameter) parameter;
                _socketConnection = new UdpClient (par.port);
                Log ("Server created listening on port " + par.port + " in UDP");
                while (true)
                    try {
                        var ep = new IPEndPoint (IPAddress.Any, 0);
                        var data = _socketConnection.Receive (ref ep);
                        _datas.Enqueue (new QueueEntry { data = Encoding.ASCII.GetString (data), ep = ep });
                    }
                catch (ThreadAbortException e) {
                    LogError (e);
                } catch (Exception e) {
                    LogError (e);
                }
            } catch (ThreadAbortException e) {
                LogError (e);
            } catch (Exception e) {
                LogError (e);
            } finally {
                _socketConnection.Close ();
            }
        }

        private void SendDatas () {
            while (!_stop)
                if (_datasToSend.Count > 0 && _socketConnection != null)
                    try {
                        var entry = _datasToSend.Dequeue ();
                        var bytes = Encoding.ASCII.GetBytes (entry.data);
                        _socketConnection.Send (bytes, bytes.Length, entry.ep);
                    }
            catch (ThreadAbortException e) {
                LogError (e);
            } catch (Exception e) {
                LogError (e);
            }
        }

        public void Send (object datas, IPEndPoint ep) {
            _datasToSend.Enqueue (new QueueEntry { data = JsonUtility.ToJson (datas), ep = ep });
        }

        public struct ThreadParameter {
            public string address;
            public int port;

            public ThreadParameter (string address, int port) {
                this.address = address;
                this.port = port;
            }
        }

        public struct QueueEntry {
            public string data;
            public IPEndPoint ep;
        }
    }
}