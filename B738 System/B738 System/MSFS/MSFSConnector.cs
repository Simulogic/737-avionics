﻿using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Diagnostics;

namespace B738_System.MSFS
{
    public class MSFSConnector : SimConnection
    {
        /// User-defined win32 event
        public const int WM_USER_SIMCONNECT = 0x0402;

        /// Window handle
        private IntPtr _handle = new IntPtr(0);

        /// SimConnect object
        private SimConnect _simConnect = null;

        private bool _simConnectConnected = false;
        private bool _connected = false;

        public override bool IsConnected => _connected;
        public override bool IsConnectorConnected => _simConnectConnected;

        public SimConnect SimConnect { get { return _simConnect; } }

        #region Message Events
        public void ReceiveSimConnectMessage()
        {
            try
            {
                _simConnect?.ReceiveMessage();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Disconnect();
            }
        }
        #endregion

        #region Connection Events
        public override void Connect()
        {
            if(!IsConnectorConnected)
            {
                try
                {
                    _simConnect = new SimConnect("B738 System", _handle, WM_USER_SIMCONNECT, null, 0);

                    _simConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(SimConnect_OnRecvOpen);
                    _simConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(SimConnect_OnRecvQuit);

                    _simConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(SimConnect_OnRecvException);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            SIMCONNECT_EXCEPTION eException = (SIMCONNECT_EXCEPTION)data.dwException;
            Debug.WriteLine(eException.ToString());
        }

        private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            _simConnectConnected = true;

            // Register client data event
            sender.OnRecvClientData += SimConnect_OnRecvClientData;
            sender.OnRecvSimobjectData += SimConnect_OnRecvSimobjectData;
            sender.OnRecvSystemState += SimConnect_OnRecvSystemState;
        }

        private void SimConnect_OnRecvSystemState(SimConnect sender, SIMCONNECT_RECV_SYSTEM_STATE data)
        {
            RecvSimDataArgs args = new RecvSimDataArgs
            {
                data = data
            };
            OnRecvSimSystemData(args);
        }

        private void SimConnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            RecvSimDataArgs args = new RecvSimDataArgs
            {
                data = data
            };
            OnRecvSimUserData(args);
        }

        private void SimConnect_OnRecvClientData(SimConnect sender, SIMCONNECT_RECV_CLIENT_DATA data)
        {
            RecvSimDataArgs args = new RecvSimDataArgs
            {
                data = data
            };
            OnRecvSimData(args);
        }

        private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Disconnect();
        }

        public override void Disconnect()
        {
            if(_simConnect != null )
            {
                _simConnect.Dispose();
                _simConnect = null;
            }

            if(_simConnectConnected || _connected)
            {
                _simConnectConnected = false;
                _connected = false;
            }
        }
        #endregion

        public override void SetHandle(IntPtr handle)
        {
            _handle = handle;
        }

        public override void OnRecvSimData(RecvSimDataArgs e)
        {
            base.OnRecvSimData(e);
        }

        public override void OnRecvSimUserData(RecvSimDataArgs e)
        {
            base.OnRecvSimUserData(e);
        }

        public override void OnRecvSimSystemData(RecvSimDataArgs e)
        {
            base.OnRecvSimSystemData(e);
        }
    }
}
