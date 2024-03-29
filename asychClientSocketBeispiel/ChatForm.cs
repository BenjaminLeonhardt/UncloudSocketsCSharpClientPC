﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace asychClientSocketBeispiel {
    public partial class ChatForm : Form {

        public static ManualResetEvent connectDone = new ManualResetEvent(false);
        public static ManualResetEvent sendDone = new ManualResetEvent(false);
        public static ManualResetEvent receiveDone = new ManualResetEvent(false);
        public static IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        public static IPAddress ipAddress = ipHostInfo.AddressList[ipHostInfo.AddressList.Length - 1];
        public static IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 5002);
        public static Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        public ChatForm() {
            InitializeComponent();
            //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[ipHostInfo.AddressList.Length - 1];
            //IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 5002);

            //// Create a TCP/IP socket.  
            //Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                //Console.WriteLine("gebe semaphore frei");

                //while (Form1.run) {
                // Set the event to nonsignaled state.  
                //allDone.Reset();

                // Start an asynchronous socket to listen for connections.  
                //Console.WriteLine("Waiting for a connection...");

                try {
                    ParameterizedThreadStart pts = new ParameterizedThreadStart(acceptThread);
                    Thread sendThreadObj = new Thread(pts);
                    sendThreadObj.Start(listener);
                } catch {

                }

                //listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                Thread.Sleep(100);                
                // Wait until a connection is made before continuing.  
                //allDone.WaitOne();
                //}
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public void acceptThread(Object listener) {
            Socket socket = (Socket)listener;
            while (Form1.run) {
                socket.BeginAccept(new AsyncCallback(AcceptCallback), (Socket)socket);
                Thread.Sleep(100);
            }
        }

        public static Semaphore semaphoreTextSenden = new Semaphore(1, 1);
        public static List<Peer> peersListe = new List<Peer>();

        public void AcceptCallback(IAsyncResult ar) {
            // Signal the main thread to continue.  
            //allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            bool gefunden = false;

            foreach (var peer in peersListe) {
                if (peer.socket.RemoteEndPoint.ToString() == handler.RemoteEndPoint.ToString()) {
                    gefunden = true;
                }
            }
            if (!gefunden) {
                Peer p = new Peer();
                p.socket = handler;
                peersListe.Add(p);
            }

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            Thread.Sleep(100);
            //handler.BeginAccept(new AsyncCallback(AcceptCallback), state);
        }


        private void chatSendeButton_Click(object sender, EventArgs e) {

            StateObject chatPeer = new StateObject();
            foreach (StateObject item in Form1.chatObjekte) {
                if (this.Text.Contains(item.peerName)) {
                    chatPeer = item;
                    item.chatForm = this;
                    break;
                }
            }

            string _ip = chatPeer.workSocket.RemoteEndPoint.ToString();
            string[] ipArray = _ip.Split(':');
            foreach (string item in ipArray) {
                if (item.Contains("192")) {
                    int zahl = -1;
                    try {
                        zahl = Convert.ToInt32(item[item.Length - 1]);
                    } catch {

                    }
                    if (zahl != -1 && zahl != 93) {
                        _ip = item;
                        break;
                    } else {
                        _ip = item.Substring(0, ipArray[3].Length - 1);
                        break;
                    }
                }
            }

            chatPeer.ip = _ip;

            if (chatPeer.chatSocket == null || !chatPeer.chatSocket.Connected) {
                //_ip = ipArray[3].Substring(0, ipArray[3].Length - 1);
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(_ip), 5002);
                Socket client = new Socket(SocketType.Stream, ProtocolType.Tcp);
                chatPeer.chatSocket = client;
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                //connectDone.WaitOne();

                StateObject state = new StateObject();
                state.workSocket = client;
                Thread.Sleep(100);
                try {
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);

                } catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                }

                Thread.Sleep(100);
                //sendDone.WaitOne();
                sendThread(client);
            }else {
                sendThread(chatPeer.chatSocket);
            }
            
            chatText.Text = chatText.Text + Environment.NewLine + Environment.NewLine + Form1.eigenerName + ": " + chatEingabeFeld.Text;
            chatText.SelectionStart = chatText.Text.Length;
            chatText.ScrollToCaret();
            chatEingabeFeld.Text = "";
        }

        void sendThread(Object client) {

            string HostName = Dns.GetHostName();

            IPHostEntry hostInfo = Dns.GetHostEntry(HostName);
            //IpAdresse = hostInfo.AddressList[hostInfo.AddressList.Length - 1].ToString();

            IPAddress ipAddress = hostInfo.AddressList[hostInfo.AddressList.Length - 1];

            // Send test data to the remote device.  
            string text = "beg{" + "5" + "☻" + Form1.eigenerName + "☻" + ipAddress.ToString() + "☻" + chatEingabeFeld.Text + "☻" + "}end";
            //if(semaphoreTextSenden.)
            //semaphoreTextSenden.Release();
            Send((Socket)client, text);

        }

        private static void Send(Socket client, String data) {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            //Console.WriteLine("sende zu " + client.RemoteEndPoint.ToString() + " " + data);
            // Begin sending the data to the remote device.  
            if (Form1.run) {
                try {
                    client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
                } catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                }
                
            }
        }

        private static void SendCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to Peer.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar) {
            try {
                sendDone.Set();
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                state.sb = new StringBuilder();
                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0) {

                    // There  might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                    
                    string content = state.sb.ToString();

                    if (content.IndexOf("}end") > -1) {
                        // All the data has been read from the   
                        // client. Display it on the console.  
                        Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);
                        // Echo the data back to the client.  
                        int begin = content.IndexOf("beg{");
                        int ende = content.IndexOf("}end");

                        string contentOhneHeaderUndTailer = "";
                        for (int j = begin + 4; j < ende; j++) {
                            contentOhneHeaderUndTailer += content[j];
                        }
                        int aktion = -1;

                        aktion = Int32.Parse("" + contentOhneHeaderUndTailer[0]);
                        if (aktion == (int)aktionEnum.chatMessage) {

                            string[] aufgeteilteNachricht = content.Split('☻');
                            string empfangenerChatText = aufgeteilteNachricht[3];
                            foreach (StateObject item in Form1.chatObjekte) {
                                if (item.peerName.Contains(aufgeteilteNachricht[1])) {
                                    try {
                                        item.chatForm.Invoke((MethodInvoker)delegate {
                                            item.chatForm.Activate();
                                            item.chatForm.chatText.Text = item.chatForm.chatText.Text + Environment.NewLine + Environment.NewLine + aufgeteilteNachricht[1] + ": " + empfangenerChatText;
                                            item.chatForm.chatText.SelectionStart = item.chatForm.chatText.Text.Length;
                                            item.chatForm.chatText.ScrollToCaret();
                                        });
                                    } catch (Exception ex) {
                                        Console.WriteLine(ex.ToString());
                                        try {
                                            ChatForm chatformNeu = new ChatForm();
                                            chatformNeu.Text = "Chat mit " + aufgeteilteNachricht[1];
                                            item.chatForm = chatformNeu;
                                            chatformNeu.chatText.Text = item.chatForm.chatText.Text + Environment.NewLine + Environment.NewLine + aufgeteilteNachricht[1] + ": " + empfangenerChatText;
                                            chatformNeu.ShowDialog();
                                            item.chatForm.Activate();
                                            //Application.Run(chatform);
                                        } catch (Exception ex2) {
                                            Console.WriteLine(ex2.ToString());
                                        }
                                    }
                                    
                                    break;
                                }
                            }
                        }
                    }
                }
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            
        }

        public void ConnectCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public void DisconnectCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndDisconnect(ar);
                client.Shutdown(SocketShutdown.Both);
                client.Close();

                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  

            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e) {
            
        }

        private void chatEingabeFeld_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                chatSendeButton_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            
        }
    }
}
