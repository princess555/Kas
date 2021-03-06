﻿using System;
using System.Text;
using System.Linq;
using System.IO.Pipes;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DBSyncConsole
{
    /// <summary>
    /// Считаем данные чкерез канал
    /// </summary>
    class PipeClient
    {
        public static void Send(string SendStr, string PipeName, int TimeOut = 1000)
        {
            try
            {
                NamedPipeClientStream pipeStream = new NamedPipeClientStream
                   (".", PipeName, PipeDirection.Out, PipeOptions.Asynchronous);

                // The connect function will indefinitely wait for the pipe to become available
                // If that is not acceptable specify a maximum waiting time (in ms)
                pipeStream.Connect(TimeOut);
                Console.WriteLine("[Client] Pipe connection established");

                byte[] _buffer = Encoding.UTF8.GetBytes(SendStr);
                pipeStream.BeginWrite
                (_buffer, 0, _buffer.Length, new AsyncCallback(AsyncSend), pipeStream);
            }
            catch (TimeoutException oEX)
            {
                Console.WriteLine(oEX.Message);
            }
        }

        private static void AsyncSend(IAsyncResult iar)
        {
            try
            {
                // Get the pipe
                NamedPipeClientStream pipeStream = (NamedPipeClientStream)iar.AsyncState;

                // End the write
                pipeStream.EndWrite(iar);
                pipeStream.Flush();
                pipeStream.Close();
                pipeStream.Dispose();
            }
            catch (Exception oEX)
            {
                Console.WriteLine(oEX.Message);
            }
        }
    }
}