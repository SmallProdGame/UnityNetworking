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
    public class MyTcpClient : Debugger {
        /* ASYNC */
        private static readonly ManualResetEvent _connectDone = new ManualResetEvent (false);
        private static ManualResetEvent _sendDone = new ManualResetEvent (false);
        private static readonly ManualResetEvent _receiveDone = new ManualResetEvent (false);
        private string _address;
        private Thread _clientThread;

        /* PARAMETERS */
        private Action _onConnectionFail;
        private Action _onReady;
        private int _port;

        private readonly Queue<string> _receiveQueue = new Queue<string> ();
        private Socket _socketConnection;
        private bool _stop;

        public void Start (string address, int port, Action onready, Action onconnecttionfail, bool debug = true) {
            this.debug = debug;
            _onReady = onready;
            _onConnectionFail = onconnecttionfail;
            _address = address;
            _port = port;

            Connect ();
        }

        public void Reconnect () {
            if (_socketConnection == null) Connect ();
        }

        public IEnumerator GetDatas (Action<string> callback) {
            while (!_stop) {
                yield return new WaitForEndOfFrame ();
                if (_receiveQueue.Count > 0) callback (_receiveQueue.Dequeue ());
            }
        }

        public void Stop () {
            Log ("Stop tcp client");
            _stop = true;
            _clientThread.Interrupt ();
        }

        public void Send (object datas) {
            try {
                var txt = JsonUtility.ToJson (datas);
                if (_socketConnection == null) return;
                var clientMessageAsByteArray = Encoding.ASCII.GetBytes (txt);
                _socketConnection.Send (clientMessageAsByteArray);
            } catch (Exception e) {
                LogError (e);
            }
        }

        private void Connect () {
            try {
                _clientThread = new Thread (ClientThread);
                _clientThread.IsBackground = true;
                _clientThread.Start ();
            } catch (Exception e) {
                LogError (e);
            }
        }

        private void ClientThread () {
            try {
                var ipHostInfo = Dns.GetHostEntry (_address);
                var iPAddress = ipHostInfo.AddressList[0];
                var remoteEp = new IPEndPoint (iPAddress, _port);
                _socketConnection = new Socket (iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connection
                _socketConnection.BeginConnect (remoteEp, ConnectCallback, _socketConnection);
                _connectDone.WaitOne ();

                // Start receiving
                while (!_stop) {
                    Receive ();
                    _receiveDone.WaitOne ();
                }
            } catch (SocketException e) {
                _onConnectionFail ();
                LogError (e);
            } catch (ThreadAbortException e) {
                _onConnectionFail ();
                LogError (e);
            } catch (Exception e) {
                _onConnectionFail ();
                LogError (e);
            } finally {
                if (_socketConnection != null && _socketConnection.Connected) _socketConnection.Close ();
            }
        }

        #region Connect

        private void ConnectCallback (IAsyncResult ar) {
            try {
                _socketConnection.EndConnect (ar);
                Log ("Connected to TCP to server " + _address + " on port " + _port + "!");
                _onReady ();
                _connectDone.Set ();
            } catch (Exception e) {
                _onConnectionFail ();
                LogError (e);
            }
        }

        #endregion

        #region Receive

        private void Receive () {
            try {
                var stateObject = new StateObject ();
                stateObject.workSocket = _socketConnection;
                _socketConnection.BeginReceive (stateObject.buffer, 0, StateObject.bufferSize, 0, ReceiveCallback,
                    stateObject);
            } catch (Exception e) {
                LogError (e);
            }
        }

        private void ReceiveCallback (IAsyncResult ar) {
            try {
                var state = (StateObject) ar.AsyncState;
                var bytesRead = _socketConnection.EndReceive (ar);

                if (bytesRead > 0) {
                    state.sb.Append (Encoding.ASCII.GetString (state.buffer, 0, bytesRead));
                    HandleReceived (state);
                }

                _socketConnection.BeginReceive (state.buffer, 0, StateObject.bufferSize, 0, ReceiveCallback, state);
            } catch (Exception e) {
                LogError (e);
            }
        }

        private void HandleReceived (StateObject state) {
            var datas = state.sb.ToString ();
            state.sb.Clear ();
            var pos = 0;
            var nextIndex = datas.IndexOf ("\\n", pos);
            var keep = "";
            var line = "";
            while (nextIndex != -1) {
                line = keep + datas.Substring (pos, nextIndex - pos);
                pos = nextIndex + 2;
                if (pos >= datas.Length - 1 || datas[nextIndex - 1] != '\\') {
                    keep = "";
                    _receiveQueue.Enqueue (line);
                } else {
                    keep = line;
                }

                nextIndex = datas.IndexOf ("\\n", pos);
            }

            if (pos < datas.Length) {
                var oldDatas = datas.Substring (pos, datas.Length - pos);
                state.sb.Insert (0, oldDatas, 1);
            }
        }

        #endregion
    }

    // State object for receiving data from remote device.  
    public class StateObject {
        // Size of receive buffer.  
        public const int bufferSize = 256;

        // Receive buffer.  
        public byte[] buffer = new byte[bufferSize];

        // Received data string.  
        public StringBuilder sb = new StringBuilder ();

        // Client socket.  
        public Socket workSocket;
    }
}