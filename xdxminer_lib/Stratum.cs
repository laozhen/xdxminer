using System;
using System.Collections;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using xdxminer_lib.util;

namespace xdxminer_lib
{


    public enum Protocol
    {
        Stratum, EthereumStratum_1
    }
    public class Stratum 
    {
        private Logger logger;
        private Hashtable PendingACKs = new Hashtable();
        private TcpClient tcpClient;
        private string page = "";
        private bool isStopped = false;
        private bool isSSL = true;
        private string difficultyHexStr;
        private ulong extraNonce;
        private int ExtraNonce2 = 0;

        private string Server;
        private int Port;
        private string Username;
        private string Password;
        private int ID;
        private Task solutionSumitter;
        private Task reconnecter;
        private Protocol protocol = Protocol.Stratum;
        private Stream networkStream;

        public static int acceptShare = 0;
        public static int foundShare = 0;
       

        public Stratum()
        {
            logger = new Logger("inet");
        }


        public void setDetails (string server, int port, string username, bool isSSL ,string password = null)
        {
            this.isSSL = isSSL;
            Server = server;
            Port = port;
            Username = username;
            Password = password;
        }

        public void start ()
        {
            isStopped = true;
            solutionSumitter?.Wait();
            reconnecter?.Wait();
            isStopped = false;

            solutionSumitter= Task.Run(() =>
            {
                while (!isStopped)
                {
                    Solution solution;
                    if (Message.tryGetSolution(out solution))
                    {
                        string result = string.Format("{0:X16}", solution.nounce).ToLower();
                        sendSubmit(solution.jobId, result, solution.header, StratumUtil.ByteArrayToHexString(solution.mixHash));
                    }
                    Thread.Sleep(5);
                }
            }) ;

            reconnecter = Task.Run(() =>
            {
               while (!isStopped)
               {
                    try
                    {
                        connect();
                    }
                    catch (Exception ex)
                   {
                       PendingACKs.Clear();
                       logger.error("connection error url:{0}, will reconnect in 10 seconds.", Server);
                       logger.error(ex.Message);
                       Thread.Sleep(10000);
                   }
               }
            });
        }

        public void stop()
        {
            isStopped = true;
            logger.info("stopping connection.");
        }

        public void delayStop()
        {
            Task.Run(async delegate
            {
                await Task.Delay(5000);
                isStopped = true;
            });
        }


        private void closeConnection ()
        {
            tcpClient.Close();
            tcpClient.Dispose();
            tcpClient = null;
            PendingACKs.Clear();
            logger.info(" Disconnected. Closing");
        }

        private void connect()
        {
            ID = 1;
            tcpClient = new TcpClient(AddressFamily.InterNetwork);

            tcpClient.Connect(Server, Port);
            networkStream = createNetworkStream();
            subscribeAndAuthorize();

            byte[] buffer = new byte[tcpClient.ReceiveBufferSize];

            // Now we are connected start read operation.
            do
            {
                int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                processBytes(buffer, bytesRead);
            } while (!isStopped);
           
        }
        private bool ValidateServerCertificate(object sender,X509Certificate certificate,X509Chain chain,SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            logger.error("server certificate is invalid. ");
            if (Server.Contains("nanopool.org"))
            {

                return true;
            }
            else
            {
                closeConnection();
                return false;
            }
        }

        protected virtual Stream createNetworkStream()
        {
            if (tcpClient.Connected)
            {
                if (isSSL)
                {
                    SslStream sslStream = new SslStream(tcpClient.GetStream(), false,
                        new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                    sslStream.AuthenticateAsClient(Server);
                    logger.info("Connected to " + Server +" (SSL)");
                    return sslStream;
                } else
                {
                    return tcpClient.GetStream();
                }
            }
            else
            {
                logger.error("Unable to connect to server {0} on port {1}", Server, Port);
                closeConnection();
                return null;
            }
        }

        private void subscribeAndAuthorize()
        {
            sendSubscribe();
            sendAuthorize();
        }

        public void sendSubscribe()
        {
            Byte[] bytesSent;
            StratumCommand Command = new StratumCommand();

            Command.id = ID++;
            Command.method = "mining.subscribe";
            Command.parameters = new ArrayList();

            string request = StratumUtil.JsonSerialize(Command) + "\n";

            bytesSent = Encoding.ASCII.GetBytes(request);

            try
            {
                networkStream.Write(bytesSent, 0, bytesSent.Length);
                PendingACKs.Add(Command.id, Command.method);
            }
            catch (Exception ex)
            {
                logger.error("Socket error 1:" + ex.Message);
                closeConnection();
            }

            logger.debug("sending subscribe" + request);
            logger.info("Sent subscribe");
        }

        public void sendAuthorize()
        {
            Byte[] bytesSent;
            StratumCommand Command = new StratumCommand();

            Command.id = ID++;
            Command.method = "mining.authorize";
            Command.parameters = new ArrayList();
            if (Username != null) {
                Command.parameters.Add(Username);
            }
            if (Password != null)
            {
                Command.parameters.Add(Password);
            }

            string request = StratumUtil.JsonSerialize(Command) + "\n";

            logger.debug("sending " + request);

            bytesSent = Encoding.ASCII.GetBytes(request);

            try
            {
                networkStream.Write(bytesSent, 0, bytesSent.Length);
                PendingACKs.Add(Command.id, Command.method);
            }
            catch (Exception ex)
            {
                logger.error("Socket error:" + ex.Message);
                closeConnection();
            }

            logger.info("Sent mining.authorize");

        }

        public void sendSubmit(string JobID, string nonce, string header, string mixHash)
        {
            StratumCommand Command = new StratumCommand();
            Command.id = ID++;
            Command.method = "mining.submit";
            Command.parameters = new ArrayList();
            switch (protocol)
            {
                case Protocol.Stratum:
                    Command.parameters.Add(Username);
                    Command.parameters.Add(JobID);
                    Command.parameters.Add(nonce);
                    Command.parameters.Add(header);
                    Command.parameters.Add(mixHash);
                    break;
                case Protocol.EthereumStratum_1:
                    Command.parameters.Add(Username);
                    Command.parameters.Add(JobID);
                    Command.parameters.Add(nonce);
                    break;
            }


            string SubmitString = StratumUtil.JsonSerialize(Command) + "\n";

            Byte[] bytesSent = Encoding.ASCII.GetBytes(SubmitString);

            try
            {
                networkStream.Write(bytesSent, 0, bytesSent.Length);
                PendingACKs.Add(Command.id, Command.method);
            }
            catch (Exception ex)
            {
                logger.error("Socket error:" + ex.Message);
                closeConnection();
            }

            logger.debug("submitting " + SubmitString);
            foundShare++;
            logger.info("Submit share. jobId {0} , nounce {1}", JobID, nonce);
        }

        // process byres read from connection
        private void processBytes(byte[] buffer, int bytesread) 
        {
            if (bytesread == 0)
            {
                closeConnection();
            }

            // Get the data
            string data = ASCIIEncoding.ASCII.GetString(buffer, 0, bytesread);

            page = page + data;

            int FoundClose = page.IndexOf('}');

            while (FoundClose > 0)
            {
                string CurrentString = page.Substring(0, FoundClose + 1);

                // We can get either a command or response from the server. Try to deserialise both
                StratumCommand Command = StratumUtil.JsonDeserialize<StratumCommand>(CurrentString);
                StratumResponse Response = StratumUtil.JsonDeserialize<StratumResponse>(CurrentString);


                if (Command.method != null)             // got a command
                {
                    logger.debug("Got Command: {0}", CurrentString);

                    switch (Command.method)
                    {
                        case "mining.notify":
                            GotNotify(Command);
                            break;
                        case "mining.set_difficulty":
                            GotSetDifficulty(Command);
                            break;
                    }
                }
                else if (Response.error != null || Response.result != null)       // We got a response
                {
                    logger.debug(" got sesponse: " + CurrentString);

                    //Login issue 
                    if(Response.id ==1)
                    {
                        if (Response.result.Equals(false)) {
                            logger.error("fail to connect to pool, please check your connection details");
                            closeConnection();
                        }

                        if(CurrentString.Contains("EthereumStratum/1.0.0"))
                        {
                            object[] arr = (object[])Response.result;
                            protocol = Protocol.EthereumStratum_1;
                            extraNonce = NewJob.StringToULong(((string) arr[1]).PadLeft(16,'0'));
                            logger.info("setting extra nounce to " + extraNonce);
                        }
                    }

                    //Submit result
                    if(Response.id > 2)
                    {
                        if(Response.result.Equals(true))
                        {
                            acceptShare++;
                            logger.info("shared accepted !");
                        }else
                        {
                            logger.error("shared rejected, reason : {0} " + Response.error);
                        }
                    }

                    string Cmd = (string)PendingACKs[Response.id];
                    PendingACKs.Remove(Response.id);

                    if (Cmd == null)
                    {
                        logger.error("Unexpected Response");
                    }
                    else
                    {
                        GotResponse(Cmd);
                    }
                }

                page = page.Remove(0, FoundClose + 2);
                FoundClose = page.IndexOf('}');
            }
        }

        public void GotNotify(StratumCommand stratumCommand)
        {
            NewJob newJob = new NewJob(stratumCommand, protocol, difficultyHexStr, extraNonce);
            Message.addLatestJob(newJob);
        }

        public void GotSetDifficulty(StratumCommand stratumCommand)
        {
            decimal difficulty = (decimal) stratumCommand.parameters.ToArray()[0];
            difficultyHexStr = NewJob.getTargetFromDiff_V1(difficulty);
            logger.info("setting difficulty to " + difficultyHexStr);
        }

        public void GotResponse (String s)
        {
            logger.debug("Got response"+s);
        }

    }

    [DataContract]
    public class StratumCommand
    {
        [DataMember]
        public string method;
        [DataMember]
        public System.Nullable<int> id;
        [DataMember(Name = "params")]
        public ArrayList parameters;
    }

    [DataContract]
    public class StratumResponse
    {
        [DataMember]
        public object error;
        [DataMember]
        public System.Nullable<int> id;
        [DataMember]
        public object result;
    }

    public class StratumEventArgs : EventArgs
    {
        public object MiningEventArg;
    }
}
